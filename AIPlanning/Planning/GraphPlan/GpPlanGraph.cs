using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class GpPlanGraph {
    private OperatorGraph operatorGraph;
    private readonly Dictionary<int, GpLayer> _layers = new();
    private readonly GpProblem problem;

    public GpPlanGraph(GpProblem problem) {
        this.problem = problem;
        Init();
    }

    public void Init() {
        operatorGraph = new OperatorGraph(problem);

        var initialLayer = new GpLayer(0);
        foreach (var sentence in problem.InitialState) {
            initialLayer.TryAdd(new GpLiteralNode(sentence));
        }

        initialLayer.BeliefState.GetNodes.CheckMutexRelations();
        _layers.Add(0, initialLayer);
    }

    public bool StateNotMutex(int i, List<ISentence> sentences) {
        var stateNodes = _layers[i].BeliefState;
        return stateNodes.IsConflictFreeStateReachable(sentences, out var conflictFreeState);
    }

    public GpSolution ExtractSolution(int levelIndex, NoGoods noGoods) {
        var lastState = _layers[levelIndex].BeliefState;
        lastState.IsConflictFreeStateReachable(problem.Goals, out var currentState);

        var solutions = new GpSolution();
        FindSolutions(levelIndex, currentState, noGoods, new Dictionary<int, GpLayer>(), solutions);
        return solutions;
    }

    private void FindSolutions(int levelIndex, GpBeliefState curBeliefState, NoGoods noGoods, Dictionary<int, GpLayer> outcome, GpSolution solutions) {
        if (curBeliefState.GetNodes.Count == 0) {
            return;
        }

        var possibleConditionalActionSets = curBeliefState.GetPossibleConflictFreeActionSets();
        if (possibleConditionalActionSets.Count == 0) {
            noGoods.Add(levelIndex, curBeliefState);
            //Logger.Log("No possible actions found!");
            return;
        }

        levelIndex--;

        foreach (var possibleConditionalActions in possibleConditionalActionSets) {
            var preConditionalState = possibleConditionalActions.GetConflictFreeState();

            if (preConditionalState.GetNodes is not { Count: > 0 }) {
                //Logger.Log($"Dead End!\n{possiblePrevState}\n{possiblePrevActions}");
                continue;
            }

            var possibleLayer = new GpLayer(levelIndex, preConditionalState, possibleConditionalActions);
            var outcomeBranch = new Dictionary<int, GpLayer>(outcome) { { levelIndex, possibleLayer } };

            if (levelIndex == 0) {
                solutions.Add(outcomeBranch);
                return;
            }

            FindSolutions(levelIndex, preConditionalState, noGoods, outcomeBranch, solutions);
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
        return _layers.Aggregate("", (current, layer) => current + layer.Value);
    }
}