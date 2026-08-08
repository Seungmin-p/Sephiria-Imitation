using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FSM;
using FSM.PlayerStates;

namespace FSMGraph
{
    public class FSMRuntimeGraph : ScriptableObject
    {
        [SerializeReference]
        public List<FSMRuntimeNode> Nodes = new();
        public List<FSMRuntimeConnection> Connections = new();

        public StateMachine<Player> CreateStateMachine(Player owner)
        {
            if (owner == null)
            {
                Debug.LogError("FSMRuntimeGraph: Owner is null.");
                return null;
            }

            var fsm = new StateMachine<Player>();
            var stateById = new Dictionary<string, State<Player>>();

            var runtimeNodeById = Nodes
                .Where(node => !string.IsNullOrEmpty(node.Id))
                .ToDictionary(node => node.Id, node => node);

            var stateNodeById = Nodes
                .OfType<FSMRuntimeStateNode>()
                .Where(node => !string.IsNullOrEmpty(node.Id))
                .ToDictionary(node => node.Id, node => node);
            
            foreach (var pair in stateNodeById)
            {
                var state = CreateState(owner, fsm, pair.Value.StateType);

                if (state != null)
                    stateById[pair.Key] = state;
            }

            var transitionToTargetState = new Dictionary<string, string>();
            foreach (var connection in Connections)
            {
                if (!runtimeNodeById.TryGetValue(connection.FromNodeId, out var fromNode) ||
                    !runtimeNodeById.TryGetValue(connection.ToNodeId, out var toNode))
                {
                    continue;
                }

                if (fromNode is FSMRuntimeTransitionNode transition && toNode is FSMRuntimeStateNode targetState)
                {
                    transitionToTargetState[transition.Id] = targetState.Id;
                }
            }

            var transitionPairsByStateId = new Dictionary<string, List<TransitionStatePair>>();
            foreach (var connection in Connections)
            {
                if (!runtimeNodeById.TryGetValue(connection.FromNodeId, out var fromNode) ||
                    !runtimeNodeById.TryGetValue(connection.ToNodeId, out var toNode))
                {
                    continue;
                }

                if (fromNode is FSMRuntimeStateNode state && toNode is FSMRuntimeTransitionNode transition)
                {
                    if (!transitionToTargetState.TryGetValue(transition.Id, out var targetStateId))
                        continue;

                    if (!transitionPairsByStateId.TryGetValue(state.Id, out var list))
                    {
                        list = new List<TransitionStatePair>();
                        transitionPairsByStateId[state.Id] = list;
                    }

                    if (!stateById.TryGetValue(targetStateId, out var targetState))
                        continue;

                    list.Add(new TransitionStatePair
                    {
                        Properties = transition.Properties,
                        NextState = targetState,
                    });
                }
            }
            
            foreach (var pair in stateById)
            {
                var transitions = transitionPairsByStateId.TryGetValue(pair.Key, out var list)
                    ? list
                    : new List<TransitionStatePair>();

                if (pair.Value is PlayerMovementStateBase movementState)
                    movementState.SetTransitions(transitions);
                else if (pair.Value is PlayerActionStateBase actionState)
                    actionState.SetTransitions(transitions);
            }

            var startState = FindStartState(runtimeNodeById, stateNodeById);
            if (startState == null)
            {
                Debug.LogError("FSMRuntimeGraph: No state nodes were found in the runtime graph.");
                return null;
            }

            if (stateById.TryGetValue(startState.Id, out var initialState))
                fsm.ChangeState(initialState);
            else
                Debug.LogError($"FSMRuntimeGraph: Initial state instance was not found for node id '{startState.Id}'.");

            return fsm;
        }

        private State<Player> CreateState(Player owner, StateMachine<Player> fsm, string stateType)
        {
            //각종 상태 생성
            switch (stateType)
            {
                case "PlayerIdleState":
                    return new PlayerIdleState(owner, fsm);

                case "PlayerMoveState":
                    return new PlayerMoveState(owner, fsm);

                case "PlayerDashState":
                    return new PlayerDashState(owner, fsm);

                case "PlayerNoneState":
                    return new PlayerNoneState(owner, fsm);

                case "PlayerGuardState":
                    return new PlayerGuardState(owner, fsm);

                case "PlayerAttackState":
                    return new PlayerAttackState(owner, fsm);

                case "PlayerComboWaitState":
                    return new PlayerComboWaitState(owner, fsm);

                case "PlayerStrikeAttackState":
                    return new PlayerStrikeAttackState(owner, fsm);

                case "PlayerDashAttackState":
                    return new PlayerDashAttackState(owner, fsm);

                case "PlayerDownState":
                    return new PlayerDownState(owner, fsm);

                default:
                    Debug.LogError($"FSMRuntimeGraph: Unknown state type '{stateType}'.");
                    return null;
            }
        }

        private FSMRuntimeStateNode FindStartState(
            Dictionary<string, FSMRuntimeNode> runtimeNodeById,
            Dictionary<string, FSMRuntimeStateNode> stateNodeById)
        {
            var startConnection = Connections
                .FirstOrDefault(c => runtimeNodeById.TryGetValue(c.FromNodeId, out var fromNode) && fromNode is FSMRuntimeStartNode &&
                                     runtimeNodeById.TryGetValue(c.ToNodeId, out var toNode) && toNode is FSMRuntimeStateNode);

            if (startConnection != null)
                return stateNodeById[startConnection.ToNodeId];

            return stateNodeById.Values.FirstOrDefault(node => node.IsStartState) ?? stateNodeById.Values.FirstOrDefault();
        }
    }
}