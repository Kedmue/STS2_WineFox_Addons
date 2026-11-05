using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves;
using STS2_WineFox.Relics;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 获得时依次获得准备背包、女仆背包和精妙背包。
    /// </summary>
    public sealed class WineFoxBackpackCollectorRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_mobile_home_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_mobile_home_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_mobile_home_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            HoverTipFactory.FromRelic<BagOfPreparation>()
                .Concat(HoverTipFactory.FromRelic<MaidBackpack>())
                .Concat(HoverTipFactory.FromRelic<SophisticatedBackpack>());

        public override async Task AfterObtained()
        {
            Flash();

            var preparationBag = await RelicCmd.Obtain<BagOfPreparation>(Owner);
            SaveManager.Instance.MarkRelicAsSeen(preparationBag);

            var maidBackpack = await RelicCmd.Obtain<MaidBackpack>(Owner);
            SaveManager.Instance.MarkRelicAsSeen(maidBackpack);

            var sophisticatedBackpack = await RelicCmd.Obtain<SophisticatedBackpack>(Owner);
            SaveManager.Instance.MarkRelicAsSeen(sophisticatedBackpack);
        }
    }
}
