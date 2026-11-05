using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Saves;
using STS2_WineFox.Cards.Rare;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 在每个回合开始时，将 1 张新的“全物品仓库”加入手牌。
    /// </summary>
    public sealed class WineFoxMyriadDepotRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_pandoras_box_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_pandoras_box_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_pandoras_box_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            HoverTipFactory.FromCardWithCardHoverTips<AllItem>();

        public override async Task AfterPlayerTurnStart(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (player != Owner || Owner.Creature.IsDead)
                return;

            var combatState = Owner.Creature.CombatState;
            if (combatState == null ||
                !CombatManager.Instance.IsInProgress ||
                CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            var card = combatState.CreateCard<AllItem>(Owner);
            SaveManager.Instance.MarkCardAsSeen(card);

            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner,
                CardPilePosition.Top);
        }
    }
}
