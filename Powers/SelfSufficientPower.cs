using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Character;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Powers
{
    /// <summary>
    /// 自给自足：每回合开始时获得 1 个随机普通食物。
    /// 用 Counter 堆叠：一局内打出几次就叠几层，每回合按层数发放对应数量的食物。
    /// </summary>
    [RegisterPower]
    public sealed class SelfSufficientPower : ModPowerTemplate
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override PowerAssetProfile AssetProfile =>
            new("res://STS2_WineFox_Addons/powers/self_sufficient_power.png",
                "res://STS2_WineFox_Addons/powers/self_sufficient_power.png");

        public override async Task AfterPlayerTurnStart(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (player.Creature != Owner)
                return;

            if (Amount <= 0)
                return;

            // 药水槽满了就跳过，避免食物凭空消失。
            if (!player.HasOpenPotionSlots)
                return;

            var foods = ModelDb.PotionPool<WineFoxFoodPotionPool>()
                .GetUnlockedPotions(player.UnlockState)
                .Where(p => p.Rarity == PotionRarity.Common)
                .ToList();

            if (foods.Count == 0)
                return;

            Flash();

            // 层数 = 每回合发放的食物个数，逐次发放并在槽位满时停下。
            for (var i = 0; i < Amount; i++)
            {
                if (!player.HasOpenPotionSlots)
                    break;

                var picked = player.PlayerRng.Rewards.NextItem(foods);
                if (picked == null)
                    break;

                await PotionCmd.TryToProcure(picked.ToMutable(), player);
            }
        }
    }
}
