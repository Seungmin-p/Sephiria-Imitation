using System;
using Unity.GraphToolkit.Editor;

namespace FSMGraph
{
    [Serializable]
    public class FSMContextNode : ContextNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("In").Build();
            context.AddOutputPort("Out").Build();
        }
    }

    //움직임 입력
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class HasMoveInputCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("HasMoveInput")
                .WithDisplayName("Has Move Input")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("HasMoveInput").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.HasMoveInput,
                ExpectedValue = value
            };
        }
    }

    //대시 입력
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class DashActionTriggerCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("DashActionTrigger")
                .WithDisplayName("Dash Action Trigger")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("DashActionTrigger").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.DashActionTrigger,
                ExpectedValue = value
            };
        }
    }

    //가드 입력
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class IsGuardHeldCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("IsGuardHeld")
                .WithDisplayName("Is Guard Held")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsGuardHeld").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.IsGuardHeld,
                ExpectedValue = value
            };
        }
    }

    //공격 입력
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class AttackActionTriggerCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("AttackActionTrigger")
                .WithDisplayName("Attack Action Trigger")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("AttackActionTrigger").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.AttackActionTrigger,
                ExpectedValue = value
            };
        }
    }

    //공격 모션 종료
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class AttackMotionEndCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("AttackMotionEnd")
                .WithDisplayName("Attack Motion End")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("AttackMotionEnd").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.AttackMotionEnd,
                ExpectedValue = value
            };
        }
    }

    //공격 대기 모션 종료
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class AttackWaitMotionEndCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("AttackWaitMotionEnd")
                .WithDisplayName("Attack Wait Motion End")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("AttackWaitMotionEnd").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.AttackWaitMotionEnd,
                ExpectedValue = value
            };
        }
    }

    //특수 공격 모션 종료
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class StrikeAttackMotionEndCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("StrikeAttackMotionEnd")
                .WithDisplayName("Strike Attack Motion End")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("StrikeAttackMotionEnd").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.StrikeAttackMotionEnd,
                ExpectedValue = value
            };
        }
    }

    //대시 공격 모션 종료
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class DashAttackMotionEndCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("DashAttackMotionEnd")
                .WithDisplayName("Dash Attack Motion End")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("DashAttackMotionEnd").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.DashAttackMotionEnd,
                ExpectedValue = value
            };
        }
    }

    //3타 공격
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class IsThirdComboCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("IsThirdCombo")
                .WithDisplayName("Is Third Combo")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsThirdCombo").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.IsThirdCombo,
                ExpectedValue = value
            };
        }
    }

    //대시 상태 여부
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class IsDashingCheck : BlockNode, IConditionBlockNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<bool>("IsDashing")
                .WithDisplayName("Is Dashing")
                .WithDefaultValue(true)
                .Build();
        }

        public ICondition CreateRuntimeCondition()
        {
            GetNodeOptionByName("IsDashing").TryGetValue(out bool value);

            return new PlayerBoolCondition
            {
                StateType = BoolStateType.IsDashing,
                ExpectedValue = value
            };
        }
    }

    //대시 종료
    //대시 시간 종료 or 대시 공격 진행
    [Serializable]
    [UseWithContext(typeof(FSMContextNode))]
    public class CanExitDashCheck : BlockNode, IConditionBlockNode
    {
        public ICondition CreateRuntimeCondition()
        {
            return new CanExitDashCondition();
        }
    }
}