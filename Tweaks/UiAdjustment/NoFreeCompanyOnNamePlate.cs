using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Interface;
using ImGuiNET;
using SimpleTweaksPlugin.Tweaks.UiAdjustment;

namespace SimpleTweaksPlugin
{
    public partial class UiAdjustmentsConfig {
        public NoFreeCompanyOnNamePlate.Configs NoFreeCompanyOnNamePlate = new();
    }
}

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment
{
    public unsafe class NoFreeCompanyOnNamePlate : UiAdjustments.SubTweak {

        public class Configs {
            public bool KeepWandererTag;
            public bool ShortenedWandererTag;
            public List<string> KeepNameVisible = new();
        }

        public Configs Config => PluginConfig.UiAdjustments.NoFreeCompanyOnNamePlate;
        
        private IntPtr playerNamePlateSetTextAddress;
        private Hook<PlayerNamePlateSetText> playerNamePlateSetTextHook;
        private IntPtr shortenedWandererTag;
        private delegate IntPtr PlayerNamePlateSetText(byte* a1, byte a2, byte a3, byte* a4, byte* a5, byte* a6, uint a7);

        public override string Name => "隐藏部队名";
        public override string Description => "隐藏姓名版中的部队名和'放浪神'";

        public override void Setup() {
            try {
                shortenedWandererTag = Marshal.AllocHGlobal(60);
                var seStr = new SeString(new List<Payload>() {
                    new TextPayload(" «"),
                    new IconPayload(BitmapFontIcon.CrossWorld),
                    new TextPayload("»"),
                });
                var bytes = seStr.Encode();
                Marshal.Copy(bytes, 0, shortenedWandererTag, bytes.Length);
                Marshal.WriteByte(shortenedWandererTag, bytes.Length, 0);
                playerNamePlateSetTextAddress = PluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 5C 24 ?? 45 38 A7 ?? ?? ?? ??");
                base.Setup();
            } catch (Exception ex) {
                SimpleLog.Log($"Failed Setup of {GetType().Name}: {ex.Message}");
            }
        }

        public override void Enable() {
            playerNamePlateSetTextHook ??= new Hook<PlayerNamePlateSetText>(playerNamePlateSetTextAddress, new PlayerNamePlateSetText(NamePlateDetour));
            playerNamePlateSetTextHook?.Enable();
            base.Enable();
        }

        public override void Disable() {
            playerNamePlateSetTextHook?.Disable();
            base.Disable();
        }

        public override void Dispose() {
            Marshal.FreeHGlobal(shortenedWandererTag);
            playerNamePlateSetTextHook?.Dispose();
            base.Dispose();
        }

        private IntPtr NamePlateDetour(byte* a1, byte a2, byte a3, byte* a4, byte* a5, byte* a6, uint a7) {
            var isHidden = true;
            if (Config.KeepWandererTag || Config.KeepNameVisible.Count > 0) {
                var i = 0;
                for (i = 0; i < 20; i++) if (a6[i] == 0) break;
                if (i >= 1) {
                    var str = Encoding.UTF8.GetString(a6, i).Trim(' ', '«', '»');
                    var isWanderer = Config.KeepWandererTag && PluginInterface.ClientState.ClientLanguage switch {
                        ClientLanguage.German => str == "Wanderin" || str == "Wanderer",
                        ClientLanguage.French => str == "Baroudeuse" || str == "Baroudeur",
                        ClientLanguage.ChineseSimplified => str == "放浪神加护",
                        _ => str == "Wanderer",
                    };

                    if (isWanderer || Config.KeepNameVisible.Contains(str)) {
                        if (isWanderer && Config.ShortenedWandererTag) {
                            a6 = (byte*) shortenedWandererTag;
                        }
                        isHidden = false;
                    }
                }
            } 

            if (isHidden && a6 != null) a6[0] = 0;
            return playerNamePlateSetTextHook.Original(a1, a2, a3, a4, a5, a6, a7);
        }


        private string inputStringIgnoreTag = string.Empty;
        protected override DrawConfigDelegate DrawConfigTree => (ref bool _) => {
            ImGui.Checkbox("显示'放浪神'", ref Config.KeepWandererTag);
            if (Config.KeepWandererTag) {
                ImGui.SameLine();
                ImGui.Checkbox($"使用'{(char) SeIconChar.CrossWorld}'代表'放浪神'", ref Config.ShortenedWandererTag);
            }
            ImGui.Text("显示部队:");
            ImGui.Indent();
            foreach (var keep in Config.KeepNameVisible) {
                ImGui.Text(keep);
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.SmallButton($"{(char) FontAwesomeIcon.Times}##removeIgnoredFC_{keep}")) {
                    Config.KeepNameVisible.Remove(keep);
                    ImGui.PopFont();
                    break;
                }
                ImGui.PopFont();
            }

            ImGui.SetNextItemWidth(100 * ImGui.GetIO().FontGlobalScale);
            ImGui.InputText("###addIgnoredFCTag", ref inputStringIgnoreTag, 5);
            ImGui.SameLine();
            if (ImGui.SmallButton("添加##allowNamePlateFC")) {
                if (inputStringIgnoreTag.Length > 0 && !Config.KeepNameVisible.Contains(inputStringIgnoreTag)) {
                    Config.KeepNameVisible.Add(inputStringIgnoreTag);
                    inputStringIgnoreTag = string.Empty;
                }
            }

            ImGui.Unindent();
        };
        
    }
}
