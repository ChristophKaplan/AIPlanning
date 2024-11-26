using FirstOrderLogic.Planning.GraphPlan;

var initialState = GpActionFabric.StringToSentence(["NOT (Haben(Kuchen))", "NOT (Gegessen(Kuchen))"]);
var goals = GpActionFabric.StringToSentence(["Gegessen(Kuchen)"]);
var essen = GpActionFabric.Create("Essen(Kuchen)", ["Haben(Kuchen)"], ["NOT (Haben(Kuchen))", "Gegessen(Kuchen)"]);
var backen = GpActionFabric.Create("Backen(Kuchen)", ["NOT (Haben(Kuchen))"], ["Haben(Kuchen)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [essen, backen]);