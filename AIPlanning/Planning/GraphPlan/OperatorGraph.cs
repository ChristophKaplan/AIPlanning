using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class OperatorGraph(List<ISentence> initialState, List<ISentence> goal, List<GpAction> actions)
{
    private readonly GpActionNode _startNode = new(new GpAction("Start", new(), initialState));
    private readonly GpActionNode _finishNode = new(new GpAction("Finish", goal, new()));

    private readonly List<GpStateNode> _preconditionNodes = new();
    private readonly Dictionary<GpAction, GpActionNode> _operatorNodes = new();
    private int useCountStop = 10;

    public void Init()
    {
        actions.Add(_startNode.GpAction);
        actions.Add(_finishNode.GpAction);
        
        //_operatorNodes.Add(_startNode.GpAction, _startNode);
        //_operatorNodes.Add(_finishNode.GpAction, _finishNode);
        ConstructRecursivly(_finishNode);

        var a = CollectUnificators(_finishNode, new()); //check welche unis esüberhaupt gibt, 
        PropagateForward();

        var b = CollectUnificators(_finishNode, new()); //check welche unis esüberhaupt gibt, 
    }

    public bool TryGetPreConditionNode(ISentence literal, out GpStateNode preConNode)
    {
        foreach (var node in _preconditionNodes)
        {
            if (node.Literal.Equals(literal))
            {
                preConNode = node;
                return true;
            }
        }

        preConNode = null;
        return false;
    }

    private void ConstructRecursivly(GpNode curNode)
    {
        switch (curNode)
        {
            case GpActionNode curOperator:
                ConstructRecursivlyOperator(curOperator);
                break;
            case GpStateNode curState:
                ConstructRecursivlyState(curState);
                break;
        }
    }
    
    private void ConstructRecursivlyOperator(GpActionNode curOperator)
    {
        foreach (var preCon in curOperator.GpAction.Preconditions)
        {
            if (!TryGetPreConditionNode(preCon, out var preConNode))
            {
                preConNode = new GpStateNode(preCon);
                _preconditionNodes.Add(preConNode);
            }

            preConNode.ConnectTo(curOperator);
            ConstructRecursivly(preConNode);
        }
    }

    private void ConstructRecursivlyState(GpStateNode curState)
    {
        var actionInstances = actions.Select(action => new GpAction(action)).ToList();
        foreach (var action in actionInstances)
        {

            
            if (!IsApplicable(action, curState.Literal))
            {
                continue;
            }
            
            if (action.Equals(_startNode.GpAction))
            {
                var a = 1;
            }
            
            if (_operatorNodes.TryGetValue(action, out var operatorNode))
            {
                if (operatorNode.useCount > useCountStop)
                {
                    //Logger.Log($"Infinite loop detected {action}");
                    continue;
                }

                operatorNode.useCount++;
            }
            else
            {
                operatorNode = new GpActionNode(action);
                _operatorNodes.Add(action, operatorNode);
            }

            operatorNode.ConnectTo(curState);
            ConstructRecursivly(operatorNode);
        }
    }
    
    private bool IsApplicable(GpAction action, ISentence literal)
    {
        var uniList = new List<Unificator>();
        var isMatch = false;
        foreach (var effect in action.Effects)
        {
            var match = effect.Match(literal, out var uni);
            if (!match) continue;
            
            isMatch = true;
            if (!uni.IsEmpty) uniList.Add(uni);
        }

        if (!isMatch)
        {
            return false;
        }

        if (uniList.Count > 0)
        {
            //action.SpecifyAction(uniList[0]); //choose or all? is (always) usually 1
            action.Unificators.AddRange(uniList);
            action.Unificators = action.Unificators.Distinct().ToList();
        }

        return action.IsConsistent();
    }

    public List<GpAction> GetPossibleActionsFor(ISentence literal)
    {
        var instances = new List<GpAction>();

        if (TryGetPreConditionNode(literal, out var node))
        {
            var direct = node.OutEdges.Select(outEdge => ((GpActionNode)outEdge).GpAction).ToList();
            instances.AddRange(direct);
        }

        return instances;
        //wir müssen auch alle unifizierbaren hinzufügen

        //hack
        foreach (var preConNode in _preconditionNodes)
        {
            if (preConNode.Literal.Match(literal, out var uni))
            {
                var actions = preConNode.OutEdges.Select(outEdge => ((GpActionNode)outEdge).GpAction).ToList();
                foreach (var action in actions)
                {
                    var clone = action.Clone();

                    if (!uni.IsEmpty)
                    {
                        clone.SpecifyAction(uni);
                    }

                    if (clone.IsConsistent() && !instances.Contains(clone))
                    {
                        instances.Add(clone);
                    }
                }
            }
        }

        return instances;
    }

    private void PropagateForward()
    {
        foreach (var preConNode in _preconditionNodes)
        {
            var preCon = preConNode.Literal;
            var anyVar = preCon.GetPredicate().Terms.Any(t => t is Variable);
            if (!anyVar)
            {
                continue;
            }

            foreach (var inEdge in preConNode.InEdges)
            {
                var unificators = ((GpActionNode)inEdge).GpAction.Unificators;
                SpecifyNodes(preConNode, unificators[0], new());
            }
        }
    }

    private List<Unificator> CollectUnificators(GpNode node, List<GpNode> closed)
    {
        if (closed.Contains(node))
        {
            return new();
        }

        closed.Add(node);

        List<Unificator> uni = new();
        if (node is GpActionNode actionNode)
        {
            uni.AddRange(actionNode.GpAction.Unificators);
        }

        foreach (var inEdge in node.InEdges)
        {
            uni.AddRange(CollectUnificators(inEdge, closed));
        }

        foreach (var outEdge in node.OutEdges)
        {
            uni.AddRange(CollectUnificators(outEdge, closed));
        }

        return uni;
    }


    private void SpecifyNodes(GpNode node, Unificator unificator, List<GpNode> closed)
    {
        if (node is GpStateNode stateNode)
        {
            var literal = stateNode.Literal;
            unificator.Substitute(ref literal);
        }
        else if (node is GpActionNode actionNode)
        {
            actionNode.GpAction.SpecifyAction(unificator);
        }

        closed.Add(node);

        foreach (var outNode in node.OutEdges)
        {
            if (closed.Contains(outNode))
            {
                continue;
            }

            SpecifyNodes(outNode, unificator, closed);
        }
    }

    public override string ToString()
    {
        string output = "Operator Graph\n";

        foreach (var preConNode in _preconditionNodes)
        {
            var preCon = preConNode.Literal;
            var outEdges = preConNode.OutEdges.Aggregate("", (acc, edge) => acc + $"{((GpActionNode)edge).GpAction},");
            output += $"Precondition: {preCon} in:{preConNode.InEdges.Count} -> [{outEdges}]\n";
        }

        return output;
    }
}