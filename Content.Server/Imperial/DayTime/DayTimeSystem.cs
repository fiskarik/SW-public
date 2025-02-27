using Robust.Shared.Map.Components;
using Content.Shared.DayTime;
using Robust.Shared.Prototypes;
using Content.Shared.Imperial.DayTime;
using Robust.Shared.Timing;
using System.Linq;
using Robust.Server.GameObjects;

namespace Content.Server.Imperial.DayTime
{
    public sealed class DayTimeSystem : EntitySystem
    {
        public void ChangePreset(string groupID, string presetID, bool instantStartUpdate = true)
        {
        }

        public void OnStartupComponent(EntityUid uid, DayTimeComponent comp, ComponentStartup args)
        {
        }
    }
}
