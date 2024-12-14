using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public class OperatorGraph(Problem problem)
{
    private readonly GpActionNode _startNode = new(new GpAction("Start", new(), problem.InitialState));
    private readonly GpActionNode _finishNode = new(new GpAction("Finish", problem.Goals, new()));

    private readonly List<GpLiteralNode> _literalNodes = new();
    private readonly Dictionary<GpAction, GpActionNode> _operatorNodes = new(); //can probably be local in recursion
    private int useCountStop = 10;

    public void Init()
    {
        //init the effects of start as preconditions
        foreach (var preCon in _startNode.GpAction.Effects)
        {
            var preConNode = new GpLiteralNode(preCon);
            _literalNodes.Add(preConNode);
        }
        
        problem.Actions.Add(_startNode.GpAction);
        problem.Actions.Add(_finishNode.GpAction);
        
        ConstructRecursivly(_finishNode);

        var instanceMap = InstantiateActions();

        foreach (var literalNode in _literalNodes) {
            var allInstances = new List<GpAction>();
            for (var i = literalNode.OutEdges.Count-1; i >= 0; i--) {
                var outEdge = literalNode.OutEdges[i];
                if (outEdge is GpActionNode actionNode && instanceMap.TryGetValue(actionNode.GpAction, out var instances)) {
                    allInstances.AddRange(instances);
                    literalNode.OutEdges.Remove(actionNode);
                }
            }

            foreach (var instance in allInstances) {
                literalNode.ConnectTo(new GpActionNode(instance)); 
            }
        }
        
        Logger.Log("Operator Graph: " + this.ToString());
    }
    
    private bool TryGetMatchingLiteralNodes(ISentence literal, out List<GpLiteralNode> preConNodes, out List<Unificator> unificators) {
        var isMatch = false;
        unificators = new List<Unificator>();
        preConNodes = new List<GpLiteralNode>();
        
        foreach (var node in _literalNodes) {
            if (!node.Literal.Match(literal, out var uni)) {
                continue;
            }

            preConNodes.Add(node);
            unificators.Add(uni);
            isMatch = true;
        }

        return isMatch;
    }
    
    private Dictionary<GpAction, List<GpAction>> InstantiateActions() {
        var mapping = new Dictionary<GpAction, List<GpAction>>();
        foreach (var action in problem.Actions) {
            var conflictFreeUnificatorPossibilities = action.GetConflictFreeUnificatorPossibilities(action.Unificators);
            
            var possibleInstances = new List<GpAction>();
            foreach (var unificator in conflictFreeUnificatorPossibilities) {
                var clone = action.Clone();
                clone.SpecifyAction(unificator);
                if (clone.IsConsistent()) {
                    possibleInstances.Add(clone);
                }
            }
            
            mapping.Add(action, possibleInstances);
        }
        
        return mapping;
    }

    private void ConstructRecursivly(GpNode curNode)
    {
        switch (curNode)
        {
            case GpActionNode curOperator:
                MapPreConditionsToAction(curOperator);
                break;
            case GpLiteralNode curState:
                FindApplicableAction(curState);
                break;
        }
    }
    
    private void MapPreConditionsToAction(GpActionNode curAction)
    {
        //necessary preconditions of an action but not sufficient
        
        foreach (var preCon in curAction.GpAction.Preconditions)
        {
            if (!TryGetMatchingLiteralNodes(preCon, out var literalNodes, out var unificators)) {
                literalNodes = new List<GpLiteralNode>() { new (preCon) };
                _literalNodes.AddRange(literalNodes);
            }

            curAction.GpAction.Unificators.AddRange(unificators);

            foreach (var literalNode in literalNodes) {
                literalNode.ConnectTo(curAction);
                FindApplicableAction(literalNode);
            }
        }
    }

    private void FindApplicableAction(GpLiteralNode curLiteral)
    {
        foreach (var action in problem.Actions)
        {
            if (!IsEffectsApplicable(action, curLiteral.Literal))
            {
                continue;
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

            operatorNode.ConnectTo(curLiteral);
            MapPreConditionsToAction(operatorNode);
        }
    }
    
    private bool IsEffectsApplicable(GpAction action, ISentence literal)
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
            action.Unificators.AddRange(uniList);
            action.Unificators = action.Unificators.Distinct().ToList();
        }

        return action.IsConsistent();
    }

    public List<GpAction> GetActionsForLiteral(ISentence literal)
    {
        var instances = new List<GpAction>();
        var node = _literalNodes.FirstOrDefault(node => literal.Equals(node.Literal));
        if (node == null) {
            return instances;
        }

        var direct = node.OutEdges.Select(outEdge => ((GpActionNode)outEdge).GpAction).ToList();
        instances.AddRange(direct);

        return instances;
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

    public override string ToString()
    {
        string output = "Operator Graph\n";

        foreach (var preConNode in _literalNodes)
        {
            var preCon = preConNode.Literal;
            var outEdges = preConNode.OutEdges.Aggregate("", (acc, edge) => acc + $"\n\t\t\t\t{((GpActionNode)edge).GpAction},");
            output += $"Literal: {preCon} out:{preConNode.OutEdges.Count} -> [{outEdges}]\n";
        }

        return output;
    }
}