using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using SimpleTweaksPlugin.GameStructs;
using static SimpleTweaksPlugin.Tweaks.TooltipTweaks;
using static SimpleTweaksPlugin.Tweaks.TooltipTweaks.ItemTooltip.TooltipField;

namespace SimpleTweaksPlugin {
    public partial class TooltipTweakConfig {
        public bool PrecisionDurabilityTrailingZeros = true;
    }
}

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment {
    
    public class PrecisionDurability : TooltipTweaks.SubTweak {
        public override string Name => "耐久度精确化";
        public override string Description => "显示较为精确的装备耐久百分比";

        public override void OnItemTooltip(TooltipTweaks.ItemTooltip tooltip, InventoryItem itemInfo) {
            var c = tooltip[DurabilityPercent];
            if (c != null && !(c.Payloads[0] is TextPayload tp && tp.Text.StartsWith("?"))) {
                tooltip[DurabilityPercent] = new SeString(new List<Payload>() { new TextPayload((itemInfo.Condition / 300f).ToString(PluginConfig.TooltipTweaks.PrecisionDurabilityTrailingZeros ? "F2" : "0.##") + "%") });
            }

        }

        protected override DrawConfigDelegate DrawConfigTree => (ref bool hasChanged) => {
            hasChanged |= ImGui.Checkbox($"显示尾随0###{GetType().Name}TrailingZeros", ref PluginConfig.TooltipTweaks.PrecisionDurabilityTrailingZeros);
        };
    }
}
