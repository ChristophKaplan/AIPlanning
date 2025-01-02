using System.Collections.Generic;
using System.Linq;

namespace AIPlanning.Planning.GraphPlan {
    public class NoGoods {
        private Dictionary<int, GpBeliefState> noGoods = new();

        public void Add(int level, GpBeliefState subGoalState) {
            if (noGoods.TryGetValue(level, out var beliefState)) {
                return;
            }

            noGoods.Add(level, subGoalState);
        }

        public bool IsStable() {
            var count = noGoods.Count;
            if (count < 2) {
                return false;
            }

            var last = noGoods.ElementAt(count - 1).Value;
            var prevLast = noGoods.ElementAt(count - 2).Value;
            return last.Equals(prevLast);
        }
    }
}