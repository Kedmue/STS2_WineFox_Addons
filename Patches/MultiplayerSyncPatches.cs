using MegaCrit.Sts2.Core.Multiplayer;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;
using STS2_WineFox_Addons.Settings;

namespace STS2_WineFox_Addons.Patches
{
    /// <summary>
    /// 联机同步的植入点，与酒狐本体选择的生命周期完全一致：
    /// 主机开服 / 新节点就绪时发布，客户端连接前与断开后清理缓存。
    /// </summary>
    internal static class AddonMultiplayerSyncPatches
    {
        public static void AddTo(ModPatcher patcher)
        {
            patcher.RegisterPatch<StartENetHostSyncPatch>();
            patcher.RegisterPatch<StartSteamHostSyncPatch>();
            patcher.RegisterPatch<HostPeerReadySyncPatch>();
            patcher.RegisterPatch<ClientInitializeResetPatch>();
            patcher.RegisterPatch<ClientDisconnectedResetPatch>();
        }

        private sealed class StartENetHostSyncPatch : IPatchMethod
        {
            public static string PatchId => "winefox_addons_start_enet_host_sync";
            public static bool IsCritical => false;
            public static string Description => "Broadcast addon settings when an ENet host starts.";

            public static ModPatchTarget[] GetTargets() =>
                [new(typeof(NetHostGameService), nameof(NetHostGameService.StartENetHost))];

            // ReSharper disable once InconsistentNaming
            private static void Prefix(NetHostGameService __instance)
            {
                AddonRuntimeSettings.PublishHostSettings(__instance, "start_enet_host");
            }
        }

        private sealed class StartSteamHostSyncPatch : IPatchMethod
        {
            public static string PatchId => "winefox_addons_start_steam_host_sync";
            public static bool IsCritical => false;
            public static string Description => "Broadcast addon settings when a Steam host starts.";

            public static ModPatchTarget[] GetTargets() =>
                [new(typeof(NetHostGameService), nameof(NetHostGameService.StartSteamHost))];

            // ReSharper disable once InconsistentNaming
            private static void Prefix(NetHostGameService __instance)
            {
                AddonRuntimeSettings.PublishHostSettings(__instance, "start_steam_host");
            }
        }

        private sealed class HostPeerReadySyncPatch : IPatchMethod
        {
            public static string PatchId => "winefox_addons_host_peer_ready_sync";
            public static bool IsCritical => false;
            public static string Description => "Broadcast addon settings after a peer becomes broadcast-ready.";

            public static ModPatchTarget[] GetTargets() =>
                [new(typeof(NetHostGameService), nameof(NetHostGameService.SetPeerReadyForBroadcasting))];

            // ReSharper disable once InconsistentNaming
            private static void Postfix(NetHostGameService __instance)
            {
                AddonRuntimeSettings.PublishHostSettings(__instance, "peer_ready");
            }
        }

        private sealed class ClientInitializeResetPatch : IPatchMethod
        {
            public static string PatchId => "winefox_addons_client_initialize_reset";
            public static bool IsCritical => false;
            public static string Description => "Clear cached host settings before a client connection starts.";

            public static ModPatchTarget[] GetTargets() =>
                [new(typeof(NetClientGameService), nameof(NetClientGameService.Initialize))];

            private static void Prefix()
            {
                AddonRuntimeSettings.ClearRemoteHostSettings();
            }
        }

        private sealed class ClientDisconnectedResetPatch : IPatchMethod
        {
            public static string PatchId => "winefox_addons_client_disconnected_reset";
            public static bool IsCritical => false;
            public static string Description => "Clear cached host settings after a client disconnects.";

            public static ModPatchTarget[] GetTargets() =>
                [new(typeof(NetClientGameService), nameof(NetClientGameService.OnDisconnectedFromHost))];

            private static void Postfix()
            {
                AddonRuntimeSettings.ClearRemoteHostSettings();
            }
        }
    }
}
