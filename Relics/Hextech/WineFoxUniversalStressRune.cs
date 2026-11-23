using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 让应力对原本不会触发应力的材料获取方式同样生效。
    /// </summary>
    public sealed class WineFoxUniversalStressRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_universal_stress_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_universal_stress_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_universal_stress_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
            HoverTipFactory.FromPower<StressPower>(),
        ];

        internal static bool HasRune(Player? player)
        {
            return player?.Relics.OfType<WineFoxUniversalStressRune>().Any() == true;
        }
    }
}
