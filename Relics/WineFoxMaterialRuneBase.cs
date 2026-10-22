using MegaCrit.Sts2.Core.Entities.Relics;
using STS2_WineFox_Addons.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 确保材料符文效果所需的隐藏材料事件监听能力已经存在，
    /// 同时不会在界面上显示能力图标。
    /// </summary>
    public abstract class WineFoxMaterialRuneBase : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override async Task AfterObtained()
        {
            await WineFoxMaterialRuneListenerPower.Ensure(Owner);
        }

        public override async Task BeforeCombatStart()
        {
            await WineFoxMaterialRuneListenerPower.Ensure(Owner);
        }
    }
}
