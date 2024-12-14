using AIPlanning.Planning.GraphPlan;
using Helpers;


var initialState = GpActionFabric.StringToSentence(["At(Work)", "NOT At(Supermarket)", "NOT At(Home)"]);
var goals = GpActionFabric.StringToSentence(["Have(Apple)", "Have(Cake)", "At(Home)"]);

var work = GpActionFabric.Create("Work", ["At(Work)"], ["Have(Money)"]);
var buy = GpActionFabric.Create("Buy", ["At(Supermarket)", "Have(Money)"], ["Have(x)", "NOT Have(Money)"]);
var move = GpActionFabric.Create("Move", ["NOT At(x)", "At(y)"], ["At(x)", "NOT At(y)"]);

var graph = new GraphPlan();
var solution = graph.Run(initialState, goals, [move, work, buy]);
Logger.Log(solution.ToString()); 