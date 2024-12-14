using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class GpLayer(int level) {
    public readonly int Level = level;
    public BeliefState BeliefState = new();
    public readonly ActionSet ActionSet = new();

    public GpLayer(int level, BeliefState beliefState, ActionSet actionSet) : this(level) {
        BeliefState = beliefState;
        ActionSet = actionSet;
    }

    public void TryAdd(GpNode gpNode) {
        switch (gpNode) {
            case GpLiteralNode stateNode: {
                BeliefState.TryAdd(stateNode);
                break;
            }
            case GpActionNode actionNode: {
                ActionSet.TryAdd(actionNode);
                break;
            }
        }
    }

    public List<GpAction> GetUsableActions(OperatorGraph operatorGraph) {
        List<GpAction> usableActions = new();
        foreach (var node in BeliefState.GetStateNodes) {
            var literal = node.Literal;
            var possibleActionsFor = operatorGraph.GetActionsForLiteral(literal);
            usableActions.AddRange(possibleActionsFor);
        }

        return usableActions.Distinct().ToList();
    }

    public void ExpandActions(List<GpAction> actions) {
        foreach (var action in actions) {
            if (!action.IsApplicableToPreconditions(BeliefState, out var satisfiedPreCons)) {
                continue;
            }

            var actionNode = new GpActionNode(action);
            satisfiedPreCons.ForEach(s => s.ConnectTo(actionNode));
            TryAdd(actionNode);
        }

        foreach (var stateNode in BeliefState.GetStateNodes) {
            var action = new GpAction("Persist", new List<ISentence> { stateNode.Literal }, new List<ISentence> { stateNode.Literal });
            var actionNode = new GpActionNode(action, true);
            stateNode.ConnectTo(actionNode);
            TryAdd(actionNode);
        }
    }

    public GpLayer ExpandLayer() {
        var index = Level + 1;
        var nextLayer = new GpLayer(index);
        nextLayer.BeliefState = ActionSet.ExpandBeliefState();
        nextLayer.BeliefState.GetNodes.CheckMutexRelations();
        return nextLayer;
    }

    public override string ToString() {
        string output = $"Layer: {Level}\n";

        foreach (var stateNode in BeliefState.GetStateNodes) {
            output += stateNode.ToString() + "\n";
        }

        output += "\n";
        foreach (var actionNode in ActionSet.GetActionNodes) {
            output += actionNode.ToString() + "\n";
        }

        output += "\n";
        return output;
    }
}