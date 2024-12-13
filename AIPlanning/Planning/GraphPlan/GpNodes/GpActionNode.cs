namespace AIPlanning.Planning.GraphPlan;

public class GpActionNode(GpAction gpAction, bool isPersistenceAction = false) : GpNode {
    public int useCount = 0;
    private bool IsPersistenceAction { get; } = isPersistenceAction;
    public GpAction GpAction { get; } = gpAction;

    public override string ToString() {
        return $"{GpAction} [m:{MutexRelation.Aggregate("", (s, m) => $"{s}{m}, ")}]";
    }

    public override int GetHashCode() {
        return base.GetHashCode();
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

    public bool IsConflictingNeeds(GpActionNode other) {
        return GpAction.Preconditions.Any(preCon => other.GpAction.Preconditions.Any(otherPreCon => preCon.IsNegationOf(otherPreCon)));
    }
}