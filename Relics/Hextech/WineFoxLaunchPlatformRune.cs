using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards;
using STS2_WineFox.Cards.Uncommon;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// “弹射置物台”在场时，每次消耗应力都会获得 1 点能量。
    /// </summary>
    public sealed class WineFoxLaunchPlatformRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_launch_platform_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_launch_platform_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_launch_platform_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<LaunchPlatform>(false),
            HoverTipFactory.FromPower<StressPower>(),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<LaunchPlatform>(this);
        }

        internal async Task Trigger(LaunchPlatformPower power, Creature creature)
        {
            if (creature != power.Owner || power.Owner.Player == null)
                return;

            Flash();
            await PlayerCmd.GainEnergy(1m, power.Owner.Player);
        }
    }
}
