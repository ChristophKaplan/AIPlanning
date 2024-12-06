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
        var conflictFree = incomingNodes.Where(n => !n.MutexRelation.Any(incomingNodes.Contains)).ToList();
        return new BeliefState(conflictFree);
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
}