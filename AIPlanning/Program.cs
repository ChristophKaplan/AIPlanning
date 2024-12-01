using AIPlanning.Planning.GraphPlan;

var initialState = GpActionFabric.StringToSentence(["At(Work)"]);
var goals = GpActionFabric.StringToSentence(["Have(Apple)", "At(Home)"]);

var pickup = GpActionFabric.Create("PickUp", ["At(Supermarket)"], ["Have(x)"]);
var moves = GpActionFabric.Create("MoveSupermarket", ["NOT At(Supermarket)", "At(x)"], ["At(Supermarket)", "NOT At(x)"]);
var moveh = GpActionFabric.Create("MoveHome", ["NOT At(Home)", "At(x)"], ["At(Home)", "NOT At(x)"]);
var move = GpActionFabric.Create("Move", ["NOT At(y)", "At(x)"], ["At(y)", "NOT At(x)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [moves, moveh/*,move*/, pickup]);

//works with the 2 moves, for a generic move we maybe need to specify backwards too ?