using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2_WineFox.Character;

namespace STS2_WineFox_Addons.Utils
{
    /// <summary>
    ///     判断当前队伍里是否有酒狐。
    ///     规则与酒狐本体的 <c>WineFoxCombatVisualEffects.TryIsWineFoxInParty</c> 保持一致：
    ///     只要任一玩家的角色是酒狐，或角色 ID 里含 "winefox"，就算有。
    ///     （那个辅助类是 internal，编译期引用不到，所以这里按同样的规则自行实现。）
    /// </summary>
    internal static class WineFoxPartyCompat
    {
        public static bool IsWineFoxInParty(Creature? creature)
        {
            var combatState = creature?.CombatState;
            if (combatState == null)
                return false;

            return combatState.Players.Any(player =>
                player.Character is WineFox ||
                player.Character.Id.Entry.Contains("winefox", StringComparison.OrdinalIgnoreCase));
        }
    }
}
