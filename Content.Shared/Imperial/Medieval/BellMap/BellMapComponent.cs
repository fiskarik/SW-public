
using Robust.Shared.Audio;

namespace Content.Shared.Imperial.BellMap.Components;

[RegisterComponent]
public sealed partial class BellMapComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Sound { get; set; } = "/Audio/Announcements/attention.ogg";

    public AudioParams Params = AudioParams.Default.WithVolume(5f);
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Locale = "bell-map-popup";
}
