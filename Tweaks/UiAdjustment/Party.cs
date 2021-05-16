using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using SimpleTweaksPlugin.Helper;
using SimpleTweaksPlugin.Tweaks.UiAdjustment;
using System;
using System.Collections.Generic;
using SimpleTweaksPlugin.GameStructs;


namespace SimpleTweaksPlugin
{
    public partial class UiAdjustmentsConfig
    {
        public PartyListAdjustments.Configs PartyListAdjustments = new();
    }
}

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment
{
    public unsafe class PartyListAdjustments : UiAdjustments.SubTweak
    {
        public class Configs
        {
            public bool HpPercent = true;
            public bool ShieldShift;
            public bool MpShield;
        }

        public Configs Config => PluginConfig.UiAdjustments.PartyListAdjustments;

        private delegate long PartyUiUpdate(long a1, long a2, long a3);

        private Hook<PartyUiUpdate> partyUiUpdateHook;

        private PartyUi* party;
        private DataArray* data;

        private IntPtr l1, l2, l3;


        public override string Name => "队伍列表修改";
        public override string Description => "队伍列表相关内容修改";


        protected override DrawConfigDelegate DrawConfigTree => (ref bool changed) =>
        {
            changed |= ImGui.Checkbox("HP及盾值百分比显示", ref Config.HpPercent);
            changed |= ImGui.Checkbox("盾值(估计值)替换MP值", ref Config.MpShield);
            changed |= ImGui.Checkbox("将护盾条与血条重合显示", ref Config.ShieldShift);


            if (changed)
            {
                if (Config.ShieldShift) ShiftShield();
                RefreshHooks();
            }
        };


        private void RefreshHooks()
        {
            try
            {
                partyUiUpdateHook ??= new Hook<PartyUiUpdate>(
                    Common.Scanner.ScanText(
                        "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B 7A ?? 48 8B D9 49 8B 70 ?? 48 8B 47"),
                    new PartyUiUpdate(PartyListUpdateDelegate));
                if (Enabled) partyUiUpdateHook?.Enable();
                else partyUiUpdateHook?.Disable();

                if (!Config.ShieldShift) UnShiftShield();
                if (!Config.MpShield) ResetMp();
            }
            catch (Exception e)
            {
                SimpleLog.Error(e);
                throw;
            }
        }

        private void DisposeHooks()
        {
            partyUiUpdateHook?.Dispose();
            partyUiUpdateHook = null;
        }

        private void DisableHooks()
        {
            //if (!partyUiUpdateHook.IsDisposed) 
            partyUiUpdateHook?.Disable();
        }


        #region detors

        private long PartyListUpdateDelegate(long a1, long a2, long a3)
        {
            if ((IntPtr) a1 != l1)
            {
                l1 = (IntPtr) a1;
                l2 = (IntPtr) (*(long*) (*(long*) (a2 + 0x20) + 0x20));
                l3 = (IntPtr) (*(long*) (*(long*) (a3 + 0x18) + 0x20) + 0x30); //+Index*0x68
                party = (PartyUi*) l1;
                data = (DataArray*) l2;

                if (Config.ShieldShift) ShiftShield();

                SimpleLog.Debug("NewAddress:");
                SimpleLog.Debug("L1:" + l1.ToString("X") + " L2:" + l2.ToString("X"));
                SimpleLog.Debug("L3:" + l3.ToString("X"));
            }

            var ret = partyUiUpdateHook.Original(a1, a2, a3);
            UpdatePartyUi(true);
            return ret;
        }

        #endregion

        #region string functions

        private void SetHp(AtkTextNode* node, MemberData member)
        {
            var se = new SeString(new List<Payload>());
            if (member.CurrentHP == 1)
            {
                se.Payloads.Add(new TextPayload("1"));
            }
            else if (member.MaxHp == 1)
            {
                se.Payloads.Add(new TextPayload("???"));
            }
            else
            {
                se.Payloads.Add(new TextPayload((member.CurrentHP * 100 / member.MaxHp).ToString()));
                if (member.ShieldPercent != 0)
                {
                    UIForegroundPayload uiYellow =
                        new(PluginInterface.Data, 559);
                    UIForegroundPayload uiNoColor =
                        new(PluginInterface.Data, 0);

                    se.Payloads.Add(new TextPayload("+"));
                    se.Payloads.Add(uiYellow);
                    se.Payloads.Add(new TextPayload(member.ShieldPercent.ToString()));
                    se.Payloads.Add(uiNoColor);
                }

                se.Payloads.Add(new TextPayload("%"));
            }

            Plugin.Common.WriteSeString(node->NodeText, se);
        }

        private static AtkResNode* GetNodeById(AtkComponentBase* compBase, int id)
        {
            if (compBase == null) return null;
            if ((compBase->UldManager.Flags1 & 1) == 0 || id <= 0) return null;
            if (compBase->UldManager.Objects == null) return null;
            var count = compBase->UldManager.Objects->NodeCount;
            var ptr = (long) compBase->UldManager.Objects->NodeList;
            for (var i = 0; i < count; i++)
            {
                var node = (AtkResNode*) *(long*) (ptr + 8 * i);
                if (node->NodeID == id) return node;
            }

            return null;
        }

        #endregion


        private void ShiftShield()
        {
            if (l1 == IntPtr.Zero) return;
            for (var i = 0; i < 12; i++)
            {
                var hpBarComponentBase = party->Member(i).hpBarComponentBase;
                if (hpBarComponentBase == null) return;
                var shieldNode = (AtkNineGridNode*) GetNodeById(hpBarComponentBase, 5);
                var overShieldNode = (AtkImageNode*) GetNodeById(hpBarComponentBase, 2);
                if (shieldNode != null)
                    if (Math.Abs(shieldNode->AtkResNode.Y - 8f) < 1f)
                    {
                        shieldNode->AtkResNode.Y += 8f;
                        *(float*) ((long) shieldNode + 0x6C) += 8f;
                    }

                if (overShieldNode != null)
                    if (Math.Abs(overShieldNode->AtkResNode.Y -9f) < 1f)
                    {
                        overShieldNode->AtkResNode.Y += 8f;
                        *(float*) ((long) overShieldNode + 0x6C) += 8f;
                    }
            }
        }

        private void UnShiftShield()
        {
            if (l1 == IntPtr.Zero) return;
            for (var i = 0; i < 12; i++)
            {
                var hpBarComponentBase = party->Member(i).hpBarComponentBase;
                if (hpBarComponentBase == null) return;
                var shieldNode = (AtkNineGridNode*) GetNodeById(hpBarComponentBase, 5);
                var overShieldNode = (AtkImageNode*) GetNodeById(hpBarComponentBase, 2);
                if (shieldNode != null)
                    if (Math.Abs(shieldNode->AtkResNode.Y - 16f) < 1f)
                    {
                        shieldNode->AtkResNode.Y -= 8f;
                        *(float*) ((long) shieldNode + 0x6C) -= 8f;
                    }

                if (overShieldNode != null)
                    if (Math.Abs(overShieldNode->AtkResNode.Y - 17f) < 1f)
                    {
                        overShieldNode->AtkResNode.Y -= 8f;
                        *(float*) ((long) overShieldNode + 0x6C) -= 8f;
                    }
            }
        }

        private void ShieldOnMp(int index)
        {
            if (l1 == IntPtr.Zero) return;
            var memberdata = data->MemberData(index);
            var shield = memberdata.ShieldPercent * memberdata.MaxHp / 100;
            var node1 = (AtkTextNode*) GetNodeById(party->Member(index).mpBarComponentBase, 3);
            var node2 = (AtkTextNode*) GetNodeById(party->Member(index).mpBarComponentBase, 2);
            UIForegroundPayload uiYellow =
                new(PluginInterface.Data, 559);
            SeString se = new(new List<Payload>());
            se.Payloads.Add(uiYellow);
            se.Payloads.Add(new TextPayload(shield.ToString()));
            Plugin.Common.WriteSeString(node1->NodeText, se);
            if (node1->FontSize != 12)
            {
                node1->FontSize = 12;
                node1->AlignmentFontType -= 2;
            }

            Plugin.Common.WriteSeString(node2->NodeText, "");
        }

        private void ResetMp()
        {
            if (l1 == IntPtr.Zero) return;
            for (var index = 0; index < 12; index++)
            {
                var node1 = (AtkTextNode*) GetNodeById(party->Member(index).mpBarComponentBase, 3);
                if (node1 == null) return;
                if (node1->FontSize == 12)
                {
                    node1->FontSize = 10;
                    node1->AlignmentFontType += 2;
                }
            }
        }


        private void UpdatePartyUi(bool done)
        {
            try
            {
                for (var index = 0; index < data->LocalCount + data->CrossRealmCount; index++)
                    if (!done)
                    {
                    }
                    else //改HP
                    {
                        if (Config.HpPercent)
                        {
                            var textNode = (AtkTextNode*) GetNodeById(party->Member(index).hpComponentBase, 2);
                            if (textNode != null) SetHp(textNode, data->MemberData(index));
                        }

                        if (Config.MpShield) ShieldOnMp(index);
                    }
            }
            catch (Exception e)
            {
                SimpleLog.Error(e);
                throw;
            }
        }


        #region Framework

        public override void Enable()
        {
            if (Enabled) return;
            RefreshHooks();
            Enabled = true;
        }

        public override void Disable()
        {
            if (!Enabled) return;
            DisableHooks();
            SimpleLog.Debug($"[{GetType().Name}] Reset");
            Enabled = false;
        }


        public override void Dispose()
        {
            DisposeHooks();
            Enabled = false;
            Ready = false;
            SimpleLog.Debug($"[{GetType().Name}] Disposed");
        }

        #endregion
    }
}