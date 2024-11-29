using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GraphPlan {
    public Dictionary<int, List<GpAction>> Run(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions) {
        var graph = new GpGraph(initialState, goals, actions);
        graph.Init();
        
        List<(int level, List<GpNode> subGoalState)> noGoods = new();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, goals)) {
                var solution = graph.ExtractSolution(levelIndex, noGoods);
                if (solution is { Count: > 0 }) {
                    Logger.Log($"Solution found: {solution.Aggregate("", (acc, keyValuePair) => acc + "\n STEP:" + keyValuePair.Key + " \n ACTIONS:\n" + keyValuePair.Value.Aggregate("", (acc, action) => acc + action + "\n"))}");
                    Logger.Log(graph.ToString());
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