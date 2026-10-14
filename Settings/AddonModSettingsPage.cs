using STS2RitsuLib;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using STS2_WineFox_Addons.Scripts;

namespace STS2_WineFox_Addons.Settings
{
    /// <summary>
    /// 在 RitsuLib 的模组设置界面里注册扩展 mod 的设置页。
    /// </summary>
    public static class AddonModSettingsPage
    {
        private static bool _registered;

        public static void Register()
        {
            if (_registered)
                return;

            var foodForEveryoneBinding = new ModSettingsValueBinding<AddonModSettings, bool>(
                Entry.ModId,
                AddonModSettingsStore.DataKey,
                SaveScope.Global,
                settings => settings.FoodForEveryone,
                (settings, value) =>
                {
                    // 与酒狐一致：开服/联机中不允许改，改动由主机负责发布。
                    if (!CanEditSettings())
                        return;

                    settings.FoodForEveryone = value;
                    AddonRuntimeSettings.PublishHostSettings("settings_changed");
                });

            RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
                .WithTitle(ModSettingsText.Literal("酒狐扩展设置"))
                .WithModDisplayName(ModSettingsText.Literal("酒狐扩展"))
                .WithReadOnlyOnHostSurfaces(ModSettingsHostSurface.RunPause | ModSettingsHostSurface.CombatPause)
                .AddSection("food", section => section
                    .WithTitle(ModSettingsText.Literal("食物"))
                    .AddToggle(
                        "food_for_everyone",
                        ModSettingsText.Literal("启用全角色食物掉落"),
                        foodForEveryoneBinding,
                        ModSettingsText.Literal("允许食物在队伍中没有酒狐时也能作为战斗奖励掉落。"))));

            _registered = true;
        }

        private static bool CanEditSettings()
        {
            var runManager = RunManager.Instance;
            if (runManager?.IsInProgress == true)
                return false;

            return runManager?.NetService is not NetClientGameService;
        }
    }
}
