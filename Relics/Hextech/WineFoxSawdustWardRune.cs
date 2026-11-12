using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox;
using STS2_WineFox.Cards;
using STS2_WineFox.Commands;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每次完成合成后获得五点格挡。
    /// </summary>
    public sealed class WineFoxSawdustWardRune : ModRelicTemplate, ICraftListener
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_first_aid_kit_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_first_aid_kit_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_first_aid_kit_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(WineFoxKeywords.CraftKeyword),
        ];

        public async Task AfterCraftProductDelivered(CraftExecutionContext context)
        {
            if (context.Crafter != Owner.Creature)
                return;

            Flash();
            await CreatureCmd.GainBlock(
                Owner.Creature,
                5m,
                ValueProp.Unpowered,
                null);
        }
    }
}
