namespace FSM
{
    public abstract class State<T> : IState
    {
        protected T owner; //상태 패턴 적용 대상
        protected StateMachine<T> stateMachine; //상태 머신

        public State(T owner, StateMachine<T> stateMachine)
        {
            this.owner = owner;
            this.stateMachine = stateMachine;
        }

        public virtual void OnEnter() {}
        public virtual void OnFixedUpdate() {}
        public virtual void OnUpdate() {}
        public virtual void OnExit() {}
    }
   
}