﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal;
using Dalamud.Interface;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using SimpleTweaksPlugin.Enums;
using SimpleTweaksPlugin.Helper;
using SimpleTweaksPlugin.Tweaks.UiAdjustment;
using SimpleTweaksPlugin.TweakSystem;

namespace SimpleTweaksPlugin {
    public partial class UiAdjustmentsConfig {
        public ShiftTargetCastBarText.Config ShiftTargetCastBarText = new();
    }
}

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment {
    public unsafe class ShiftTargetCastBarText : UiAdjustments.SubTweak {

        public class Config : TweakConfig {
            public int Offset = 8;
            public Alignment NameAlignment = Alignment.BottomRight;
            public bool ShowCastTimeLeft;
            public Alignment CastTimeAlignment = Alignment.TopLeft;
            public int TimerOffset = 8;
        }
        
        public Config LoadedConfig { get; private set; }
        
        public override string Name => "Reposition Target Castbar Text";
        public override string Description => "Moves the text on target castbars to make it easier to read";
        
        private readonly Vector2 buttonSize = new Vector2(26, 22);

        protected override DrawConfigDelegate DrawConfigTree => (ref bool changed) => {
            var bSize = buttonSize * ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextItemWidth(90 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.InputInt($"###{GetType().Name}_Offset", ref LoadedConfig.Offset)) {
                if (LoadedConfig.Offset > MaxOffset) LoadedConfig.Offset = MaxOffset;
                if (LoadedConfig.Offset < MinOffset) LoadedConfig.Offset = MinOffset;
                changed = true;
            }
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2));
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{(char)FontAwesomeIcon.ArrowUp}", bSize)) {
                LoadedConfig.Offset = 8;
                changed = true;
            }
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Above progress bar");

            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{(char) FontAwesomeIcon.CircleNotch}", bSize)) {
                LoadedConfig.Offset = 24;
                changed = true;
            }
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Original Position");

            
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{(char)FontAwesomeIcon.ArrowDown}", bSize)) {
                LoadedConfig.Offset = 32;
                changed = true;
            }
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Below progress bar");
            ImGui.PopStyleVar();
            ImGui.SameLine();
            ImGui.Text("Ability name vertical offset");

            changed |= ImGuiExt.HorizontalAlignmentSelector("Ability Name Alignment", ref LoadedConfig.NameAlignment, VerticalAlignment.Bottom);

            ImGui.Checkbox("Show Target Cast Time Left", ref LoadedConfig.ShowCastTimeLeft);
            if (LoadedConfig.ShowCastTimeLeft)
            {
                
                changed |= ImGuiExt.HorizontalAlignmentSelector("Target Cast Time Left Alignment", ref LoadedConfig.CastTimeAlignment, VerticalAlignment.Top);
                ImGui.SetNextItemWidth(90 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.InputInt($"###Target Cast Time Left_Offset", ref LoadedConfig.TimerOffset)) {
                    if (LoadedConfig.TimerOffset > MaxOffset) LoadedConfig.TimerOffset = MaxOffset;
                    if (LoadedConfig.TimerOffset < MinOffset) LoadedConfig.TimerOffset = MinOffset;
                    changed = true;
                }
                ImGui.SameLine();
                ImGui.Text("Target Cast Time Left vertical offset");
            }
        };

        public void OnFrameworkUpdate(Framework framework) {
            try {
                HandleBars();
            } catch (Exception ex) {
                Plugin.Error(this, ex);
            }
        }

        private void HandleBars(bool reset = false) {

            var focusTargetInfo = Common.GetUnitBase("_FocusTargetInfo");
            if (focusTargetInfo != null && (focusTargetInfo->IsVisible || reset)) {
                DoShift(focusTargetInfo->UldManager.NodeList[16]);
                if (LoadedConfig.ShowCastTimeLeft)
                {
                    AddCastTimeTextNode(focusTargetInfo, (AtkTextNode*)focusTargetInfo->UldManager.NodeList[16], focusTargetInfo->UldManager.NodeList[16]->IsVisible);
                }

            }


            var splitCastBar = Common.GetUnitBase("_TargetInfoCastBar");
            if (splitCastBar != null && (splitCastBar->IsVisible || reset)) {
                DoShift(splitCastBar->UldManager.NodeList[5]);
                if (LoadedConfig.ShowCastTimeLeft)
                {
                    AddCastTimeTextNode(splitCastBar,(AtkTextNode*)splitCastBar->UldManager.NodeList[5],splitCastBar->UldManager.NodeList[5]->IsVisible);
                }
                if (!reset) return;
            }

            var mainTargetInfo = Common.GetUnitBase("_TargetInfo");
            if (mainTargetInfo != null && (mainTargetInfo->IsVisible || reset)) {
                DoShift(mainTargetInfo->UldManager.NodeList[44]);
                if (LoadedConfig.ShowCastTimeLeft)
                {
                    AddCastTimeTextNode(mainTargetInfo,(AtkTextNode*)mainTargetInfo->UldManager.NodeList[44],mainTargetInfo->UldManager.NodeList[44]->IsVisible);
                }
            }
        }
        
        private const int MinOffset = 0;
        private const int MaxOffset = 48;

        private void DoShift(AtkResNode* node, bool reset = false) {
            if (node == null) return;
            var p = LoadedConfig.Offset;
            if (p < MinOffset) p = MinOffset;
            if (p > MaxOffset) p = MaxOffset;
            node->Height = reset ? (ushort) 24 : (ushort) p;
            var textNode = (AtkTextNode*) node;
            textNode->AlignmentFontType = reset ? (byte) AlignmentType.BottomRight : (byte) LoadedConfig.NameAlignment;
            if (reset) {
                UiHelper.SetPosition(node, 0, null);
                UiHelper.SetSize(node, 197, null);
            } else {
                UiHelper.SetPosition(node, 8, null);
                UiHelper.SetSize(node, 188, null);
            }
            
        }

        private const int TargetCastNodeId = 99990002;

        private void AddCastTimeTextNode(AtkUnitBase* unit, AtkTextNode* cloneTextNode, bool visible = false)
        {
            var textNode = (AtkTextNode*)GetNodeById(unit, TargetCastNodeId);
            
            if (textNode == null)
            {
                textNode = UiHelper.CloneNode(cloneTextNode);
                textNode->AtkResNode.NodeID = TargetCastNodeId;
                var newStrPtr = Common.Alloc(512);
                textNode->NodeText.StringPtr = (byte*) newStrPtr;
                textNode->NodeText.BufSize = 512;
                UiHelper.SetText(textNode, "");
                UiHelper.ExpandNodeList(unit, 1);
                unit->UldManager.NodeList[unit->UldManager.NodeListCount++] = (AtkResNode*) textNode;

                var nextNode = (AtkTextNode*)GetNodeById(unit, cloneTextNode->AtkResNode.NodeID-1);

                textNode->AtkResNode.ParentNode = nextNode->AtkResNode.ParentNode;
                textNode->AtkResNode.ChildNode = null;
                //textNode->AtkResNode.NextSiblingNode = (AtkResNode*) nextNode;
                //textNode->AtkResNode.PrevSiblingNode = null;
                //nextNode->AtkResNode.PrevSiblingNode = (AtkResNode*) textNode;
                nextNode->AtkResNode.ParentNode->ChildCount += 1;
            }

            if (!visible)
            {
                UiHelper.Hide(textNode);
            }
            else
            {
                textNode->AlignmentFontType = (byte)(0x26 + (byte)LoadedConfig.CastTimeAlignment);
                textNode->AtkResNode.Height = (ushort) LoadedConfig.TimerOffset;
                //UiHelper.SetPosition(textNode, PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeOffsetX,
                //    PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeOffsetY);
                //UiHelper.SetSize(textNode, cloneTextNode->AtkResNode.Width, cloneTextNode->AtkResNode.Height);
                textNode->FontSize = 15;//(byte) PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeFontSize;
                UiHelper.SetText(textNode, GetTargetCastTime().ToString("00.00"));
                UiHelper.Show(textNode);
            }

        }


        private float GetTargetCastTime()
        {
            if (PluginInterface.ClientState.LocalPlayer == null ||
                PluginInterface.ClientState.Targets.CurrentTarget == null)
                return 0;
            var target = PluginInterface.ClientState.Targets.CurrentTarget;
            if (target is Chara)
            {
                var castTime =
                    Marshal.PtrToStructure<float>(target.Address +
                                                  Dalamud.Game.ClientState.Structs.ActorOffsets.CurrentCastTime);
                var totalCastTime =
                    Marshal.PtrToStructure<float>(target.Address +
                                                  Dalamud.Game.ClientState.Structs.ActorOffsets.TotalCastTime);
                return totalCastTime - castTime;
            }

            return 0;
        }

        
        private static AtkResNode* GetNodeById(AtkUnitBase* compBase, uint id)
        {
            if (compBase == null) return null;
            if ((compBase->UldManager.Flags1 & 1) == 0 || id <= 0) return null;
            var count = compBase->UldManager.NodeListCount;
            for (var i = 0; i < count; i++)
            {
                
                var node = compBase->UldManager.NodeList[i];
                //SimpleLog.Information(i+"@"+node->NodeID);
                if (node->NodeID == id) return node;
            }
            return null;
        }

        public override void Enable() {
            if (Enabled) return;
            LoadedConfig = LoadConfig<Config>() ?? PluginConfig.UiAdjustments.ShiftTargetCastBarText ?? new Config();
            PluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;
            Enabled = true;
        }

        public override void Disable() {
            if (!Enabled) return;
            SaveConfig(LoadedConfig);
            PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
            SimpleLog.Debug($"[{GetType().Name}] Reset");
            HandleBars(true);
            Enabled = false;
        }

        public override void Dispose() {
            PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
            Enabled = false;
            Ready = false;
        }
    }
}
