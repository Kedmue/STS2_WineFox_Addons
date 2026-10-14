using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Patching.Models;
using STS2_WineFox_Addons.Potions;
using STS2_WineFox_Addons.Rewards;
using STS2_WineFox_Addons.Scripts;
using STS2_WineFox_Addons.Settings;
using STS2_WineFox_Addons.Utils;

namespace STS2_WineFox_Addons.Patches
{
    /// <summary>
    ///     击败 Boss 后必定掉落一个菜肴。
    ///     与酒狐的食物掉落补丁挂在同一个目标上，两者互不影响：
    ///     酒狐那条是概率掉落普通食物（用的是 WineFoxFoodPotionPool），
    ///     本补丁是必掉菜肴（用的是 WineFoxAddonsDishPotionPool）。
    ///     队伍限制沿用扩展的"所有角色都能掉落食物"设置：
    ///     设置开启时任何角色都能拿到菜肴，关闭时要求队伍里有酒狐（与酒狐本体规则一致）。
    /// </summary>
    public sealed class BossDishRewardPatch : IPatchMethod
    {
        public static string PatchId => "sts2_winefox_addons_boss_dish_reward";
        public static bool IsCritical => false;

        public static string Description =>
            "Grants a guaranteed dish potion as a reward after each boss fight.";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(RewardsSet), nameof(RewardsSet.WithRewardsFromRoom))];

        // ReSharper disable once InconsistentNaming
        public static void Postfix(RewardsSet __instance, AbstractRoom room)
        {
            // 这个 Postfix 跑在奖励生成的主流程里，一旦抛异常会导致房间结算直接中断、
            // 读档时再次触发并卡死。所以这里整体兜住，出问题只记录日志、不产出菜肴。
            try
            {
                TryAddDishReward(__instance, room);
            }
            catch (Exception ex)
            {
                Entry.Logger.Error($"[菜肴掉落] 生成奖励时发生异常，已跳过：{ex.Message}");
            }
        }

        private static void TryAddDishReward(RewardsSet __instance, AbstractRoom room)
        {
            if (room is not CombatRoom)
                return;

            if (room.RoomType != RoomType.Boss)
                return;

            var player = __instance.Player;
            if (player?.RunState == null)
                return;

            // 与酒狐/原版保持一致：末章 Boss 不产出结算奖励，这里也不额外塞东西。
            if (player.RunState.CurrentActIndex >= player.RunState.Acts.Count - 1)
                return;

            // 队伍限制：设置开启时任何角色都算，否则沿用酒狐的规则。
            // 放在取随机菜肴之前，判定不通过时不会消耗奖励 RNG。
            if (!AddonRuntimeSettings.FoodForEveryone && !WineFoxPartyCompat.IsWineFoxInParty(player.Creature))
                return;

            // 同一份奖励集只塞一个，避免重复结算导致叠加。
            if (__instance.Rewards.Any(reward => reward is DishPotionReward))
                return;

            var dish = DishLoot.PickRandom(player);
            if (dish == null)
                return;

            __instance.Rewards.Add(new DishPotionReward(dish, player));
        }
    }
}
