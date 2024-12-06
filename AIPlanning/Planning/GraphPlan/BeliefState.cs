using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class BeliefState {
    private readonly List<GpStateNode> nodes = new();
    
    public BeliefState() { }
    
    public BeliefState(List<GpNode> nodes) {
        this.nodes = nodes.Select(n => (GpStateNode)n).ToList();
    }
    
    public List<GpNode> GetNodes => nodes.Select(n => (GpNode)n).ToList();
    
    public List<GpStateNode> GetStateNodes => nodes;
    
    public void TryAdd(GpStateNode stateNode) {
        var contained = nodes.FirstOrDefault(stateNode.Equals);
        if (contained != null) {
            stateNode.MergeRelations(contained);
            return;
        }

        nodes.Add(stateNode);
    }
    
    public bool EqualStateLiterals(BeliefState other) {
        var aSubsetB = nodes.All(a => other.nodes.Any(a.EqualLiteral));
        var bSubsetA = other.nodes.All(b => nodes.Any(b.EqualLiteral));
        return aSubsetB && bSubsetA;
    }
    
    public BeliefState GetConflictFreeStateFromGoals(List<ISentence> goals) {
        var reachedSubState = GetReachedSubState(goals);
        
        if(reachedSubState == null) {
            return null;
        }
        
        var conflictFree = reachedSubState.Where(n => !n.MutexRelation.Any(reachedSubState.Contains)).ToList();
        return new BeliefState(conflictFree);
    }

    private List<GpNode> GetReachedSubState(List<ISentence> goals) {
        var reachedSubState = new List<GpNode>();
        
        foreach (var goal in goals) {
            foreach (var stateNode in nodes) {
                if (stateNode.Literal.Match(goal, out var unificator)) {
                    if (!unificator.IsEmpty) {
                        stateNode.InEdges.ForEach(action => ((GpActionNode)action).GpAction.EffectUnificators.Add(unificator)); // this is rther the effect unificator
                    }

                    reachedSubState.Add(stateNode);
                }
            }
        }

        var isReached = reachedSubState.Count == goals.Count;
        return isReached ? reachedSubState : null;
    }
    
    public List<ActionSet> GetCFIncomingActions() {
        var listOfInEdgesActionLists = nodes.Select(stateNode => stateNode.InEdges).ToList();

        foreach (var inEdgesActionList in listOfInEdgesActionLists) {
            var inconsistentActions = inEdgesActionList.Where(actionNode => !((GpActionNode)actionNode).GpAction.IsConsistent()).ToArray();
            for (var i = inconsistentActions.Length - 1; i >= 0; i--) {
                inEdgesActionList.Remove(inconsistentActions[i]);
            }
        }

        var combinationOfEachActionPerInEdges = listOfInEdgesActionLists.GetCombinations().Select(c => c.Distinct().ToList()).ToList();

        var possibleActions = new List<ActionSet>();
        foreach (var combinedActionNodes in combinationOfEachActionPerInEdges) {
            if (combinedActionNodes.Count == 0) {
                continue;
            }

            bool isConflict = combinedActionNodes.Any(actionNode => actionNode.MutexRelation.Any(combinedActionNodes.Contains));
            if (!isConflict) {
                possibleActions.Add(new ActionSet(combinedActionNodes));
            }
        }

        return possibleActions;
    }

    public override int GetHashCode() {
        var hash = 17;
        foreach (var node in nodes) {
            hash = hash * 31 + node.GetHashCode();
        }

        return hash;
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        
        return nodes.SequenceEqual(((BeliefState)obj).nodes);
    }
}