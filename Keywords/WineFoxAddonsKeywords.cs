using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_WineFox_Addons.Scripts;

namespace STS2_WineFox_Addons.Keywords
{
    /// <summary>
    ///     扩展 mod 自己的卡牌关键词注册声明。
    ///     <para>
    ///         关键词的 ID 和本地化键都由 ritsulib 按 <c>{模组ID}_KEYWORD_{词干}</c> 约定生成，
    ///         所以这里只需要给出词干，本地化写在
    ///         <c>res://STS2_WineFox_Addons/localization/zhs/card_keywords.json</c>。
    ///     </para>
    ///     <para>
    ///         承载 <see cref="RegisterOwnedCardKeywordAttribute" /> 的类型<b>不能是 static</b>：
    ///         ritsulib 扫描时会跳过 abstract（static 类在 IL 里就是 abstract+sealed）和 interface。
    ///         因此沿用酒狐 <c>WineFoxAutoRegistrationKeywords</c> 的写法 —— 静态容器 + 嵌套私有类。
    ///     </para>
    /// </summary>
    internal static class WineFoxAddonsAutoRegistrationKeywords
    {
        // 滋养。悬停提示用能力本身的图标，和游戏里其它能力型关键词保持一致。
        [RegisterOwnedCardKeyword(WineFoxAddonsKeywords.NourishmentStem,
            IconPath = WineFoxAddonsKeywords.NourishmentIconPath)]
        private sealed class Nourishment;
    }

    /// <summary>
    ///     供代码引用的关键词句柄。
    /// </summary>
    public static class WineFoxAddonsKeywords
    {
        /// <summary>「滋养」的本地关键词词干。</summary>
        public const string NourishmentStem = "nourishment";

        /// <summary>「滋养」悬停提示使用的图标。</summary>
        public const string NourishmentIconPath = "res://STS2_WineFox_Addons/powers/nourishment_power.png";

        /// <summary>
        ///     「滋养」关键词的确定性 <see cref="CardKeyword" /> 值。
        ///     <para>
        ///         完整 ID 为 <c>STS2_WINE_FOX_ADDONS_KEYWORD_NOURISHMENT</c>，
        ///         本地化键是 <c>card_keywords</c> 表里的同名键 + <c>.title</c> / <c>.description</c>。
        ///         取值只依赖字符串，不需要注册先行，因此可以放在静态字段里。
        ///     </para>
        /// </summary>
        public static readonly CardKeyword Nourishment =
            ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, NourishmentStem).GetModCardKeyword();
    }
}
