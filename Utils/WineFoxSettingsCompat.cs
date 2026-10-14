using System.Reflection;
using STS2_WineFox.Character;

namespace STS2_WineFox_Addons.Utils
{
    // 酒狐的设置类 WineFoxRuntimeSettings 在其程序集中是 internal，
    // 扩展 mod 无法直接引用，这里通过反射读取运行时生效的开关值。
    internal static class WineFoxSettingsCompat
    {
        private static readonly Type? WineFoxRuntimeSettingsType =
            typeof(WineFox).Assembly.GetType(
                "STS2_WineFox.Settings.WineFoxRuntimeSettings",
                throwOnError: false);

        public static bool FoodEnabled => GetBool("FoodEnabled");
        public static bool EventsEnabled => GetBool("EventsEnabled");
        public static bool PotionsEnabled => GetBool("PotionsEnabled");
        public static bool PublicRelicsEnabled => GetBool("PublicRelicsEnabled");

        private static bool GetBool(string propertyName)
        {
            if (WineFoxRuntimeSettingsType == null)
                return true;

            var property = WineFoxRuntimeSettingsType.GetProperty(
                propertyName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null)
                return true;

            var value = property.GetValue(null);
            return value is bool enabled && enabled;
        }
    }
}
