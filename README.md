# FiniteStateMachine
A Finite State Machine generalization

## Installation
Either clone the repo from GitHub or download the NuGet package (look for FiniteStateMachine - package name *Deberdt.Yarp.FiniteStateMachine*)

## Table of Contents
- [Quickstart](#quickstart)

## Quickstart
Using the FSM is pretty straightforward. States and inputs are defined as enums. State transitions have provisions for guard conditions and optional actions, to be performed when transitioning from one state to another. 

Below is a simple example (borrowed from SimpleExpressionCalculator, which can be found in its entirety on GitHub)

```c#

    public enum State
    {
        Start,
        InInteger,
        Integer,
        InOperator,
        Operator,
        EndOfExpression
    }

    public enum Input
    {
        Digit,
        Operator,
        EndOfExpression
    }

    class ExpressionScanner
    {
        private string _expression;
        private FiniteStateMachine<State, Input> _sm = new FiniteStateMachine<State, Input>();

        public ExpressionScanner(string expression)
        {
            _expression = expression;

            _sm.Initialize(State.Start);

            _sm.WhenIn(State.Start).On(Input.EndOfExpression).Goto(State.EndOfExpression); // End-state
            _sm.WhenIn(State.Start).On(Input.Digit).Goto(State.InInteger);
            _sm.WhenIn(State.InInteger).On(Input.Digit);
            _sm.WhenIn(State.InInteger).On(Input.Operator, Input.EndOfExpression).Goto(State.Integer); // End-state
            _sm.WhenIn(State.Start).On(Input.Operator).Goto(State.InOperator);
            _sm.WhenIn(State.InOperator).On(Input.Digit, Input.EndOfExpression).Goto(State.Operator); // End-state
        }
        
        private Input GetCharacterCategory(char c)
        {
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (Char.IsDigit(c)) return Input.Digit;
            if (c == '+' || c == '-' || c == '/' || c == '*') return Input.Operator;
            else if (c == '\n') return Input.EndOfExpression;

            throw new InvalidInputException(c, _position);
        }        
        
        // Rest of the class omitted for brevity
    }
```

