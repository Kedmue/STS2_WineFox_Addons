using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using STS2_WineFox_Addons.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Events
{
    /// <summary>
    ///     尖塔村落 —— 一处村民的小村落。他们把货物标上天价，热情地请你掏钱。
    ///     <para>
    ///         三个选项：忍痛付款（花钱换一件遗物）、奋起抗议（拿到只对删卡打折的绿宝石徽章）、
    ///         大肆劫掠（拿到 150 金币，但会往牌组里塞一张原版的[red]贪婪[/red]诅咒）。
    ///         写法参照原版事件：付款用原版「兰伟德长老」的金币变量 + <c>LoseGold(Spent)</c>，
    ///         劫掠用原版「沉没的宝库」的 <c>GainGold</c> + <c>AddCurseToDeck&lt;Greed&gt;</c>。
    ///     </para>
    ///     <para>
    ///         出现条件：只在<b>第二幕</b>（章节索引 1，即「巢穴」Hive）出现，且队伍里<b>每个人</b>身上都有
    ///         <see cref="PayGold" /> 金币（沿用原版「兰伟德长老」的写法：事件生成时就要求全场付得起）。
    ///         因此拿到事件的人一定点得起"忍痛付款"，不需要"钱不够"的锁定选项。
    ///     </para>
    /// </summary>
    [RegisterActEvent(typeof(Hive))]
    public sealed class SpireVillage : ModEventTemplate
    {
        // 占位立绘：原版事件会按事件 ID 自动去找 images/events/{id}.png 并预加载，
        // 找不到就会抛 AssetLoadException 导致事件房间黑屏，所以这里必须给出一个存在的图。
        public override EventAssetProfile AssetProfile =>
            new(InitialPortraitPath: "res://STS2_WineFox_Addons/events/spire_village.png");

        /// <summary>付款价格。</summary>
        private const int PayGold = 75;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new GoldVar(PayGold),
            new DynamicVar("RaidGold", 150m),
        ];

        public override bool IsAllowed(IRunState runState)
        {
            // 只在第二幕出现（索引 1 是「巢穴」；第一幕的「密林 / 暗港」都是索引 0）。
            if (runState.CurrentActIndex != 1)
                return false;

            // 队伍里每个人都出得起 75 金币（与 NPC 议价场景一致：谁进来都买得起，所以不用做锁定选项）。
            return runState.Players.All(player => player.Gold >= PayGold);
        }

        protected override IReadOnlyList<EventOption> GenerateInitialOptions()
        {
            ArgumentNullException.ThrowIfNull(Owner);

            return
            [
                new EventOption(this, PayUp, InitialOptionKey("PAY_UP")),

                new EventOption(this, Protest, InitialOptionKey("PROTEST"),
                    HoverTipFactory.FromRelic<EmeraldBadge>()),

                new EventOption(this, Raid, InitialOptionKey("RAID"),
                    HoverTipFactory.FromCardWithCardHoverTips<Greed>()),
            ];
        }

        private async Task PayUp()
        {
            var owner = Owner ?? throw new InvalidOperationException("事件缺少所有者。");

            await PlayerCmd.LoseGold(DynamicVars.Gold.IntValue, owner, GoldLossType.Spent);
            await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(owner).ToMutable(), owner);

            SetEventFinished(PageDescription("PAY_UP"));
        }

        private async Task Protest()
        {
            var owner = Owner ?? throw new InvalidOperationException("事件缺少所有者。");

            await RelicCmd.Obtain(ModelDb.Relic<EmeraldBadge>().ToMutable(), owner);

            SetEventFinished(PageDescription("PROTEST"));
        }

        private async Task Raid()
        {
            var owner = Owner ?? throw new InvalidOperationException("事件缺少所有者。");

            await PlayerCmd.GainGold(DynamicVars["RaidGold"].BaseValue, owner);
            await CardPileCmd.AddCurseToDeck<Greed>(owner);

            SetEventFinished(PageDescription("RAID"));
        }
    }
}
