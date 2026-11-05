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
    /// 在拥有者回合结束时，每持有 2 份材料，
    /// 就对所有可被攻击的敌人造成 1 点伤害。
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
            var damage = Math.Floor(materialCount / 2m);
            if (damage <= 0m)
                return;

            Flash(enemies);
            await CreatureCmd.Damage(
                choiceContext,
                enemies,
                damage,
                ValueProp.Unpowered,
                Owner.Creature,
                null);
        }
    }
}
