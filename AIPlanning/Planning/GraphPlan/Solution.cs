using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class Solution {
    int levelIndex;
    private List<Dictionary<int, GpLayer>> _solutions = new ();
    public bool IsEmpty => _solutions.Count == 0;
    public Dictionary<int, ActionSet> GetOneSolution() {
        var solutionLayers = _solutions.Last();
        var solution = new Dictionary<int, ActionSet>();
        foreach (var solutionLayer in solutionLayers.Reverse()) {
            var actions = solutionLayer.Value.ActionSet;
            var step = solutionLayer.Key;
            solution.Add(step, actions);
        }
        
        return solution;
    }

    public override string ToString() {
        if (IsEmpty) {
            return "No solutions found!";
        }

        string result = "";
        foreach (var step in GetOneSolution()) {
            
            var actions = "";
            foreach (var actionNode in step.Value.GetActionNodes) {
                if(actionNode.IsPersistenceAction) continue;
                actions += actionNode + "\n";
            }
            
            result += "\n STEP:" + step.Key + " \n ACTIONS:\n" + actions;
        }

        var output = $"Found {_solutions.Count} solutions for level {levelIndex} \n " +
                     $"Solution: {result}";
        return output;
    }

    public void Add(Dictionary<int, GpLayer> outcomeBranch) {
        _solutions.Add(outcomeBranch);
    }
}