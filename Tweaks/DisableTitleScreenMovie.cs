using Dalamud.Game;
using Dalamud.Game.Internal;
using FFXIVClientInterface.Client.UI.Agent;
using SimpleTweaksPlugin.TweakSystem;

namespace SimpleTweaksPlugin.Tweaks {
    internal unsafe class DisableTitleScreenMovie : Tweak {
        public override string Name => "禁用开场动画";
        public override string Description => "禁用在主界面闲置过久时会播放的开场动画";
        
        public override void Enable() {
            Service.Framework.Update += FrameworkUpdate;
            base.Enable();
        }
        
        public override void Disable() {
            Service.Framework.Update -= FrameworkUpdate;
            base.Disable();
        }

        private void FrameworkUpdate(Framework framework) {
            try {
                if (Service.Condition == null) return;
                if (Service.Condition.Any()) return;
                SimpleTweaksPlugin.Client.UiModule.AgentModule.GetAgent<AgentLobby>().Data->IdleTime = 0;
            } catch {
                // Ignored
            }
        }
    }
}
