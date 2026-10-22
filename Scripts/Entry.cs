using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2_WineFox_Addons.Hextech;
using STS2_WineFox_Addons.Patches;
using STS2_WineFox_Addons.Settings;

namespace STS2_WineFox_Addons.Scripts
{

// 必须要加的属性，用于注册Mod。字符串和初始化函数命名一致。
    [ModInitializer(nameof(Init))]
    public class Entry
    {
        // 你的modid
        public const string ModId = "STS2_WineFox_Addons";

        // 酒狐前置的最低版本要求。
        //
        // 注意：不要在 mod_manifest.json 里给酒狐写 min_version。
        // 酒狐使用 1.2.15.1 这种四段版本号，而游戏内置的 SemanticVersion 只认
        // 主.次.修订 三段，解析失败后 Mod.version 会是 null，游戏就会报
        // DEPENDENCY_VERSION_INVALID，直接拒绝加载本扩展。
        // 因此这里改为在运行时自行解析并提示（见 WarnIfWineFoxOutdated）。
        private const string WineFoxModId = "STS2_WineFox";
        private static readonly Version MinimumWineFoxVersion = new(1, 2, 15);

        public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

        public static void Init()
        {
            // harmony可用，但是最好用ritsu的封装patch，见补丁系统一章
            // var harmony = new Harmony("com.example.testmod");
            // harmony.PatchAll();
            // 必须在模组类型发现最终确定存档属性名称表之前，
            // 注册卡牌持久化状态。
            WineFoxPersistentCardState.Initialize();

            var assembly = Assembly.GetExecutingAssembly();
            
            ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
            RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

            AddonModSettingsStore.Initialize();
            AddonRuntimeSettings.Initialize();
            AddonModSettingsPage.Register();
            ApplyPatches();
            WarnIfWineFoxOutdated();
        }

        /// <summary>
        /// 应用扩展自己的补丁。这些补丁都不是关键补丁，
        /// 失败时只记录日志，不影响 mod 本体加载。
        /// </summary>
        private static void ApplyPatches()
        {
            try
            {
                var patcher = RitsuLibFramework.CreatePatcher(ModId, "addon-food-drops", "食物掉落扩展");
                patcher.RegisterPatch<FoodRewardPartyGatePatch>();
                patcher.RegisterPatch<WineFoxPartyCheckBypassPatch>();
                patcher.RegisterPatch<BossDishRewardPatch>();
                patcher.RegisterPatch<FoodEatLabelPatch>();
                patcher.RegisterPatch<HextechInteropInitializationPatch>();
                AddonMultiplayerSyncPatches.AddTo(patcher);
                patcher.PatchAll();
            }
            catch (Exception ex)
            {
                Logger.Error($"应用食物掉落补丁失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 运行时检查酒狐版本。清单里不能声明 min_version（见上），
        /// 所以这里补一个宽松的检查：只做提示，不影响加载。
        /// </summary>
        private static void WarnIfWineFoxOutdated()
        {
            try
            {
                var rawVersion = ModManager.GetLoadedMods()
                    .FirstOrDefault(m => string.Equals(m.manifest?.id, WineFoxModId, StringComparison.Ordinal))
                    ?.manifest?.version;

                if (string.IsNullOrWhiteSpace(rawVersion))
                {
                    Logger.Warn($"未读取到酒狐 mod（{WineFoxModId}）的版本号，跳过版本检查。");
                    return;
                }

                if (!TryParseVersion(rawVersion, out var version))
                {
                    Logger.Info($"酒狐 mod 版本为 {rawVersion}，无法解析为数字版本，跳过比较。");
                    return;
                }

                if (version.CompareTo(MinimumWineFoxVersion) < 0)
                    Logger.Error($"酒狐 mod 版本过低：当前 {rawVersion}，本扩展至少需要 {MinimumWineFoxVersion}，部分内容可能无法正常加载。");
                else
                    Logger.Info($"检测到酒狐 mod 版本 {rawVersion}。");
            }
            catch (Exception ex)
            {
                Logger.Warn($"检查酒狐 mod 版本时发生异常：{ex.Message}");
            }
        }

        // 兼容 1.2.15、1.2.15.1、v1.2.15、1.2.15-beta 等写法。
        private static bool TryParseVersion(string raw, out Version version)
        {
            version = new Version(0, 0);

            var text = raw.Trim();
            if (text.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                text = text[1..];

            var cut = text.IndexOfAny(['-', '+']);
            if (cut >= 0)
                text = text[..cut];

            if (!Version.TryParse(text, out var parsed))
                return false;

            version = parsed;
            return true;
        }
    }
}
