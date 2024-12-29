using AIPlanning.Planning.GraphPlan;
using Helpers;

var initialState = GpActionFactory.StringToSentence([
    "At(Subject1,Home)", "-At(Subject1,Supermarket)", "-At(Subject1,Work)", "Food(Cake)", "-Drink(Cake)", "Subject(Subject1)"]);
var goals = GpActionFactory.StringToSentence(["Have(Cake)", "At(Subject1,Home)"]);

var work = GpActionFactory.Create("Work", ["At(z, Work)", "Subject(z)"], ["Have(Money)"]);
var buyFood = GpActionFactory.Create("BuyFood", ["At(z, Supermarket)", "Have(Money)", "Food(x)", "Subject(z)"], ["Have(x)", "-Have(Money)"]);
var move = GpActionFactory.Create("Move", ["-At(z, x)", "At(z, y)", "Subject(z)"], ["At(z, x)", "-At(z, y)"]);

var problem = new GpProblem(initialState, goals, [move, work, buyFood ]);
var solution = problem.Solve();
Logger.Log(solution.ToString());