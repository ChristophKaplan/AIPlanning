namespace AIPlanning.Planning.GraphPlan;

public class GpActionNode(GpAction gpAction, bool isPersistenceAction = false) : GpNode {
    public int useCount = 0;
    public bool IsPersistenceAction { get; } = isPersistenceAction;
    public GpAction GpAction { get; } = gpAction;

    public override string ToString() {
        var showMutex = false;
        var mutex = showMutex ? $"[m:{MutexRelation.Aggregate("", (s, m) => $"{s}{m},")}]" : string.Empty;
        return $"{GpAction} {mutex}";
    }

    public override int GetHashCode() {
        return HashCode.Combine(GpAction, IsPersistenceAction);
    }

    public override bool Equals(object? obj) {
        if (obj is GpActionNode actionNode) {
            return GpAction.Equals(actionNode.GpAction) && IsPersistenceAction == actionNode.IsPersistenceAction;
        }

        return false;
    }

    public bool IsInconsistentEffects(GpActionNode other) {
        return GpAction.Effects.Any(effect => other.GpAction.Effects.Any(otherEffect => effect.IsNegationOf(otherEffect)));
    }

    public bool IsInterference(GpActionNode other) {
        var isInterference = GpAction.Effects.Any(effect => other.GpAction.Preconditions.Any(otherPreCon => effect.IsNegationOf(otherPreCon))) ||
                             other.GpAction.Effects.Any(effect => GpAction.Preconditions.Any(preCon => effect.IsNegationOf(preCon)));
        return isInterference;
    }

    public bool IsCompetingNeeds(GpActionNode other) {
        //in the paper
        //return InEdges.Any(inNode => other.InEdges.Any(otherInNode => inNode.GetMutexType(otherInNode) != MutexType.None));

        //try
        return InEdges.Any(inNode =>
            other.InEdges.Any(otherInNode => inNode.MutexRelation.Any(m => m.Node.Equals(otherInNode) && m.Type != MutexType.None)));

        //in the book
        return GpAction.Preconditions.Any(preCon => other.GpAction.Preconditions.Any(otherPreCon => preCon.IsNegationOf(otherPreCon)));
    }
}