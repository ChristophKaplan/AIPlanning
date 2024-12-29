using AIPlanning.Planning.GraphPlan;
using Helpers;

var initialState = GpActionFactory.StringToSentence(["At(Home)", "-At(Supermarket)", "-At(Work)", "Food(Cake)", "-Drink(Cake)"]);
var goals = GpActionFactory.StringToSentence(["Have(Cake)", "At(Home)"]);

var work = GpActionFactory.Create("Work", ["At(Work)"], ["Have(Money)"]);
var buyFood = GpActionFactory.Create("BuyFood", ["At(Supermarket)", "Have(Money)", "Food(x)"], ["Have(x)", "-Have(Money)"]);
var buyDrink = GpActionFactory.Create("BuyDrink", ["At(Supermarket)", "Have(Money)","Drink(x)"], ["Have(x)", "-Have(Money)"]);
var move = GpActionFactory.Create("Move", ["-At(x)", "At(y)"], ["At(x)", "-At(y)"]);

var problem = new GpProblem(initialState, goals, [move, work, buyFood, buyDrink]);
var solution = problem.Solve();
Logger.Log(solution.ToString());