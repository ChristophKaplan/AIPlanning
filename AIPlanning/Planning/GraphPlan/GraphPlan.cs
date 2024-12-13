using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class GraphPlan {
    public Dictionary<int, ActionSet> Run(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions) {
        var operatorGraph = new OperatorGraph(initialState, goals, actions);
        operatorGraph.Init();
        Logger.Log("Operator Graph: " + operatorGraph.ToString());
        return null;
        //check ob wir die variablen (x,y usw) schon im operator graph eliminieren können, das führt nur zu probleme.
        //operator graph zeigt keine möglichkeit vom supermark to home, stimmt w snicht
        
        var graph = new GpGraph(initialState, goals, actions, operatorGraph);
        graph.Init();

        NoGoods noGoods = new();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, goals)) {
                var solution = graph.ExtractSolution(levelIndex, noGoods);
                if (solution is { Count: > 0 }) {
                    Logger.Log(
                        $"Solution found: {solution.Aggregate("", (acc, keyValuePair) => acc + "\n STEP:" + keyValuePair.Key + " \n ACTIONS:\n" + keyValuePair.Value.GetActionNodes.Aggregate("", (acc, action) => acc + action + "\n"))}");
                    //Logger.Log(graph.ToString());
                    return solution;
                }
            }

            if (noGoods.IsStable() && graph.Stable(levelIndex)) {
                Logger.Log("Graph stabilized, no solution!");
                //Logger.Log(graph.ToString());
                return null;
            }

            graph.ExpandGraph();
            levelIndex++;
        }
    }
}