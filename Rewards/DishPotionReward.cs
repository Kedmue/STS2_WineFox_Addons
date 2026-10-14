using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace STS2_WineFox_Addons.Rewards
{
    /// <summary>
    ///     菜肴奖励。和酒狐的 <c>FoodPotionReward</c> 一样是 <see cref="PotionReward" /> 的子类，
    ///     唯一目的是把具体菜肴的 ID 写进存档，避免读档后奖励被重新随机成别的药水。
    ///     （读档还原由酒狐的 <c>WineFoxPredeterminedPotionRewardSavePatch</c> 统一处理。）
    /// </summary>
    internal sealed class DishPotionReward(PotionModel potion, Player player) : PotionReward(potion, player)
    {
        public override SerializableReward ToSerializable()
        {
            var save = base.ToSerializable();
            save.PredeterminedModelId = Potion?.Id ?? save.PredeterminedModelId;
            return save;
        }
    }
}
