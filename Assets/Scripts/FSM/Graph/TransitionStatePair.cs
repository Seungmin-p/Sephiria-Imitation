using System;
using FSM;

namespace FSMGraph
{
    [Serializable]
    public class TransitionStatePair
    {
        public ITransitionProperty Properties;
        public State<Player> NextState;
    }
}
