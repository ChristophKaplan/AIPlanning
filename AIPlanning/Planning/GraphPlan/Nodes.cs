namespace FirstOrderLogic.Planning.GraphPlan;

public abstract class Node {
    public int Level { get; set; }
    public List<Node> InEdges { get; set; }
    public List<Node> OutEdges { get; set; }
    public List<Node> MutexRelation { get; set; }
    
    protected Node(int level) {
        Level = level;
        InEdges = new List<Node>();
        OutEdges = new List<Node>();
        MutexRelation = new List<Node>();
    }
    
    public void ConnectTo(Node connectToMe) {
        OutEdges.Add(connectToMe);
        connectToMe.InEdges.Add(this);
    }

    public bool IsMutex(Node other) {
        if (Equals(other)) return false;

        return this switch {
            StateNode s1 when other is StateNode s2 => s1.IsInconsistentSupport(s2) || s1.Literal.IsNegationOf(s2.Literal),
            ActionNode a1 when other is ActionNode a2 => a1.IsInconsistentEffects( a2) || a1.IsInterference( a2) || a1.IsConflictingNeeds(a2),
            _ => throw new Exception("Invalid node type")
        };
    }
    
    public void TryAddMutexRelations(Node other) {
        if (!MutexRelation.Contains(other)) {
            MutexRelation.Add(other);
        }

        if (!other.MutexRelation.Contains(this)) {
            other.MutexRelation.Add(this);
        }
    }
}

public class ActionNode : Node {
    public bool IsPersistenceAction { get; set; }
    public Action Action { get; set; }
    public ActionNode(int level, Action action, bool isPersistenceAction) : base(level) {
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
            return Level.Equals(actionNode.Level) && Action.Equals(actionNode.Action) && IsPersistenceAction == actionNode.IsPersistenceAction;
        }
        
        return false;
    }
    
    public bool IsInconsistentEffects(ActionNode other) {
        return Action.Effects.Any(effect => other.Action.Effects.Any(effect.IsNegationOf));
    }

    public bool IsInterference(ActionNode other) {
        return Action.Effects.Any(effect => other.Action.Preconditions.Any(effect.IsNegationOf)) ||
               other.Action.Effects.Any(effect => Action.Preconditions.Any(effect.IsNegationOf));
    }

    public bool IsConflictingNeeds(ActionNode other) {
        return Action.Preconditions.Any(preCon => other.Action.Preconditions.Any(preCon.IsNegationOf));
    }
}

public class StateNode : Node {
    public ISentence Literal { get; set; }
    public StateNode(int level, ISentence literal) : base(level) {
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
            return Level.Equals(stateNode.Level) && Literal.Equals(stateNode.Literal);
        }
        
        return false;
    }
    
    public bool EqualLiteral(StateNode stateNode) {
        return Literal.Equals(stateNode.Literal);
    }
    
    public bool IsInconsistentSupport(StateNode other) {
        return InEdges.Any(inNode => other.InEdges.Any(inNode.IsMutex));
    }
}