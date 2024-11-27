using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public abstract class GpNode(int level) {
    public int Level { get; } = level;
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

public class GpActionNode(int level, GpAction gpAction, bool isPersistenceAction) : GpNode(level) {
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
            return Level.Equals(actionNode.Level) && GpAction.Equals(actionNode.GpAction) && IsPersistenceAction == actionNode.IsPersistenceAction;
        }

        return false;
    }

    public bool IsInconsistentEffects(GpActionNode other) {
        return GpAction.Effects.Any(effect => other.GpAction.Effects.Any(effect.IsNegationOf));
    }

    public bool IsInterference(GpActionNode other) {
        return GpAction.Effects.Any(effect => other.GpAction.Preconditions.Any(effect.IsNegationOf)) ||
               other.GpAction.Effects.Any(effect => GpAction.Preconditions.Any(effect.IsNegationOf));
    }

    public bool IsConflictingNeeds(GpActionNode other) {
        return GpAction.Preconditions.Any(preCon => other.GpAction.Preconditions.Any(preCon.IsNegationOf));
    }
}

public class GpStateNode(int level, ISentence literal) : GpNode(level) {
    public ISentence Literal { get; } = literal;

    public override string ToString() {
        return $"{Literal} [m:{MutexRelation.Count}]";
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is GpStateNode stateNode) {
            return Level.Equals(stateNode.Level) && Literal.Equals(stateNode.Literal);
        }

        return false;
    }

    public bool EqualLiteral(GpStateNode gpStateNode) {
        return Literal.Equals(gpStateNode.Literal);
    }

    public bool IsInconsistentSupport(GpStateNode other) {
        return InEdges.Any(inNode => other.InEdges.Any(inNode.IsMutex));
    }
}