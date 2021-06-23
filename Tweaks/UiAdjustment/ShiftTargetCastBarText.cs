using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Internal;
using Dalamud.Interface;
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
            public bool ShowCastTime;
            public Alignment CastTimeAlignment = Alignment.TopLeft;
        }
        
        public Config LoadedConfig { get; private set; }
        
        public override string Name => "调整目标咏唱栏文字位置";
        public override string Description => "调整目标咏唱栏文字位置以方便阅读";
        
        private readonly Vector2 buttonSize = new Vector2(26, 22);

        protected override DrawConfigDelegate DrawConfigTree => (ref bool changed) =>
        {
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
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("显示在进度条上方");

            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{(char) FontAwesomeIcon.CircleNotch}", bSize)) {
                LoadedConfig.Offset = 24;
                changed = true;
            }

            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("初始位置");


            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button($"{(char)FontAwesomeIcon.ArrowDown}", bSize)) {
                LoadedConfig.Offset = 32;
                changed = true;
            }

            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("显示在进度条下方");
            ImGui.PopStyleVar();
            ImGui.SameLine();
            ImGui.Text("垂直偏移量");

            changed |= ImGuiExt.HorizontalAlignmentSelector("Ability Name Alignment", ref LoadedConfig.NameAlignment, VerticalAlignment.Bottom);

<<<<<<< Updated upstream
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Checkbox($"###{GetType().Name}_EnableCastTime",
                ref PluginConfig.UiAdjustments.ShiftTargetCastBarText.EnableCastTime)) changed = true;
            ImGui.PopFont();
            ImGui.SameLine();
            ImGui.Text("显示咏唱时间");

            ImGui.SetNextItemWidth(90 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.InputInt($"###{GetType().Name}_CastTimeFontSize",
                ref PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeFontSize))
            {
                if (PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeFontSize <= 0)
                    PluginConfig.UiAdjustments.ShiftTargetCastBarText.Offset = 1;
                changed = true;
            }

            ImGui.SameLine();
            ImGui.Text("咏唱时间字体大小");

            ImGui.SetNextItemWidth(90 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.InputInt($"###{GetType().Name}_CastTimeOffsetX",
                ref PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeOffsetX)) changed = true;
            ImGui.SameLine();
            ImGui.Text("咏唱时间水平偏移");

            ImGui.SetNextItemWidth(90 * ImGui.GetIO().FontGlobalScale);
            if (ImGui.InputInt($"###{GetType().Name}_CastTimeOffsetY",
                ref PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeOffsetY)) changed = true;
            ImGui.SameLine();
            ImGui.Text("咏唱时间垂直偏移");
=======
            if (ImGui.Checkbox("Show Cast Time Left",ref LoadedConfig.ShowCastTime))
            {
                ImGui.Text("Cast Time Alignment");

                changed |= ImGuiExt.HorizontalAlignmentSelector("Cast time left Alignment", ref LoadedConfig.CastTimeAlignment, VerticalAlignment.Top);
            }
>>>>>>> Stashed changes
        };

        public void OnFrameworkUpdate(Framework framework) {
            try {
                HandleBars();
            } catch (Exception ex) {
                Plugin.Error(this, ex);
            }
        }

        private void HandleBars(Framework framework, bool reset = false)
        {
            var focusTargetInfo = framework.Gui.GetAddonByName("_FocusTargetInfo", 1);
            if (focusTargetInfo != null && (focusTargetInfo.Visible || reset))
                HandleFocusTargetInfo(focusTargetInfo, reset);

<<<<<<< Updated upstream
            var seperatedCastBar = framework.Gui.GetAddonByName("_TargetInfoCastBar", 1);
            if (seperatedCastBar != null && (seperatedCastBar.Visible || reset))
            {
                HandleSeperatedCastBar(seperatedCastBar, reset);
                if (!reset) return;
=======
            var focusTargetInfo = Common.GetUnitBase("_FocusTargetInfo");
            if (focusTargetInfo != null && (focusTargetInfo->IsVisible || reset)) {
                DoShift(focusTargetInfo->UldManager.NodeList[16]);
                if (LoadedConfig.ShowCastTime)
                {
                    AddCastTimeTextNode(focusTargetInfo,(AtkTextNode*)focusTargetInfo->UldManager.NodeList[16],focusTargetInfo->UldManager.NodeList[16]->IsVisible);
                }
>>>>>>> Stashed changes
            }

            var mainTargetInfo = framework.Gui.GetAddonByName("_TargetInfo", 1);
            if (mainTargetInfo != null && (mainTargetInfo.Visible || reset))
                HandleMainTargetInfo(mainTargetInfo, reset);
        }

<<<<<<< Updated upstream
        private unsafe void HandleSeperatedCastBar(Addon addon, bool reset = false)
        {
            var addonStruct = (AtkUnitBase*) addon.Address;
            if (addonStruct->RootNode == null) return;
            var rootNode = addonStruct->RootNode;
            if (rootNode->ChildNode == null) return;
            var child = rootNode->ChildNode;
            DoShift(child, reset);
            if (!PluginConfig.UiAdjustments.ShiftTargetCastBarText.EnableCastTime)
                return;
            var textNode = (AtkTextNode*)  GetNodeById(addonStruct,4);
            AddCastTimeTextNode(addonStruct, textNode, textNode->AtkResNode.IsVisible);
        }

        private unsafe void HandleMainTargetInfo(Addon addon, bool reset = false)
        {
            var addonStruct = (AtkUnitBase*) addon.Address;
            if (addonStruct->RootNode == null) return;

            var child = GetNodeById(addonStruct, 10);
            if (child == null) return;
            DoShift(child, reset);

            if (!PluginConfig.UiAdjustments.ShiftTargetCastBarText.EnableCastTime)
                return;
            var textNode = (AtkTextNode*) GetNodeById(addonStruct,12);
            AddCastTimeTextNode(addonStruct, textNode, textNode->AtkResNode.IsVisible);
        }

        private unsafe void HandleFocusTargetInfo(Addon addon, bool reset = false)
        {
            var addonStruct = (AtkUnitBase*) addon.Address;
            var child = GetNodeById(addonStruct, 3);
            if (child == null) return;
            DoShift(child, reset);

            
            if (!PluginConfig.UiAdjustments.ShiftTargetCastBarText.EnableCastTime)
                return;
            var textNode = (AtkTextNode*) GetNodeById(addonStruct,5);
            AddCastTimeTextNode(addonStruct, textNode, textNode->AtkResNode.IsVisible);
=======
            var splitCastBar = Common.GetUnitBase("_TargetInfoCastBar");
            if (splitCastBar != null && (splitCastBar->IsVisible || reset)) {
                DoShift(splitCastBar->UldManager.NodeList[5]);
                if (LoadedConfig.ShowCastTime)
                {
                    AddCastTimeTextNode(focusTargetInfo,(AtkTextNode*)focusTargetInfo->UldManager.NodeList[5],focusTargetInfo->UldManager.NodeList[5]->IsVisible);
                }
                if (!reset) return;
            }

            var mainTargetInfo = Common.GetUnitBase("_TargetInfo");
            if (mainTargetInfo != null && (mainTargetInfo->IsVisible || reset)) {
                DoShift(mainTargetInfo->UldManager.NodeList[44]);
                if (LoadedConfig.ShowCastTime)
                {
                    AddCastTimeTextNode(focusTargetInfo,(AtkTextNode*)focusTargetInfo->UldManager.NodeList[44],focusTargetInfo->UldManager.NodeList[44]->IsVisible);
                }
            }
>>>>>>> Stashed changes
        }
        
        private const int MinOffset = -24;
        private const int MaxOffset = 24;

        private unsafe void DoShift(AtkResNode* node, bool reset = false)
        {
            if (node == null) return;
            if (node->ChildCount < 5) return; // Should have 5 children
            var skillTextNode = UiAdjustments.GetResNodeByPath(node, Child, Previous, Previous, Previous);
            if (skillTextNode == null) return;
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

        private unsafe void AddCastTimeTextNode(AtkUnitBase* unit, AtkTextNode* cloneTextNode, bool visible = false)
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
                textNode->AtkResNode.NextSiblingNode = (AtkResNode*) nextNode;
                textNode->AtkResNode.PrevSiblingNode = null;
                nextNode->AtkResNode.PrevSiblingNode = (AtkResNode*) textNode;
                nextNode->AtkResNode.ParentNode->ChildCount += 1;
            }

            if (!visible)
            {
                UiHelper.Hide(textNode);
            }
            else
            {
                textNode->AlignmentFontType = (byte) LoadedConfig.CastTimeAlignment;
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

<<<<<<< Updated upstream
        private const int TargetCastNodeId = 99990002;

        private unsafe void AddCastTimeTextNode(AtkUnitBase* unit, AtkTextNode* cloneTextNode, bool visible = false)
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
                textNode->AtkResNode.NextSiblingNode = (AtkResNode*) nextNode;
                textNode->AtkResNode.PrevSiblingNode = null;
                nextNode->AtkResNode.PrevSiblingNode = (AtkResNode*) textNode;
                nextNode->AtkResNode.ParentNode->ChildCount += 1;
            }

            if (!visible)
            {
                UiHelper.Hide(textNode);
            }
            else
            {
                textNode->AlignmentFontType = 0x25;
                UiHelper.SetPosition(textNode, PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeOffsetX,
                    PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeOffsetY);
                UiHelper.SetSize(textNode, cloneTextNode->AtkResNode.Width, cloneTextNode->AtkResNode.Height);
                textNode->FontSize = (byte) PluginConfig.UiAdjustments.ShiftTargetCastBarText.CastTimeFontSize;
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

        
=======
>>>>>>> Stashed changes
        private static unsafe AtkResNode* GetNodeById(AtkUnitBase* compBase, uint id)
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

<<<<<<< Updated upstream
        public override void Enable()
        {
=======


        public override void Enable() {
>>>>>>> Stashed changes
            if (Enabled) return;
            LoadedConfig = LoadConfig<Config>() ?? PluginConfig.UiAdjustments.ShiftTargetCastBarText ?? new Config();
            PluginInterface.Framework.OnUpdateEvent += OnFrameworkUpdate;
            Enabled = true;
        }

        public override void Disable()
        {
            if (!Enabled) return;
            SaveConfig(LoadedConfig);
            PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
            SimpleLog.Debug($"[{GetType().Name}] Reset");
            HandleBars(true);
            Enabled = false;
        }

        public override void Dispose()
        {
            PluginInterface.Framework.OnUpdateEvent -= OnFrameworkUpdate;
            Enabled = false;
            Ready = false;
        }
    }
}