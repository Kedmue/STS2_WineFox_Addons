using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每场战斗开始时获得 1 层蒸汽。
    /// </summary>
    public sealed class WineFoxSteamPreludeRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_tank_engine_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_tank_engine_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_tank_engine_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromPower<SteamPower>(),
        ];

        public override async Task BeforeCombatStart()
        {
            if (Owner.Creature.CombatState == null)
                return;

            Flash();
            await PowerCmd.Apply<SteamPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                1m,
                Owner.Creature,
                null);
        }
    }
}
