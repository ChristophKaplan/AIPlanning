using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public interface IGpAction {
    string Signifier { get; }
    List<ISentence> Preconditions { get; }
    List<ISentence> Effects { get; }
    bool IsApplicableToPreconditions(BeliefState beliefState, out List<GpNode> satisfied);
}

public class GpAction(string name, List<ISentence> preconditions, List<ISentence> effects) : IGpAction {
    public string Signifier { get; } = name;
    public List<ISentence> Preconditions { get; } = preconditions;
    public List<ISentence> Effects { get; } = effects;

    public List<Unificator> Unificators { get; set; } = new();
    public bool IsSpecified;

    public bool IsApplicableToPreconditions(BeliefState beliefState, out List<GpNode> satisfied) {
        satisfied = beliefState.GetSubBeliefStateMatchingTo(Preconditions);
        return satisfied != null && satisfied.Count == Preconditions.Count; 
    }
    
    public GpAction(GpAction action) : this(action.Signifier,
        action.Preconditions.Select(p => p.Clone()).ToList(),
        action.Effects.Select(e => e.Clone()).ToList()) {
        IsSpecified = action.IsSpecified;
    }

    public void SpecifyAction(Unificator unificator) {
        foreach (var preCon in Preconditions) {
            var tempPreCon = preCon;
            unificator.Substitute(ref tempPreCon);
        }

        foreach (var effect in Effects) {
            var tempEffect = effect;
            unificator.Substitute(ref tempEffect);
        }
        
        IsSpecified = true;
    }

    public bool ContainsVariable() {
        foreach (var preCon in Preconditions) {
            var anyVar = preCon.GetPredicate().Terms.Any(t => t is Variable);
            if (anyVar) {
                return true;
            }
        }

        foreach (var effect in Effects) {
            var anyVar = effect.GetPredicate().Terms.Any(t => t is Variable);
            if (anyVar) {
                return true;
            }
        }
        
        return false;
    }

    public bool IsConsistent(bool inMatchedTerms = false) {
        if (inMatchedTerms) {
            var prem = Preconditions.Any(p1 => Preconditions.Any(p2 => p1.IsNegationOfAndMatch(p2, out _)));
            var effm = Effects.Any(eff1 => Effects.Any(eff2 => eff1.IsNegationOfAndMatch(eff2, out _)));

            return !(prem || effm);
        }
        
        var pre = Preconditions.Any(p1 => Preconditions.Any(p2 => p1.IsNegationOf(p2)));
        var eff = Effects.Any(eff1 => Effects.Any(eff2 => eff1.IsNegationOf(eff2)));

        return !(pre || eff);
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

    public GpAction Clone() {
        return new GpAction(this);
    }
}