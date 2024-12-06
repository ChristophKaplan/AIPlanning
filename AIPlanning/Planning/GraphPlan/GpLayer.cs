using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class GpLayer(int level) {
    public readonly int Level = level;
    public readonly BeliefState BeliefState = new();
    public readonly ActionSet ActionSet = new();

    public GpLayer(int level, BeliefState beliefState, ActionSet actionSet) : this(level) {
        BeliefState = beliefState;
        ActionSet = actionSet;
    }
    
    public void TryAdd(GpNode gpNode) {
        switch (gpNode) {
            case GpStateNode stateNode: {
                BeliefState.TryAdd(stateNode);
                break;
            }
            case GpActionNode actionNode: {
                ActionSet.TryAdd(actionNode);
                break;
            }
        }
    }
    
    public void ExpandActionNodesFromState(List<GpAction> actions) {
        var actionInstances = actions.Select(action => new GpAction(action)).ToList();

        foreach (var action in actionInstances) {
            if (!action.IsApplicable(BeliefState, out var satisfiedPreconditions)) {
                continue;
            }
            
            var actionNode = new GpActionNode(action, false);
            
            satisfiedPreconditions.ForEach(preCon => preCon.ConnectTo(actionNode));
            TryAdd(actionNode);
        }

        foreach (var stateNode in BeliefState.GetStateNodes) {
            var action = new GpAction("Persist", new List<ISentence> { stateNode.Literal }, new List<ISentence> { stateNode.Literal });
            var actionNode = new GpActionNode(action, true);
            stateNode.ConnectTo(actionNode);
            TryAdd(actionNode);
        }
        
        ActionSet.GetNodes.CheckMutexRelations();
    }

    public GpLayer ExpandNextLayer() {
        var newIndex = Level + 1;
        var newLayer = new GpLayer(newIndex);
        
        foreach (var actionNode in ActionSet.GetActionNodes) {
            foreach (var effect in actionNode.GpAction.Effects) {
                var effectNode = new GpStateNode(effect);
                actionNode.ConnectTo(effectNode);
                newLayer.TryAdd(effectNode);
            }
        }

        newLayer.BeliefState.GetNodes.CheckMutexRelations();
        return newLayer;
    }

    public override string ToString() {
        string output = $"Layer: {Level}\n";
        
        foreach (var stateNode in BeliefState.GetStateNodes) {
            output += stateNode.ToString() + "\n";
        }
        output +=  "\n";
        foreach (var actionNode in ActionSet.GetActionNodes) {
            output += actionNode.ToString() + "\n";
        }
        output +=  "\n";
        return output;
    }
}