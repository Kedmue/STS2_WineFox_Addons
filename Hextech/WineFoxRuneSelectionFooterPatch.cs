using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.addons.mega_text;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Adds the built-in upgrade-rune pickup footer to Wine Fox runes without
    /// referencing the Hextech assembly. ARAM's footer interface is internal,
    /// so this optional Harmony patch mirrors its existing presentation.
    /// </summary>
    internal static class WineFoxRuneSelectionFooterPatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.RuneSelectionFooter";
        private const string FooterPrefix =
            "\n[font_size=13][color=#d8d4c090]";
        private const string FooterSuffix = "[/color][/font_size]";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var screenType = FindLoadedType("HextechRunes.HextechRuneSelectionScreen");
                if (screenType == null)
                {
                    logger.Warn("未找到海克斯符文选择界面，跳过升级符文页脚补丁。");
                    return;
                }

                var createCardButton = AccessTools.Method(
                    screenType,
                    "CreateCardButton",
                    [typeof(RelicModel), typeof(int)]);
                if (createCardButton == null)
                {
                    logger.Warn("未找到 HextechRuneSelectionScreen.CreateCardButton，跳过升级符文页脚补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    createCardButton,
                    postfix: new HarmonyMethod(
                        typeof(WineFoxRuneSelectionFooterPatch),
                        nameof(Postfix)));

                _installed = true;
                logger.Info("已安装海克斯升级符文选择页脚补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装海克斯升级符文页脚补丁失败，页脚将不会显示：{ex.Message}");
            }
        }

        private static void Postfix(RelicModel relic, Button __result)
        {
            if (__result == null ||
                !WineFoxRuneCardGrant.TryGetSelectionFooter(relic, out var footerText))
            {
                return;
            }

            var description = FindFirst<MegaRichTextLabel>(__result);
            if (description == null)
                return;

            var currentText = description.Text;
            if (currentText.Contains(footerText, StringComparison.Ordinal))
                return;

            description.SetTextAutoSize(
                currentText + FooterPrefix + footerText + FooterSuffix);
        }

        private static T? FindFirst<T>(Node root) where T : Node
        {
            foreach (var child in root.GetChildren(false))
            {
                if (child is T match)
                    return match;
                var nested = FindFirst<T>(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static Type? FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    var type = assembly.GetType(fullName, throwOnError: false);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                    // Ignore partially loaded assemblies and keep looking.
                }
            }

            return null;
        }
    }
}
