using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每当拥有者获得材料时，对随机敌人造成 5 点伤害。
    /// </summary>
    public sealed class WineFoxMaterialSparkRune : WineFoxMaterialRuneBase
    {
        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_byproduct_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_byproduct_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_byproduct_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
        ];
    }
}
