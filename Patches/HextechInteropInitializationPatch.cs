using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Patching.Models;
using STS2_WineFox_Addons.Hextech;
using STS2_WineFox_Addons.Scripts;

namespace STS2_WineFox_Addons.Patches
{
    /// <summary>
    /// RitsuLib 会在 <c>LocManager.Initialize</c> 的前置补丁中绑定
    /// <c>[ModInterop]</c> 代理桩。因此这里使用后置补丁，在模型与内容注册
    /// 冻结之前，通过强类型代理调用可选扩展集成。
    /// </summary>
    public sealed class HextechInteropInitializationPatch : IPatchMethod
    {
        public static string PatchId => "sts2_winefox_addons_hextech_interop";

        public static bool IsCritical => false;

        public static string Description =>
            "Registers optional ARAM: Mayhem rune content after RitsuLib interop binding.";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(LocManager), nameof(LocManager.Initialize))];

        public static void Postfix()
        {
            WineFoxHextechIntegration.TryRegister(Entry.Logger);
        }
    }
}
