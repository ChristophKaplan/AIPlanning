using Helpers;

namespace FirstOrderLogic.Planning.GraphPlan;

public class GraphPlan {
    public List<Action> Run(List<ISentence> initialState, List<ISentence> goals, List<Action> actions) {
        var graph = new Graph(initialState, goals, actions);
        List<(int level, List<Node> subGoalState)> nogoods = new();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, goals)) {
                var solution = graph.ExtractSolution(levelIndex, nogoods);
                if (solution is { Count: > 0 }) {
                    Logger.Log($"Solution found: {solution.Aggregate("", (acc, action) => acc +"\n"+ action.ToString()) }");
                    return solution;
                }
            }

            if (graph.Stabilized(levelIndex)) {
                Logger.Log("Graph stabilized, no solution!");
                Logger.Log(graph.ToString());
                return null;
            }

            graph.ExpandGraph();
            levelIndex++;
        }
    }
}