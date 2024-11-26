using System.Text;

namespace FirstOrderLogic.Planning.GraphPlan;

public class Graph {
    private readonly Dictionary<int, Layer> _layers = new();
    private readonly List<Action> _actions;
    private readonly List<ISentence> _initialState;
    private readonly List<ISentence> _goal;
    public int NumLevels => _layers.Count;

    public Graph(List<ISentence> initialState, List<ISentence> goal, List<Action> actions) {
        _initialState = initialState;
        _goal = goal;
        _actions = actions;

        var initialLayer = new Layer(0);
        foreach (var sentence in _initialState) {
            initialLayer.TryAdd(new StateNode(0, sentence));
        }
        
        _layers.Add(0, initialLayer);
    }

    public bool StateNotMutex(int i, List<ISentence> goals) {
        var stateNodes = _layers[i].StateNodes;
        var state = GetConflictFreeStateFromGoals(stateNodes, goals);
        return state != null;
    }

    public List<Action> ExtractSolution(int levelIndex, List<(int level, List<Node> subGoalState)> nogoods) {
        var lastState = _layers[levelIndex].StateNodes;
        var conflictFreeStateFromGoals = GetConflictFreeStateFromGoals(lastState, _goal);

        var isSat = false;
        var currentState = conflictFreeStateFromGoals;
        var solution = new List<Action>();

        while (!isSat) {
            var actions = GetConflictFreeSubsetOfIncomingNodes(currentState);

            if (actions.Count == 0) {
                nogoods.Add(new(levelIndex, currentState));
                return null;
            }

            currentState = GetConflictFreeSubsetOfIncomingNodes(actions);

            isSat = currentState.All(s => s.Level == 0);
            solution.AddRange(actions.Select(n => (ActionNode)n).Select(n => n.Action));
        }

        solution.Reverse();
        return solution;
    }

    private List<Node> GetConflictFreeSubsetOfIncomingNodes(List<Node> nodes) {
        var incomingNodes = nodes.SelectMany(node => node.InEdges).ToList();
        return GetConflictFreeSubset(incomingNodes);
    }

    private List<Node> GetConflictFreeStateFromGoals(List<StateNode> state, List<ISentence> goals) {
        var stateNodesSubset = GetSatisfiedState(state, goals);
        return stateNodesSubset == null ? null : GetConflictFreeSubset(stateNodesSubset);
    }

    private List<Node> GetConflictFreeSubset(List<Node> nodes) {
        return nodes.Where(actionNode => !actionNode.MutexRelation.Any(nodes.Contains)).ToList();
    }

    private List<Node> GetSatisfiedState(List<StateNode> state, List<ISentence> satSentences) {
        var nodesSubset = new List<Node>();
        foreach (var sentence in satSentences) {
            nodesSubset.AddRange(state.Where(node => sentence.Equals(node.Literal)));
        }

        return nodesSubset.Count != satSentences.Count ? null : nodesSubset;
    }

    public bool Stabilized(int levelIndex) {
        return _layers.Count >= 2 && _layers[levelIndex].EqualStateLiterals(_layers[levelIndex - 1]);
    }

    public void ExpandGraph() {
        var lastLayer = _layers.Last().Value;
        lastLayer.ExpandActionNodes(_actions);
        var nextLayer = lastLayer.ExpandNextLayer();
        _layers.Add(nextLayer.Level, nextLayer);
    }

    public override string ToString() {
        var sb = new StringBuilder();
        foreach (var layer in _layers) {
            sb.AppendLine(layer.Value.ToString());
        }

        return sb.ToString();
    }
}