using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Cards.Token.Craft;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 升级“砖石甲”，使其附带的回合结束能力不再降低敏捷。
    /// </summary>
    public sealed class WineFoxStoneArmorRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_eternal_armor_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_eternal_armor_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_eternal_armor_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<StoneArmor>(false),
            HoverTipFactory.FromPower<DexterityPower>(),
        ];
    }
}
