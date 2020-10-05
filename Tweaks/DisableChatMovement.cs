﻿using System;
using Dalamud.Hooking;
using Dalamud.Plugin;

namespace SimpleTweaksPlugin.Tweaks {
    
    public class DisableChatMovement : Tweak {

        public override string Name => "Disable Chat Movement";

        private delegate IntPtr SetUiPositionDelegate(IntPtr _this, IntPtr uiObject, ulong y);

        private unsafe delegate void ChatPanelDragControlDelegate(IntPtr _this, ulong controlCode, ulong a3, IntPtr a4, short* a5);

        private IntPtr setUiPositionAddress = IntPtr.Zero;
        private IntPtr chatPanelControlAddress = IntPtr.Zero;

        private Hook<SetUiPositionDelegate> setUiPositionHook;
        private Hook<ChatPanelDragControlDelegate> chatPanelDragControlHook;
        
        public override void Setup() {
            if (Ready) return;

            try {
                if (setUiPositionAddress == IntPtr.Zero) {
                    setUiPositionAddress = PluginInterface.TargetModuleScanner.ScanText("40 53 48 83 EC 20 80 A2 ?? ?? ?? ?? ??");
                }

                if (chatPanelControlAddress == IntPtr.Zero) {
                    chatPanelControlAddress = PluginInterface.TargetModuleScanner.ScanText("40 55 57 48 81 EC ?? ?? ?? ?? 48 8B F9 45 8B C8");
                }
                
                if (setUiPositionAddress == IntPtr.Zero || chatPanelControlAddress == IntPtr.Zero) {
                    PluginLog.LogError($"Failed to setup {GetType().Name}: Failed to find required functions.");
                    return;
                }

                base.Setup();

            } catch (Exception ex) {
                PluginLog.LogError($"Failed to setup {this.GetType().Name}: {ex.Message}");
            }
        }

        public override unsafe void Enable() {
            if (!Ready) return;
            setUiPositionHook ??= new Hook<SetUiPositionDelegate>(setUiPositionAddress, new SetUiPositionDelegate(SetUiPositionDetour));
            chatPanelDragControlHook ??= new Hook<ChatPanelDragControlDelegate>(chatPanelControlAddress, new ChatPanelDragControlDelegate(ChatPanelControlDetour));

            setUiPositionHook?.Enable();
            chatPanelDragControlHook?.Enable();
            base.Enable();
        }

        private unsafe void ChatPanelControlDetour(IntPtr a1, ulong controlCode, ulong a3, IntPtr a4, short* a5) {
            if (controlCode == 0x17) return; // Suppress Start Drag
            chatPanelDragControlHook.Original(a1, controlCode, a3, a4, a5);
        }

        private unsafe IntPtr SetUiPositionDetour(IntPtr _this, IntPtr uiObject, ulong a3) {
            var k = *(ulong*) (uiObject + 8);
            if (k == 0x50676F4C74616843 || k == 0x676F4C74616843) {
                // Suppress Movement of "ChatLog" and "ChatLogPanel_*"
                return IntPtr.Zero;
            }

            return setUiPositionHook.Original(_this, uiObject, a3);
        }

        public override void Disable() {
            setUiPositionHook?.Disable();
            chatPanelDragControlHook?.Disable();
            base.Disable();
        }

        public override void Dispose() {
            setUiPositionHook?.Dispose();
            chatPanelDragControlHook?.Dispose();
            base.Dispose();
        }
    }
}
