namespace FirstOrderLogic.Planning.GraphPlan;

public interface IGpAction {
    string Signifier { get; }
    List<ISentence> Preconditions { get; }
    List<ISentence> Effects { get; }
    bool IsApplicable(List<GpStateNode> state, out List<GpNode> satisfiedPreconditionNodes);
}

public class GpAction(string name, List<ISentence> preconditions, List<ISentence> effects) : IGpAction {
    public string Signifier { get; } = name;
    public List<ISentence> Preconditions { get; } = preconditions;
    public List<ISentence> Effects { get; } = effects;

    public bool IsApplicable(List<GpStateNode> state, out List<GpNode> satisfiedPreconditionNodes) {
        satisfiedPreconditionNodes = [];

        foreach (var precondition in Preconditions) {
            foreach (var stateNode in state) {
                if (stateNode.Literal.IsNegationOf(precondition)) {
                    break;
                }
                
                var unificator = new Unificator(precondition, stateNode.Literal);
                if (unificator.IsUnifiable) {
                    satisfiedPreconditionNodes.Add(stateNode);
                    break;
                }
            }
        }

        return true;
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