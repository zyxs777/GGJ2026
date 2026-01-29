namespace STool.SInterfaces
{
    public interface ITimeSource
    {
        float TimeDelta { get; set; }
        float TimeScale { get; set; }
    }
        
    public interface ITimeUpdater
    {
        void TimeUpdate(float deltaTime);
    }

    /// <summary>
    /// 对应Mono的OnEnable OnDisable，用于开关重置某些功能
    /// </summary>
    public interface IActivator
    {
        virtual void DoOnEnable(){}
        virtual void DoOnDisable(){}
    }

    public interface ITimeScaler
    {
        void SetTimeScale(float timeScale);
    }
    
}
