using UnityEngine;

namespace FSM.PlayerStates
{
    public class PlayerDeathState : PlayerActionStateBase
    {
        private enum DeathPhase
        {
            Airborne,
            Down,
            GameOver
        }

        private readonly PlayerDeath playerDeath;

        private DeathPhase deathPhase;
        private float gameOverTimer;

        public PlayerDeathState(Player owner, StateMachine<Player> stateMachine, PlayerDeath playerDeath) : base(owner, stateMachine)
        {
            this.playerDeath = playerDeath;
        }

        public override void OnEnter()
        {
            //처음은 에어본으로 시작
            deathPhase = DeathPhase.Airborne;
            gameOverTimer = 0f;

            //현재 공격 상태 초기화
            owner.ExecuteAttackEnd();

            //사망 처리 시작
            playerDeath.StartDeath();
        }

        public override void OnUpdate()
        {
            //다운 상태가 아니면 패스
            if (deathPhase != DeathPhase.Down)
                return;

            gameOverTimer += Time.deltaTime;

            //게임 오버 대기시간 전까지 패스
            if (gameOverTimer < playerDeath.GameOverDelay)
                return;

            //다운 이후 게임오버 대기시간이 지나면 게임오버 처리
            deathPhase = DeathPhase.GameOver;
            playerDeath.GameOver();
        }

        public override void OnFixedUpdate()
        {
            //에어본 상태가 아니면 패스
            if (deathPhase != DeathPhase.Airborne)
                return;

            //에어본이 끝날 때 까지 진행
            if (!playerDeath.ExecuteAirborne())
                return;

            //에어본이 끝나면 착지 상태로 전환
            deathPhase = DeathPhase.Down;
            playerDeath.StartDown();
        }

        public override void OnExit()
        {
        }
    }
}