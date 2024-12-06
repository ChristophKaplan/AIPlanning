namespace AIPlanning.Planning.GraphPlan;


public class GpActionNode(GpAction gpAction, bool isPersistenceAction) : GpNode {
    private bool IsPersistenceAction { get; } = isPersistenceAction;
    public GpAction GpAction { get; } = gpAction;

    public override string ToString() {
        return $"{GpAction.ToString()} [m:{MutexRelation.Count}]";
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
    
    /*public void SpecifyPrecon() {
        //GpAction.SpecifyPrecon();

        foreach (var node in InEdges) {
            foreach (var uni in GpAction.PreConUnificators) {
                var tempPreCon = ((GpStateNode)node).Literal;
                uni.Substitute(ref tempPreCon);
            }
        }

        foreach (var node in OutEdges) {
            foreach (var uni in GpAction.PreConUnificators) {
                var tempEffect = ((GpStateNode)node).Literal;
                uni.Substitute(ref tempEffect);
            }
        }
    }*/
}