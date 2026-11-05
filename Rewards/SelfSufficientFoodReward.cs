using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace STS2_WineFox_Addons.Rewards
{
    /// <summary>
    /// 自给自足生成的食物奖励。和酒狐的食物奖励一样保存具体药水 ID，
    /// 避免战斗结束后读档时把已经出现的食物重新随机成另一种食物。
    /// </summary>
    internal sealed class SelfSufficientFoodReward(PotionModel potion, Player player)
        : PotionReward(potion, player)
    {
        public override SerializableReward ToSerializable()
        {
            var save = base.ToSerializable();
            save.PredeterminedModelId = Potion?.Id ?? save.PredeterminedModelId;
            return save;
        }
    }
}
