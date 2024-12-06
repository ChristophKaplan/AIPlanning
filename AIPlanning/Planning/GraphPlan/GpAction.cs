using FirstOrderLogic;
using Helpers;

namespace AIPlanning.Planning.GraphPlan;

public interface IGpAction {
    string Signifier { get; }
    List<ISentence> Preconditions { get; }
    List<ISentence> Effects { get; }
    bool IsApplicable(List<GpStateNode> stateNodes, out List<GpNode> satisfiedPreconditionNodes);
}

public class GpAction(string name, List<ISentence> preconditions, List<ISentence> effects) : IGpAction {
    public string Signifier { get; } = name;
    public List<ISentence> Preconditions { get; } = preconditions;
    public List<ISentence> Effects { get; } = effects;
    public List<Unificator> PreConUnificators { get; } = new();
    public List<Unificator> EffectUnificators { get; } = new();

    public bool IsApplicable(List<GpStateNode> stateNodes, out List<GpNode> satisfiedPreconditionNodes) {
        satisfiedPreconditionNodes = GetMatchingNodes(stateNodes, Preconditions);
        return satisfiedPreconditionNodes != null;
    }

    private List<GpNode> GetMatchingNodes(List<GpStateNode> stateNodes, List<ISentence> preconditions) {
        var satisfiedPreconditionNodes = new List<GpNode>();

        foreach (var preCon in preconditions) {
            var applicableNode = stateNodes.FirstOrDefault(node => ApplicableSingle(node.Literal, preCon));
            if (applicableNode == null) {
                
                if (preCon.IsNegation) {
                    Logger.Log($"Negation {preCon} not found in state");
                    
                    var isContainedPositivly = stateNodes.Any(sn => sn.Literal.Equals(preCon.Children[0]));
                    if (!isContainedPositivly) {
                        Logger.Log($"Negation {preCon} added to state");
                        var negation = new GpStateNode(preCon);
                        stateNodes.Add(negation);
                        satisfiedPreconditionNodes.Add(negation);
                    }

                    continue;
                }

                return null;
            }

            satisfiedPreconditionNodes.Add(applicableNode);
        }

        return satisfiedPreconditionNodes.Distinct().ToList();
    }

    private bool ApplicableSingle(ISentence literal, ISentence preCon) {
        if (!literal.Match(preCon, out var unificator)) {
            return false;
        }

        if (!unificator.IsEmpty) {
            PreConUnificators.Add(unificator);
        }
        
        return true;
    }

    public GpAction(GpAction action) : this(action.Signifier,
        action.Preconditions.Select(p => p.Clone()).ToList(),
        action.Effects.Select(e => e.Clone()).ToList()) {
    }
    
    /*public void SpecifyPrecon() {
        //specification macht noch probleme, führt zu inkonsistenzen actions
        
        foreach (var preCon in Preconditions) {
            foreach (var uni in PreConUnificators) {
                var tempPreCon = preCon;
                uni.Substitute(ref tempPreCon);
            }
        }

        foreach (var effect in Effects) {
            foreach (var uni in PreConUnificators) {
                var tempEffect = effect;
                uni.Substitute(ref tempEffect);
            }
        }
    }*/
    
    public void SpecifyEffects() {
        //specification macht noch probleme, führt zu inkonsistenzen actions
        if (PreConUnificators.Count > 1) {
            //throw new Exception("Multiple unificators for preconditions ?");
        }
        
        foreach (var preCon in Preconditions) {
            foreach (var uni in EffectUnificators) {
                var tempPreCon = preCon;
                uni.Substitute(ref tempPreCon);
            }
        }

        foreach (var effect in Effects) {
            foreach (var uni in EffectUnificators) {
                var tempEffect = effect;
                uni.Substitute(ref tempEffect);
            }
        }
    }
    
    public void SpecifyEffects_REVERSE() {
        foreach (var preCon in Preconditions) {
            foreach (var uni in EffectUnificators) {
                var tempPreCon = preCon;
                uni.SubstituteReverse(ref tempPreCon);
            }
        }

        foreach (var effect in Effects) {
            foreach (var uni in EffectUnificators) {
                var tempEffect = effect;
                uni.SubstituteReverse(ref tempEffect);
            }
        }
    }
    

    public bool IsConsistent() {
        var pre = Preconditions.Any(p1 => Preconditions.Any(p2 => p1.IsNegationOf(p2)));
        var eff = Effects.Any(eff1 => Effects.Any(eff2 => eff1.IsNegationOf(eff2)));
        return !(pre || eff);
    }

    public override string ToString() {
        return $"{Signifier} {string.Join(",", Preconditions)} -> {string.Join(",", Effects)}";
    }

    public override int GetHashCode() {
        return ToString().GetHashCode();
    }

    public override bool Equals(object? obj) {
        return ToString().Equals(obj.ToString());
    }
}