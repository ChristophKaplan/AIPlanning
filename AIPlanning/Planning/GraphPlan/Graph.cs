using System.Text;

namespace FirstOrderLogic.Planning.GraphPlan;

public struct Layer {
    public readonly char Type;
    public readonly int Level;

    public Layer(char type, int level) {
        this.Type = type;
        this.Level = level;
    }

    public override string ToString() {
        return $"{Type}{Level}";
    }
}

public class Laier {
    public int level;
    private List<Node> state;
    private List<Node> action;
}

public class Graph {
    private readonly Dictionary<Layer, List<Node>> _nodes = new();
    private readonly List<Action> _actions;
    private List<ISentence> _initialState;
    private List<ISentence> _goal;
    public int NumLevels => _nodes.Count;

    public Graph(List<ISentence> initialState, List<ISentence> goal, List<Action> actions) {
        _initialState = initialState;
        _goal = goal;
        _actions = actions;

        var initialLayer = new Layer('S', 0);
        foreach (var sentence in _initialState) {
            var stateNode = new StateNode(initialLayer, sentence);
            TryAddToLayer(initialLayer, stateNode);
        }
    }

    public bool StateNotMutex(int i, List<ISentence> goals) {
        var elementAt = _nodes.FirstOrDefault(n => n.Key.Type == 'S' && n.Key.Level == i);
        var stateNodes = elementAt.Value.Select(n => (StateNode)n).ToList();
        var state = GetConflictFreeStateFromGoals(stateNodes, goals);
        return state != null;
    }

    public List<Action> ExtractSolution(int numLevels, List<(int level, List<Node> subGoalState)> nogoods) {
        var elementAt = _nodes.LastOrDefault(n => n.Key.Type == 'S');
        var lastState = elementAt.Value.Select(n => (StateNode)n).ToList();

        var conflictFreeStateFromGoals = GetConflictFreeStateFromGoals(lastState, _goal);

        var isSat = false;
        var currentState = conflictFreeStateFromGoals;
        var solution = new List<Action>();

        while (!isSat) {
            var actions = GetConflictFreeSubsetOfIncomingNodes(currentState);

            if (actions.Count == 0) {
                nogoods.Add(new(numLevels, currentState));
                return null;
            }

            currentState = GetConflictFreeSubsetOfIncomingNodes(actions);

            isSat = currentState.All(s => s.Layer.Level == 0);
            solution.AddRange(actions.Select(n => (ActionNode)n).Select(n => n.Action));
        }

        solution.Reverse();
        return solution;
    }

    private void TryAddToLayer(Layer layer, Node node) {
        _nodes.TryGetValue(layer, out var layerNodes);
        if (layerNodes == null) {
            layerNodes = new List<Node>();
            _nodes.Add(layer, layerNodes);
        }

        var contained = layerNodes.FirstOrDefault(node.Equals);
        if (contained != null) {
            MergeRelations(node, contained);
            return;
        }

        layerNodes.Add(node);
    }

    private void MergeRelations(Node mergeFrom, Node mergeTo) {
        foreach (var inNode in mergeFrom.InEdges) {
            mergeTo.InEdges.Add(inNode);
        }

        foreach (var outNode in mergeFrom.OutEdges) {
            mergeTo.OutEdges.Add(outNode);
        }

        foreach (var mutexNode in mergeFrom.MutexRelation) {
            mergeTo.MutexRelation.Add(mutexNode);
        }
    }

    private List<Node> GetConflictFreeSubsetOfIncomingNodes(List<Node> nodes) {
        var incomingNodes = nodes.SelectMany(node => node.InEdges).ToList();
        return GetConflictFreeSubset(incomingNodes);
    }

    private List<Node> GetConflictFreeStateFromGoals(List<StateNode> state, List<ISentence> goals) {
        var stateNodesSubset = GetSatisfiedState(state, goals);
        return stateNodesSubset == null ? null : GetConflictFreeSubset(stateNodesSubset);
    }

    private List<Node> GetConflictFreeSubset(List<Node> nodes) {
        return nodes.Where(actionNode => !actionNode.MutexRelation.Any(nodes.Contains)).ToList();
    }

    private List<Node> GetSatisfiedState(List<StateNode> state, List<ISentence> satSentences) {
        var nodesSubset = new List<Node>();
        foreach (var sentence in satSentences) {
            nodesSubset.AddRange(state.Where(node => sentence.Equals(node.Literal)));
        }

        return nodesSubset.Count != satSentences.Count ? null : nodesSubset;
    }

    public bool Stabilized() {
        var count = _nodes.Count;

        if (count < 3) {
            return false;
        }

        var lastState = _nodes.ElementAt(count - 1);
        if (lastState.Key.Type != 'S') {
            throw new Exception("last layer is not a state layer");
        }

        var prevState = _nodes.ElementAt(count - 3);
        var balanced = EqualLit(prevState.Value.Select(n => (StateNode)n).ToList(), lastState.Value.Select(n => (StateNode)n).ToList());

        return balanced;
    }

    private bool EqualLit(List<StateNode> stateA, List<StateNode> stateB) {
        var aSubsetB = stateA.All(a => stateB.Any(a.EqualLiteral));
        var bSubsetA = stateB.All(b => stateA.Any(b.EqualLiteral));
        return aSubsetB && bSubsetA;
    }

    public void ExpandGraph() {
        ExpandLastLayer();
        ExpandLastLayer();
    }

    private void ExpandLastLayer() {
        var currentLayer = _nodes.Last();
        switch (currentLayer.Key.Type) {
            case 'S':
                ExpandStateNodes(currentLayer.Key, currentLayer.Value.Select(n => (StateNode)n).ToList());
                break;
            case 'A':
                ExpandActionNodes(currentLayer.Key, currentLayer.Value.Select(n => (ActionNode)n).ToList());
                break;
        }
    }

    private void ExpandStateNodes(Layer layer, List<StateNode> stateNodeList) {
        var newLayer = new Layer('A', layer.Level);
        
        foreach (var action in _actions) {
            if (!action.IsApplicable(stateNodeList, out var satisfiedPreconditions)) {
                continue;
            }

            var actionNode = new ActionNode(newLayer, action, false);
            satisfiedPreconditions.ForEach(preCon => preCon.ConnectTo(actionNode));
            TryAddToLayer(newLayer, actionNode);
        }

        foreach (var stateNode in stateNodeList) {
            
            var action = new Action("Persist", new List<ISentence> { stateNode.Literal }, new List<ISentence> { stateNode.Literal });
            var actionNode = new ActionNode(newLayer, action, true);
            stateNode.ConnectTo(actionNode);
            TryAddToLayer(newLayer, actionNode);
        }

        CheckMutexRelations(newLayer);
    }

    private void ExpandActionNodes(Layer layer, List<ActionNode> actionNodes) {
        var newLayer = new Layer('S', layer.Level + 1);
        foreach (var actionNode in actionNodes) {
            foreach (var effect in actionNode.Action.Effects) {
                var effectNode = new StateNode(newLayer, effect);
                actionNode.ConnectTo(effectNode);
                TryAddToLayer(newLayer, effectNode);
            }
        }

        CheckMutexRelations(newLayer);
    }

    private void CheckMutexRelations(Layer layer) {
        _nodes.TryGetValue(layer, out var layerNodes);
        if (layerNodes == null) {
            return;
        }
        
        for (var i = 0; i < layerNodes.Count; i++) {
            for (var j = i+1; j < layerNodes.Count; j++) {
                var nodeA = layerNodes[i];
                var nodeB = layerNodes[j];
                if (!nodeA.Equals(nodeB) && nodeA.IsMutex(nodeB)) {
                    nodeA.TryAddMutexRelations(nodeB);
                }
            }
        }
    }
    
    public override string ToString() {
        var sb = new StringBuilder();
        foreach (var layer in _nodes) {
            sb.AppendLine($"Layer {layer.Key}");
            foreach (var node in layer.Value) {
                sb.AppendLine(node.ToString());
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}