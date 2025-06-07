namespace Content.Server.Imperial.Medieval.BloodRegenBed
{
    [RegisterComponent]
    public sealed partial class BloodRegenBedComponent : Component
    {
        [DataField("bloodRegenMultiplier", required: true)]
        public float BloodRegenMultiplier = 1.0f; // Множитель по умолчанию 1.1x

        [DataField("regenInterval")]
        public float RegenInterval = 5.0f; // Регенерация каждые 5 секунд

        public TimeSpan NextRegenTime = TimeSpan.FromSeconds(0); // Время следующей регенерации
    }
}
