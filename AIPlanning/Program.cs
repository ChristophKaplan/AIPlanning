using FirstOrderLogic;
using FirstOrderLogic.Planning.GraphPlan;
using Action = FirstOrderLogic.Planning.GraphPlan.Action;

/*
var satplan = new SATPLan();

var given = new List<string>() {
    "HaveIngredients^0",
    "NOT Food^0",
    "Hungry^0",
    "Cook^0",
};

var transitions = new List<string>() {
    "Food^1 <=> Cook^0 AND HaveIngredients^0",
    "Hungry^1 <=> NOT Eat^0",
    //"Cook^0 => (HaveIngredients^0 AND Food^1 AND (NOT HaveIngredients^1))",
    //"Eat^0 => (Food^0 AND (NOT Hungry^1) AND (NOT Food^1))",
    //"(NOT Eat^0) OR (NOT Food^0)",
    //"(NOT HaveIngredients^0) => (NOT HaveIngredients^1)",
};

var goal = new List<string>() {
    "NOT (Hungry^2)",
};

var actions = satplan.Run(given, transitions, goal);

foreach (var action in actions) {
    Logger.Log(action.ToString());
}
*/

FirstOrderLogic.FirstOrderLogic logic = new FirstOrderLogic.FirstOrderLogic();

var habenKuchen = (ISentence)logic.TryParse("Haben(Kuchen)");
var notHabenKuchen = (ISentence)logic.TryParse("NOT (Haben(Kuchen))");
var gegessenKuchen = (ISentence)logic.TryParse("Gegessen(Kuchen)");
var notGegessenKuchen = (ISentence)logic.TryParse("NOT (Gegessen(Kuchen))");

var initialState = new List<ISentence>() { notHabenKuchen, notGegessenKuchen };
var goals = new List<ISentence>() { gegessenKuchen };

var actions = new List<Action>() {
    new("Essen(Kuchen)", new List<ISentence>() { habenKuchen }, new List<ISentence>() { notHabenKuchen, gegessenKuchen }),
    new("Backen(Kuchen)", new List<ISentence>() { notHabenKuchen }, new List<ISentence>() { habenKuchen })
};

var graph = new GraphPlan();
graph.Run(initialState, goals, actions);