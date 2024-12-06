namespace AIPlanning.Planning.GraphPlan;

public class NoGoods {
    private Dictionary<int, BeliefState> noGoods = new();
    
    public void Add(int level, BeliefState subGoalState) {
        
        if(!noGoods.TryGetValue(level, out var beliefState)) {
            noGoods.Add(level, subGoalState);
            return;
        }
        
        throw new Exception("NoGoods already contains a belief state for this level");
    }
    
    public bool IsStable() {
        var count = noGoods.Count;
        if (count < 2) return false;
        var last = noGoods.ElementAt(count-1).Value; 
        var prevLast = noGoods.ElementAt(count-2).Value;
        return last.Equals(prevLast);
    }
}