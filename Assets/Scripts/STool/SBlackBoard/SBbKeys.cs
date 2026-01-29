#nullable enable
namespace STool.SBlackBoard
{
    public static class SBBKeys
    {
        //Unit Usage
        public static readonly BBKey<float> Health = new("Health", 100f);
        public static readonly BBKey<float> MaxHealth = new("MaxHealth", 100f);
        public static readonly BBKey<bool> IsDead = new("IsDead", false);

        //Level Global
        public static readonly BBKey<object?> Player = new("Player", null);
    }
    
    public sealed class Ref<T>
    {
        public T Value;
        public Ref(T value) => Value = value;
    }
}
