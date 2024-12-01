using FirstOrderLogic;
using Helpers;

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

public class GpActionNode(int level, GpAction gpAction, bool isPersistenceAction) : GpNode {
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
        var b = GpAction.Effects.Any(effect => other.GpAction.Preconditions.Any(otherPreCon => effect.IsNegationOf(otherPreCon))) ||
               other.GpAction.Effects.Any(effect => GpAction.Preconditions.Any(preCon => effect.IsNegationOf(preCon)));
        
        if (b) {
            //Logger.Log($"Interference: {GpAction} AND {other.GpAction}");
        }
        return b;
    }

    public bool IsConflictingNeeds(GpActionNode other) {
        return GpAction.Preconditions.Any(preCon => other.GpAction.Preconditions.Any(otherPreCon => preCon.IsNegationOf(otherPreCon)));
    }
}

public class GpStateNode(ISentence literal) : GpNode {
    public ISentence Literal { get; } = literal;

    public override string ToString() {
        return $"{Literal} [m:{MutexRelation.Count}]";
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is GpStateNode stateNode) {
            return Literal.Equals(stateNode.Literal);
        }

        return false;
    }

    public bool EqualLiteral(GpStateNode gpStateNode) {
        return Literal.Equals(gpStateNode.Literal);
    }

    public bool IsInconsistentSupport(GpStateNode other) {
        var isAPossibleWay = InEdges.Any(inNode => other.InEdges.Any(otherInNode => !inNode.IsMutex(otherInNode)));
        if (!isAPossibleWay) {
            //Logger.Log($"Inconsistent support: {Literal} {other.Literal}");
        }
        return !isAPossibleWay;
    }
}