using STool.SBlackBoard;
using STool.SBlackBoard.Unity;

namespace STool.SConditions.BlackboardAdapter
{
    public sealed class SBlackboardValueSource : IValueSource
    {
        private IBlackboard _bb;
        public void SetBlackboard(IBlackboard blackboard) => _bb = blackboard;
        public bool TryGet<T>(string key, out T value)
        {
            var bbKey = BBKeyShare.GetSharedKey<T>(key);
            if (_bb.TryGet(bbKey, out value)) return true;
            
            value = default!;
            return false;
        }
        
        public void Set<T>(string id, T value)
        {
            var bbKey = BBKeyShare.GetSharedKey<T>(id);
            _bb.Set(bbKey, value);
        }

        public void Create<T>(string id, T value)
        {
            
        }

        //Template & init src
        private SBlackboardDefinition _sBlackboardDefinition;

        //Editor support
        public void SetDefinition(SBlackboardDefinition definition) => _sBlackboardDefinition = definition;
        public void BakeCondition(ICondition iCondition) => _sBlackboardDefinition?.EmbedBlackBoardValueSource(iCondition);
    }
}