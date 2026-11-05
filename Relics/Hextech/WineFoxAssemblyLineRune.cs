using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2_WineFox;
using STS2_WineFox.Cards;
using STS2_WineFox.Commands;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每回合第一次完成合成时，抽三张牌。
    /// </summary>
    public sealed class WineFoxAssemblyLineRune : ModRelicTemplate, ICraftListener
    {
        private bool _triggeredThisTurn;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_hundred_refinements_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_hundred_refinements_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_hundred_refinements_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(WineFoxKeywords.CraftKeyword),
        ];

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public bool TriggeredThisTurn
        {
            get => _triggeredThisTurn;
            set
            {
                AssertMutable();
                _triggeredThisTurn = value;
            }
        }

        public override Task BeforeCombatStart()
        {
            TriggeredThisTurn = false;
            return Task.CompletedTask;
        }

        public override Task AfterPlayerTurnStart(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (player == Owner)
                TriggeredThisTurn = false;

            return Task.CompletedTask;
        }

        public async Task AfterCraftProductDelivered(CraftExecutionContext context)
        {
            if (context.Crafter != Owner.Creature || TriggeredThisTurn)
                return;

            TriggeredThisTurn = true;
            Flash();
            await CardPileCmd.Draw(
                context.ChoiceContext,
                3m,
                Owner,
                fromHandDraw: false);
        }
    }
}
