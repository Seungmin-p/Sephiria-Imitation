using System.Collections.Generic;
using FSMGraph;

namespace FSM.PlayerStates
{
    public abstract class PlayerMovementStateBase : State<Player>
    {
        protected List<TransitionStatePair> transitions = new();

        protected PlayerMovementStateBase(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine) {}

        public void SetTransitions(List<TransitionStatePair> transitions)
        {
            this.transitions = transitions;
        }
        
        protected bool CheckTransitions()
        {
            foreach (var transition in transitions)
            {
                if (!transition.Properties.CanChangeState(owner))
                    continue;

                stateMachine.ChangeState(transition.NextState);
                return true;
            }

            return false;
        }
    }

    public abstract class PlayerActionStateBase : State<Player>
    {
        protected List<TransitionStatePair> transitions = new();

        protected PlayerActionStateBase(Player owner, StateMachine<Player> stateMachine) : base(owner, stateMachine) {}

        public void SetTransitions(List<TransitionStatePair> transitions)
        {
            this.transitions = transitions;
        }
        
        protected bool CheckTransitions()
        {
            foreach (var transition in transitions)
            {
                if (!transition.Properties.CanChangeState(owner))
                    continue;

                stateMachine.ChangeState(transition.NextState);
                return true;
            }

            return false;
        }
    }
}