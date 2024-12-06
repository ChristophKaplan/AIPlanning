namespace AIPlanning.Planning.GraphPlan;

public abstract class GpNode {
    public List<GpNode> InEdges { get; } = [];
    public List<GpNode> OutEdges { get; } = [];
    public List<GpNode> MutexRelation { get; } = [];

    public void ConnectTo(GpNode connectToMe) {
        OutEdges.Add(connectToMe);
        connectToMe.InEdges.Add(this);
    }

    public bool IsMutex(GpNode other) {
        if (Equals(other)) return false;

        return this switch {
            GpStateNode s1 when other is GpStateNode s2 => s1.IsInconsistentSupport(s2) || s1.Literal.IsNegationOf(s2.Literal),
            GpActionNode a1 when other is GpActionNode a2 => a1.IsInconsistentEffects(a2) || a1.IsInterference(a2) || a1.IsConflictingNeeds(a2),
            _ => throw new Exception("Invalid node type")
        };
    }

    public void TryAddMutexRelations(GpNode other) {
        if (!MutexRelation.Contains(other)) {
            MutexRelation.Add(other);
        }

        if (!other.MutexRelation.Contains(this)) {
            other.MutexRelation.Add(this);
        }
    }
}
