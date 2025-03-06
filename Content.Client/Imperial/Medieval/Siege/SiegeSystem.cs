using Robust.Client.GameObjects;
using Content.Shared.Siege.Components;

namespace Content.Client.Imperial.Siege;

public sealed class SiegeSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SiegeWeaponComponent, AppearanceChangeEvent>(OnChangeAppearance);
    }

    public void OnChangeAppearance(EntityUid uid, SiegeWeaponComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null) return;
        args.Sprite.LayerSetState(CatapultVisualKey.Ready, component.AnimationState);
    }
}
