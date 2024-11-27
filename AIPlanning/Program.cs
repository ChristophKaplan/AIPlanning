using FirstOrderLogic.Planning.GraphPlan;

var initialState = GpActionFabric.StringToSentence(["NOT (Have(Money))", "NOT (Job(McDonalds))", "NOT (Victim(Joe))"]);
var goals = GpActionFabric.StringToSentence(["Have(Money)"]);

var essen = GpActionFabric.Create("Work", ["Have(Money)"], ["Job(x)"]);
var backen = GpActionFabric.Create("Rob", ["Have(Money)"], ["Victim(x)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [essen, backen]);