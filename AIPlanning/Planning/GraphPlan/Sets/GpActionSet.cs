using System;
using System.Collections.Generic;
using System.Linq;

namespace AIPlanning.Planning.GraphPlan {
    public class GpActionSet {
        private readonly List<GpActionNode> _actionNodes = new();
        public List<GpNode> GetNodes => _actionNodes.Select(n => (GpNode)n).ToList();
        public List<GpActionNode> GetActionNodes => _actionNodes;
        public List<GpAction> GetActions(bool ignorePersitance = true) => _actionNodes
            .Where(n => !ignorePersitance || !n.IsPersistenceAction)
            .Select(n => n.GpAction).ToList();

        public GpActionSet() {
        }

        public GpActionSet(List<GpNode> actionNodes) {
            _actionNodes = actionNodes.Select(n => (GpActionNode)n).ToList();
        }

        public void TryAdd(GpActionNode actionNode) {
            var contained = _actionNodes.FirstOrDefault(actionNode.Equals);
            if (contained != null) {
                actionNode.MergeRelations(contained);
                return;
            }

            _actionNodes.Add(actionNode);
        }

        public GpBeliefState GetConflictFreeState() {
            var incomingLitNodes = _actionNodes.SelectMany(node => node.InEdges).Distinct().ToList();
            var conflictFree = incomingLitNodes.GetConflictFreeSubset();
            return new GpBeliefState(conflictFree);
        }

        public GpBeliefState ExpandBeliefState() {
            var beliefState = new GpBeliefState();

            foreach (var actionNode in _actionNodes) {
                var literalNodes = actionNode.GpAction.Effects.Select(effect => new GpLiteralNode(effect));
                foreach (var literalNode in literalNodes) {
                    actionNode.ConnectTo(literalNode);
                    beliefState.TryAdd(literalNode);
                }
            }

            beliefState.GetNodes.CheckMutexRelations();
            return beliefState;
        }

        public override int GetHashCode() {
            var hash = 0;
            foreach (var actionNode in _actionNodes) {
                hash = HashCode.Combine(hash, actionNode);
            }

            return hash;
        }

        public override bool Equals(object? obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            return _actionNodes.SequenceEqual(((GpActionSet)obj)._actionNodes);
        }

        public override string ToString() {
            return _actionNodes.Aggregate("ActionSet:\n", (current, node) => current + $"\t{node}\n");
        }
    }
}