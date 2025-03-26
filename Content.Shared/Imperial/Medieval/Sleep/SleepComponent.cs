namespace Content.Shared.Bed.Sleep;

[RegisterComponent]
public sealed partial class SleepingImperialComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan Timing = TimeSpan.FromSeconds(15);
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan CurTime = TimeSpan.Zero;
    [ViewVariables(VVAccess.ReadWrite)]
    public Color Color = Color.DarkRed;
    [ViewVariables(VVAccess.ReadWrite)]
    public List<string> Words { get; private set; } = new();
}
