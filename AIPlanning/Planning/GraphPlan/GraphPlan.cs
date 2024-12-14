using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GraphPlan {
    public Solution Run(Problem problem) {
        var operatorGraph = new OperatorGraph(problem);
        operatorGraph.Init();
        
        var graph = new GpGraph(problem, operatorGraph);
        graph.Init();
        
        var noGoods = new NoGoods();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, problem.Goals)) {
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