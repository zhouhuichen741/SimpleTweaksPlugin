using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;
using SimpleTweaksPlugin.GameStructs;
using static SimpleTweaksPlugin.Tweaks.TooltipTweaks;
using static SimpleTweaksPlugin.Tweaks.TooltipTweaks.ItemTooltip.TooltipField;

namespace SimpleTweaksPlugin {
    public partial class TooltipTweakConfig {
        public bool PrecisionSpiritbondTrailingZeros = true;
    }
}

namespace SimpleTweaksPlugin.Tweaks.UiAdjustment {
    public class PrecisionSpiritbond : SubTweak {
        public override string Name => "精炼度精确化";
        public override string Description => "显示较为精确的精炼度百分比";

        public override void OnItemTooltip(ItemTooltip tooltip, InventoryItem itemInfo) {
            var c = tooltip[SpiritbondPercent];
            if (c != null && !(c.Payloads[0] is TextPayload tp && tp.Text.StartsWith("?"))) {
                tooltip[SpiritbondPercent] = new SeString(new List<Payload>() { new TextPayload((itemInfo.Spiritbond / 100f).ToString(PluginConfig.TooltipTweaks.PrecisionSpiritbondTrailingZeros ? "F2" : "0.##") + "%") });
            }
        }

        protected override DrawConfigDelegate DrawConfigTree => (ref bool hasChanged) => {
            hasChanged |= ImGui.Checkbox($"显示尾随0###{GetType().Name}TrailingZeros", ref PluginConfig.TooltipTweaks.PrecisionSpiritbondTrailingZeros);
        };
    }

}

