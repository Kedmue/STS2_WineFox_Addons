using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Powers
{
    /// <summary>
    ///     滋养：每层在玩家回合结束时回复 1 点生命；受到未被格挡的伤害时免疫该伤害并移除一层。
    ///     <para>
    ///         两部分机制分别照搬原版里最接近的能力：
    ///         <list type="bullet">
    ///             <item>
    ///                 回合结束回血 —— 用 <c>RegenPower</c> 的 <c>AfterSideTurnEnd</c> 钩子。
    ///                 区别是再生每次结算都会掉一层，而滋养<b>不随时间递减</b>，只有挡住伤害才会掉层。
    ///             </item>
    ///             <item>
    ///                 免疫伤害 —— 用 <c>BufferPower</c> 的 <c>ModifyHpLostAfterOstyLate</c> 与
    ///                 <c>AfterModifyingHpLostAfterOsty</c> 组合。用 "Late" 是为了排在其它减伤之后结算，
    ///                 避免和其它把伤害归零的效果抢触发。
    ///             </item>
    ///         </list>
    ///     </para>
    /// </summary>
    [RegisterPower]
    public sealed class NourishmentPower : ModPowerTemplate
    {
        // 记录这一次结算里本能力是否真的挡下了伤害，避免在伤害本来就是 0 的情况下白白掉层。
        private bool _preventedDamage;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override PowerAssetProfile AssetProfile =>
            new("res://STS2_WineFox_Addons/powers/nourishment_power.png",
                "res://STS2_WineFox_Addons/powers/nourishment_power.png");

        /// <summary>
        ///     回合结束时按层数回复生命。层数不在这里减少。
        /// </summary>
        public override async Task AfterSideTurnEnd(
            PlayerChoiceContext choiceContext,
            CombatSide side,
            IEnumerable<Creature> participants)
        {
            if (!participants.Contains(Owner) || Owner.IsDead)
                return;

            Flash();
            await CreatureCmd.Heal(Owner, Amount);
        }

        /// <summary>
        ///     把"未被格挡的伤害"归零，实现免疫。
        /// </summary>
        public override decimal ModifyHpLostAfterOstyLate(
            Creature target,
            decimal amount,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource)
        {
            // 每次结算先复位，避免上一次的标记残留。
            _preventedDamage = false;

            if (target != Owner || amount <= 0m)
                return amount;

            _preventedDamage = true;
            return 0m;
        }

        public override async Task AfterModifyingHpLostAfterOsty()
        {
            if (!_preventedDamage)
                return;

            _preventedDamage = false;
            Flash();
            await PowerCmd.Decrement(this);
        }
    }
}
