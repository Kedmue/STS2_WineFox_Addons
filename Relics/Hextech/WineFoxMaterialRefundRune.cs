using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每次消耗材料时，有 50% 概率返还本次消耗的整批材料。
    /// </summary>
    public sealed class WineFoxMaterialRefundRune : WineFoxMaterialRuneBase
    {
        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_jinlian_box_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_jinlian_box_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_jinlian_box_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
        ];
    }
}
