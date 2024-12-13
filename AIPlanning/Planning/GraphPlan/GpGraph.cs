using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GpGraph(List<ISentence> initialState, List<ISentence> goal, List<GpAction> actions, OperatorGraph operatorGraph) {
    private readonly Dictionary<int, GpLayer> _layers = new();

    public void Init() {
        var initialLayer = new GpLayer(0);
        foreach (var sentence in initialState) {
            initialLayer.TryAdd(new GpStateNode(sentence));
        }
        initialLayer.BeliefState.GetNodes.CheckMutexRelations();
        _layers.Add(0, initialLayer);
    }

    public bool StateNotMutex(int i, List<ISentence> sentences) {
        var stateNodes = _layers[i].BeliefState;
        return stateNodes.IsCFStateFromSentencesReachable(sentences, out var conflictFreeState);
    }
    
    public Dictionary<int, ActionSet> ExtractSolution(int levelIndex, NoGoods noGoods) {
        Logger.Log($"Extracting solution for level {levelIndex}");

        var lastState = _layers[levelIndex].BeliefState;
        lastState.IsCFStateFromSentencesReachable(goal, out var currentState);
        
        var solutions = new List<Dictionary<int, GpLayer>>();
        FindSolutions(levelIndex, currentState, noGoods, new Dictionary<int, GpLayer>(), solutions);
        
        if (solutions.Count == 0) {
            return null;
        }
        
        var solutionLayers = solutions.Last();
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

        //maybe lets try a different approach ?
        //
        
        var currentPossibleActionSets = curBeliefState.GetCFIncomingActions();

        Logger.Log($"Find-> Level:{levelIndex}\n{curBeliefState}");
        
        if (currentPossibleActionSets.Count == 0) {
            noGoods.Add(levelIndex, curBeliefState);
            Logger.Log("No possible actions found!");
            return;
        }

        levelIndex--;
        Logger.Log($"Find-> Level:{levelIndex}\n{currentPossibleActionSets.Aggregate("", (acc, action) => acc + action + "\n")}");

        
        foreach (var possiblePrevActions in currentPossibleActionSets) {
            
            var possiblePrevState = possiblePrevActions.GetCFIncomingState();
            
            if (possiblePrevState.GetNodes is not { Count: > 0 }) {
                Logger.Log($"Dead End!\n{possiblePrevState}\n{possiblePrevActions}");
                continue;
            }

            var possibleLayer = new GpLayer(levelIndex, possiblePrevState, possiblePrevActions);
            var outcomeBranch = new Dictionary<int, GpLayer>(outcome);
            outcomeBranch.Add(levelIndex, possibleLayer);
                
            if (levelIndex == 0) {
                Logger.Log($"Solution found for:\n{possiblePrevState}\n{possiblePrevActions}");
                solutions.Add(outcomeBranch);
                return;
            }
            Logger.Log($"down from\n{possiblePrevActions}");
            FindSolutions(levelIndex, possiblePrevState, noGoods, outcomeBranch, solutions);
        }
        
        noGoods.Add(levelIndex, curBeliefState);
        Logger.Log("No states found");
    }
    
    public bool Stable(int levelIndex) {
        return _layers.Count >= 2 && _layers[levelIndex].BeliefState.EqualStateLiterals(_layers[levelIndex - 1].BeliefState);
    }

    public void ExpandGraph() {
        var curLayer = _layers.Last().Value;
        
        var usableActions = curLayer.GetUsableActions(operatorGraph);
        
        curLayer.ExpandActions(usableActions);
        curLayer.ActionSet.GetNodes.CheckMutexRelations();
        var nextLayer = curLayer.ExpandLayer();
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