using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves;
using STS2_WineFox.Cards.Basic;
using STS2_WineFox.Cards.Rare;
using STS2_WineFox.Cards.Uncommon;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Mirrors the built-in card-upgrade rune pickup: grant one matching card
    /// to hand during combat, otherwise add it to the deck with a preview.
    /// </summary>
    internal static class WineFoxRuneCardGrant
    {
        private const string SelectionFooterLocKey =
            "WINE_FOX_UPGRADE_RUNE.selectionFooter";

        private static readonly IReadOnlyDictionary<Type, Type> UpgradeCardTypes =
            new Dictionary<Type, Type>
            {
                [typeof(WineFoxBasicCraftInnateRune)] = typeof(BasicCraft),
                [typeof(WineFoxIntermittentChantingRune)] = typeof(IntermittentChanting),
                [typeof(WineFoxMechanicalSawRune)] = typeof(MechanicalSaw),
                [typeof(WineFoxNetheriteChestplateRune)] = typeof(NetheriteChestPlate),
                [typeof(WineFoxWirelessTerminalRune)] = typeof(WirelessTerminal),
            };

        public static bool TryGetSelectionFooter(
            RelicModel? rune,
            out string footerText)
        {
            footerText = string.Empty;
            if (rune == null ||
                !UpgradeCardTypes.TryGetValue(rune.GetType(), out var cardType))
            {
                return false;
            }

            try
            {
                var card = ModelDb.GetByIdOrNull<CardModel>(ModelDb.GetId(cardType));
                if (card == null)
                    return false;

                var footer = new LocString("relics", SelectionFooterLocKey);
                footer.Add("CardName", card.Title);
                footerText = footer.GetFormattedText();
                return !string.IsNullOrWhiteSpace(footerText);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task Grant<TCard>(RelicModel rune) where TCard : CardModel
        {
            var owner = rune.Owner;
            var combatState = owner.Creature.CombatState;

            if (owner.PlayerCombatState != null &&
                combatState != null &&
                CombatManager.Instance.IsInProgress &&
                !CombatManager.Instance.IsOverOrEnding)
            {
                var combatCard = combatState.CreateCard<TCard>(owner);
                await CardPileCmd.AddGeneratedCardToCombat(
                    combatCard,
                    PileType.Hand,
                    owner,
                    CardPilePosition.Bottom);
                SaveManager.Instance.MarkCardAsSeen(combatCard);
                return;
            }

            var card = owner.RunState.CreateCard<TCard>(owner);
            var result = await CardPileCmd.Add(
                card,
                PileType.Deck,
                CardPilePosition.Bottom,
                null,
                false);
            SaveManager.Instance.MarkCardAsSeen(card);
            CardCmd.PreviewCardPileAdd(
                [result],
                2f,
                CardPreviewStyle.HorizontalLayout);
        }
    }
}
