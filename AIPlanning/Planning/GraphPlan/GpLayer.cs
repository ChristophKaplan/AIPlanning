using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class GpLayer {
    public readonly int Level;
    public GpBeliefState BeliefState = new();
    public readonly GpActionSet ActionSet = new();

    public GpLayer(int level, GpBeliefState beliefState, GpActionSet actionSet) : this(level) {
        BeliefState = beliefState;
        ActionSet = actionSet;
    }

    public GpLayer(int level) {
        Level = level;
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
        foreach (var node in BeliefState.GetLiteralNodes) {
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

        foreach (var stateNode in BeliefState.GetLiteralNodes) {
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
        return nextLayer;
    }

    public override string ToString() {
        var output = $"Layer: {Level}\n";
        output = BeliefState.GetLiteralNodes.Aggregate(output, (current, stateNode) => current + $"{stateNode}\n");
        output += "\n";
        output = ActionSet.GetActionNodes.Aggregate(output, (current, actionNode) => current + $"{actionNode}\n");
        output += "\n";
        return output;
    }
}