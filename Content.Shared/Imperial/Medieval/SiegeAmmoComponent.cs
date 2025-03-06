using Robust.Shared.Audio;

namespace Content.Shared.Siege.Components;

[RegisterComponent]
public sealed partial class SiegeAmmoComponent : Component
{
    [DataField]
    public string AmmoType = "";
}
