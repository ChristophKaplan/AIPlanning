using System.Text;
using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class GpLayer(int level) {
    public readonly int Level = level;
    public readonly List<GpStateNode> StateNodes = new();
    public readonly List<GpActionNode> ActionNodes = new();

    public GpLayer(int level, List<GpStateNode> stateNodes, List<GpActionNode> actionNodes) : this(level) {
        this.StateNodes = stateNodes;
        this.ActionNodes = actionNodes;
    }
    
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
    
    private void RemoveAction(GpActionNode actionNode) {
        foreach (var inNode in actionNode.InEdges) {
            inNode.OutEdges.Remove(actionNode);
        }
        
        foreach (var outNode in actionNode.OutEdges) {
            outNode.InEdges.Remove(actionNode);
        }
        
        ActionNodes.Remove(actionNode);
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

    public void ExpandActionNodesFromState(List<GpAction> actions) {

        actions = actions.Select(action => new GpAction(action)).ToList();
        
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
        
        ActionNodes.ForEach(n => n.GpAction.SpecifyPrecon());
        /*

        //remove inconsistent actions (?)
        for (var i = ActionNodes.Count-1; i >= 0; i--) {
            var cur = ActionNodes[i];
            if (!cur.GpAction.IsConsistent()) {
                RemoveAction(cur);
            }
        }*/
        
        CheckMutexRelations(ActionNodes.Select(n => (GpNode)n).ToList());
    }

    public GpLayer ExpandNextLayer() {
        var newIndex = Level + 1;
        var newLayer = new GpLayer(newIndex);
        foreach (var actionNode in ActionNodes) {
            foreach (var effect in actionNode.GpAction.Effects) {
                var effectNode = new GpStateNode(effect);
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
        string output = $"Layer: {Level}\n";
        
        foreach (var stateNode in StateNodes) {
            output += stateNode.ToString() + "\n";
        }
        output +=  "\n";
        foreach (var actionNode in ActionNodes) {
            output += actionNode.ToString() + "\n";
        }
        output +=  "\n";
        return output;
    }
}