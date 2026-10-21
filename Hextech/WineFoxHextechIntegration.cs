using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2_WineFox.Character;
using STS2_WineFox_Addons.Scripts;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Optional ARAM: Mayhem integration. This class deliberately has no
    /// compile-time reference to the HextechRunes assembly.
    /// </summary>
    internal static class WineFoxHextechIntegration
    {
        private static bool _attempted;
        private static bool _registered;

        public static void TryRegister(Logger logger)
        {
            if (_attempted)
                return;
            _attempted = true;

            try
            {
                if (!IsHextechInteropAvailable())
                {
                    logger.Info("未检测到海克斯大乱斗，跳过可选符文联动。");
                    return;
                }

                WineFoxMechanicalSawPatch.Install(logger);
                WineFoxNetheriteChestplatePatch.Install(logger);
                WineFoxRuneSelectionFooterPatch.Install(logger);

                foreach (var runeType in WineFoxHextechContent.RuneTypes)
                {
                    HextechRunesInteropBridge.RegisterPlayerRune(
                        runeType,
                        rarity: WineFoxHextechContent.GetRarity(runeType),
                        flags: null,
                        characterPool: null,
                        characterOrder: 0,
                        tagKey: WineFoxHextechContent.GetTagKey(runeType),
                        assetModId: Entry.ModId,
                        isAvailableForPlayer: IsWineFoxPlayer);

                    // Wine Fox is an external character, so the vanilla character-pool
                    // enum cannot represent it. The interop API maps each rune to the
                    // actual character model and supplies its config/menu labels.
                    HextechRunesInteropBridge.SetPlayerRuneCharacter(runeType, typeof(WineFox));
                    HextechRunesInteropBridge.SetPlayerRunePoolLabel(runeType, "WINE_FOX");
                }

                HextechRunesInteropBridge.RegisterConfigSectionTitle(
                    Entry.ModId,
                    "WINE_FOX_ADDON_SECTION");

                _registered = true;
                logger.Info(
                    $"已通过 RitsuLib ModInterop 向海克斯大乱斗注册 " +
                    $"{WineFoxHextechContent.RuneTypes.Count} 枚酒狐专属符文。");
                WineFoxHextechCollectionHooks.Install(logger);

                VerifyAfterEssentialInitialization(logger);
            }
            catch (Exception ex)
            {
                logger.Error($"注册可选海克斯符文失败，已跳过：{ex}");
            }
        }

        private static bool IsHextechInteropAvailable()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    if (assembly.GetType("HextechRunes.HextechRunesInterop", throwOnError: false) != null)
                        return true;
                }
                catch (Exception)
                {
                    // A partially loaded assembly is equivalent to the optional target being unavailable.
                }
            }

            return false;
        }

        private static bool IsWineFoxPlayer(Player player)
        {
            return player.Character is WineFox ||
                   player.Character.Id.Entry.Contains("winefox", StringComparison.OrdinalIgnoreCase);
        }

        private static void VerifyAfterEssentialInitialization(Logger logger)
        {
            RitsuLibFramework.SubscribeLifecycle<EssentialInitializationCompletedEvent>(
                (_, subscription) =>
                {
                    subscription.Dispose();
                    if (_registered)
                    {
                        foreach (var runeType in WineFoxHextechContent.RuneTypes)
                            VerifyRegisteredModel(runeType, logger);
                    }
                },
                replayCurrentState: true);
        }

        private static void VerifyRegisteredModel(Type runeType, Logger logger)
        {
            try
            {
                var id = ModelDb.GetId(runeType);
                var relic = ModelDb.GetByIdOrNull<RelicModel>(id);
                if (relic == null)
                {
                    logger.Error($"海克斯符文模型未进入 ModelDb：{id}。");
                    return;
                }

                var iconPath = relic.IconPath;
                var hoverTipCount = relic.HoverTips.Count();
                var poolLabelExists = LocString.Exists("relic_collection", "HEXTECH_POOL.WINE_FOX");
                var tagLabelExists = LocString.Exists("relic_collection", "HEXTECH_TAG.FOOD");
                var sourceLabelExists = LocString.Exists("relic_collection", "WINE_FOX_ADDON_SECTION");
                logger.Info(
                    $"海克斯符文自检通过：id={id}, title={relic.Title.GetFormattedText()}, " +
                    $"hextech={HextechRunesApiBridge.IsHextechRelic(relic)}, " +
                    $"icon={iconPath}, iconExists={ResourceLoader.Exists(iconPath)}, " +
                    $"hoverTips={hoverTipCount}, " +
                    $"labels={poolLabelExists}/{tagLabelExists}/{sourceLabelExists}。");
            }
            catch (Exception ex)
            {
                logger.Error($"海克斯符文运行期自检失败：{ex}");
            }
        }
    }
}
