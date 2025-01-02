using AIPlanning.Planning.GraphPlan;
using Helpers;

var initialState = GpActionFactory.StringToSentence(new(){"At(Subject1,Home)", "-At(Subject1,Supermarket)", "-At(Subject1,Work)", "Food(Cake)", "-Drink(Cake)", "Subject(Subject1)"});
var goals = GpActionFactory.StringToSentence(new(){"Have(Cake)", "At(Subject1,Home)"});

var work = GpActionFactory.Create("Work", new(){"At(z, Work)", "Subject(z)"}, new(){"Have(Money)"});
var buyFood = GpActionFactory.Create("BuyFood", new(){"At(z, Supermarket)", "Have(Money)", "Food(x)", "Subject(z)"}, new(){"Have(x)", "-Have(Money)"});
var move = GpActionFactory.Create("Move", new(){"-At(z, x)", "At(z, y)", "Subject(z)"}, new(){"At(z, x)", "-At(z, y)"});

var problem = new GpProblem(initialState, goals, new(){move, work, buyFood });
var solution = problem.Solve();
Logger.Log(solution.ToString());