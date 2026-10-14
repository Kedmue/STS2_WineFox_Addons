using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Character;
using STS2_WineFox.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Relics
{
    // 酒狐专属遗物：美食家围裙。每回合开始时获得再生。
    [RegisterRelic(typeof(WineFoxRelicPool))]
    public sealed class GourmetApron : WineFoxRelic
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;

        public override RelicAssetProfile AssetProfile =>
            Icons("res://STS2_WineFox_Addons/potions/foods/tomato.png");

        public override async Task AfterPlayerTurnStart(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (player != Owner)
                return;

            await PowerCmd.Apply<RegenPower>(
                choiceContext,
                Owner.Creature,
                1,
                Owner.Creature,
                cardSource: null);
        }
    }
}
