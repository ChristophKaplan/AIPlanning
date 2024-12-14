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

    public bool IsApplicableToPreconditions(BeliefState beliefState, out List<GpNode> satisfied) {
        satisfied = beliefState.GetSubBeliefStateMatchingTo(Preconditions);
        return satisfied != null && satisfied.Count == Preconditions.Count; 
    }
    
    public GpAction(GpAction action) : this(action.Signifier,
        action.Preconditions.Select(p => p.Clone()).ToList(),
        action.Effects.Select(e => e.Clone()).ToList()) {
    }

    private Dictionary<Variable, List<Term>> GetPossibleConflictFreeSubstitutions(List<Unificator> unificators) {
        Dictionary<Variable, List<Term>> collectPossibilities = new();
        foreach (var unificator in unificators) {
            if(unificator.IsEmpty) {
                continue;
            }

            foreach (var substitution in unificator.Substitutions) {
                if (!collectPossibilities.TryAdd(substitution.Key, new() { substitution.Value })) {
                    collectPossibilities[substitution.Key].Add(substitution.Value);
                }
            }
        }
        
        return collectPossibilities;
    }

    public List<Unificator> GetConflictFreeUnificatorPossibilities(List<Unificator> unificators) {
        var substitutions = GetPossibleConflictFreeSubstitutions(unificators);
        var lk = substitutions.Keys.ToList();
        var lv = substitutions.Values.ToList();
        var combs = lv.GetCombinations();

        List<Unificator> possibilities = new();
        foreach (var comb in combs) {
            Dictionary<Variable, Term> possibility = new();
            for (var i = 0; i < lk.Count; i++) {
                possibility.Add(lk[i], comb[i]);
            }
            possibilities.Add(new Unificator(possibility));
        }
        
        return possibilities;
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
        var hash = HashCode.Combine(Signifier);
        for (var i = 0; i < Preconditions.Count; i++) {
            hash = HashCode.Combine(hash, Preconditions[i]);
        }

        for (var i = 0; i < Effects.Count; i++) {
            hash = HashCode.Combine(hash, Effects[i]);
        }

        return hash;
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        var other = (GpAction)obj;
        return Signifier == other.Signifier && Preconditions.SequenceEqual(other.Preconditions) && Effects.SequenceEqual(other.Effects);
    }

    public GpAction Clone() {
        return new GpAction(this);
    }
}