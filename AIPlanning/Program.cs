using AIPlanning.Planning.GraphPlan;

var initialState = GpActionFabric.StringToSentence(["At(Work)"]);
var goals = GpActionFabric.StringToSentence(["Have(Apple)", "At(Home)"]);

var pickup = GpActionFabric.Create("PickUp", ["At(Supermarket)"], ["Have(x)"]);
var moves = GpActionFabric.Create("MoveSupermarket", ["At(x)"], ["At(Supermarket)", "NOT At(x)"]);
var moveh = GpActionFabric.Create("MoveHome", ["At(x)"], ["At(Home)", "NOT At(x)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [moves, moveh, pickup]);