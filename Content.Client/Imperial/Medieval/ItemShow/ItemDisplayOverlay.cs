using System.Numerics;
using Content.Shared.Imperial.Medieval.ItemShow;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.Imperial.Medieval.ItemShow;

public sealed class ItemDisplayOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private readonly EntityLookupSystem _entityLookupSystem;
    private readonly SpriteSystem _spriteSystem;

    private ShaderInstance _shader;

    public ItemDisplayOverlay()
    {
        IoCManager.InjectDependencies(this);

        _spriteSystem = _entityManager.System<SpriteSystem>();
        _entityLookupSystem = _entityManager.System<EntityLookupSystem>();

        _shader = _prototypeManager.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var localPlayer = _playerManager.LocalEntity;

        if (!localPlayer.HasValue)
        {
            return;
        }

        var query = _entityManager.EntityQueryEnumerator<ItemDisplayComponent, TransformComponent, SpriteComponent>();

        while (query.MoveNext(out var uid, out var itemShowComponent, out  var xformComponent, out var spriteComponent))
        {
            if (xformComponent.MapID != args.MapId ||
                !spriteComponent.Visible ||
                !_entityManager.TryGetComponent<SpriteComponent>(itemShowComponent.ItemUid,
                    out var itemSpriteComponent))
            {
                continue;
            }

            var currentZoom = _eyeManager.CurrentEye.Scale;


            var aabb = _entityLookupSystem.GetWorldAABB(uid);

            var screenCoordinates = _eyeManager.WorldToScreen(aabb.Center +
                                                              new Angle(-_eyeManager.CurrentEye.Rotation).RotateVec(
                                                                  aabb.TopRight - aabb.Center));

            _spriteSystem.ForceUpdate(itemShowComponent.ItemUid);

            args.ScreenHandle.UseShader(_shader);

            args.ScreenHandle.DrawEntity(itemShowComponent.ItemUid, screenCoordinates, currentZoom, Angle.Zero, Angle.Zero, Direction.South);

            args.ScreenHandle.UseShader(null);
        }
    }
}
