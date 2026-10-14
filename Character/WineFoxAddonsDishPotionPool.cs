using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using STS2_WineFox.Character;
using STS2_WineFox_Addons.Utils;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Character
{
    /// <summary>
    ///     扩展 mod 的"菜肴"药水池。
    ///     菜肴属于食物（可卖给商人），但和普通食物分开存放，
    ///     所以不会进入战斗掉落、女仆背包、小料理这些普通食物的获取途径。
    ///     注册为共享池只是为了让它能被 ModelDb 解析到，
    ///     原版的随机药水生成只取角色池和原版 SharedPotionPool，不会碰到这里。
    /// </summary>
    [RegisterSharedPotionPool]
    public sealed class WineFoxAddonsDishPotionPool : TypeListPotionPoolModel
    {
        public override string EnergyColorName => "winefox";
        public override string? BigEnergyIconPath => "res://STS2_WineFox/winefox/winefox_energy_icon.png";
        public override string? TextEnergyIconPath
        {
            get => "res://STS2_WineFox/winefox/winefox_energy_icon.png";
        }

        public override Color LabOutlineColor => WineFox.Color;

        public override IEnumerable<PotionModel> GetUnlockedPotions(UnlockState unlockState)
        {
            return WineFoxSettingsCompat.FoodEnabled
                ? base.GetUnlockedPotions(unlockState)
                : Array.Empty<PotionModel>();
        }
    }
}
