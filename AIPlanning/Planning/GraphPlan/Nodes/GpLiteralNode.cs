using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class GpLiteralNode : GpNode {
    public GpLiteralNode(ISentence literal) {
        Literal = literal;
    }

    public ISentence Literal { get; set; }

    public override string ToString() {
        return $"{Literal} [m:{MutexRelation.Aggregate("", (s, m) => $"{s}{m}, ")}]";
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if(ReferenceEquals(this,obj)) {
            return true;
        }
        
        if (obj is GpLiteralNode stateNode) {
            return Literal.Equals(stateNode.Literal);
        }

        return false;
    }

    public bool EqualLiteral(GpLiteralNode gpLiteralNode) {
        return Literal.Equals(gpLiteralNode.Literal);
    }

    public bool IsInconsistentSupport(GpLiteralNode other) {
        if (InEdges.Count == 0 || other.InEdges.Count == 0) {
            return false;
        }

        var isAPossibleWay = InEdges.Any(inNode => other.InEdges.Any(otherInNode => inNode.GetMutexType(otherInNode) == MutexType.None));
        return !isAPossibleWay;
    }
}