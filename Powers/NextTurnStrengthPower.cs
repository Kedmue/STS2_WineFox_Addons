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
    ///     下回合开始时给予力量（永久）。
    ///     <para>
    ///         结构照抄原版的 <c>EnergyNextTurnPower</c> / <c>BlockNextTurnPower</c>：
    ///         挂在自己这一侧的"回合开始"钩子上，结算一次后自我移除。
    ///         原版没有现成的"下回合给力量"能力（只有 DemonForm 那种每回合循环的），所以按同一套模式补一个。
    ///     </para>
    /// </summary>
    [RegisterPower]
    public sealed class NextTurnStrengthPower : ModPowerTemplate
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        // 占位图标：暂用「自给自足」的能力图标，等专属图标就位后替换本文件即可。
        public override PowerAssetProfile AssetProfile =>
            new("res://STS2_WineFox_Addons/powers/next_turn_strength_power.png",
                "res://STS2_WineFox_Addons/powers/next_turn_strength_power.png");

        public override async Task AfterSideTurnStart(
            CombatSide side,
            IReadOnlyList<Creature> participants,
            ICombatState combatState)
        {
            if (!participants.Contains(Owner))
                return;

            Flash();
            await PowerCmd.Apply<StrengthPower>(
                new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, cardSource: null);
            await PowerCmd.Remove(this);
        }
    }
}
