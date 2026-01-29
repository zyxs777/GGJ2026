namespace STool.SConditions.BlackboardAdapter
{
    public interface ISBlackboardBakeTarget
    {
        public IValueSource ValueSource { get; set; }

        /// <summary>
        /// 自行在此往目标黑板模板添加变量，使用Entry注册或索引
        /// </summary>
        public void DoBake(SBlackboardMono blackboardMono)
        {
            
        }
    }
}
