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
    ///     蜜汁火腿 —— 分量扎实的一道硬菜。
    ///     <para>
    ///         设计思路是"一口顶两口"：火腿分量足，糖蜜又厚重，
    ///         吃完这一顿，接下来的两张牌都等于吃出了双份 ——
    ///         本回合打出的下两张牌各额外打出一次。直接用原版「复制」能力实现：
    ///         它每层对应"能多结算一张牌"，所以 2 层刚好是"下两张牌各多打一次"。
    ///         现有食物全部是数值型增益，没有任何一种改变过卡牌的结算次数，这条轴是空着的。
    ///     </para>
    ///     <para>
    ///         回复量给到 9：它不提供即时伤害或防御，价值全压在那两次额外结算上。
    ///         遵守食物的通用约定：战斗外只保留回复。
    ///     </para>
    /// </summary>
    [RegisterPotion(typeof(WineFoxAddonsDishPotionPool))]
    public sealed class HoneyGlazedHam : DishPotionModel
    {
        public override PotionRarity Rarity => PotionRarity.Rare;

        protected override int SellGold => 45;

        public override PotionAssetProfile AssetProfile =>
            Art("res://STS2_WineFox_Addons/potions/dishes/honey_glazed_ham.png");

        protected override IEnumerable<CardKeyword> DisplayKeywords => [WineFoxAddonsKeywords.Nourishment];

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);

            await CreatureCmd.Heal(targetCreature, 9);
            await PowerCmd.Apply<NourishmentPower>(choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<DuplicationPower>(choiceContext, targetCreature, 2, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) =>
            CreatureCmd.Heal(Owner.Creature, 9);
    }
}
