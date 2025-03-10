using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Friends.Components
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class FactionHeadComponent : Component
    {
        [DataField(required: true)]
        public EntProtoId FactionMenuAction = "";

        public EntityUid? FactionMenuActionEntity;

        [AutoNetworkedField]
        public Dictionary<NetEntity, FactionMemberData> CachedMembers = new();
    }
}
