using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards;
using STS2_WineFox.Commands;
using STS2_WineFox.Powers;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 战斗开始时获得所有已注册材料各八份。
    /// </summary>
    public sealed class WineFoxRichVeinRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_infinite_loop_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_infinite_loop_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_infinite_loop_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromPower<WoodPower>(),
            HoverTipFactory.FromPower<StonePower>(),
            HoverTipFactory.FromPower<IronPower>(),
            HoverTipFactory.FromPower<DiamondPower>(),
            HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
        ];

        public override async Task BeforeCombatStart()
        {
            if (Owner.Creature.CombatState == null)
                return;

            await WineFoxMaterialRuneListenerPower.Ensure(Owner);
            Flash();
            await MaterialCmd.GainAllMaterials(
                Owner.Creature,
                8m,
                applyStress: false);
        }
    }
}
