using System.Diagnostics.CodeAnalysis;

namespace AIPlanning.Planning.GraphPlan;

public enum MutexType {
    None,
    InconsistentSupport,
    LiteralNegation,
    Interference,
    CompetingNeeds,
    InconsistentEffects
}

public struct MutexTo() {
    public MutexType Type;
    public GpNode Node;
    
    public MutexTo(MutexType type, GpNode node) : this() {
        Type = type;
        Node = node;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj is MutexTo other && Type == other.Type && Node.Equals(other.Node);
    }
    
    public override int GetHashCode() {
        return HashCode.Combine(Type, Node);
    }

    public override string ToString() {
        return $"{Type}";
    }
}

public abstract class GpNode {
    public List<GpNode> InEdges { get; } = [];
    public List<GpNode> OutEdges { get; } = [];
    public HashSet<MutexTo> MutexRelation { get; } = [];

    private void AddInEdge(GpNode edge) {
        if(!InEdges.Contains(edge)) InEdges.Add(edge);
    }

    private void AddOutEdge(GpNode edge) {
        if(!OutEdges.Contains(edge)) OutEdges.Add(edge);
    }

    private void AddMutexRelation(MutexTo mutexTo) {
        MutexRelation.Add(mutexTo);
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

        if (this is GpLiteralNode s1 && other is GpLiteralNode s2) {
            if( s1.IsInconsistentSupport(s2)) return MutexType.InconsistentSupport;
            else if(s1.Literal.IsNegationOf(s2.Literal)) return MutexType.LiteralNegation;
        }

        if (this is GpActionNode a1 && other is GpActionNode a2) {
            if (a1.IsInconsistentEffects(a2)) return MutexType.InconsistentEffects;
            else if (a1.IsInterference(a2)) return MutexType.Interference;
            else if (a1.IsCompetingNeeds(a2)) return MutexType.CompetingNeeds;
        }

        return MutexType.None;
    }

    public void TryAddMutexRelations(GpNode other, MutexType type) {
        AddMutexRelation(new MutexTo(type,other));
        other.AddMutexRelation(new MutexTo(type, this));
    }
}
