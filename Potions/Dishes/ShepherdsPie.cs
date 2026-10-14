using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox_Addons.Character;
using STS2_WineFox_Addons.Keywords;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Potions.Dishes
{
    /// <summary>
    ///     牧羊人派 —— 主食型的正餐。
    ///     <para>
    ///         设计思路是"给下一顿留粮"：派是农家填肚子的主食，
    ///         所以它给的不是当下的爆发，而是把资源留到下一个回合 ——
    ///         下回合额外抽 3 张牌、额外获得 3 点能量
    ///         （原版「下回合抽牌」「下回合能量」能力，都可叠加）。
    ///         现有食物里没有任何一种提供过跨回合的手牌或能量资源（面包只补 1 张、闪烁的西瓜只给当下 1 点能量），
    ///         这条轴是空着的。
    ///     </para>
    ///     <para>
    ///         战斗内：回复 7 点生命，获得 1 层[滋养]，并在下回合额外抽 3 张牌、额外获得 3 点能量。
    ///         遵守食物的通用约定：战斗外只保留回复。
    ///     </para>
    /// </summary>
    [RegisterPotion(typeof(WineFoxAddonsDishPotionPool))]
    public sealed class ShepherdsPie : DishPotionModel
    {
        public override PotionRarity Rarity => PotionRarity.Rare;

        protected override int SellGold => 40;

        public override PotionAssetProfile AssetProfile =>
            Art("res://STS2_WineFox_Addons/potions/dishes/shepherds_pie.png");

        protected override IEnumerable<CardKeyword> DisplayKeywords => [WineFoxAddonsKeywords.Nourishment];

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);

            await CreatureCmd.Heal(targetCreature, 7);
            await PowerCmd.Apply<NourishmentPower>(choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, targetCreature, 3, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, targetCreature, 3, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) =>
            CreatureCmd.Heal(Owner.Creature, 7);
    }
}
