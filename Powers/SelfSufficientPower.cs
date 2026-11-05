using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2_WineFox.Character;
using STS2_WineFox_Addons.Rewards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Powers
{
    /// <summary>
    /// 自给自足：战斗结束时将 1 个随机食物加入奖励列表。
    /// 用 Counter 堆叠：一局内打出几次就叠几层，每次战斗结束按层数添加对应数量的食物奖励。
    /// </summary>
    [RegisterPower]
    public sealed class SelfSufficientPower : ModPowerTemplate
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override PowerAssetProfile AssetProfile =>
            new("res://STS2_WineFox_Addons/powers/self_sufficient_power.png",
                "res://STS2_WineFox_Addons/powers/self_sufficient_power.png");

        public override Task AfterCombatEnd(CombatRoom room)
        {
            if (Amount <= 0)
                return Task.CompletedTask;

            var player = Owner.Player;
            if (player == null)
                return Task.CompletedTask;

            var foods = ModelDb.PotionPool<WineFoxFoodPotionPool>()
                .GetUnlockedPotions(player.UnlockState)
                .ToList();

            if (foods.Count == 0)
                return Task.CompletedTask;

            Flash();

            // 层数 = 战斗结束时额外加入奖励列表的食物个数。
            for (var i = 0; i < Amount; i++)
            {
                var picked = player.PlayerRng.Rewards.NextItem(foods);
                if (picked == null)
                    break;

                room.AddExtraReward(
                    player,
                    new SelfSufficientFoodReward(picked.ToMutable(), player));
            }

            return Task.CompletedTask;
        }
    }
}
