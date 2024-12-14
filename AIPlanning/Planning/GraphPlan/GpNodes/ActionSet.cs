using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class ActionSet{
    public readonly List<GpActionNode> nodes = new();
    
    public List<GpNode> GetNodes => nodes.Select(n => (GpNode)n).ToList();
    public List<GpActionNode> GetActionNodes => nodes;

    public ActionSet() { }
    
    public ActionSet(List<GpNode> combinedActionNodes) {
        nodes = combinedActionNodes.Select(n => (GpActionNode)n).ToList();
    }
    
    public void TryAdd(GpActionNode actionNode) {
        var contained = nodes.FirstOrDefault(actionNode.Equals);
        if (contained != null) {
            actionNode.MergeRelations(contained);
            return;
        }

        nodes.Add(actionNode);
    }
    
    public BeliefState GetCFIncomingState() {
        var incomingNodes = nodes.SelectMany(node => node.InEdges).Distinct().ToList();
        //incomingNodes.CheckMutexRelations();
        var conflictFree = incomingNodes.GetConflictFreeSubset();
        
        if(conflictFree.Count == 0) {
            Logger.Log($"Empty state for: {incomingNodes.Aggregate("", (s, n) => s + n + "\n")}");
        }
        
        return new BeliefState(conflictFree);
    }
    
    public BeliefState ExpandBeliefState() {
        var beliefState = new BeliefState();
        foreach (var actionNode in nodes) {
            var list = actionNode.GpAction.Effects;
            foreach (var effect in list) {
                var stateNode = new GpLiteralNode(effect);
                actionNode.ConnectTo(stateNode);
                beliefState.TryAdd(stateNode);
            }
        }
        
        return beliefState;
    }
    
    public override int GetHashCode() {
        var hash = 17;
        foreach (var node in nodes) {
            hash = hash * 31 + node.GetHashCode();
        }

        return hash;
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        
        return nodes.SequenceEqual(((ActionSet)obj).nodes);
    }
    
    public override string ToString() {
        var output = "ActionSet:\n";
        
        foreach (var node in nodes) {
            output += $"\t{node}\n";
        }
        
        return output;
    }
}