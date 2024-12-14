using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public class Problem
{
    public List<ISentence> Goals { get; private set; }
    public List<GpAction> Actions{ get; private set; }
    public List<ISentence> InitialState{ get; private set; }

    public Problem(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions)
    {
        this.InitialState = initialState;
        this.Goals = goals;
        this.Actions = actions;
    }

    public Solution Solve(GraphPlan graphPlan = null)
    { 
        if(graphPlan == null) graphPlan = new GraphPlan(); 
        return graphPlan.Run(this);
    }
}