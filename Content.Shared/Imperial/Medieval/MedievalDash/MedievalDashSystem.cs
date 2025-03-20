using System.Linq;
using System.Numerics;
using Content.Shared.Damage.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Imperial.PhaseSpace;
using Content.Shared.Input;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.Imperial.Dash;


public sealed partial class MedievalDashSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;


    private Dictionary<EntityUid, TimeSpan> _dashedEntities = new();


    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.MedievalDash, new PointerInputCmdHandler(DashButtonPressed))
            .Register<MedievalDashSystem>();

        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (performer, dashTime) in _dashedEntities.ToDictionary())
        {
            if (_timing.CurTime < dashTime) continue;

            RemComp<PhaseSpaceShadowComponent>(performer);

            _dashedEntities.Remove(performer);
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();

        Cleanup();
    }


    private bool DashButtonPressed(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
    {
        if (playerSession?.AttachedEntity is not { Valid: true } player || !Exists(player)) return false;

        if (!TryComp<MedievalDashComponent>(player, out var component)) return false;
        if (!TryComp<PhysicsComponent>(player, out var physicsComponent)) return false;
        if (!TryComp<InputMoverComponent>(player, out var inputMoverComponent)) return false;

        if (physicsComponent.LinearVelocity == Vector2.Zero) return false;
        if ((inputMoverComponent.HeldMoveButtons & MoveButtons.AnyDirection) == 0) return false;

        var targetRotation = physicsComponent.LinearVelocity.ToAngle();

        var force = new Vector2(component.Force);
        var forceDirection = targetRotation - Angle.FromDegrees(45);

        var impulse = forceDirection.RotateVec(force);
        var dashTime = TimeSpan.FromSeconds(component.Force / 990 / physicsComponent.Mass) + _timing.CurTime;

        if (!_staminaSystem.TryTakeStamina(player, component.StaminaDamage)) return false;

        _physicsSystem.ApplyLinearImpulse(player, impulse);

        var shadowComponent = EnsureComp<PhaseSpaceShadowComponent>(player);

        shadowComponent.ShadowUpdateRate = TimeSpan.FromSeconds(0);
        shadowComponent.PositionUpdateRate = TimeSpan.FromSeconds(0);

        if (!_dashedEntities.TryAdd(player, dashTime))
            _dashedEntities[player] = dashTime;

        return false;
    }

    private void OnRoundEnd(RoundEndMessageEvent args)
    {
        Cleanup();
    }

    #region Helpers

    private void Cleanup()
    {
        _dashedEntities = new();
    }

    #endregion
}
