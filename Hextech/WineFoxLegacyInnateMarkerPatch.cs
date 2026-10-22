using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2_WineFox.Cards.Basic;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 在酒狐同步状态可用后，从“基础合成”卡牌上移除海克斯旧有的
    /// 进程内“固有”标记。这样即使旧存档只在联机一端留有该标记，
    /// 双方也能在重新序列化后收敛到一致状态。
    /// </summary>
    internal static class WineFoxLegacyInnateMarkerPatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.LegacyBasicCraftInnateMarker";
        private const string LegacyMarkerName = "SavedCosplayInnateMarker";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var hooksType = FindLoadedType(
                    "HextechRunes.HextechThoughtOverwriteKeywordPersistenceHooks");
                if (hooksType == null)
                {
                    logger.Warn("未找到海克斯关键词持久化钩子，跳过旧 Innate 标记迁移。");
                    return;
                }

                var writeMarkers = AccessTools.Method(
                    hooksType,
                    "WriteMarkers",
                    [typeof(CardModel), typeof(SerializableCard)]);
                if (writeMarkers == null)
                {
                    logger.Warn("未找到海克斯 WriteMarkers，跳过旧 Innate 标记迁移。");
                    return;
                }

                var toSerializable = AccessTools.Method(
                    typeof(CardModel),
                    "ToSerializable",
                    Type.EmptyTypes);
                if (toSerializable == null)
                {
                    logger.Warn("未找到 CardModel.ToSerializable，跳过旧 Innate 标记迁移。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    toSerializable,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxLegacyInnateMarkerPatch),
                        nameof(ToSerializablePrefix)));
                harmony.Patch(
                    writeMarkers,
                    postfix: new HarmonyMethod(
                        typeof(WineFoxLegacyInnateMarkerPatch),
                        nameof(Postfix)));

                _installed = true;
                logger.Info("已安装基础合成旧 Innate 标记迁移补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装基础合成旧 Innate 标记迁移补丁失败：{ex.Message}");
            }
        }

        private static void ToSerializablePrefix(CardModel __instance)
        {
            if (__instance is BasicCraft basicCraft &&
                !WineFoxPersistentCardState.HasBasicCraftInnate(basicCraft) &&
                basicCraft.Keywords.Contains(CardKeyword.Innate))
            {
                // 必须在 RitsuLib 导出 SavedAttachedState 之前完成迁移，
                // 使升级后的首次序列化就写入新的同步属性，而不只是删除旧标记。
                WineFoxPersistentCardState.MarkBasicCraftInnate(basicCraft);
            }
        }

        private static void Postfix(CardModel card, SerializableCard save)
        {
            if (card is not BasicCraft basicCraft)
                return;

            // 旧存档可能已经显示“固有”，但还没有新的同步状态。
            // 因此先根据可见关键词完成迁移，再决定是否可以移除旧的本地标记。
            if (!WineFoxPersistentCardState.HasBasicCraftInnate(basicCraft) &&
                basicCraft.Keywords.Contains(CardKeyword.Innate))
            {
                WineFoxPersistentCardState.MarkBasicCraftInnate(basicCraft);
            }

            if (!WineFoxPersistentCardState.HasBasicCraftInnate(basicCraft))
                return;

            save.Props?.ints?.RemoveAll(
                property => string.Equals(
                    property.name,
                    LegacyMarkerName,
                    StringComparison.Ordinal));
        }

        private static Type? FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    var type = assembly.GetType(fullName, throwOnError: false);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                    // 忽略尚未完整加载的程序集，并继续查找。
                }
            }

            return null;
        }
    }
}
