﻿using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ImGuiNET;
using SimpleTweaksPlugin.Helper;
using SimpleTweaksPlugin.TweakSystem;

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment {
    public unsafe class SmartNameplates : UiAdjustments.SubTweak {
        public override string Name => "智能隐藏姓名板";
        public override string Description => "提供在战斗中隐藏特定目标姓名板的选项";
        protected override string Author => "UnknownX";

        public class Configs : TweakConfig {
            public bool ShowHP = false;
            public bool IgnoreParty = false;
            public bool IgnoreAlliance = false;
            public bool IgnoreFriends = false;
            public bool IgnoreDead = false;
            public bool IgnoreTargets = false;
        }

        private Configs config;

        private IntPtr targetManager = IntPtr.Zero;
        private delegate byte ShouldDisplayNameplateDelegate(IntPtr raptureAtkModule, GameObject* actor, GameObject* localPlayer, float distance);
        private Hook<ShouldDisplayNameplateDelegate> shouldDisplayNameplateHook;
        private delegate byte GetTargetTypeDelegate(GameObject* actor);
        private GetTargetTypeDelegate GetTargetType;

        protected override DrawConfigDelegate DrawConfigTree => (ref bool _) => {
            ImGui.Checkbox("在战斗中不隐藏以下目标姓名版##SmartNameplatesShowHP", ref config.ShowHP);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("总是显示特定目标的HP条");

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextUnformatted("不隐藏以下目标的的HP条:");
            ImGui.Checkbox("队友##SmartNameplatesIgnoreParty", ref config.IgnoreParty);
            ImGui.Checkbox("团队成员##SmartNameplatesIgnoreAlliance", ref config.IgnoreAlliance);
            ImGui.Checkbox("好友##SmartNameplatesIgnoreFriends", ref config.IgnoreFriends);
            ImGui.Checkbox("已死亡角色##SmartNameplatesIgnoreDead", ref config.IgnoreDead);
            ImGui.Checkbox("目标角色##SmartNameplatesIgnoreTargets", ref config.IgnoreTargets);
        };

        // returns 2 bits (b01 == display name, b10 == display hp)
        private byte ShouldDisplayNameplateDetour(IntPtr raptureAtkModule, GameObject* actor, GameObject* localPlayer, float distance) {
            if (actor->ObjectKind != (byte) ObjectKind.Pc) goto ReturnOriginal;
            var pc = (BattleChara*) actor;

            var targets = TargetSystem.Instance();

            if (actor == localPlayer // Ignore localplayer
                || (((BattleChara*) localPlayer)->Character.StatusFlags & 2) == 0 // Alternate in combat flag
                || GetTargetType(actor) == 3

                || (config.IgnoreParty && (pc->Character.StatusFlags & 16) > 0) // Ignore party members
                || (config.IgnoreAlliance && (pc->Character.StatusFlags & 32) > 0) // Ignore alliance members
                || (config.IgnoreFriends && (pc->Character.StatusFlags & 64) > 0) // Ignore friends
                || (config.IgnoreDead && pc->Character.Health == 0) // Ignore dead players

                // Ignore targets
                || (config.IgnoreTargets && targets->Target == actor
                    || targets->SoftTarget == actor
                    || targets->FocusTarget == actor)) goto ReturnOriginal;

            return (byte)(config.ShowHP ? (shouldDisplayNameplateHook.Original(raptureAtkModule, actor, localPlayer, distance) & ~1) : 0); // Ignore HP


            ReturnOriginal:
            return shouldDisplayNameplateHook.Original(raptureAtkModule, actor, localPlayer, distance);
        }

        public override void Enable() {
            config = LoadConfig<Configs>() ?? new Configs();
            targetManager = targetManager != IntPtr.Zero ? targetManager : Common.Scanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB", 3); // Taken from Dalamud
            GetTargetType ??= Marshal.GetDelegateForFunctionPointer<GetTargetTypeDelegate>(Common.Scanner.ScanText("E8 ?? ?? ?? ?? 83 F8 06 0F 87 ?? ?? ?? ?? 48 8D 15 ?? ?? ?? ?? 8B C0"));
            shouldDisplayNameplateHook ??= new Hook<ShouldDisplayNameplateDelegate>(Common.Scanner.ScanText("E8 ?? ?? ?? ?? 89 44 24 40 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ??"), ShouldDisplayNameplateDetour);
            shouldDisplayNameplateHook?.Enable();
            base.Enable();
        }

        public override void Disable() {
            SaveConfig(config);
            shouldDisplayNameplateHook?.Disable();
            base.Disable();
        }

        public override void Dispose() {
            shouldDisplayNameplateHook?.Dispose();
            base.Dispose();
        }
    }
}
