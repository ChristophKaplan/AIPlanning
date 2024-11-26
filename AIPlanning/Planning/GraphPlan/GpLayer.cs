using System.Text;

namespace FirstOrderLogic.Planning.GraphPlan;

public class GpLayer(int level) {
    public readonly int Level = level;
    public readonly List<GpStateNode> StateNodes = new();
    public readonly List<GpActionNode> ActionNodes = new();

    public void TryAdd(GpNode gpNode) {
        switch (gpNode) {
            case GpStateNode stateNode: {
                var contained = StateNodes.FirstOrDefault(gpNode.Equals);
                if (contained != null) {
                    MergeRelations(gpNode, contained);
                    return;
                }

                StateNodes.Add(stateNode);
                break;
            }
            case GpActionNode actionNode: {
                var contained = ActionNodes.FirstOrDefault(gpNode.Equals);
                if (contained != null) {
                    MergeRelations(gpNode, contained);
                    return;
                }

                ActionNodes.Add(actionNode);
                break;
            }
        }
    }

    private void MergeRelations(GpNode mergeFrom, GpNode mergeTo) {
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

    public bool EqualStateLiterals(GpLayer other) {
        var aSubsetB = StateNodes.All(a => other.StateNodes.Any(a.EqualLiteral));
        var bSubsetA = other.StateNodes.All(b => StateNodes.Any(b.EqualLiteral));
        return aSubsetB && bSubsetA;
    }

    public void ExpandActionNodes(List<GpAction> actions) {
        foreach (var action in actions) {
            if (!action.IsApplicable(StateNodes, out var satisfiedPreconditions)) {
                continue;
            }

            var actionNode = new GpActionNode(Level, action, false);
            satisfiedPreconditions.ForEach(preCon => preCon.ConnectTo(actionNode));
            TryAdd(actionNode);
        }

        foreach (var stateNode in StateNodes) {
            var action = new GpAction("Persist", new List<ISentence> { stateNode.Literal }, new List<ISentence> { stateNode.Literal });
            var actionNode = new GpActionNode(Level, action, true);
            stateNode.ConnectTo(actionNode);
            TryAdd(actionNode);
        }

        CheckMutexRelations(ActionNodes.Select(n => (GpNode)n).ToList());
    }

    public GpLayer ExpandNextLayer() {
        var newIndex = Level + 1;
        var newLayer = new GpLayer(newIndex);
        foreach (var actionNode in ActionNodes) {
            foreach (var effect in actionNode.GpAction.Effects) {
                var effectNode = new GpStateNode(newIndex, effect);
                actionNode.ConnectTo(effectNode);
                newLayer.TryAdd(effectNode);
            }
        }

        CheckMutexRelations(newLayer.StateNodes.Select(n => (GpNode)n).ToList());
        return newLayer;
    }

    private void CheckMutexRelations(List<GpNode> nodes) {
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