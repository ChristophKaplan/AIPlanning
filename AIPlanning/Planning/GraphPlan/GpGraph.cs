using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GpGraph(List<ISentence> initialState, List<ISentence> goal, List<GpAction> actions, OperatorGraph operatorGraph) {
    private readonly Dictionary<int, GpLayer> _layers = new();

    public void Init() {
        var initialLayer = new GpLayer(0);
        foreach (var sentence in initialState) {
            initialLayer.TryAdd(new GpLiteralNode(sentence));
        }
        initialLayer.BeliefState.GetNodes.CheckMutexRelations();
        _layers.Add(0, initialLayer);
    }

    public bool StateNotMutex(int i, List<ISentence> sentences) {
        var stateNodes = _layers[i].BeliefState;
        return stateNodes.IsCFStateFromSentencesReachable(sentences, out var conflictFreeState);
    }
    
    public Solution ExtractSolution(int levelIndex, NoGoods noGoods) {
        var lastState = _layers[levelIndex].BeliefState;
        lastState.IsCFStateFromSentencesReachable(goal, out var currentState);
        
        var solutions = new Solution();
        FindSolutions(levelIndex, currentState, noGoods, new Dictionary<int, GpLayer>(), solutions);
        return solutions;
    }

    private void FindSolutions(int levelIndex, BeliefState curBeliefState, NoGoods noGoods, Dictionary<int, GpLayer> outcome, Solution solutions) {
        if (curBeliefState.GetNodes.Count == 0) {
            return;
        }
        
        var currentPossibleActionSets = curBeliefState.GetCFIncomingActions();
        if (currentPossibleActionSets.Count == 0) {
            noGoods.Add(levelIndex, curBeliefState);
            //Logger.Log("No possible actions found!");
            return;
        }

        levelIndex--;

        foreach (var possiblePrevActions in currentPossibleActionSets) {
            
            var possiblePrevState = possiblePrevActions.GetCFIncomingState();
            
            if (possiblePrevState.GetNodes is not { Count: > 0 }) {
                //Logger.Log($"Dead End!\n{possiblePrevState}\n{possiblePrevActions}");
                continue;
            }

            var possibleLayer = new GpLayer(levelIndex, possiblePrevState, possiblePrevActions);
            var outcomeBranch = new Dictionary<int, GpLayer>(outcome);
            outcomeBranch.Add(levelIndex, possibleLayer);
                
            if (levelIndex == 0) {
                solutions.Add(outcomeBranch);
                return;
            }
            
            FindSolutions(levelIndex, possiblePrevState, noGoods, outcomeBranch, solutions);
        }
        
        noGoods.Add(levelIndex, curBeliefState);
        //Logger.Log("No states found");
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