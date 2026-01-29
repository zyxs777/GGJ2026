using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace STool
{
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public class LightFSM<TState> where TState : notnull
    {
        public class State
        {
            public Action OnEnter;
            public Action OnExit;
            public Dictionary<TState, Func<bool>> Transitions = new();
        }

        private readonly Dictionary<TState, State> _states = new();
        [ShowInInspector,ReadOnly] private TState _currentStateKey;
        private State _currentState;
        private TState _lastStateKey;
        public IReadOnlyDictionary<TState, State> States => _states;
        public TState GetState() => _currentStateKey;
        public TState GetLastState() => _lastStateKey;
        public StateBuilder AddState(TState key)
        {
            var state = new State();
            _states[key] = state;
            return new StateBuilder(this, key, state);
        }
        public void Start(TState initialState)
        {
            if (!_states.TryGetValue(initialState, out _currentState)) return;
            _currentStateKey = initialState;
            _currentState.OnEnter?.Invoke();
        }
        public void Exit()
        {
            _currentState?.OnExit?.Invoke();
        }
        public void Update()
        {
            if (_currentState == null) return;
            foreach (var transition in _currentState.Transitions)
            {
                //UnityEngine.Debug.Log($"{transition.Key} : {transition.Value()} - {transition.Value.Invoke()}");
                if (!transition.Value()) continue;
                TransitionTo(transition.Key);
                break;
            }
        }
        public bool TryTransition(TState from, TState to)
        {
            if (!_currentStateKey.Equals(from)) return false;
            return TryTransition(to);
        }
        public bool TryTransition(TState state)
        {
            if (_currentState == null) return false;
            if (!_currentState.Transitions.TryGetValue(state, out var func)) return false;
            if (!func()) return false;

            TransitionTo(state);
            return true;
        }
        private void TransitionTo(TState newStateKey)
        {
            if (!_states.TryGetValue(newStateKey, out var newState)) return;
            //UnityEngine.Debug.Log($"SC:\t{currentStateKey} => {newStateKey}");
            _currentState?.OnExit?.DynamicInvoke();
            _lastStateKey = _currentStateKey;
            _currentStateKey = newStateKey;
            _currentState = newState;
            _currentState?.OnEnter?.DynamicInvoke();
        }
        public class StateBuilder
        {
            private readonly LightFSM<TState> fsm;
            private readonly TState key;
            private readonly State state;

            public StateBuilder(LightFSM<TState> fsm, TState key, State state)
            {
                this.fsm = fsm;
                this.key = key;
                this.state = state;
            }
            public StateBuilder OnEnter(Action action)
            {
                state.OnEnter = action;
                return this;
            }
            public StateBuilder OnExit(Action action)
            {
                state.OnExit = action;
                return this;
            }
            public StateBuilder AllowTransitionTo(TState targetState, Func<bool> condition)
            {
                state.Transitions[targetState] = condition;
                return this;
            }
            public StateBuilder AddState(TState key) => fsm.AddState(key);
        }


        public void Reset()
        {
            _states.Clear();
            _currentState = null;
            _currentStateKey = default;
        }
        public static bool AlwaysTrue() => true;
    }
}

