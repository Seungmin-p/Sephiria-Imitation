using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace FSMGraph
{
    [ScriptedImporter(1, FSMGraph.AssetExtension)]
    internal class FSMImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<FSMGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"Failed to load FSM graph asset: {ctx.assetPath}");
                return;
            }

            var runtimeAsset = ScriptableObject.CreateInstance<FSMRuntimeGraph>();
            var nodeMap = new Dictionary<INode, FSMRuntimeNode>();

            foreach (var node in graph.GetNodes())
            {
                FSMRuntimeNode runtimeNode = node switch
                {
                    StateNode stateNode => new FSMRuntimeStateNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        StateType = TryGetOptionValue<string>(stateNode, StateNode.StateNameOptionName),
                    },
                    FSMContextNode contextNode => new FSMRuntimeTransitionNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        ContextProperties = CreateContextProperties(contextNode)
                    },
                    StartNode _ => new FSMRuntimeStartNode { Id = Guid.NewGuid().ToString() },
                    _ => null,
                };

                if (runtimeNode != null)
                {
                    runtimeAsset.Nodes.Add(runtimeNode);
                    nodeMap[node] = runtimeNode;
                }
            }
            
            static ContextProperties CreateContextProperties(FSMContextNode contextNode)
            {
                var props = new ContextProperties();

                foreach (var block in contextNode.blockNodes)
                {
                    if (block is IConditionBlockNode conditionBlock)
                    {
                        props.Conditions.Add(conditionBlock.CreateRuntimeCondition());
                    }
                }
                return props;
            }

            static T TryGetOptionValue<T>(INode node, string optionName)
            {
                var stateNode = node as StateNode;
                var option = stateNode.GetNodeOptionByName(optionName);
                if (option.TryGetValue(out T value))
                    return value;
                return default;
            }

            foreach (var node in graph.GetNodes())
            {
                var outputPorts = node.GetOutputPorts();
                if (outputPorts == null) continue;

                foreach (var port in outputPorts)
                {
                    var connectedPorts = new List<IPort>();
                    port.GetConnectedPorts(connectedPorts);

                    foreach (var connectedPort in connectedPorts)
                    {
                        var toNode = connectedPort.GetNode();
                        if (nodeMap.TryGetValue(node, out var fromRuntimeNode) && nodeMap.TryGetValue(toNode, out var toRuntimeNode))
                        {
                            runtimeAsset.Connections.Add(new FSMRuntimeConnection
                            {
                                FromNodeId = fromRuntimeNode.Id,
                                ToNodeId = toRuntimeNode.Id,
                            });
                        }
                    }
                }
            }

            ctx.AddObjectToAsset("RuntimeAsset", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);
        }
    }
}