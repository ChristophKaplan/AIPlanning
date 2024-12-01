using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;


public class NoGoods {
    private Dictionary<int, List<GpNode>> noGoods = new();
    
    public void Add(int level, List<GpNode> subGoalState) {
        if(!noGoods.TryGetValue(level, out var list)) {
            noGoods.Add(level, subGoalState);
            return;
        }
        
        list.AddRange(subGoalState);
    }

    public bool Contains(int key, List<GpNode> noGood) {
        return noGoods.ContainsKey(key) && noGoods[key].Count == noGood.Count;
    }
    
    public bool IsStable() {
        var count = noGoods.Count;
        if (count < 2) return false;
        var last = noGoods.ElementAt(count-1).Value; 
        var prevLast = noGoods.ElementAt(count-2).Value;
        return last.SequenceEqual(prevLast);
    }
}

public class GraphPlan {
    public Dictionary<int, List<GpAction>> Run(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions) {
        var graph = new GpGraph(initialState, goals, actions);
        graph.Init();
        
        NoGoods noGoods = new();
        var levelIndex = 0;

        while (true) {
            if (graph.StateNotMutex(levelIndex, goals)) {
                var solution = graph.ExtractSolution(levelIndex, noGoods);
                if (solution is { Count: > 0 }) {
                    Logger.Log($"Solution found: {solution.Aggregate("", (acc, keyValuePair) => acc + "\n STEP:" + keyValuePair.Key + " \n ACTIONS:\n" + keyValuePair.Value.Aggregate("", (acc, action) => acc + action + "\n"))}");
                    //Logger.Log(graph.SolutionToString());
                    Logger.Log(graph.ToString());
                    return solution;
                }
            }

            if (noGoods.IsStable() && graph.Stable(levelIndex)) {
                Logger.Log("Graph stabilized, no solution!");
                Logger.Log(graph.ToString());
                return null;
            }

            graph.ExpandGraph();
            levelIndex++;
        }
    }
}