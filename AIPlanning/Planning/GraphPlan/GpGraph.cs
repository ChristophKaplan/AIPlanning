using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GpGraph(List<ISentence> initialState, List<ISentence> goal, List<GpAction> actions) {
    private readonly Dictionary<int, GpLayer> _layers = new();

    public void Init() {
        var initialLayer = new GpLayer(0);
        foreach (var sentence in initialState) {
            initialLayer.TryAdd(new GpStateNode(sentence));
        }

        _layers.Add(0, initialLayer);
    }

    private void Rekursion(int levelIndex, List<GpNode> currentState, NoGoods noGoods, Dictionary<int, GpLayer> outcome, List<Dictionary<int, GpLayer>> solutions) {
        if (currentState.Count == 0) {
            Logger.Log("State is empty?");
            return;
        }

        var currentPossibleActionSets = GetConflictFreeSubsetsOfIncomingActionNodes(currentState);

        if (currentPossibleActionSets.Count == 0) {
            noGoods.Add(levelIndex, currentState);
            Logger.Log("No possible actions found");
            return;
        }

        foreach (var possibleActions in currentPossibleActionSets) {
            var possibleState = GetConflictFreeSubsetOfIncomingNodes(possibleActions);

            if (possibleState is not { Count: > 0 }) {
                continue;
            }

            var possibleLayer = new GpLayer(levelIndex,
                possibleState.Select(n => (GpStateNode)n).ToList(),
                possibleActions.Select(n => (GpActionNode)n).ToList());

            var outcomeBranch = new Dictionary<int, GpLayer>(outcome);
            outcomeBranch.Add(levelIndex, possibleLayer);
                
            if (levelIndex == 0) {
                Logger.Log("Solution found");
                solutions.Add(outcomeBranch);
                return;
            }
                
            Rekursion(levelIndex - 1, possibleState, noGoods, outcomeBranch, solutions);
        }
        
        noGoods.Add(levelIndex, currentState);
        Logger.Log("No states found");
    }


    public bool StateNotMutex(int i, List<ISentence> goals) {
        var stateNodes = _layers[i].StateNodes;
        var state = GetConflictFreeStateFromGoals(stateNodes, goals);
        return state != null && state.Count == goals.Count;
    }
    
    public Dictionary<int, List<GpAction>> ExtractSolution(int levelIndex, NoGoods noGoods) {
        Logger.Log($"Extracting solution for level {levelIndex}");

        var lastState = _layers[levelIndex].StateNodes;
        var currentState = GetConflictFreeStateFromGoals(lastState, goal);
        
        
        var solutions = new List<Dictionary<int, GpLayer>>();
        Rekursion(levelIndex-1, currentState, noGoods, new Dictionary<int, GpLayer>(), solutions);
        
        if (solutions.Count == 0) {
            return null;
        }
        
        var solutionLayers = solutions.First();
        var solution = new Dictionary<int, List<GpAction>>();
        foreach (var solutionLayer in solutionLayers.Reverse()) {
            var actions = solutionLayer.Value.ActionNodes;
            var step = solutionLayer.Key;
            solution.Add(step, actions.Select(n => n.GpAction).ToList());
        }

        return solution;
    }

    private List<List<GpNode>> GetConflictFreeSubsetsOfIncomingActionNodes(List<GpNode> stateNodes) {
        var listOfInEdgesActionLists = stateNodes.Select(stateNode => stateNode.InEdges).ToList();

        foreach (var inEdgesActionList in listOfInEdgesActionLists) {
            var inconsistentActions = inEdgesActionList.Where(actionNode => !((GpActionNode)actionNode).GpAction.IsConsistent()).ToArray();
            for (var i = inconsistentActions.Length - 1; i >= 0; i--) {
                inEdgesActionList.Remove(inconsistentActions[i]);
            }
        }

        var combinationOfEachActionPerInEdges = Combinations.GetCombinations(listOfInEdgesActionLists).Select(c => c.Distinct().ToList()).ToList();

        var possibleActions = new List<List<GpNode>>();
        foreach (var combinedActionNodes in combinationOfEachActionPerInEdges) {
            if (combinedActionNodes.Count == 0) {
                continue;
            }

            bool isConflict = combinedActionNodes.Any(actionNode => actionNode.MutexRelation.Any(combinedActionNodes.Contains));
            if (!isConflict) {
                possibleActions.Add(combinedActionNodes);
            }
        }

        return possibleActions;
    }


    private List<GpNode> GetConflictFreeSubsetOfIncomingNodes(List<GpNode> nodes) {
        var incomingNodes = nodes.SelectMany(node => node.InEdges).Distinct().ToList();
        return GetConflictFreeSubset(incomingNodes);
    }

    private List<GpNode> GetConflictFreeStateFromGoals(List<GpStateNode> state, List<ISentence> goals) {
        var reachedSubState = GetReachedSubState(state, goals);
        return reachedSubState == null ? null : GetConflictFreeSubset(reachedSubState);
    }

    private List<GpNode> GetReachedSubState(List<GpStateNode> state, List<ISentence> goals) {
        var reachedSubState = new List<GpNode>();
        foreach (var goal in goals) {
            foreach (var stateNode in state) {
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

    private List<GpNode> GetConflictFreeSubset(List<GpNode> nodes) {
        return nodes.Where(actionNode => !actionNode.MutexRelation.Any(nodes.Contains)).ToList();
    }

    public bool Stable(int levelIndex) {
        return _layers.Count >= 2 && _layers[levelIndex].EqualStateLiterals(_layers[levelIndex - 1]);
    }

    public void ExpandGraph() {
        var lastLayer = _layers.Last().Value;
        lastLayer.ExpandActionNodesFromState(actions);
        var nextLayer = lastLayer.ExpandNextLayer();
        _layers.Add(nextLayer.Level, nextLayer);
    }

    public override string ToString() {
        var output = "";
        foreach (var layer in _layers) {
            output += layer.Value.ToString();
        }

        return output;
    }
}

public static class Combinations {
    public static List<List<T>> GetCombinations<T>(List<List<T>> lists) {
        var c = lists.CartesianProduct().Select(l => l.ToList()).ToList();
        return c;
    }

    static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences) {
        IEnumerable<IEnumerable<T>> emptyProduct = [[]];
        return sequences.Aggregate(emptyProduct,
            (accumulator, sequence) => from accseq in accumulator from item in sequence select accseq.Concat([item]));
    }
}