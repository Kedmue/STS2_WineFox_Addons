using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Character;
using STS2_WineFox.Potions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Potions.Food
{
    [RegisterPotion(typeof(WineFoxFoodPotionPool))]
    public sealed class Onion : SellableToMerchantPotionModel
    {
        protected override int SellGold => 5;
        public override PotionRarity Rarity => PotionRarity.Common;
        public override bool CanBeGeneratedInCombat => false;

        public override PotionAssetProfile AssetProfile => Art("res://STS2_WineFox_Addons/potions/foods/onion.png");

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);
            await PowerCmd.Apply<StrengthPower>(choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<ThornsPower>(choiceContext, targetCreature, 2, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) => Task.CompletedTask;
    }
}