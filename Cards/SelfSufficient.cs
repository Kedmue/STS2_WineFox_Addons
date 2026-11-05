using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Cards
{
    /// <summary>
    /// 自给自足：无色罕见能力牌。
    /// 战斗结束时将 1 个随机食物加入奖励列表，可叠加（打出几次就额外加入几个）。
    /// 升级后费用由 1 降为 0。
    /// </summary>
    [RegisterCard(typeof(ColorlessCardPool))]
    public sealed class SelfSufficient() : ModCardTemplate(
        1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar("Food", 1m),
        ];

        public override CardAssetProfile AssetProfile =>
            new("res://STS2_WineFox_Addons/cards/self_sufficient.png",
                "res://STS2_WineFox_Addons/cards/self_sufficient.png");

        protected override async Task OnPlay(
            PlayerChoiceContext choiceContext,
            CardPlay play)
        {
            await PowerCmd.Apply<SelfSufficientPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars["Food"].BaseValue,
                Owner.Creature,
                this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1); // 1 → 0
        }
    }
}
