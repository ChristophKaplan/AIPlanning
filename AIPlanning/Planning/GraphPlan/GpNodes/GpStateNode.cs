using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;


public class GpStateNode(ISentence literal) : GpNode {
    public ISentence Literal { get; set; } = literal;

    public override string ToString() {
        return $"{Literal} [m:{MutexRelation.Aggregate("", (s, m) => $"{s}{m}, ")}]";
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
        if (InEdges.Count == 0 || other.InEdges.Count == 0) {
            return false;
        }
        
        var isAPossibleWay = InEdges.Any(inNode => other.InEdges.Any(otherInNode => inNode.GetMutexType(otherInNode) == MutexType.None));
        return !isAPossibleWay;
    }
}