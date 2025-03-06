using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Siege.Events;

[NetSerializable, Serializable]
public sealed partial class SiegeChargeDoAfterArgs : SimpleDoAfterEvent { }

[NetSerializable, Serializable]
public sealed partial class SiegeShootDoAfterArgs : SimpleDoAfterEvent { }
