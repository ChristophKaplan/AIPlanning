namespace FirstOrderLogic.Planning.GraphPlan;

public class Action {
    private string Name { get; }
    public List<ISentence> Preconditions { get; }
    public List<ISentence> Effects { get; }

    public Action(string name, List<ISentence> preconditions, List<ISentence> effects) {
        Name = name;
        Preconditions = preconditions;
        Effects = effects;
    }

    public bool IsApplicable(List<StateNode> state, out List<Node> satisfiedPreconditions) {
        satisfiedPreconditions = [];
        foreach (var precondition in Preconditions) {
            
            var satisfied = state.FirstOrDefault(stateNode => precondition.Equals(stateNode.Literal));
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