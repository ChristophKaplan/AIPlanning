using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public interface IGpAction {
    string Signifier { get; }
    List<ISentence> Preconditions { get; }
    List<ISentence> Effects { get; }
    bool IsApplicableToPreconditions(GpBeliefState beliefState, out List<GpNode> satisfied);
}

public class GpAction : IGpAction {
    private int _hashcode;
    public string Signifier { get; }
    public List<ISentence> Preconditions { get; }
    public List<ISentence> Effects { get; }
    public List<Unificator> Unificators { get; } = new();

    private GpAction(GpAction action) : this(action.Signifier,
        action.Preconditions.Select(p => p.Clone()).ToList(),
        action.Effects.Select(e => e.Clone()).ToList()) {
    }

    public GpAction(string name, List<ISentence> preconditions, List<ISentence> effects)
    {
        Signifier = name;
        Preconditions = preconditions;
        Effects = effects;
        UpdateHashCode();
    }

    public GpAction Clone() {
        return new GpAction(this);
    }

    public bool IsApplicableToPreconditions(GpBeliefState beliefState, out List<GpNode> satisfied) {
        satisfied = beliefState.GetSubSetOfNodesMatching(Preconditions);
        return satisfied != null && satisfied.Count == Preconditions.Count;
    }

    public List<Unificator> GetConflictFreeUnificatorPossibilities(List<Unificator> unificators) {
        var substitutions = ArrangeSubstitutionsAsTrees(unificators);

        var variables = substitutions.Keys.ToList();
        var termLists = substitutions.Values.ToList();
        var combs = termLists.GetCombinations();

        var possibilities = new List<Unificator>();

        foreach (var comb in combs) {
            var possibility = new Dictionary<Variable, Term>();
            for (var i = 0; i < variables.Count; i++) {
                possibility.Add(variables[i], comb[i]);
            }
            
            possibilities.Add(new Unificator(possibility));
        }

        return possibilities;
    }

    private Dictionary<Variable, List<Term>> ArrangeSubstitutionsAsTrees(List<Unificator> unificators) {
        var collectPossibilities = new Dictionary<Variable, List<Term>>();

        foreach (var unificator in unificators) {
            if (unificator.IsEmpty) {
                continue;
            }

            foreach (var substitution in unificator.Substitutions) {
                if (!collectPossibilities.TryAdd(substitution.Key, new List<Term> { substitution.Value })) {
                    collectPossibilities[substitution.Key].Add(substitution.Value);
                }
            }
        }

        return collectPossibilities;
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
        
        UpdateHashCode();
    }

    public bool IsConsistent() {
        var isConflictInPreCons = Preconditions.Any(p1 => Preconditions.Any(p2 => p1.IsNegationOf(p2)));
        var isConflictInEffects = Effects.Any(eff1 => Effects.Any(eff2 => eff1.IsNegationOf(eff2)));
        return !isConflictInPreCons && !isConflictInEffects;
    }

    public override int GetHashCode()
    {
        return _hashcode;
    }

    private void UpdateHashCode()
    {
        var hash = HashCode.Combine(Signifier);
        hash = Preconditions.Aggregate(hash, HashCode.Combine);
        _hashcode = Effects.Aggregate(hash, HashCode.Combine);
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        return GetHashCode() == obj.GetHashCode();
        
        var other = (GpAction)obj;
        return Signifier == other.Signifier && Preconditions.SequenceEqual(other.Preconditions) && Effects.SequenceEqual(other.Effects);
    }

    public override string ToString() {
        return $"{Signifier} {string.Join(",", Preconditions)} -> {string.Join(",", Effects)}";
    }
}