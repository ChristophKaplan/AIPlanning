using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class BeliefState {
    private readonly List<GpLiteralNode> nodes = new();
    public List<Unificator> Unificators { get; } = new();
    
    public BeliefState() { }
    
    public BeliefState(List<GpNode> nodes) {
        this.nodes = nodes.Select(n => (GpLiteralNode)n).ToList();
    }
    
    public List<GpNode> GetNodes => nodes.Select(n => (GpNode)n).ToList();
    
    public List<GpLiteralNode> GetStateNodes => nodes;
    
    public void TryAdd(GpLiteralNode literalNode) {
        var contained = nodes.FirstOrDefault(literalNode.Equals);
        if (contained != null) {
            literalNode.MergeRelations(contained);
            return;
        }

        nodes.Add(literalNode);
    }
    
    public List<GpNode> GetSubBeliefStateMatchingTo(List<ISentence> sentences) {
        var satisfiedPreconditionNodes = new List<GpNode>();

        foreach (var literal in sentences) {
            var applicableNode = nodes.FirstOrDefault(node => IsMatchAndAddUnificator(node.Literal, literal));
            if (applicableNode == null) {
                /*if (literal.IsNegation) {
                    Logger.Log($"Negation {literal} not found in state");
                    continue;
                }*/

                return null;
            }
            

            satisfiedPreconditionNodes.Add(applicableNode);
        }

        return satisfiedPreconditionNodes.Distinct().ToList();
    }
    
    private bool IsMatchAndAddUnificator(ISentence literal, ISentence sentence) {
        if (!literal.Match(sentence, out var unificator)) {
            return false;
        }

        if (!unificator.IsEmpty) {
            Unificators.Add(unificator);
        }
        
        return true;
    }
    
    private List<GpNode> GetReachedSubState(List<ISentence> goals) {
        //same as above ??
        var reachedSubState = new List<GpNode>();
        
        foreach (var goal in goals) {
            foreach (var stateNode in nodes) {
                if (stateNode.Literal.Equals(goal)) {
                    reachedSubState.Add(stateNode);
                }
            }
        }

        var isReached = reachedSubState.Count >= goals.Count;
        return isReached ? reachedSubState : null;
    }
    
    public bool EqualStateLiterals(BeliefState other) {
        var aSubsetB = nodes.All(a => other.nodes.Any(a.EqualLiteral));
        var bSubsetA = other.nodes.All(b => nodes.Any(b.EqualLiteral));
        return aSubsetB && bSubsetA;
    }
    
    public bool IsCFStateFromSentencesReachable(List<ISentence> sentences, out BeliefState conflictFreeState) {
        var reachedSubState = GetReachedSubState(sentences);
        
        if(reachedSubState == null) {
            conflictFreeState = null;
            return false;
        }
        var conflictFree = reachedSubState.GetConflictFreeSubset();
        conflictFreeState = new BeliefState(conflictFree);
        return conflictFree.Count == sentences.Count;
    }
    
    public List<ActionSet> GetCFIncomingActions() {
        var listOfInEdgesActionLists = nodes.Select(stateNode => stateNode.InEdges).ToList();

        var combinationOfEachActionPerInEdges = listOfInEdgesActionLists.GetCombinations().Select(c => c.Distinct().ToList()).ToList();
        
        var possibleActions = new List<ActionSet>();
        foreach (var combinedActionNodes in combinationOfEachActionPerInEdges) {
            if (combinedActionNodes.Count == 0) {
                continue;
            }

            bool isConflict = combinedActionNodes.IsConflictFree();
            if (!isConflict) {
                possibleActions.Add(new ActionSet(combinedActionNodes));
            }
        }

        return possibleActions;
    }

    public override int GetHashCode() {
        var hash = 17;
        foreach (var node in nodes) {
            hash = hash * 31 + node.GetHashCode();
        }

        return hash;
    }

    public override bool Equals(object? obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        
        return nodes.SequenceEqual(((BeliefState)obj).nodes);
    }
    
    public override string ToString() {
        var output = "BeliefState:\n";
        
        foreach (var node in nodes) {
            output += $"\t{node}\n";
        }
        
        return output;
    }
}