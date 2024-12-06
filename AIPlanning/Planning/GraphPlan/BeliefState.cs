using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class BeliefState {
    private readonly List<GpStateNode> StateNodes = new();
    
    public BeliefState(List<GpStateNode> stateNodes) {
        StateNodes = stateNodes;
    }
    
    public BeliefState(List<ISentence> sentences) {
        foreach (var sentence in sentences) {
            StateNodes.Add(new GpStateNode(sentence));
        }
    }
}