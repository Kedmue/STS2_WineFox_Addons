using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Character;
using STS2_WineFox.Potions;
using STS2_WineFox.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Relics
{
    // 酒狐专属遗物：美食家围裙。战斗开始时，药水栏位里每有一个食物就给 2 层再生。
    //
    // 食物的判定沿用酒狐自己的标准：继承 SellableToMerchantPotionModel（可卖给商人的药水）。
    // 酒狐本体的全部食物与扩展的食物、菜肴都继承自它，所以「菜肴也算食物」这点和扩展本体的设定一致。
    [RegisterRelic(typeof(WineFoxRelicPool))]
    public sealed class GourmetApron : WineFoxRelic
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;

        public override RelicAssetProfile AssetProfile =>
            Icons("res://STS2_WineFox_Addons/relics/gourmet_apron.png");

        /// <summary>遗物提示里带上「再生」的说明（原版带能力效果的遗物都是这么做的）。</summary>
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<RegenPower>()];

        public override async Task BeforeCombatStart()
        {
            var foodCount = Owner.Potions.Count(potion => potion is SellableToMerchantPotionModel);
            if (foodCount <= 0)
                return;

            Flash();
            await PowerCmd.Apply<RegenPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                foodCount * 2,
                Owner.Creature,
                cardSource: null);
        }
    }
}
