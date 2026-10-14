using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Character;
using STS2_WineFox.Potions;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Potions.Food
{
    /// <summary>
    ///     烤玉米 —— 玉米在篝火处变成的熟食。
    ///     <para>
    ///         遵循酒狐食物链的既定规律：熟食比生食高一个稀有度、售价明显提升、效果翻倍并追加一项。
    ///         本回合给 2 点临时敏捷（原版 SpeedPotionPower，即 TemporaryDexterityPower 那条线，
    ///         回合结束自动收回）；下回合开始时再获得 1 点永久力量。
    ///         与生玉米形成"本回合 X / 下回合 Y"的对照。
    ///     </para>
    /// </summary>
    [RegisterPotion(typeof(WineFoxFoodPotionPool))]
    public sealed class RoastedCorn : SellableToMerchantPotionModel
    {
        protected override int SellGold => 14;
        public override PotionRarity Rarity => PotionRarity.Uncommon;
        public override bool CanBeGeneratedInCombat => false;

        public override PotionAssetProfile AssetProfile =>
            Art("res://STS2_WineFox_Addons/potions/foods/roasted_corn.png");

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);

            await PowerCmd.Apply<SpeedPotionPower>(
                choiceContext, targetCreature, 2, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<NextTurnStrengthPower>(
                choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) => Task.CompletedTask;
    }
}
