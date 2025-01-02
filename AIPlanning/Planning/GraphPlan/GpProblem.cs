using System.Collections.Generic;
using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan {
    public class GpProblem {
        public List<ISentence> Goals { get; private set; }
        public List<GpAction> Actions { get; private set; }
        public List<ISentence> InitialState { get; private set; }

        public GpProblem(List<ISentence> initialState, List<ISentence> goals, List<GpAction> actions) {
            InitialState = initialState;
            Goals = goals;
            Actions = actions;
        }

        public GpSolution Solve(GraphPlanAlgo graphPlanAlgo = null) {
            graphPlanAlgo ??= new GraphPlanAlgo(); //TODO: fix lazy initialization
            return graphPlanAlgo.Run(this);
        }
    }
}