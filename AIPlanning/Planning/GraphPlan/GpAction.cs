using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public interface IGpAction {
    string Signifier { get; }
    List<ISentence> Preconditions { get; }
    List<ISentence> Effects { get; }
    bool IsApplicable(List<GpStateNode> stateNodes, out List<GpNode> satisfiedPreconditionNodes);
}

public class GpAction(string name, List<ISentence> preconditions, List<ISentence> effects) : IGpAction {
    public string Signifier { get; } = name;
    public List<ISentence> Preconditions { get; } = preconditions;
    public List<ISentence> Effects { get; } = effects;

    public bool IsApplicable(List<GpStateNode> stateNodes, out List<GpNode> satisfiedPreconditionNodes) {
        satisfiedPreconditionNodes = [];

        foreach (var preCon in preconditions) {
            var applicableNode = stateNodes.FirstOrDefault(node => ApplicableSingle(node.Literal, preCon));
            if (applicableNode == null) return false;
            
            satisfiedPreconditionNodes.Add(applicableNode);
        }

        return true;

        bool ApplicableSingle(ISentence literal, ISentence preCon) {
            var unificator = new Unificator(preCon, literal);
            if (!unificator.IsUnifiable) return false;
            
            //TODO: just compare signature and negation
            unificator.Substitute(ref literal);
            unificator.Substitute(ref preCon);
            return !literal.IsNegationOf(preCon);
        }
    }

    public override string ToString() {
        return $"{Signifier} {string.Join(",", Preconditions)} -> {string.Join(",", Effects)}";
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        return ToString().Equals(obj.ToString());
    }
}