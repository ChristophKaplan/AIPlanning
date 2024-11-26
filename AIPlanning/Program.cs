using FirstOrderLogic.Planning.GraphPlan;

var initialState = ActionFabric.StringToSentence(["NOT (Haben(Kuchen))", "NOT (Gegessen(Kuchen))"]);
var goals = ActionFabric.StringToSentence(["Gegessen(Kuchen)"]);
var essen = ActionFabric.Create("Essen(Kuchen)", ["Haben(Kuchen)"], ["NOT (Haben(Kuchen))", "Gegessen(Kuchen)"]);
var backen = ActionFabric.Create("Backen(Kuchen)", ["NOT (Haben(Kuchen))"], ["Haben(Kuchen)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [essen, backen]);