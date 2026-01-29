using System;

namespace STool.STypeEventBus
{
    public interface ITypeEvent { }

    public interface IQueryEvent { }
    public interface IQueryParam { }

    #region Interface for TypeEventBus
    //For EventBus
    public interface ITypeBusSubscriber
    {
        public IDisposable Subscribe<T>(Action<T> handler);
        public void Unsubscribe<T>(Action<T> handler);
    }
    public interface ITypeBusPublisher
    {
        public void Publish<T>(T message);
    }
    
    
    //For Components
    public interface ITypeBusSubscribeRegister
    {
        protected ITypeBusSubscriber Subscriber { get; }
        public void Init(ITypeBusSubscriber subscriber);
    }
    public interface ITypeBusPublishRegister
    {
        protected ITypeBusPublisher Publisher { get; }
        public void Init(ITypeBusPublisher publisher);
    }
    
    #endregion

    
}
