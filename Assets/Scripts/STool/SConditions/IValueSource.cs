// STool.Conditions (Runtime)
namespace STool.SConditions
{
    public interface IValueSource
    {
        bool TryGet<T>(string id, out T value);
        void Set<T>(string id, T value);
        void Create<T>(string id, T value);
        virtual void BakeCondition(ICondition iCondition){}
    }

    public interface IValueSourceHolder
    {
        public IValueSource ValueSource { get; set; }
    }
}