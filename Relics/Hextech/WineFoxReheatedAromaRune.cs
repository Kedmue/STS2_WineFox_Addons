using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Potions;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 海克斯大乱斗可选联动中的酒狐参考符文。
    ///
    /// 食物与菜肴都继承 <see cref="SellableToMerchantPotionModel" />，
    /// 因此符文可以用同一种稳定方式识别所有酒狐食物。
    /// </summary>
    public sealed class WineFoxReheatedAromaRune : ModRelicTemplate
    {
        // 海克斯大乱斗在内部会把所有外部玩家符文视为初始稀有度遗物；
        // 白银、黄金、棱彩等实际稀有度由注册元数据提供。
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_stoke_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_stoke_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_stoke_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            new HoverTip(
                new LocString("gameplay_ui", "WINE_FOX_RUNE_FOOD.title"),
                new LocString("gameplay_ui", "WINE_FOX_RUNE_FOOD.description")),
        ];

        public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
        {
            if (potion is not SellableToMerchantPotionModel || potion.Owner != Owner)
                return;

            var combatManager = CombatManager.Instance;
            if (combatManager == null || !combatManager.IsInProgress || combatManager.IsOverOrEnding)
                return;

            if (Owner.Creature.IsDead)
                return;

            Flash();

            await PlayerCmd.GainEnergy(1m, Owner);
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 1m, Owner, fromHandDraw: false);
        }
    }
}
