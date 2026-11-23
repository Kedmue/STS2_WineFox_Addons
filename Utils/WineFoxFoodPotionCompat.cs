using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using STS2_WineFox.Character;

namespace STS2_WineFox_Addons.Utils
{
    /// <summary>
    /// 酒狐随机食物工厂是 internal 类型，这里通过最小反射封装复用其
    /// 食物开关、解锁池和稀有度权重规则；接口缺失时回退到食物池均匀抽取。
    /// </summary>
    internal static class WineFoxFoodPotionCompat
    {
        private static readonly MethodInfo? CreateRandomFoodMethod =
            typeof(WineFox).Assembly
                .GetType("STS2_WineFox.Potions.FoodPotionFactory", throwOnError: false)
                ?.GetMethod(
                    "CreateRandomFoodPotionForReward",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public static PotionModel? CreateRandomFoodPotionForReward(Player player, Rng rng)
        {
            if (CreateRandomFoodMethod != null)
            {
                try
                {
                    return CreateRandomFoodMethod.Invoke(
                        null,
                        [player, rng, null]) as PotionModel;
                }
                catch (Exception)
                {
                    // 酒狐接口发生变化时使用下面的公开池回退。
                }
            }

            var options = ModelDb
                .PotionPool<WineFoxFoodPotionPool>()
                .GetUnlockedPotions(player.UnlockState)
                .ToList();
            return options.Count == 0 ? null : rng.NextItem(options);
        }
    }
}
