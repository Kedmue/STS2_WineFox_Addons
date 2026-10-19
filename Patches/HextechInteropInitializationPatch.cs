using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Patching.Models;
using STS2_WineFox_Addons.Hextech;
using STS2_WineFox_Addons.Scripts;

namespace STS2_WineFox_Addons.Patches
{
    /// <summary>
    /// RitsuLib binds [ModInterop] stubs in a prefix of LocManager.Initialize.
    /// The postfix is therefore the first safe point to call optional integrations
    /// by their strong proxy, before model/content registration is frozen.
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
