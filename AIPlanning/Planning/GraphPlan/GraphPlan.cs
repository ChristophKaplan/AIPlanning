using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GraphPlan {
    public Dictionary<int, ActionSet> Run(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions) {
        var graph = new GpGraph(initialState, goals, actions);
        graph.Init();
        
        NoGoods noGoods = new();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, goals)) {
                var solution = graph.ExtractSolution(levelIndex, noGoods);
                if (solution is { Count: > 0 }) {
                    Logger.Log($"Solution found: {solution.Aggregate("", (acc, keyValuePair) => acc + "\n STEP:" + keyValuePair.Key + " \n ACTIONS:\n" + keyValuePair.Value.GetActionNodes.Aggregate("", (acc, action) => acc + action + "\n"))}");
                    Logger.Log(graph.ToString());
                    return solution;
                }
            }

            if (noGoods.IsStable() && graph.Stable(levelIndex)) {
                Logger.Log("Graph stabilized, no solution!");
                return null;
            }

            graph.ExpandGraph();
            levelIndex++;
        }
    }
}