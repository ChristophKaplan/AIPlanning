namespace AIPlanning.Planning.GraphPlan;


public enum MutexType {
    None,
    InconsistentSupport,
    LiteralNegation,
    Interference,
    ConflictingNeeds,
    InconsistentEffects
}

public struct MutexTo() {
    public MutexType Type;
    public GpNode Node;
    
    public MutexTo(MutexType type, GpNode node) : this() {
        Type = type;
        Node = node;
    }
    
    public override string ToString() {
        return $"{Type}";
    }
}

public abstract class GpNode {
    public List<GpNode> InEdges { get; } = [];
    public List<GpNode> OutEdges { get; } = [];
    public List<MutexTo> MutexRelation { get; } = [];
    
    public void AddInEdge(GpNode edge) {
        if(!InEdges.Contains(edge)) InEdges.Add(edge);
    }
    
    public void AddOutEdge(GpNode edge) {
        if(!OutEdges.Contains(edge)) OutEdges.Add(edge);
    }

    private void AddMutexRelation(MutexTo mutexTo) {
        if(!MutexRelation.Contains(mutexTo)) MutexRelation.Add(mutexTo);
    }
    
    public void ConnectTo(GpNode connectToMe) {
        AddOutEdge(connectToMe);
        connectToMe.AddInEdge(this);
    }

    public void MergeRelations(GpNode mergeTo) {
        foreach (var inNode in InEdges) {
            mergeTo.AddInEdge(inNode);
        }

        foreach (var outNode in OutEdges) {
            mergeTo.AddOutEdge(outNode);
        }

        foreach (var mutexNode in MutexRelation) {
            mergeTo.AddMutexRelation(mutexNode);
        }
    }
    
    public MutexType GetMutexType(GpNode other) {
        if (Equals(other)) return MutexType.None;

        if (this is GpStateNode s1 && other is GpStateNode s2) {
            if( s1.IsInconsistentSupport(s2)) return MutexType.InconsistentSupport;
            else if(s1.Literal.IsNegationOf(s2.Literal)) return MutexType.LiteralNegation;
        }

        if (this is GpActionNode a1 && other is GpActionNode a2) {
            if (a1.IsInconsistentEffects(a2)) return MutexType.InconsistentEffects;
            else if (a1.IsInterference(a2)) return MutexType.Interference;
            else if (a1.IsConflictingNeeds(a2)) return MutexType.ConflictingNeeds;
        }

        return MutexType.None;
    }

    public void TryAddMutexRelations(GpNode other, MutexType type) {
        AddMutexRelation(new MutexTo(type,other));
        other.AddMutexRelation(new MutexTo(type, this));
    }
}
