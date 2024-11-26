namespace FirstOrderLogic.Planning.GraphPlan;

public static class GpActionFabric {
    static readonly FirstOrderLogic Logic = new ();
    public static GpAction Create(string name, List<string> preconditions, List<string> effects) {
        return new GpAction(name, preconditions.Select(p => (ISentence)Logic.TryParse(p)).ToList(), effects.Select(e => (ISentence)Logic.TryParse(e)).ToList());
    }
    public static List<ISentence> StringToSentence(List<string> strings) {
        return new List<ISentence>(strings.Select(s => (ISentence)Logic.TryParse(s)));
    }
}

public class GpAction(string name, List<ISentence> preconditions, List<ISentence> effects) {
    private string Name { get; } = name;
    public List<ISentence> Preconditions { get; } = preconditions;
    public List<ISentence> Effects { get; } = effects;

    public bool IsApplicable(List<GpStateNode> state, out List<GpNode> satisfiedPreconditions) {
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