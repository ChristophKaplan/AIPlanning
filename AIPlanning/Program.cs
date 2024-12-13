using AIPlanning.Planning.GraphPlan;

var initialState = GpActionFabric.StringToSentence(["At(Work)", "NOT At(Supermarket)"]);
var goals = GpActionFabric.StringToSentence(["Have(Apple)", "At(Home)"]);

var pickup = GpActionFabric.Create("PickUp", ["At(Supermarket)"], ["Have(x)"]);
var move = GpActionFabric.Create("Move", ["NOT At(x)", "At(y)"], ["At(x)", "NOT At(y)"]);

var moves = GpActionFabric.Create("MoveSupermarket", ["NOT At(Supermarket)", "At(x)"], ["At(Supermarket)", "NOT At(x)"]);
var moveh = GpActionFabric.Create("MoveHome", ["NOT At(Home)", "At(x)"], ["At(Home)", "NOT At(x)"]);


var graph = new GraphPlan();
//var solution = graph.Run(initialState, goals, [moves, moveh, pickup]);
var solution = graph.Run(initialState, goals, [move, pickup]);
