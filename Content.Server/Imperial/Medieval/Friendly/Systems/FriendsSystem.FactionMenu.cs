using System.Linq;
using Content.Server.MedievalPasport.Components;
using Content.Shared.Friends;
using Content.Shared.Friends.Components;
using Content.Shared.Friends.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Friends;

public sealed partial class FriendsSystem
{
    private void InitializeMenu()
    {
        SubscribeLocalEvent<FriendsComponent, MapInitEvent>(OnFriendsInit);

        SubscribeNetworkEvent<SetFactionMemberObjectiveMessage>(OnSetObjective);
        SubscribeNetworkEvent<SetFactionMemberGroupMessage>(OnSetGroup);
        SubscribeLocalEvent<RemoveFactionMemberMessage>(OnMemberRemoved);
    }

    private void OnFriendsInit(EntityUid uid, FriendsComponent comp, MapInitEvent args)
    {
        if (!TryGetFactionDataContainer(out var container))
            return;

        var data = new FactionMemberData()
        {
            Name = Name(uid),
            Job = CompOrNull<MedievalPasportPersonComponent>(uid)?.PersonJob ?? "Нет должности"
        };
        container.Value.Comp.CachedMembers.GetOrNew(comp.Faction).Add(GetNetEntity(uid), data);

        _action.AddAction(uid, ref comp.FactionMenuActionEntity, comp.FactionMenuAction);

        Dirty(uid, comp);
        RefreshFactionMenu(comp.Faction);
    }

    private void OnSetObjective(SetFactionMemberObjectiveMessage args)
    {
        if (!TryGetFactionDataContainer(out var ent))
            return;
        var dict = ent.Value.Comp.Objectives.GetOrNew(args.Faction);
        if (dict.TryGetValue(args.Group, out _))
            dict[args.Group] = args.Objective;
        else
            dict.Add(args.Group, args.Objective);

        RefreshFactionMenu(args.Faction);
    }

    private void OnSetGroup(SetFactionMemberGroupMessage args)
    {
        var uid = GetEntity(args.Ent);
        if (!uid.IsValid())
            return;
        if (!TryComp<FriendsComponent>(uid, out var comp))
            return;
        if (!TryGetFactionDataContainer(out var container))
            return;
        if (!TryGetFactionMemberData(args.Ent, out var data))
            return;

        data.Group = args.Group;

        RefreshFactionMenu(comp.Faction);
    }

    private void OnMemberRemoved(RemoveFactionMemberMessage args)
    {
        var uid = GetEntity(args.Ent);
        if (!uid.IsValid())
            return;
        if (!TryComp<FriendsComponent>(uid, out var comp))
            return;
        // comp.Faction = "Voluntary";
        // comp.MemberData.Job = "Нет должности";
        // comp.MemberData.Objective = "";
        //comp.MemberData.Group = "";
    }

    public void RefreshFactionMenu(ProtoId<MedievalFactionPrototype> proto)
    {
        Dictionary<NetEntity, FactionMemberData> dict = new();

        if (!TryGetFactionDataContainer(out var container))
            return;

        dict = container.Value.Comp.CachedMembers.GetOrNew(proto);
        dict.OrderBy(x => Comp<FriendsComponent>(GetEntity(x.Key)).Priority);

        var headQuery = EntityQueryEnumerator<FactionDataContainerComponent>();
        while (headQuery.MoveNext(out var uid, out var data))
        {
            if (data.CachedMembers.TryGetValue(proto, out _))
                data.CachedMembers[proto] = dict;
            else
                data.CachedMembers.Add(proto, dict);
            Dirty(uid, data);
        }
    }
}
