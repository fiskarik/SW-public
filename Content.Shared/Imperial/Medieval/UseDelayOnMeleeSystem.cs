using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Imperial.DurabilityDisplay.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class UseDelayOnMeleeSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UseDelayOnMeleeComponent, MeleeHitEvent>(OnUseMelee);
        SubscribeLocalEvent<UseDelayOnMeleeComponent, ComponentStartup>(OnStart);
    }

    private void OnStart(EntityUid uid, UseDelayOnMeleeComponent component, ref ComponentStartup args)
    {
        if (EnsureComp<UseDelayComponent>(uid, out var useDelay) && TryComp<MeleeWeaponComponent>(uid, out var weapon))
            _delay.SetLength(uid, TimeSpan.FromSeconds(1f / weapon.AttackRate));
        if (TryComp<MeleeWeaponComponent>(uid, out var weap))
            EnsureComp<DurabilityDisplayComponent>(uid, out var dur);
    }

    private void OnUseMelee(EntityUid uid, UseDelayOnMeleeComponent component, ref MeleeHitEvent args)
    {
        if (EnsureComp<UseDelayComponent>(args.Weapon, out var useDelay) && TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon))
        {
            _delay.SetLength(args.Weapon, TimeSpan.FromSeconds(1f / weapon.AttackRate));
            _delay.TryResetDelay((args.Weapon, useDelay));
        }
    }
}
