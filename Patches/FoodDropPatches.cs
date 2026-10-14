using System;
using STS2RitsuLib.Patching.Models;
using STS2_WineFox.Character;
using STS2_WineFox.Patches;
using STS2_WineFox_Addons.Settings;

namespace STS2_WineFox_Addons.Patches
{
    /// <summary>
    /// 线程内的"放行队伍判定"开关。只在酒狐食物掉落补丁执行的那一瞬间有效。
    /// 用 ThreadStatic 是为了不干扰其它线程，也避免影响同批次的其他结算。
    /// </summary>
    internal static class FoodForEveryoneGate
    {
        [ThreadStatic] private static bool _active;

        public static bool Active
        {
            get => _active;
            set => _active = value;
        }
    }

    /// <summary>
    /// 在酒狐的掉落补丁前后打标：补丁体执行期间若扩展设置开启，则视为"队伍里有酒狐"。
    /// Prefix / Postfix 挂在同一个方法上，执行顺序由 Harmony 保证，无需依赖跨补丁的顺序。
    /// </summary>
    public sealed class FoodRewardPartyGatePatch : IPatchMethod
    {
        public static string PatchId => "sts2_winefox_addons_food_reward_party_gate";
        public static bool IsCritical => false;

        public static string Description =>
            "Marks WineFox's food reward postfix so the party requirement can be relaxed.";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(WineFoxFoodPotionRewardPatch), "Postfix", ignoreIfMissing: true)];

        // ReSharper disable once InconsistentNaming
        public static void Prefix()
        {
            FoodForEveryoneGate.Active = AddonRuntimeSettings.FoodForEveryone;
        }

        // ReSharper disable once InconsistentNaming
        public static void Postfix()
        {
            FoodForEveryoneGate.Active = false;
        }
    }

    /// <summary>
    /// 放行酒狐的"队伍里必须有酒狐"判定。
    /// 只在标记生效时短路返回 true，所以不会影响特效补丁对同一个辅助方法的调用。
    /// </summary>
    public sealed class WineFoxPartyCheckBypassPatch : IPatchMethod
    {
        private const string VisualEffectsTypeName = "STS2_WineFox.Patches.WineFoxCombatVisualEffects";
        private const string PartyCheckMethodName = "TryIsWineFoxInParty";

        // 该辅助类是 internal，编译期无法直接引用，只能按名字反射取到。
        private static readonly Type? VisualEffectsType =
            typeof(WineFox).Assembly.GetType(VisualEffectsTypeName, throwOnError: false);

        public static string PatchId => "sts2_winefox_addons_party_check_bypass";
        public static bool IsCritical => false;

        public static string Description =>
            "Reports a WineFox in the party while a food reward roll is in progress.";

        public static ModPatchTarget[] GetTargets()
        {
            var type = VisualEffectsType;
            if (type == null)
                return [];

            return [new(type, PartyCheckMethodName, ignoreIfMissing: true)];
        }

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(ref bool __result)
        {
            if (!FoodForEveryoneGate.Active)
                return true;

            // 用掉即清，避免酒狐补丁体万一抛异常时把状态泄漏给特效补丁。
            FoodForEveryoneGate.Active = false;
            __result = true;
            return false;
        }
    }
}
