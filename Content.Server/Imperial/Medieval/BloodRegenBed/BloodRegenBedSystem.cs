using Content.Server.Body.Systems;
using Content.Server.Imperial.Medieval.BloodRegenBed;
using Content.Shared.Buckle.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Server.Imperial.Medieval.BloodRegenBed
{
    public sealed class BloodRegenBedSystem : EntitySystem
    {
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BloodRegenBedComponent, StrappedEvent>(OnStrapped);
            SubscribeLocalEvent<BloodRegenBedComponent, UnstrappedEvent>(OnUnstrapped);
        }

        private void OnStrapped(Entity<BloodRegenBedComponent> bed, ref StrappedEvent args)
        {
            bed.Comp.NextRegenTime = _timing.CurTime + TimeSpan.FromSeconds(bed.Comp.RegenInterval);
        }

        private void OnUnstrapped(Entity<BloodRegenBedComponent> _bed, ref UnstrappedEvent _args)
        {
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<BloodRegenBedComponent, StrapComponent>();
            while (query.MoveNext(out var _, out var bloodRegen, out var strap))
            {
                if (_timing.CurTime < bloodRegen.NextRegenTime)
                    continue;

                bloodRegen.NextRegenTime += TimeSpan.FromSeconds(bloodRegen.RegenInterval);

                if (strap.BuckledEntities.Count == 0)
                    continue;

                foreach (var buckledEntity in strap.BuckledEntities)
                {
                    if (!HasComp<SleepingComponent>(buckledEntity) || _mobStateSystem.IsDead(buckledEntity))
                        continue;

                    if (_bloodstreamSystem.GetBloodLevelPercentage(buckledEntity) >= 1.0f)
                        continue;

                    Entity<SolutionComponent>? solutionEntity = null;
                    if (_solutionContainerSystem.ResolveSolution(buckledEntity, "bloodstream", ref solutionEntity, out var bloodSolution) && bloodSolution != null)
                    {
                        var currentVolume = bloodSolution.Volume;
                        var newVolume = bloodRegen.BloodRegenMultiplier;
                        var volumeToAdd = newVolume;
                        _bloodstreamSystem.TryModifyBloodLevel(buckledEntity, volumeToAdd);
                    }
                }
            }
        }
    }
}
