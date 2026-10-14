using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Potions;
using STS2_WineFox.Potions;
using STS2RitsuLib.Patching.Models;
using STS2_WineFox_Addons.Scripts;

namespace STS2_WineFox_Addons.Patches
{
    /// <summary>
    ///     把食物的第一个选项从"饮用"改成"食用"。
    ///     <para>
    ///         原版药水弹窗的第一个按钮只有两种文案，都在 <c>NPotionPopup._Ready</c> 里算出来：
    ///         用在自己身上的显示"饮用"（<c>POTION_POPUP.drink</c>），需要选目标扔出去的显示"扔出"（<c>POTION_POPUP.throw</c>）。
    ///         那段判定写死在方法里、没有留扩展点，而且本地化键是全局共用的 ——
    ///         直接改 <c>gameplay_ui</c> 表里的那个键会把治疗药水之类的非食物药水一起改掉。
    ///         所以这里在 <c>_Ready</c> 跑完之后，只对食物把"饮用"换成扩展自己的键。
    ///     </para>
    ///     <para>
    ///         食物以酒狐的 <see cref="SellableToMerchantPotionModel" />（可卖给商人的药水）为准，
    ///         酒狐本体的全部食物和扩展的菜肴都继承自它；药水必须投出去的情况保持原版的"扔出"不变。
    ///     </para>
    /// </summary>
    public sealed class FoodEatLabelPatch : IPatchMethod
    {
        /// <summary>扩展自己的"食用"文案键，写在 localization/zhs/gameplay_ui.json。</summary>
        private const string EatLocKey = "STS2_WINE_FOX_ADDONS_POTION_POPUP.eat";

        private const string VanillaLocTable = "gameplay_ui";

        private static readonly FieldInfo HolderField = AccessTools.Field(typeof(NPotionPopup), "_holder");
        private static readonly FieldInfo UseButtonField = AccessTools.Field(typeof(NPotionPopup), "_useButton");

        private static bool _loggedFailure;

        public static string PatchId => "sts2_winefox_addons_food_eat_label";

        public static bool IsCritical => false;

        public static string Description =>
            "Labels the potion popup's use button as '食用' (eat) instead of '饮用' (drink) for foods.";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(NPotionPopup), "_Ready")];

        // ReSharper disable once InconsistentNaming
        public static void Postfix(NPotionPopup __instance)
        {
            if (_loggedFailure)
                return;

            try
            {
                var holder = HolderField?.GetValue(__instance) as NPotionHolder;
                var potion = holder?.Potion?.Model;
                if (potion is not SellableToMerchantPotionModel)
                    return;

                // 复用原版判定：只有原版会显示"饮用"时才替换，需要扔出去的情况仍显示"扔出"。
                var targetType = potion.TargetType;
                var selectionBased =
                    (uint)(targetType - 2) <= 1u || targetType == TargetType.TargetedNoCreature;
                if (selectionBased || potion.CanThrowAtAlly())
                    return;

                // 没有这份本地化的语言下宁可不换，也不要让 LocString 取不到键而抛异常。
                var table = LocManager.Instance?.GetTable(VanillaLocTable);
                if (table == null || !table.HasEntry(EatLocKey))
                    return;

                var useButton = UseButtonField?.GetValue(__instance) as NPotionPopupButton;
                useButton?.SetLocKey(EatLocKey);
            }
            catch (Exception ex)
            {
                _loggedFailure = true;
                Entry.Logger.Warn($"[食物文案] 替换药水弹窗按钮文案失败，已跳过：{ex.Message}");
            }
        }
    }
}
