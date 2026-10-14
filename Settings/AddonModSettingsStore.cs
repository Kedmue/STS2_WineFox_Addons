using STS2RitsuLib;
using STS2RitsuLib.Utils.Persistence;
using STS2_WineFox_Addons.Scripts;

namespace STS2_WineFox_Addons.Settings
{
    /// <summary>
    /// 扩展 mod 设置的数据存储。沿用酒狐本体的做法：
    /// 先在全局作用域注册一个 JSON 存档，再从中读取当前值。
    /// </summary>
    public static class AddonModSettingsStore
    {
        public const string DataKey = "settings";
        private const string FileName = "winefox_addons_settings.json";

        private static bool _initialized;

        public static bool FoodForEveryone => Current.FoodForEveryone;

        public static void Initialize()
        {
            if (_initialized)
                return;

            using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId, false))
            {
                RitsuLibFramework.GetDataStore(Entry.ModId).Register(
                    DataKey,
                    FileName,
                    SaveScope.Global,
                    defaultFactory: () => new AddonModSettings(),
                    autoCreateIfMissing: true);
            }

            RitsuLibFramework.GetDataStore(Entry.ModId).InitializeGlobal();
            _initialized = true;
        }

        private static AddonModSettings Current
        {
            get
            {
                Initialize();
                return RitsuLibFramework.GetDataStore(Entry.ModId).Get<AddonModSettings>(DataKey);
            }
        }
    }
}
