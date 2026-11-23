using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2_WineFox_Addons.Utils;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 战斗开始时，使用酒狐食物池中的随机食物填满所有空药水栏位。
    /// </summary>
    public sealed class WineFoxFeastPreparationRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_feast_preparation_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_feast_preparation_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_feast_preparation_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            new HoverTip(
                new LocString("gameplay_ui", "WINE_FOX_RUNE_FOOD.title"),
                new LocString("gameplay_ui", "WINE_FOX_RUNE_FOOD.description")),
        ];

        public override async Task BeforeCombatStart()
        {
            if (Owner.Creature.IsDead)
                return;

            Flash();
            while (Owner.HasOpenPotionSlots)
            {
                var potionModel = WineFoxFoodPotionCompat.CreateRandomFoodPotionForReward(
                    Owner,
                    Owner.RunState.Rng.CombatPotionGeneration);
                if (potionModel == null)
                    break;

                if (!(await PotionCmd.TryToProcure(potionModel.ToMutable(), Owner)).success)
                    break;
            }
        }
    }
}
