using System.Text;

namespace FirstOrderLogic.Planning.GraphPlan;

public class Layer {
    public readonly int Level;
    public readonly List<StateNode> StateNodes;
    public readonly List<ActionNode> ActionNodes;

    public Layer(int level) {
        Level = level;
        StateNodes = new List<StateNode>();
        ActionNodes = new List<ActionNode>();
    }

    public void TryAdd(Node node) {
        switch (node) {
            case StateNode stateNode: {
                var contained = StateNodes.FirstOrDefault(node.Equals);
                if (contained != null) {
                    MergeRelations(node, contained);
                    return;
                }

                StateNodes.Add(stateNode);
                break;
            }
            case ActionNode actionNode: {
                var contained = ActionNodes.FirstOrDefault(node.Equals);
                if (contained != null) {
                    MergeRelations(node, contained);
                    return;
                }

                ActionNodes.Add(actionNode);
                break;
            }
        }
    }

    private void MergeRelations(Node mergeFrom, Node mergeTo) {
        foreach (var inNode in mergeFrom.InEdges) {
            mergeTo.InEdges.Add(inNode);
        }

        foreach (var outNode in mergeFrom.OutEdges) {
            mergeTo.OutEdges.Add(outNode);
        }

        foreach (var mutexNode in mergeFrom.MutexRelation) {
            mergeTo.MutexRelation.Add(mutexNode);
        }
    }

    public bool EqualStateLiterals(Layer other) {
        var aSubsetB = StateNodes.All(a => other.StateNodes.Any(a.EqualLiteral));
        var bSubsetA = other.StateNodes.All(b => StateNodes.Any(b.EqualLiteral));
        return aSubsetB && bSubsetA;
    }

    public void ExpandActionNodes(List<Action> actions) {
        foreach (var action in actions) {
            if (!action.IsApplicable(StateNodes, out var satisfiedPreconditions)) {
                continue;
            }

            var actionNode = new ActionNode(Level, action, false);
            satisfiedPreconditions.ForEach(preCon => preCon.ConnectTo(actionNode));
            TryAdd(actionNode);
        }

        foreach (var stateNode in StateNodes) {
            var action = new Action("Persist", new List<ISentence> { stateNode.Literal }, new List<ISentence> { stateNode.Literal });
            var actionNode = new ActionNode(Level, action, true);
            stateNode.ConnectTo(actionNode);
            TryAdd(actionNode);
        }

        CheckMutexRelations(ActionNodes.Select(n => (Node)n).ToList());
    }

    public Layer ExpandNextLayer() {
        var newIndex = Level + 1;
        var newLayer = new Layer(newIndex);
        foreach (var actionNode in ActionNodes) {
            foreach (var effect in actionNode.Action.Effects) {
                var effectNode = new StateNode(newIndex, effect);
                actionNode.ConnectTo(effectNode);
                newLayer.TryAdd(effectNode);
            }
        }

        CheckMutexRelations(newLayer.StateNodes.Select(n => (Node)n).ToList());
        return newLayer;
    }

    private void CheckMutexRelations(List<Node> nodes) {
        for (var i = 0; i < nodes.Count; i++) {
            for (var j = i + 1; j < nodes.Count; j++) {
                var nodeA = nodes[i];
                var nodeB = nodes[j];
                if (!nodeA.Equals(nodeB) && nodeA.IsMutex(nodeB)) {
                    nodeA.TryAddMutexRelations(nodeB);
                }
            }
        }
    }

    public override string ToString() {
        var sb = new StringBuilder();
        sb.AppendLine($"Layer {Level}\n\t");
        foreach (var stateNode in StateNodes) {
            sb.AppendLine(stateNode.ToString());
        }
        sb.AppendLine("\n\t");
        foreach (var actionNode in ActionNodes) {
            sb.AppendLine(actionNode.ToString());
        }
        return sb.ToString();
    }
}