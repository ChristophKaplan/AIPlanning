using AIPlanning.Planning.GraphPlan;

var initialState = GpActionFabric.StringToSentence(["NOT (Have(Money))", "Job(McDonalds)", "Victim(Joe)"]);
var goals = GpActionFabric.StringToSentence(["Have(Money)"]);

var work = GpActionFabric.Create("Work", ["Job(x)"], ["Have(Money)"]);
var rob = GpActionFabric.Create("Rob", ["Victim(x)"], ["Have(Money)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [work, rob]);