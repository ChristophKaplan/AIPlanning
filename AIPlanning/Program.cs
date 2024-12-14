using AIPlanning.Planning.GraphPlan;
using Helpers;

var initialState = GpActionFactory.StringToSentence(["At(Home)", "NOT At(Supermarket)", "NOT At(Work)"]);
var goals = GpActionFactory.StringToSentence(["Have(Apple)", "Have(Cake)", "At(Home)"]);

var work = GpActionFactory.Create("Work", ["At(Work)"], ["Have(Money)"]);
var buy = GpActionFactory.Create("Buy", ["At(Supermarket)", "Have(Money)"], ["Have(x)", "NOT Have(Money)"]);
var move = GpActionFactory.Create("Move", ["NOT At(x)", "At(y)"], ["At(x)", "NOT At(y)"]);

var problem = new GpProblem(initialState, goals, [move, work, buy]);
var solution = problem.Solve();
Logger.Log(solution.ToString());