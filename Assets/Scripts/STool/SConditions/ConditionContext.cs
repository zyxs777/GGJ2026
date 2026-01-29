namespace STool.SConditions
{
    public readonly struct ConditionContext
    {
        public readonly IValueSource Source;
        public readonly float Now;

        public ConditionContext(IValueSource source, float now)
        {
            Source = source;
            Now = now;
        }
    }
}