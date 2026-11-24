using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Cards.Rare;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// “全副武装”原本的翻倍条件同时拥有应力时，改为三倍伤害。
    /// </summary>
    public sealed class WineFoxArmToTeethRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_arm_to_teeth_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_arm_to_teeth_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_arm_to_teeth_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<ArmToTeeth>(false),
            HoverTipFactory.FromPower<PlatingPower>(),
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.FromPower<StressPower>(),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<ArmToTeeth>(this);
        }

        internal static bool HasStress(ArmToTeethPower power)
        {
            return power.Owner.Powers
                .OfType<StressPower>()
                .Any(stress => stress.Amount > 0m);
        }
    }
}
