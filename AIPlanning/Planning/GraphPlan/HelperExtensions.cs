using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan;

public static class HelperExtensions {
    public static bool Match(this ISentence sentence, ISentence other, out Unificator unificator) {
        unificator = null;
        if (sentence.IsNegationOf(other, true)) { return false; }

        var temp = new Unificator(other, sentence);
        var isUnifiable = temp.IsUnifiable;
        
        if (isUnifiable) {
            unificator = temp;
        }

        return isUnifiable;
    }

    public static bool IsNegationOfAndMatch(this ISentence sentence, ISentence other, out Unificator unificator) {
        unificator = null;
        if (!sentence.IsNegationOf(other, true)) {
            return false;
        }

        var temp = new Unificator(other, sentence);
        var isUnifiable = temp.IsUnifiable;
        if (isUnifiable) {
            unificator = temp;
        }

        return isUnifiable;
    }

    public static List<GpNode> GetConflictFreeSubset(this List<GpNode> nodes) {
        return nodes.Where(node => !node.MutexRelation.Any(mutexTo => nodes.Contains(mutexTo.ToNode))).ToList();
    }

    public static bool IsConflictFree(this List<GpNode> nodes) {
        return !nodes.Any(node => node.MutexRelation.Any(mutexTo => nodes.Contains(mutexTo.ToNode)));
    }

    public static void CheckMutexRelations(this List<GpNode> nodes) {
        for (var i = 0; i < nodes.Count; i++) {
            for (var j = i + 1; j < nodes.Count; j++) {
                var nodeA = nodes[i];
                var nodeB = nodes[j];
                if (!nodeA.Equals(nodeB)) {
                    var mutexType = nodeA.GetMutexType(nodeB);
                    if (mutexType != MutexType.None) {
                        nodeA.TryAddMutexRelations(nodeB, mutexType);
                    }
                }
            }
        }
    }

    public static List<List<T>> GetCombinations<T>(this List<List<T>> lists) {
        var c = lists.CartesianProduct().Select(l => l.ToList()).ToList();
        return c;
    }

    private static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences) {
        IEnumerable<IEnumerable<T>> emptyProduct = [[]];
        return sequences.Aggregate(emptyProduct,
            (accumulator, sequence) => from accseq in accumulator from item in sequence select accseq.Concat([item]));
    }
}