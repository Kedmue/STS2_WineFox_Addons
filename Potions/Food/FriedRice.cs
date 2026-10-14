using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Character;
using STS2_WineFox.Potions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Potions.Food
{
    [RegisterPotion(typeof(WineFoxFoodPotionPool))]
    public sealed class FriedRice : SellableToMerchantPotionModel
    {
        protected override int SellGold => 14;
        public override PotionRarity Rarity => PotionRarity.Uncommon;
        public override bool CanBeGeneratedInCombat => false;

        public override PotionAssetProfile AssetProfile => Art("res://STS2_WineFox_Addons/potions/foods/fried_rice.png");

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);
            await CreatureCmd.GainBlock(targetCreature, 5, ValueProp.Unpowered, cardPlay: null);
            await PowerCmd.Apply<PlatingPower>(choiceContext, targetCreature, 4, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) => Task.CompletedTask;
    }
}
