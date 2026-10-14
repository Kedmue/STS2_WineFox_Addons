using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox.Character;
using STS2_WineFox.Potions;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Potions.Food
{
    /// <summary>
    ///     玉米（生食）—— 扩展 mod 的新增食物，注册进酒狐的食物池，
    ///     因此战斗掉落、女仆背包、橘子虎皮蛋糕卷、小料理卡都能产出它。
    ///     <para>
    ///         设计：定位与扩展已有的五个食物一致 —— 无回复、纯增益、可卖给商人。
    ///         本回合先垫 3 点格挡；下回合开始时再获得 1 点临时力量
    ///         （临时力量用的是原版 FlexPotionPower，即 TemporaryStrengthPower 那条线，
    ///         会在那个回合结束时自动失效）。
    ///     </para>
    ///     <para>
    ///         在篝火处变成熟食「烤玉米」，稀有度、售价与效果随之提升，
    ///         与「稻米 → 炒饭」是同一条链路。
    ///     </para>
    /// </summary>
    [RegisterPotion(typeof(WineFoxFoodPotionPool))]
    public sealed class Corn : SellableToMerchantPotionModel, ICampfireTransformPotion
    {
        protected override int SellGold => 6;
        public override PotionRarity Rarity => PotionRarity.Common;
        public override bool CanBeGeneratedInCombat => false;

        public override PotionAssetProfile AssetProfile =>
            Art("res://STS2_WineFox_Addons/potions/foods/corn.png");

        public PotionModel CampfireTransformResult => ModelDb.Potion<RoastedCorn>();

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);

            await CreatureCmd.GainBlock(targetCreature, 3, ValueProp.Unpowered, cardPlay: null);
            await PowerCmd.Apply<NextTurnTemporaryStrengthPower>(
                choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) => Task.CompletedTask;
    }
}
