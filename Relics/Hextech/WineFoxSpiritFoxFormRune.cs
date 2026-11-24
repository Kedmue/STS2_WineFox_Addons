using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2_WineFox.Cards.Rare;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每场战斗的第一次玩家回合开始时，自动打出牌组中的“灵狐形态”。
    /// </summary>
    public sealed class WineFoxSpiritFoxFormRune : ModRelicTemplate
    {
        private bool _autoPlayedThisCombat;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_spirit_fox_form_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_spirit_fox_form_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_spirit_fox_form_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<SpiritFoxForm>(false),
            HoverTipFactory.FromPower<SlowPower>(),
        ];

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public bool AutoPlayedThisCombat
        {
            get => _autoPlayedThisCombat;
            set
            {
                AssertMutable();
                _autoPlayedThisCombat = value;
            }
        }

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<SpiritFoxForm>(this);
        }

        public override Task BeforeCombatStart()
        {
            AutoPlayedThisCombat = false;
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom room)
        {
            AutoPlayedThisCombat = false;
            return Task.CompletedTask;
        }

        public override async Task AfterPlayerTurnStartLate(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (AutoPlayedThisCombat ||
                player != Owner ||
                Owner.Creature.IsDead ||
                Owner.PlayerCombatState == null ||
                !CombatManager.Instance.IsInProgress ||
                CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            AutoPlayedThisCombat = true;
            var cards = Owner.PlayerCombatState.AllCards
                .OfType<SpiritFoxForm>()
                .Where(card => card.Pile?.Type is PileType.Draw or PileType.Hand or PileType.Discard)
                .ToList();
            if (cards.Count == 0)
                return;

            Flash();
            foreach (var card in cards)
            {
                await CardCmd.AutoPlay(
                    choiceContext,
                    card,
                    null,
                    AutoPlayType.Default,
                    skipXCapture: false,
                    skipCardPileVisuals: false);

                if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
                    break;
            }
        }
    }
}
