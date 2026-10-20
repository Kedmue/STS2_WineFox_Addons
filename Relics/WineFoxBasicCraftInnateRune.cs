using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Cards.Basic;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Gives every Basic Craft owned by the player the Innate keyword and keeps
    /// that keyword through deck copies, combat clones, saves, and upgrades.
    /// </summary>
    public sealed class WineFoxBasicCraftInnateRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_startup_routine_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_startup_routine_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_startup_routine_rune.png");

        public override Task AfterObtained()
        {
            var applied = false;
            foreach (var card in Owner.Deck.Cards)
            {
                if (TryApplyInnate(card))
                    applied = true;
            }

            if (applied)
                Flash();

            return Task.CompletedTask;
        }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card.Owner == Owner && card is BasicCraft)
                HextechRunesApiBridge.RestorePersistentInnate(card);

            return Task.CompletedTask;
        }

        public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
        {
            newCard = null;
            if (!TryApplyInnate(card))
                return false;

            newCard = card;
            Flash();
            return true;
        }

        private bool TryApplyInnate(CardModel? card)
        {
            if (card == null || card.Owner != Owner || card is not BasicCraft)
                return false;

            HextechRunesApiBridge.TrackPersistentInnate(card);
            if (!card.Keywords.Contains(CardKeyword.Innate))
                CardCmd.ApplyKeyword(card, [CardKeyword.Innate]);

            return true;
        }
    }
}
