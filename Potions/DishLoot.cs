using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox_Addons.Character;

namespace STS2_WineFox_Addons.Potions
{
    /// <summary>
    ///     菜肴的获取入口。目前只有"击败 Boss 必掉一个"这一条路径。
    /// </summary>
    internal static class DishLoot
    {
        /// <summary>
        ///     从菜肴池里随机取一个已解锁的菜肴，并返回它的<strong>可变副本</strong>。
        ///     奖励系统（<c>PotionReward</c> 构造函数）要求传入可变实例，传规范模型会直接抛
        ///     <c>CanonicalModelException</c>，所以这里统一做 <c>ToMutable()</c>。
        ///     池为空时返回 null —— 包括食物功能被关闭（池在那种情况下会返回空集合）以及还没有制作任何菜肴。
        /// </summary>
        public static PotionModel? PickRandom(Player player)
        {
            var dishes = ModelDb.PotionPool<WineFoxAddonsDishPotionPool>()
                .GetUnlockedPotions(player.UnlockState)
                .ToList();

            return dishes.Count == 0 ? null : player.PlayerRng.Rewards.NextItem(dishes)?.ToMutable();
        }
    }
}
