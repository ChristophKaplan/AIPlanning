using System;
using System.Collections.Generic;
using System.Linq;
using FirstOrderLogic;

namespace AIPlanning.Planning.GraphPlan {
    public class GpBeliefState {
        private readonly List<GpLiteralNode> _literalNodes = new();

        public GpBeliefState() {
        }

        public GpBeliefState(List<GpNode> nodes) {
            _literalNodes = nodes.Select(n => (GpLiteralNode)n).ToList();
        }

        public List<GpNode> GetNodes => _literalNodes.Select(n => (GpNode)n).ToList();
        public List<GpLiteralNode> GetLiteralNodes => _literalNodes;

        public void TryAdd(GpLiteralNode literalNode) {
            var contained = _literalNodes.FirstOrDefault(literalNode.Equals);
            if (contained != null) {
                literalNode.MergeRelations(contained);
                return;
            }

            _literalNodes.Add(literalNode);
        }

        public List<GpNode> GetSubSetOfNodesMatching(List<ISentence> literals) {
            var subset = new List<GpNode>();

            foreach (var literal in literals) {
                var applicableNode = _literalNodes.FirstOrDefault(node => node.Literal.Equals(literal));
                if (applicableNode == null) {
                    return null;
                }

                subset.Add(applicableNode);
            }

            return subset.Distinct().ToList();
        }

        public bool EqualStateLiterals(GpBeliefState other) {
            var aSubsetB = _literalNodes.All(a => other._literalNodes.Any(a.EqualLiteral));
            var bSubsetA = other._literalNodes.All(b => _literalNodes.Any(b.EqualLiteral));
            return aSubsetB && bSubsetA;
        }

        public bool IsConflictFreeStateReachable(List<ISentence> literals, out GpBeliefState conflictFreeState) {
            var reachedSubState = GetSubSetOfNodesMatching(literals);

            if (reachedSubState == null) {
                conflictFreeState = null;
                return false;
            }

            var conflictFree = reachedSubState.GetConflictFreeSubset();
            conflictFreeState = new GpBeliefState(conflictFree);
            return conflictFree.Count == literals.Count;
        }

        public List<GpActionSet> GetPossibleConflictFreeActionSets() {
            var inEdgesActionLists = _literalNodes.Select(stateNode => stateNode.InEdges).ToList();
            var possibleCombinationsOfActions = inEdgesActionLists.GetCombinations().Select(c => c.Distinct().ToList()).ToList();

            var possibleActionSets = new List<GpActionSet>();

            foreach (var possibleActionNodes in possibleCombinationsOfActions) {
                if (possibleActionNodes.Count == 0) {
                    continue;
                }

                if (possibleActionNodes.IsConflictFree()) {
                    possibleActionSets.Add(new GpActionSet(possibleActionNodes));
                }
            }

            return possibleActionSets;
        }

        public override int GetHashCode() {
            return _literalNodes.Aggregate(0, HashCode.Combine);
        }

        public override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            return _literalNodes.SequenceEqual(((GpBeliefState)obj)._literalNodes);
        }

        public override string ToString() {
            var output = "BeliefState:\n";

            foreach (var node in _literalNodes) {
                output += $"\t{node}\n";
            }

            return output;
        }
    }
}