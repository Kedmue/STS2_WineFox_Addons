using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Powers
{
    /// <summary>
    ///     下回合开始时给予"临时力量"。
    ///     临时力量用的是原版的 <see cref="FlexPotionPower" />（<c>TemporaryStrengthPower</c> 那条线）：
    ///     获得力量后会在该回合结束时自动扣回，正是原版"在本回合结束前获得力量"的机制。
    ///     这里只是把它的施加时机推迟到下一回合开始。
    /// </summary>
    [RegisterPower]
    public sealed class NextTurnTemporaryStrengthPower : ModPowerTemplate
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        // 占位图标：暂用「自给自足」的能力图标，等专属图标就位后替换本文件即可。
        public override PowerAssetProfile AssetProfile =>
            new("res://STS2_WineFox_Addons/powers/next_turn_temporary_strength_power.png",
                "res://STS2_WineFox_Addons/powers/next_turn_temporary_strength_power.png");

        public override async Task AfterSideTurnStart(
            CombatSide side,
            IReadOnlyList<Creature> participants,
            ICombatState combatState)
        {
            if (!participants.Contains(Owner))
                return;

            Flash();
            await PowerCmd.Apply<FlexPotionPower>(
                new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, cardSource: null);
            await PowerCmd.Remove(this);
        }
    }
}
