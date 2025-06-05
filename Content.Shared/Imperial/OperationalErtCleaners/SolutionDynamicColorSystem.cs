using Content.Shared.Imperial.OperationalErtCleaners.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Roles;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Imperial.OperationalErtCleaners;

public sealed class SolutionDynamicColorOfStampSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SolutionDynamicColorOfStampComponent, SolutionContainerChangedEvent>(OnStampDynamicColor);
    }

    public void OnStampDynamicColor(EntityUid uid, SolutionDynamicColorOfStampComponent comp, ref SolutionContainerChangedEvent args)
    {
        var colorSocution = args.Solution.GetColor(_protoManager);

        /// do not stamp if the mop is reagent-free (default color = white)
        if (colorSocution.Equals(Color.White.WithAlpha(0)))
        {
            RemComp<StampComponent>(uid);
        }
        else
        {
            var stamp = EnsureComp<StampComponent>(uid);
            if (comp.CheckValidRole)
            {
                if (!TryGetUserHoldingItem(uid, out var user) || user == null)
                    return;

                if (!IsUserJanitor(user.Value, comp))
                {
                    stamp.StampedName = comp.FalseStampedName;
                }
            }
            stamp.StampedColor = colorSocution;
        }
    }

    private bool IsUserJanitor(EntityUid user, SolutionDynamicColorOfStampComponent comp)
    {
        if (string.IsNullOrEmpty(comp.RoleName))
            return false;
        if (string.IsNullOrEmpty(comp.FalseStampedName))
            return false;

        if (!_mindSystem.TryGetMind(user, out var mindId, out var mind))
            return false;

        foreach (var role in _roleSystem.MindGetAllRoleInfo(mindId))
        {
            if (role.Name.Contains(comp.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetUserHoldingItem(EntityUid item, [NotNullWhen(true)] out EntityUid? user)
    {
        user = null;

        var query = EntityQueryEnumerator<HandsComponent>();
        while (query.MoveNext(out var uid, out var hands))
        {
            foreach (var hand in _handsSystem.EnumerateHands(uid, hands))
            {
                if (hand.HeldEntity == item)
                {
                    user = uid;
                    return true;
                }
            }
        }

        return false;
    }
}

