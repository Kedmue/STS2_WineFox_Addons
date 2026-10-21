using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox.Cards;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// At the end of the owner's turn, each held Material deals 1 damage to
    /// every hittable enemy.
    /// </summary>
    public sealed class WineFoxMaterialBlastRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_doomsday_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_doomsday_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_doomsday_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
        ];

        public override async Task BeforeSideTurnEnd(
            PlayerChoiceContext choiceContext,
            CombatSide side,
            IEnumerable<Creature> participants)
        {
            if (Owner.Creature.IsDead ||
                side != Owner.Creature.Side ||
                !participants.Contains(Owner.Creature))
            {
                return;
            }

            var combatState = Owner.Creature.CombatState;
            if (combatState == null)
                return;

            var enemies = combatState.HittableEnemies.ToList();
            if (enemies.Count == 0)
                return;

            var materialCount = Owner.Creature.Powers
                .OfType<MaterialPower>()
                .Sum(power => power.Amount);
            if (materialCount <= 0)
                return;

            Flash(enemies);
            await CreatureCmd.Damage(
                choiceContext,
                enemies,
                materialCount,
                ValueProp.Unpowered,
                Owner.Creature,
                null);
        }
    }
}
