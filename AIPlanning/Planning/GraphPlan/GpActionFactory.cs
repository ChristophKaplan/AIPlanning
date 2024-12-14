using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public static class GpActionFactory {
    private static readonly FirstOrderLogic.FirstOrderLogic Logic = new();

    public static GpAction Create(string name, List<string> preconditions, List<string> effects) {
        return new GpAction(name,
            preconditions.Select(p => (ISentence)Logic.TryParse(p)).ToList(),
            effects.Select(e => (ISentence)Logic.TryParse(e)).ToList());
    }

    public static List<ISentence> StringToSentence(List<string> strings) {
        return [..strings.Select(s => (ISentence)Logic.TryParse(s))];
    }
}