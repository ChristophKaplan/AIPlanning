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

    public bool StateNotMutex(int i, List<ISentence> goals) {
        var stateNodes = _layers[i].BeliefState;
        var state = stateNodes.GetConflictFreeStateFromGoals(goals);
        return state != null && state.GetNodes.Count == goals.Count;
    }
    
    public Dictionary<int, ActionSet> ExtractSolution(int levelIndex, NoGoods noGoods) {
        Logger.Log($"Extracting solution for level {levelIndex}");

        var lastState = _layers[levelIndex].BeliefState;
        var currentState = lastState.GetConflictFreeStateFromGoals(goal);
        
        var solutions = new List<Dictionary<int, GpLayer>>();
        FindSolutions(levelIndex-1, currentState, noGoods, new Dictionary<int, GpLayer>(), solutions);
        
        if (solutions.Count == 0) {
            return null;
        }
        
        var solutionLayers = solutions.First();
        var solution = new Dictionary<int, ActionSet>();
        foreach (var solutionLayer in solutionLayers.Reverse()) {
            var actions = solutionLayer.Value.ActionSet;
            var step = solutionLayer.Key;
            solution.Add(step, actions);
        }

        return solution;
    }

    private void FindSolutions(int levelIndex, BeliefState curBeliefState, NoGoods noGoods, Dictionary<int, GpLayer> outcome, List<Dictionary<int, GpLayer>> solutions) {
        if (curBeliefState.GetNodes.Count == 0) {
            Logger.Log("State is empty?");
            return;
        }

        var currentPossibleActionSets = curBeliefState.GetCFIncomingActions();

        if (currentPossibleActionSets.Count == 0) {
            noGoods.Add(levelIndex, curBeliefState);
            Logger.Log("No possible actions found");
            return;
        }

        foreach (var possibleActions in currentPossibleActionSets) {
            var possibleState = possibleActions.GetCFIncomingState();

            if (possibleState.GetNodes is not { Count: > 0 }) {
                continue;
            }

            var possibleLayer = new GpLayer(levelIndex, possibleState, possibleActions);

            var outcomeBranch = new Dictionary<int, GpLayer>(outcome);
            outcomeBranch.Add(levelIndex, possibleLayer);
                
            if (levelIndex == 0) {
                Logger.Log("Solution found");
                solutions.Add(outcomeBranch);
                return;
            }
                
            FindSolutions(levelIndex - 1, possibleState, noGoods, outcomeBranch, solutions);
        }
        
        noGoods.Add(levelIndex, curBeliefState);
        Logger.Log("No states found");
    }
    
    public bool Stable(int levelIndex) {
        return _layers.Count >= 2 && _layers[levelIndex].BeliefState.EqualStateLiterals(_layers[levelIndex - 1].BeliefState);
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