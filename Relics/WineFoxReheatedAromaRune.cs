using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2_WineFox.Potions;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Reference Wine Fox rune for the optional ARAM: Mayhem integration.
    ///
    /// Food and dishes both derive from <see cref="SellableToMerchantPotionModel" />,
    /// which gives the rune one stable way to identify every Wine Fox food item.
    /// </summary>
    public sealed class WineFoxReheatedAromaRune : ModRelicTemplate
    {
        public const int MaxTriggersPerCombat = 2;

        private int _triggersThisCombat;

        // ARAM treats every external player rune as a starter-rarity relic internally;
        // registration metadata supplies the Silver/Gold/Prismatic presentation.
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool ShowCounter => true;

        public override int DisplayAmount => Math.Max(0, MaxTriggersPerCombat - TriggersThisCombat);

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_stoke_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_stoke_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_stoke_rune.png");

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public int TriggersThisCombat
        {
            get => _triggersThisCombat;
            set
            {
                AssertMutable();
                _triggersThisCombat = Math.Clamp(value, 0, MaxTriggersPerCombat);
                InvokeDisplayAmountChanged();
            }
        }

        public override Task BeforeCombatStart()
        {
            TriggersThisCombat = 0;
            return Task.CompletedTask;
        }

        public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
        {
            if (potion is not SellableToMerchantPotionModel || potion.Owner != Owner)
                return;

            var combatManager = CombatManager.Instance;
            if (combatManager == null || !combatManager.IsInProgress || combatManager.IsOverOrEnding)
                return;

            if (Owner.Creature.IsDead || TriggersThisCombat >= MaxTriggersPerCombat)
                return;

            TriggersThisCombat++;
            Flash();

            await PlayerCmd.GainEnergy(1m, Owner);
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 1m, Owner, fromHandDraw: false);
        }
    }
}
