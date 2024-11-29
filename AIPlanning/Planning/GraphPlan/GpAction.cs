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

    public List<Unificator> Unificators { get; } = new();

    public bool IsApplicable(List<GpStateNode> stateNodes, out List<GpNode> satisfiedPreconditionNodes) {
        satisfiedPreconditionNodes = [];

        foreach (var preCon in Preconditions) {
            var applicableNode = stateNodes.FirstOrDefault(node => ApplicableSingle(node.Literal, preCon));
            if (applicableNode == null) return false;
            
            satisfiedPreconditionNodes.Add(applicableNode);
        }

        return true;

        bool ApplicableSingle(ISentence literal, ISentence preCon) {
            var b = literal.Match(preCon, out var u);
            Unificators.Add(u);
            return b;
            
            if (literal.IsNegationOf(preCon, true)) {
                return false;
            }
            
            var unificator = new Unificator(preCon, literal);
            var unify = unificator.IsUnifiable;
            if (unify) Unificators.Add(unificator);
            
            return unify;
        }
    }
    
    public GpAction(GpAction action) : this(action.Signifier, 
        action.Preconditions.Select(p => p.Clone()).ToList(), 
        action.Effects.Select(e => e.Clone()).ToList()) { }
    
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