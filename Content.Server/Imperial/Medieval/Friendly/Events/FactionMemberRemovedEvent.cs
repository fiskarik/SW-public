using Content.Shared.Friends.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Friends;

[ByRefEvent]
public record struct FactionMemberRemovedEvent(EntityUid Member, ProtoId<MedievalFactionPrototype> Faction);
