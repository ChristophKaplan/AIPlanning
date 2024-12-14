namespace AIPlanning.Planning.GraphPlan;

public class GraphPlanAlgo {
    public GpSolution Run(GpProblem problem) {
        var graph = new GpPlanGraph(problem);

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
                //Logger.Log("Graph stabilized, no solution!");
                return null;
            }

            graph.ExpandGraph();
            levelIndex++;
        }
    }
}