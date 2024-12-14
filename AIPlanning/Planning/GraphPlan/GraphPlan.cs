using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GraphPlan {
    public Solution Run(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions) {
        var operatorGraph = new OperatorGraph(initialState, goals, actions);
        operatorGraph.Init();
        
        var graph = new GpGraph(initialState, goals, actions, operatorGraph);
        graph.Init();
        
        NoGoods noGoods = new();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, goals)) {
                var solution = graph.ExtractSolution(levelIndex, noGoods);
                if (!solution.IsEmpty) { 
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