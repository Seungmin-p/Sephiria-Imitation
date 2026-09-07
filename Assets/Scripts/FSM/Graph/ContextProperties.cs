using UnityEngine;
using System;
using System.Collections.Generic;

namespace FSMGraph
{
    public enum BoolStateType
    {
        HasMoveInput,
        DashActionTrigger,
        IsGuardHeld,
        AttackActionTrigger,
        AttackMotionEnd,
        AttackWaitMotionEnd,
        StrikeAttackMotionEnd,
        DashAttackMotionEnd,
        IsThirdCombo,
        IsDashing,
        CanStrikeAttack
    }
    
    [Serializable]
    public class ContextProperties : ITransitionProperty
    {
        [SerializeReference]
        public List<ICondition> Conditions = new();

        public bool CanChangeState(Player owner)
        {
            //조건들 체크
            foreach (var condition in Conditions)
            {
                //안맞는 조건이 하나라도 있다면 false
                if (!condition.Evaluate(owner))
                    return false;
            }

            //조건에 전부 문제 없다면 통과
            return true;
        }
    }
    
    [Serializable]
    public class PlayerBoolCondition : ICondition
    {
        public BoolStateType StateType;
        public bool ExpectedValue;

        public bool Evaluate(Player owner)
        {
            bool actualValue = StateType switch
            {
                BoolStateType.HasMoveInput => owner.HasMoveInput,
                BoolStateType.DashActionTrigger => owner.DashActionTrigger,
                BoolStateType.IsGuardHeld => owner.IsGuardHeld,
                BoolStateType.AttackActionTrigger => owner.AttackActionTrigger,
                BoolStateType.AttackMotionEnd => owner.AttackMotionEnd,
                BoolStateType.AttackWaitMotionEnd => owner.AttackWaitMotionEnd,
                BoolStateType.StrikeAttackMotionEnd => owner.StrikeAttackMotionEnd,
                BoolStateType.DashAttackMotionEnd => owner.DashAttackMotionEnd,
                BoolStateType.IsThirdCombo => owner.IsThirdCombo,
                BoolStateType.IsDashing => owner.IsDashing,
                BoolStateType.CanStrikeAttack => owner.CanStrikeAttack,
                _ => false
            };

            return actualValue == ExpectedValue;
        }
    }
    
    [Serializable]
    public class CanExitDashCondition : ICondition
    {
        public bool Evaluate(Player owner)
        {
            return owner.DashFinishedCondition || owner.IsDashAttacking;
        }
    }
}