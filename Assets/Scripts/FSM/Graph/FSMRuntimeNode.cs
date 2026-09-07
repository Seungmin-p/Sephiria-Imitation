using System;
using UnityEngine;

namespace FSMGraph
{
    [Serializable]
    public abstract class FSMRuntimeNode
    {
        public string Id;
    }

    [Serializable]
    public class FSMRuntimeStateNode : FSMRuntimeNode
    {
        public string StateType;
        public bool IsStartState;
    }

    [Serializable]
    public class FSMRuntimeTransitionNode : FSMRuntimeNode
    {
        [SerializeReference]
        public ContextProperties ContextProperties;

        public ITransitionProperty Properties => ContextProperties;
    }

    [Serializable]
    public class FSMRuntimeStartNode : FSMRuntimeNode
    {
    }

    [Serializable]
    public class FSMRuntimeConnection
    {
        public string FromNodeId;
        public string ToNodeId;
    }
}
