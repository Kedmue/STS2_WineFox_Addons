using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox_Addons.Character;
using STS2_WineFox_Addons.Keywords;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Potions.Dishes
{
    /// <summary>
    ///     填馅南瓜 —— 扩展 mod 的第一道菜肴，只能通过击败 Boss 获得。
    ///     <para>
    ///         定位是"熟食之上的正餐"：南瓜（生食，回复 4）→ 南瓜派（熟食，回复 8 + 人工制品）
    ///         → 填馅南瓜（菜肴）。回复量更高，人工制品也从 1 层抬到 4 层。
    ///     </para>
    ///     <para>
    ///         战斗内：回复 12 点生命，获得 4 层人工制品 —— 接在南瓜派（1 层人工制品）之上，
    ///         把"抗性"这条线做厚，一顿正餐直接顶掉接下来的一串负面效果。
    ///         另外给 1 层[滋养]：这顿正餐吃完之后还能持续回血，并在挨到未格挡伤害时替你挡一下。
    ///     </para>
    ///     <para>
    ///         遵守食物的通用约定：战斗内是完整效果，战斗外只保留回复（不施加能力）。
    ///         售价定在 40 —— 高于熟食档（12~20），低于最贵的蛋糕（100），
    ///         让"吃掉还是卖给商人"成为一个真实的选择。
    ///     </para>
    /// </summary>
    [RegisterPotion(typeof(WineFoxAddonsDishPotionPool))]
    public sealed class StuffedPumpkin : DishPotionModel
    {
        public override PotionRarity Rarity => PotionRarity.Rare;

        protected override int SellGold => 40;

        public override PotionAssetProfile AssetProfile =>
            Art("res://STS2_WineFox_Addons/potions/dishes/stuffed_pumpkin.png");

        /// <summary>
        ///     描述里写到了[gold]滋养[/gold]，所以把「滋养」关键词挂进悬停提示，
        ///     玩家把鼠标放到这道菜上就能看到滋养的说明。
        /// </summary>
        protected override IEnumerable<CardKeyword> DisplayKeywords => [WineFoxAddonsKeywords.Nourishment];

        protected override async Task OnUseInCombat(PlayerChoiceContext choiceContext, Creature? target)
        {
            var targetCreature = GetCombatTarget(target);

            await CreatureCmd.Heal(targetCreature, 12);
            await PowerCmd.Apply<ArtifactPower>(choiceContext, targetCreature, 4, Owner.Creature, cardSource: null);
            await PowerCmd.Apply<NourishmentPower>(choiceContext, targetCreature, 1, Owner.Creature, cardSource: null);
        }

        protected override Task OnUseOutOfCombat(PlayerChoiceContext choiceContext) =>
            CreatureCmd.Heal(Owner.Creature, 12);
    }
}
