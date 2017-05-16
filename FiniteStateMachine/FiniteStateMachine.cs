using System;
using System.Collections.Generic;

namespace Deberdt.Yarp.Automata
{    
    public class FiniteStateMachine<TState, TInput> where TState : struct, IComparable, IFormattable, IConvertible
                                                    where TInput : struct, IComparable, IFormattable, IConvertible
    {
        private Dictionary<Tuple<TState, TInput>, StateTransition> _transitions;
        private TState? _state;
        private TState? _initialState;

        private class StateTransition 
        {
            private TState _currentState;
            private TState _nextState;
            private TInput _input;
            private FiniteStateMachine<TState, TInput> _stateMachine;

            public StateTransition(FiniteStateMachine<TState, TInput> stateMachine, TState state)
            {
                _currentState = state;
                _nextState = state;
                _stateMachine = stateMachine;
            }

            public TState CurrentState
            {
                get { return _currentState; }
                set
                {
                    if (!typeof(TState).IsEnum) throw new ArgumentException("T1 must be an enumerated type");
                    _currentState = value;
                }
            }

            public TState NextState
            {
                get { return _nextState; }
                set
                {
                    if (!typeof(TState).IsEnum) throw new ArgumentException("T1 must be an enumerated type");
                    _nextState = value;
                }
            }

            public TInput Input
            {
                get { return _input; }
                set
                {
                    if (!typeof(TInput).IsEnum) throw new ArgumentException("T2 must be an enumerated type");
                    _input = value;
                }
            }

            public Action StateTransitionAction
            {
                get;
                set;
            }

            public Func<bool> StateTransitionCondition
            {
                get;
                set;
            }
        }

        public class StateTransitionCollection
        {
            private FiniteStateMachine<TState, TInput> _stateMachine;
            private TState[] _states;
            private TInput[] _inputs;
            private List<Tuple<TState, TInput>> _keys = new List<Tuple<TState, TInput>>();

            public StateTransitionCollection(FiniteStateMachine<TState, TInput> stateMachine, params TState[] states)
            {
                _stateMachine = stateMachine;
                _states = states;           
            }

            public StateTransitionCollection On(params TInput[] inputs)
            {
                _inputs = inputs;

                foreach (TState state in _states)
                {
                    var stateTransition = new StateTransition(_stateMachine, state);
                    foreach (TInput input in inputs)
                    {
                        stateTransition.Input = input;

                        var dictionaryKey = new Tuple<TState, TInput>(stateTransition.CurrentState, input);
                        if (_stateMachine._transitions.ContainsKey(dictionaryKey)) throw new ApplicationException("Each state can have only one transition for a given symbol.");
                        else _stateMachine._transitions.Add(dictionaryKey, stateTransition);
                        _keys.Add(dictionaryKey);
                    }
                }
                return this;
            }

            public StateTransitionCollection OnAny()
            {
                var listOfInputs = new List<TInput>();
                foreach (TInput input in Enum.GetValues(typeof(TInput)))
                {
                    listOfInputs.Add(input);
                }
                return On(listOfInputs.ToArray());
            }

            public StateTransitionCollection Guard(Func<bool> condition)
            {
                foreach (var key in _keys)
                {
                    var stateTransition = _stateMachine._transitions[key];
                    stateTransition.StateTransitionCondition = condition;
                }
                return this;
            }

            public StateTransitionCollection Goto(TState nextState)
            {
                foreach (var key in _keys)
                {   
                    var stateTransition = _stateMachine._transitions[key];
                    stateTransition.NextState = nextState;
                }
                return this;
            }

            public StateTransitionCollection Execute(Action action)
            {
                foreach (var key in _keys)
                {
                    var stateTransition = _stateMachine._transitions[key];
                    stateTransition.StateTransitionAction = action;
                }
                return this;
            }

            public void Error(string errorMessage)
            {
                foreach (var key in _keys)
                {
                    var stateTransition = _stateMachine._transitions[key];
                    stateTransition.StateTransitionAction = delegate () { throw new InvalidOperationException(errorMessage); };
                }                
            }
        }

        public FiniteStateMachine()
        {
            _transitions = new Dictionary<Tuple<TState, TInput>, StateTransition>();
        }

        public StateTransitionCollection WhenIn(params TState[] states)
        {
            return new StateTransitionCollection(this, states);
        }

        public void Transition(TInput input)
        {
            if (!_state.HasValue) throw new InvalidOperationException("The object has not been initialized.");

            var dictionaryKey = new Tuple<TState, TInput>(_state.Value, input);
            if (_transitions.ContainsKey(dictionaryKey))
            {
                var stateTransition = _transitions[dictionaryKey];
                if (stateTransition.StateTransitionCondition == null || stateTransition.StateTransitionCondition())
                {
                    _state = stateTransition.NextState;
                    stateTransition.StateTransitionAction?.Invoke();
                } 
            }
            else
            {
                throw new InvalidOperationException("No transition for input!");
            }
        }

        public int Size()
        {
            return _transitions.Count;
        }

        public void Initialize(TState state)
        {
            _initialState = _state = state;
        }

        public void Reset()
        {
            if (!_initialState.HasValue) throw new InvalidOperationException("The object has not been initialized.");
            _state = _initialState;
        }

        public TState State
        {
            get
            {
                if (!_state.HasValue) throw new InvalidOperationException("The object has not been initialized.");
                return _state.Value;
            }
        }
    }
}
