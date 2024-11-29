using System.Text;
using ConsoleTables;
using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GpGraph(List<ISentence> initialState, List<ISentence> goal, List<GpAction> actions) {
    private readonly Dictionary<int, GpLayer> _layers = new();

    public void Init() {
        var initialLayer = new GpLayer(0);
        foreach (var sentence in initialState) {
            initialLayer.TryAdd(new GpStateNode(0, sentence));
        }
        
        _layers.Add(0, initialLayer);
    }

    public bool StateNotMutex(int i, List<ISentence> goals) {
        var stateNodes = _layers[i].StateNodes;
        var state = GetConflictFreeStateFromGoals(stateNodes, goals);
        return state != null;
    }

    public Dictionary<int, List<GpAction>> ExtractSolution(int levelIndex, List<(int level, List<GpNode> subGoalState)> noGoods) {
        var lastState = _layers[levelIndex].StateNodes;
        var conflictFreeStateFromGoals = GetConflictFreeStateFromGoals(lastState, goal);
        
        var currentState = conflictFreeStateFromGoals;
        var solution = new Dictionary<int, List<GpAction>>();

        if(noGoods.Contains((levelIndex, currentState))) {
            Logger.Log("Already in noGoods");
            return null;
        }
        
        for (var step = levelIndex-1; step >= 0; step--) {
            var currentActions = GetConflictFreeSubsetOfIncomingNodes(currentState);
            
            currentActions.ForEach(c => ((GpActionNode)c).SpecifyBackward());
            currentActions = GetConflictFreeSubset(currentActions);
            
            if (currentActions.Count == 0) {
                noGoods.Add(new(levelIndex, currentState));
                return null;
            }

            currentState = GetConflictFreeSubsetOfIncomingNodes(currentActions);
            
            var possibleActions = currentActions.Select(actionNode => ((GpActionNode)actionNode).GpAction).ToList();
            solution.Add(step, possibleActions);
        }
        
        solution.Reverse();
        return solution;
    }



    private List<GpNode> GetConflictFreeSubsetOfIncomingNodes(List<GpNode> nodes) {
        var incomingNodes = nodes.SelectMany(node => node.InEdges).ToList();
        return GetConflictFreeSubset(incomingNodes);
    }

    private List<GpNode> GetConflictFreeStateFromGoals(List<GpStateNode> state, List<ISentence> goals) {
        var stateNodesSubset = GetSatisfiedState(state, goals);
        return stateNodesSubset == null ? null : GetConflictFreeSubset(stateNodesSubset);
    }

    private List<GpNode> GetConflictFreeSubset(List<GpNode> nodes) {
        return nodes.Where(actionNode => !actionNode.MutexRelation.Any(nodes.Contains)).ToList();
    }

    private List<GpNode> GetSatisfiedState(List<GpStateNode> state, List<ISentence> satSentences) {
        var nodesSubset = new List<GpNode>();
        foreach (var sentence in satSentences) {
            nodesSubset.AddRange(state.Where(node => sentence.Match(node.Literal,out var u))); //its more like the unifiyable thing ?
        }

        return nodesSubset.Count != satSentences.Count ? null : nodesSubset;
    }

    public bool Stabilized(int levelIndex) {
        return _layers.Count >= 2 && _layers[levelIndex].EqualStateLiterals(_layers[levelIndex - 1]);
    }

    public void ExpandGraph() {
        var lastLayer = _layers.Last().Value;
        lastLayer.ExpandActionNodes(actions);
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