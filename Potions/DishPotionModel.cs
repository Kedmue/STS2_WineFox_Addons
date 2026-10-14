using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Potions;

namespace STS2_WineFox_Addons.Potions
{
    /// <summary>
    ///     菜肴基类。继承酒狐的 <see cref="SellableToMerchantPotionModel" />，因此天然具备食物的全部特性：
    ///     <list type="bullet">
    ///         <item>使用时机为 AnyTime，战斗内外效果分开实现（<c>OnUseInCombat</c> / <c>OnUseOutOfCombat</c>）；</item>
    ///         <item>可以投掷给商人换取金币（售价由 <see cref="SellGold" /> 决定）。</item>
    ///     </list>
    ///     具体菜肴请继承本类，并标注 <c>[RegisterPotion(typeof(WineFoxAddonsDishPotionPool))]</c> 注册进菜肴池。
    /// </summary>
    public abstract class DishPotionModel : SellableToMerchantPotionModel, IDishPotion
    {
        /// <summary>
        ///     菜肴不会出现在普通药水的随机生成里，只能通过专属途径（击败 Boss）获得。
        /// </summary>
        public override bool CanBeGeneratedInCombat => false;

        /// <summary>
        ///     菜肴的统一默认售价，具体菜肴可以覆盖。
        /// </summary>
        protected override int SellGold => 25;

        /// <summary>
        ///     描述里提到、需要在悬停提示中额外展示的关键词。默认没有。
        ///     <para>
        ///         和原版关键词提示的做法一致：描述文本里把该词写成 <c>[gold]关键词[/gold]</c> 染色，
        ///         提示框本身则由这里挂上去。子类照下面这样声明即可：
        ///         <c>protected override IEnumerable&lt;CardKeyword&gt; DisplayKeywords =&gt; [WineFoxAddonsKeywords.Nourishment];</c>
        ///     </para>
        /// </summary>
        protected virtual IEnumerable<CardKeyword> DisplayKeywords => Array.Empty<CardKeyword>();

        /// <summary>
        ///     把 <see cref="DisplayKeywords" /> 声明的关键词转成悬停提示，接在药水自身的提示之后。
        /// </summary>
        public override IEnumerable<IHoverTip> ExtraHoverTips =>
            DisplayKeywords.Select(HoverTipFactory.FromKeyword);
    }
}
