using System.Linq;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel;
using SimpleTweaksPlugin.Helper;
using SimpleTweaksPlugin.Sheets;
using SimpleTweaksPlugin.TweakSystem;
using static SimpleTweaksPlugin.Tweaks.TooltipTweaks.ItemTooltipField;

namespace SimpleTweaksPlugin.Tweaks.Tooltips {
    public class DesynthesisSkill : TooltipTweaks.SubTweak {
        public override string Name => "显示分解等级";
        public override string Description => "当你分解物品时显示你对应的分解等级";

        private readonly uint[] desynthesisInDescription = { 46, 56, 65, 66, 67, 68, 69, 70, 71, 72 };

        public class Configs : TweakConfig {
            public bool Delta;
        }
        
        public Configs Config { get; private set; }

        private ExcelSheet<ExtendedItem> itemSheet;

        public override void Enable() {
            itemSheet = Service.Data.Excel.GetSheet<ExtendedItem>();
            if (itemSheet == null) return;
            Config = LoadConfig<Configs>() ?? new Configs();
            base.Enable();
        }

        public override void Disable() {
            SaveConfig(Config);
            base.Disable();
        }

        public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData) {
            var id = Service.GameGui.HoveredItem;
            if (id < 2000000) {
                id %= 500000;

                var item = itemSheet.GetRow((uint)id);
                if (item != null && item.Desynth > 0) {
                    var classJobOffset = 2 * (int)(item.ClassJobRepair.Row - 8);
                    // 5.5 0x6A6
                    var desynthLevel = *(ushort*)(Common.PlayerStaticAddress + (0x69A + classJobOffset)) / 100f;
                    var desynthDelta = item.LevelItem.Row - desynthLevel;

                    var useDescription = desynthesisInDescription.Contains(item.ItemSearchCategory.Row);

                    var seStr = GetTooltipString(stringArrayData, useDescription ? ItemDescription : ExtractableProjectableDesynthesizable);

                    if (seStr != null) {
                        if (seStr.Payloads.Last() is TextPayload textPayload) {
                            if (Config.Delta) {
                                textPayload.Text = textPayload.Text.Replace($"{item.LevelItem.Row},00", $"{item.LevelItem.Row} ({desynthDelta:+#;-#}");
                                textPayload.Text = textPayload.Text.Replace($"{item.LevelItem.Row}.00", $"{item.LevelItem.Row} ({desynthDelta:+#;-#})");
                            } else {
                                textPayload.Text = textPayload.Text.Replace($"{item.LevelItem.Row},00", $"{item.LevelItem.Row} ({desynthLevel:F0})");
                                textPayload.Text = textPayload.Text.Replace($"{item.LevelItem.Row}.00", $"{item.LevelItem.Row} ({desynthLevel:F0})");
                            }
                            stringArrayData->SetValue((int) ( useDescription ? ItemDescription : ExtractableProjectableDesynthesizable), seStr.Encode(), false);
                        }
                    }
                }
            }
        }

        protected override DrawConfigDelegate DrawConfigTree => (ref bool hasChanged) => {
            hasChanged |= ImGui.Checkbox($"显示差值###{GetType().Name}DesynthesisDelta", ref Config.Delta);
        };
    }
}
