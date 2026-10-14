using System.Text.Json;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.Sidecar;

namespace STS2_WineFox_Addons.Settings
{
    /// <summary>
    /// 运行时读取并同步扩展 mod 的开关。
    /// 主机发布权威状态；客户端收到后以远端状态为准，没收到时回退到本地设置。
    /// 与酒狐本体的 WineFoxRuntimeSettings 是同一套做法。
    /// </summary>
    internal static class AddonRuntimeSettings
    {
        // 主题名带上 mod 标识，避免与其它模组的同步主题冲突。
        private const string SyncTopic = "winefox_addons_runtime_settings";

        private static readonly Lock Gate = new();
        private static bool _initialized;
        private static AddonSettingsSnapshot? _remoteHostSettings;

        public static bool FoodForEveryone => Current.FoodForEveryone;

        /// <summary>
        /// 当前生效的配置快照：客户端优先用主机同步过来的，否则用本地设置。
        /// </summary>
        private static AddonSettingsSnapshot Current
        {
            get
            {
                var netService = RunManager.Instance?.NetService;
                if (netService is NetClientGameService)
                    lock (Gate)
                    {
                        if (_remoteHostSettings is { } remote)
                            return remote;
                    }

                return BuildLocalSnapshot();
            }
        }

        public static void Initialize()
        {
            lock (Gate)
            {
                if (_initialized)
                    return;

                RegisterTopicFromLocalSettings();
                RitsuLibSidecarConfigSyncService.TopicChanged += OnTopicChanged;
                _initialized = true;
            }
        }

        public static void PublishHostSettings(string reason)
        {
            PublishHostSettings(RunManager.Instance?.NetService, reason);
        }

        /// <summary>
        /// 把当前主机配置发布出去，供客户端同步读取。非主机时直接返回。
        /// </summary>
        public static void PublishHostSettings(INetGameService? netService, string reason)
        {
            if (netService is not NetHostGameService)
                return;

            lock (Gate)
            {
                RegisterTopicFromLocalSettings();
                _remoteHostSettings = null;
            }

            RitsuLibSidecarConfigSyncService.PublishHostState(netService, SyncTopic, 0, reason);
        }

        /// <summary>
        /// 清掉缓存的主机状态，强制之后重新拉取。
        /// </summary>
        public static void ClearRemoteHostSettings()
        {
            lock (Gate)
            {
                _remoteHostSettings = null;
            }
        }

        private static void RegisterTopicFromLocalSettings()
        {
            RitsuLibSidecarConfigSyncService.RegisterTopic<AddonSettingsSnapshot, AddonSettingsSnapshot>(
                SyncTopic,
                BuildLocalSnapshot(),
                (_, _) => false,
                (state, _) => state);
        }

        private static AddonSettingsSnapshot BuildLocalSnapshot()
        {
            return new(AddonModSettingsStore.FoodForEveryone);
        }

        private static void OnTopicChanged(SidecarConfigTopicChangedEvent ev)
        {
            if (ev.Topic != SyncTopic)
                return;

            if (RunManager.Instance?.NetService is not NetClientGameService)
                return;

            var snapshot = JsonSerializer.Deserialize<AddonSettingsSnapshot>(ev.StateJson);
            if (snapshot is null)
                return;

            lock (Gate)
            {
                _remoteHostSettings = snapshot;
            }
        }

        private sealed record AddonSettingsSnapshot(bool FoodForEveryone);
    }
}
