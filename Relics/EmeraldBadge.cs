using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2_WineFox.Character;
using STS2_WineFox.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Relics
{
    /// <summary>
    ///     绿宝石徽章 —— 事件「尖塔村落」里抗议成功换来的凭证。
    ///     <para>
    ///         效果是把商人的<b>删卡服务</b>价格砍半，用原版的 <c>ModifyMerchantPrice</c> 钩子实现
    ///         （原版「会员卡」用的同一个钩子，区别是它全场半价、这张只作用于删卡）。
    ///     </para>
    ///     <para>
    ///         稀有度取 <see cref="RelicRarity.Event" />：原版事件专属遗物都是这个稀有度，
    ///         而随机遗物袋只按 普通/罕见/稀有/商店 四种分桶，所以它不会出现在普通遗物掉落里。
    ///     </para>
    /// </summary>
    [RegisterRelic(typeof(WineFoxRelicPool))]
    public sealed class EmeraldBadge : WineFoxRelic
    {
        public override RelicRarity Rarity => RelicRarity.Event;

        // 占位图：借用原版的宝石图标，等专属立绘做好后替换成 res://STS2_WineFox_Addons/relics/ 下的资源。
        public override RelicAssetProfile AssetProfile =>
            Icons("res://images/relics/prismatic_gem.png");

        public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
        {
            if (player != Owner || entry is not MerchantCardRemovalEntry)
                return originalPrice;

            // 与原版「会员卡」一致：价格是在本地玩家这一侧计算并显示的，
            // 加上这个判断可以避免在联机里改动到别人的价格。
            if (!LocalContext.IsMe(Owner))
                return originalPrice;

            return originalPrice * 0.5m;
        }
    }
}
