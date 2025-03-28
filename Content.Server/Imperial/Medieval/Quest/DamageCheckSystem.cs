using Content.Server.Quest.Components;
using Content.Shared.Speech;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Server.Storage.Components;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Damage;
using Content.Server.Chat.Systems;

namespace Content.Server.DamageCheck;
public partial class DamageCheckSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageCheckableComponent, ExaminedEvent>(OnExamine);

    }

    private void OnExamine(EntityUid uid, DamageCheckableComponent comp, ExaminedEvent args)
    {
        // Пока хардкод только под состояния для ворот, это насрано, так делать не надо, но до теста меньше 12ч поэтому норм
        if (!TryComp<DamageableComponent>(uid, out var damageable)) return;
        if (damageable.TotalDamage > 3600)
            args.PushMarkup("[color=red]Объект покрыт крупными трещинами и вот-вот развалится[/color]");
        else if (damageable.TotalDamage > 2700)
            args.PushMarkup("[color=orange]Объект покрыт крупными трещинами[/color]");
        else if (damageable.TotalDamage > 1800)
            args.PushMarkup("[color=orange]По объекту расходятся трещины[/color]");
        else if (damageable.TotalDamage > 900)
            args.PushMarkup("[color=yellow]Заметны серьезные царапины[/color]");
        else if (damageable.TotalDamage > 220)
            args.PushMarkup("[color=green]Заметны легкие царапины[/color]");

    }

}
