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
    ///     烤鸡 —— 一整只下肚，接下来每一回合都还在出力。
    ///     <para>
    ///         设计思路是"长线滚雪球"：鸡是三道新菜里最扎实的肉，
    ///         所以它给的不是一次性爆发，而是随回合一路长起来的力量 ——
    ///         用原版「仪式」能力，在你的每个回合结束时获得 1 点力量（可叠加），
    ///         战斗拖得越久这顿饭越划算；配上[滋养]的持续回血与 15 点即时回复，
    ///         让它成为"越打越强"的长线选择。
    ///         现有食物里的力量全是即时数值（胡萝卜 1 层、烟熏胡萝卜 2 层、金胡萝卜 3 层），
    ///         没有任何一种提供过随回合成长的力量，这条轴是空着的。
    ///     </para>
    ///     <para>
    ///         战斗内：回复 15 点生命，获得 1 层[滋养]和 1 层[仪式]。
    ///         遵守食物的通用约定：战斗外只保留回复。
    ///     </para>
    /// </summary>
    [RegisterPotion(typeof(WineFoxAddonsDishPotionPool))]
    public sealed class RoastChicken : DishPotionModel
    {
        public override PotionRarity Rarity => PotionRarity.Rare;

        protected override int SellGold => 60;

        public override PotionAssetProfile AssetProfile =>
            Art("res://STS2_WineFox_Addons/potions/dishes/roast_chicken.png");

        protected override IEnumerable<CardKeyword> DisplayKeywords => [WineFoxAddonsKeywords.Nourishment];

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);

            await CreatureCmd.Heal(targetCreature, 15);
            await PowerCmd.Apply<NourishmentPower>(choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<RitualPower>(choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) =>
            CreatureCmd.Heal(Owner.Creature, 15);
    }
}
