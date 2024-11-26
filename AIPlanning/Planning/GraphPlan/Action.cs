namespace FirstOrderLogic.Planning.GraphPlan;

public class Action {
    public string Name { get; set; }
    public List<ISentence> Preconditions { get; set; }
    public List<ISentence> Effects { get; set; }

    public Action(string name, List<ISentence> preconditions, List<ISentence> effects) {
        Name = name;
        Preconditions = preconditions;
        Effects = effects;
    }

    public bool IsApplicable(List<StateNode> state, out List<StateNode> satisfiedPreconditions) {
        satisfiedPreconditions = new List<StateNode>();
        foreach (var precondition in Preconditions) {
            var satisfied = state.FirstOrDefault(s => s.Literal.Equals(precondition));
            if (satisfied == null) {
                return false;
            }

            satisfiedPreconditions.Add(satisfied);
        }

        return true;
    }

    public override string ToString() {
        return $"{Name} {string.Join(",", Preconditions)} -> {string.Join(",", Effects)}";
    }
    
    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        return ToString().Equals(obj.ToString());
    }
}