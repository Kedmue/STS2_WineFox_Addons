using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using STS2_WineFox.Character;
using STS2_WineFox_Addons.Utils;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Events
{
    // 扩展事件：深夜食堂。提供回复生命与打包食物。
    [RegisterActEvent(typeof(Overgrowth))]
    [RegisterActEvent(typeof(Underdocks))]
    public sealed class MidnightDiner : ModEventTemplate
    {
        public override EventAssetProfile AssetProfile =>
            new(InitialPortraitPath: "res://STS2_WineFox_Addons/events/midnight_diner.png");

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DynamicVar("HealPercent", 15m),
        ];

        public override bool IsAllowed(IRunState runState)
        {
            return WineFoxSettingsCompat.FoodEnabled
                   && base.IsAllowed(runState);
        }

        protected override IReadOnlyList<EventOption> GenerateInitialOptions()
        {
            return
            [
                new(this, EatHeartily, InitialOptionKey("EAT_HEARTILY")),
                new(this, PackTakeaway, InitialOptionKey("PACK_TAKEAWAY")),
                new(this, Leave, InitialOptionKey("LEAVE")),
            ];
        }

        private async Task EatHeartily()
        {
            ArgumentNullException.ThrowIfNull(Owner);
            var owner = Owner;

            var heal = Math.Ceiling(owner.Creature.MaxHp * DynamicVars["HealPercent"].BaseValue / 100m);
            await CreatureCmd.Heal(owner.Creature, heal);

            SetEventFinished(PageDescription("EAT_HEARTILY"));
        }

        private async Task PackTakeaway()
        {
            ArgumentNullException.ThrowIfNull(Owner);
            var owner = Owner;

            var foods = ModelDb.PotionPool<WineFoxFoodPotionPool>()
                .GetUnlockedPotions(owner.UnlockState)
                .ToList();

            if (foods.Count > 0 && owner.HasOpenPotionSlots)
            {
                var potion = owner.PlayerRng.Rewards.NextItem(foods);
                if (potion != null)
                    await PotionCmd.TryToProcure(potion.ToMutable(), owner);
            }

            SetEventFinished(PageDescription("PACK_TAKEAWAY"));
        }

        private async Task Leave()
        {
            SetEventFinished(PageDescription("LEAVE"));
        }
    }
}
