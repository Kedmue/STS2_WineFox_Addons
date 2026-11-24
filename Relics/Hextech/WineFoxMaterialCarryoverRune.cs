using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2_WineFox.Cards;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 战斗结束时记录四种剩余材料，并在下一场战斗开始时原样恢复。
    /// </summary>
    public sealed class WineFoxMaterialCarryoverRune : WineFoxMaterialRuneBase
    {
        private int _retainedWood;
        private int _retainedStone;
        private int _retainedIron;
        private int _retainedDiamond;
        private bool _restoredThisCombat;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_material_carryover_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_material_carryover_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_material_carryover_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips
        {
            get
            {
                var retained = new LocString(
                    "relics",
                    "WINE_FOX_MATERIAL_CARRYOVER_RUNE.retained");
                retained.Add("Wood", RetainedWood);
                retained.Add("Stone", RetainedStone);
                retained.Add("Iron", RetainedIron);
                retained.Add("Diamond", RetainedDiamond);

                return
                [
                    new HoverTip(
                        new LocString(
                            "relics",
                            "WINE_FOX_MATERIAL_CARRYOVER_RUNE.retainedTitle"),
                        retained),
                    HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
                ];
            }
        }

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public int RetainedWood
        {
            get => _retainedWood;
            set
            {
                AssertMutable();
                _retainedWood = Math.Max(0, value);
            }
        }

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public int RetainedStone
        {
            get => _retainedStone;
            set
            {
                AssertMutable();
                _retainedStone = Math.Max(0, value);
            }
        }

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public int RetainedIron
        {
            get => _retainedIron;
            set
            {
                AssertMutable();
                _retainedIron = Math.Max(0, value);
            }
        }

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public int RetainedDiamond
        {
            get => _retainedDiamond;
            set
            {
                AssertMutable();
                _retainedDiamond = Math.Max(0, value);
            }
        }

        [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
        public bool RestoredThisCombat
        {
            get => _restoredThisCombat;
            set
            {
                AssertMutable();
                _restoredThisCombat = value;
            }
        }

        public override async Task BeforeCombatStart()
        {
            await base.BeforeCombatStart();
            if (RestoredThisCombat)
                return;

            RestoredThisCombat = true;
            if (RetainedWood <= 0 &&
                RetainedStone <= 0 &&
                RetainedIron <= 0 &&
                RetainedDiamond <= 0)
            {
                return;
            }

            Flash();
            await Restore<WoodPower>(RetainedWood);
            await Restore<StonePower>(RetainedStone);
            await Restore<IronPower>(RetainedIron);
            await Restore<DiamondPower>(RetainedDiamond);
        }

        public override Task AfterCombatEnd(CombatRoom room)
        {
            RetainedWood = GetAmount<WoodPower>();
            RetainedStone = GetAmount<StonePower>();
            RetainedIron = GetAmount<IronPower>();
            RetainedDiamond = GetAmount<DiamondPower>();
            RestoredThisCombat = false;
            return Task.CompletedTask;
        }

        private async Task Restore<TPower>(int amount)
            where TPower : MaterialPower
        {
            if (amount <= 0)
                return;

            await PowerCmd.Apply<TPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                amount,
                Owner.Creature,
                null);
        }

        private int GetAmount<TPower>()
            where TPower : MaterialPower
        {
            return Owner.Creature.Powers
                .OfType<TPower>()
                .Select(power => Math.Max(0, (int)Math.Floor((decimal)power.Amount)))
                .FirstOrDefault();
        }
    }
}
