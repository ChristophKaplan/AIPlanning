using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public static class Extensions {
    public static bool Match(this ISentence sentence, ISentence other, out Unificator unificator) {
        unificator = null;
        if (sentence.IsNegationOf(other, true)) {
            return false;
        }

        var temp = new Unificator(other, sentence);
        var unify = temp.IsUnifiable;
        if (unify) unificator = temp;

        return unify;
    }
}