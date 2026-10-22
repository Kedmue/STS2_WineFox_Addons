using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using STS2_WineFox_Addons.Scripts;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 参照观者扩展的遗物百科集成方式：先从通用海克斯列表中移除
    /// 酒狐符文，再在海克斯符文分类下添加独立的“酒狐海克斯”子分类。
    /// </summary>
    internal static class WineFoxHextechCollectionHooks
    {
        private const string HarmonyId = "Kedmue.STS2_WineFox_Addons.HextechCollection";
        private const string HextechSubcategoryKey = "HEXTECH_RUNES_SUBCATEGORY";
        private const string WineFoxSubcategoryKey = "HEXTECH_CHARACTER.WINE_FOX";
        private const string WineFoxPoolKey = "WINE_FOX";
        private const string SpacerName = "WineFoxHexGenericToCharacterPoolSpacer";
        private const float GenericToCharacterPoolSpacing = 96f;
        private const string FallbackHeader =
            "[gold][font_size=28][b]Wine Fox Hextech:[/b][/font_size][/gold] " +
            "Hextech runes available only to the Wine Fox.";

        private static readonly FieldInfo? HeaderLabelField =
            AccessTools.DeclaredField(typeof(NRelicCollectionCategory), "_headerLabel");

        private static readonly FieldInfo? SubCategoriesField =
            AccessTools.DeclaredField(typeof(NRelicCollectionCategory), "_subCategories");

        private static readonly MethodInfo? CreateForSubcategoryMethod =
            AccessTools.DeclaredMethod(typeof(NRelicCollectionCategory), "CreateForSubcategory");

        private static readonly MethodInfo? LoadSubcategoryMethod =
            AccessTools.DeclaredMethod(
                typeof(NRelicCollectionCategory),
                "LoadSubcategory",
                [
                    typeof(NRelicCollection),
                    typeof(LocString),
                    typeof(IEnumerable<RelicModel>),
                    typeof(HashSet<RelicModel>),
                    typeof(HashSet<RelicModel>),
                ]);

        private static readonly MethodInfo? LoadRelicsMethod =
            AccessTools.DeclaredMethod(typeof(NRelicCollectionCategory), "LoadRelics");

        private static MethodInfo? _isVisibleInCollection;
        private static NRelicCollectionCategory? _pendingHextechCategory;
        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var catalogType = FindHextechCatalogType()
                    ?? throw new TypeLoadException("HextechRunes.HextechCatalog not found");

                _isVisibleInCollection = catalogType.GetMethod(
                    "IsPlayerRuneTypeVisibleInCollection",
                    BindingFlags.Public | BindingFlags.Static);

                var harmony = new Harmony(HarmonyId);

                var poolKeyMethod = catalogType.GetMethod(
                    "GetPlayerRunePoolKey",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [typeof(RelicModel)],
                    null)
                    ?? throw new MissingMethodException("HextechCatalog.GetPlayerRunePoolKey not found");
                harmony.Patch(
                    poolKeyMethod,
                    postfix: new HarmonyMethod(
                        typeof(WineFoxHextechCollectionHooks),
                        nameof(PlayerRunePoolKeyPostfix)));

                var genericRunesMethod = catalogType.GetMethod(
                    "GetCanonicalGenericVisibleRunes",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    Type.EmptyTypes,
                    null)
                    ?? throw new MissingMethodException("HextechCatalog.GetCanonicalGenericVisibleRunes not found");
                harmony.Patch(
                    genericRunesMethod,
                    postfix: new HarmonyMethod(
                        typeof(WineFoxHextechCollectionHooks),
                        nameof(GenericVisibleRunesPostfix)));

                if (HeaderLabelField == null ||
                    SubCategoriesField == null ||
                    CreateForSubcategoryMethod == null ||
                    LoadSubcategoryMethod == null ||
                    LoadRelicsMethod == null)
                {
                    throw new MissingMemberException(
                        "NRelicCollectionCategory members changed.");
                }

                harmony.Patch(
                    LoadSubcategoryMethod,
                    postfix: new HarmonyMethod(
                        typeof(WineFoxHextechCollectionHooks),
                        nameof(LoadSubcategoryPostfix)));

                var loadRelicsPostfix = new HarmonyMethod(
                    typeof(WineFoxHextechCollectionHooks),
                    nameof(LoadRelicsPostfix))
                {
                    priority = 200,
                };
                harmony.Patch(LoadRelicsMethod, postfix: loadRelicsPostfix);

                _installed = true;
                logger.Info("已安装酒狐海克斯百科分类补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装酒狐海克斯百科分类补丁失败，百科仍可正常使用：{ex.Message}");
            }
        }

        private static Type? FindHextechCatalogType()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    var type = assembly.GetType("HextechRunes.HextechCatalog", throwOnError: false);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                    // 忽略尚未完整加载的可选程序集。
                }
            }

            return null;
        }

        private static void PlayerRunePoolKeyPostfix(RelicModel relic, ref string __result)
        {
            if (WineFoxHextechContent.IsWineFoxRune(relic) &&
                LocString.Exists("relic_collection", "HEXTECH_POOL.WINE_FOX"))
            {
                __result = WineFoxPoolKey;
            }
        }

        private static void GenericVisibleRunesPostfix(ref IReadOnlyList<RelicModel> __result)
        {
            if (__result == null || __result.Count == 0)
                return;

            __result = __result
                .Where(relic => !WineFoxHextechContent.IsWineFoxRune(relic))
                .ToArray();
        }

        private static void LoadSubcategoryPostfix(NRelicCollectionCategory __instance, LocString? header)
        {
            if (header != null &&
                header.LocTable == "relic_collection" &&
                header.LocEntryKey == HextechSubcategoryKey)
            {
                _pendingHextechCategory = __instance;
            }
        }

        private static void LoadRelicsPostfix(
            NRelicCollectionCategory __instance,
            RelicRarity relicRarity,
            NRelicCollection collection,
            HashSet<RelicModel> seenRelics,
            HashSet<RelicModel> allUnlockedRelics)
        {
            if (relicRarity != RelicRarity.Starter)
                return;

            var pending = _pendingHextechCategory;
            _pendingHextechCategory = null;

            if (pending == null ||
                !GodotObject.IsInstanceValid(pending) ||
                pending.GetParent() != __instance)
            {
                return;
            }

            try
            {
                if (AddWineFoxSubcategory(pending, collection, seenRelics, allUnlockedRelics))
                    Entry.Logger.Info("已添加“酒狐海克斯”百科子分类。");
            }
            catch (Exception ex)
            {
                Entry.Logger.Warn($"添加酒狐海克斯子分类失败：{ex.GetType().Name}: {ex.Message}");
            }
        }

        private static bool AddWineFoxSubcategory(
            NRelicCollectionCategory hextechCategory,
            NRelicCollection collection,
            HashSet<RelicModel> seenRelics,
            HashSet<RelicModel> allUnlockedRelics)
        {
            var visibleRunes = BuildVisibleRunes();
            if (visibleRunes.Count == 0)
                return false;

            var subcategories = (List<NRelicCollectionCategory>)SubCategoriesField!.GetValue(hextechCategory)!;
            var headerLabel = (Control)HeaderLabelField!.GetValue(hextechCategory)!;
            var hadNoSubcategories = subcategories.Count == 0;

            var insertIndex = hadNoSubcategories
                ? headerLabel.GetIndex() + 1
                : subcategories[^1].GetIndex() + 1;

            var subcategory = (NRelicCollectionCategory)CreateForSubcategoryMethod!
                .Invoke(hextechCategory, null)!;
            subcategories.Add(subcategory);
            hextechCategory.AddChild(subcategory, false, Node.InternalMode.Disabled);
            hextechCategory.MoveChild(subcategory, insertIndex);

            if (hadNoSubcategories)
            {
                var spacer = new Control
                {
                    Name = SpacerName,
                    CustomMinimumSize = new Vector2(0f, GenericToCharacterPoolSpacing),
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                };
                hextechCategory.AddChild(spacer, false, Node.InternalMode.Disabled);
                hextechCategory.MoveChild(spacer, subcategory.GetIndex());
            }

            var mergedSeen = seenRelics.Concat(visibleRunes).ToHashSet();
            var mergedUnlocked = allUnlockedRelics.Concat(visibleRunes).ToHashSet();

            var hasHeader = LocString.Exists("relic_collection", WineFoxSubcategoryKey);
            LoadSubcategoryMethod!.Invoke(
                subcategory,
                [
                    collection,
                    hasHeader ? new LocString("relic_collection", WineFoxSubcategoryKey) : null,
                    visibleRunes,
                    mergedSeen,
                    mergedUnlocked,
                ]);

            if (!hasHeader &&
                HeaderLabelField.GetValue(subcategory) is MegaRichTextLabel label)
            {
                label.SetTextAutoSize(FallbackHeader);
            }

            return true;
        }

        private static IReadOnlyList<RelicModel> BuildVisibleRunes()
        {
            var result = new List<RelicModel>();
            var orderedTypes = WineFoxHextechContent.RuneTypes
                .Select((type, index) => (Type: type, Index: index))
                .OrderBy(entry => WineFoxHextechContent.GetRaritySortOrder(entry.Type))
                .ThenBy(entry => entry.Index);

            foreach (var entry in orderedTypes)
            {
                if (!IsVisibleInCollection(entry.Type))
                    continue;

                var relic = ModelDb.GetByIdOrNull<RelicModel>(ModelDb.GetId(entry.Type));
                if (relic != null)
                    result.Add(relic);
            }

            return result;
        }

        private static bool IsVisibleInCollection(Type runeType)
        {
            if (_isVisibleInCollection == null)
                return true;

            try
            {
                return _isVisibleInCollection.Invoke(null, [runeType]) is true;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
