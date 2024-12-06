using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;


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