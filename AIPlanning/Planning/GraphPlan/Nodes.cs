namespace FirstOrderLogic.Planning.GraphPlan;

public abstract class Node {
    public Layer Layer { get; set; }
    public List<Node> InEdges { get; set; }
    public List<Node> OutEdges { get; set; }
    public List<Node> MutexRelation { get; set; }
    
    protected Node(Layer layer) {
        Layer = layer;
        InEdges = new List<Node>();
        OutEdges = new List<Node>();
        MutexRelation = new List<Node>();
    }
    
    public void ConnectTo(Node node) {
        OutEdges.Add(node);
        node.InEdges.Add(this);
    }
}

public class ActionNode : Node {
    public bool IsPersistenceAction { get; set; }
    public Action Action { get; set; }
    public ActionNode(Layer layer, Action action, bool isPersistenceAction) : base(layer) {
        IsPersistenceAction = isPersistenceAction;
        Action = action;
    }
    
    public override string ToString() {
        return $"{Action.ToString()} [m:{MutexRelation.Count}]";
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if(obj is ActionNode actionNode) {
            return Layer.Equals(actionNode.Layer) && Action.Equals(actionNode.Action) && IsPersistenceAction == actionNode.IsPersistenceAction;
        }
        
        return false;
    }
}

public class StateNode : Node {
    public ISentence Literal { get; set; }
    public StateNode(Layer layer, ISentence literal) : base(layer) {
        Literal = literal;
    }
    
    public override string ToString() { 
        return $"{Literal.ToString()} [m:{MutexRelation.Count}]";
    }
    
    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override bool Equals(object? obj) {
        if(obj is StateNode stateNode) {
            return Layer.Equals(stateNode.Layer) && Literal.Equals(stateNode.Literal);
        }
        
        return false;
    }
    
    public bool EqualLiteral(StateNode stateNode) {
        return Literal.Equals(stateNode.Literal);
    }
}