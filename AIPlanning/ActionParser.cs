namespace AIPlanning;

using System.Text;
using LRParser.CFG;
using LRParser.Language;
using LRParser.Lexer;

public enum Terminal
{
    Open,
    Comma,
    Close,
    Identifier,
    Conjunction,
    Disjunction,
    Implication,
    Negation,
    Boolean,
    Quantifier,
    Biconditional,
    TimeAttribute
}

public enum NonTerminal
{
    LangObject,
    AtomicSentence,
    Term,
    TermList,
    TermListExt,
    TermExt,
    Sentence,
    AtomicSentenceExt,
    ComplexSentence,
    LogicalOperator,
    ComplexSentenceUnary
}

public class FirstOrderLogic : Language<Terminal, NonTerminal>
{
    public FirstOrderLogic() 
    {
        Console.OutputEncoding = Encoding.UTF8;
    }
    
    protected override TokenDefinition<Terminal>[] SetUpTokenDefinitions()
    {
        return new[]
        {
            new TokenDefinition<Terminal>(Terminal.Open, "\\("),
            new TokenDefinition<Terminal>(Terminal.Comma, ","),
            new TokenDefinition<Terminal>(Terminal.Close, "\\)"),
            new TokenDefinition<Terminal>(Terminal.Conjunction, "AND|&&"),
            new TokenDefinition<Terminal>(Terminal.Disjunction, "OR|\\|\\|"),
            new TokenDefinition<Terminal>(Terminal.Implication, "IMPLIES|=>"),
            new TokenDefinition<Terminal>(Terminal.Biconditional, "IFF|<=>"),
            new TokenDefinition<Terminal>(Terminal.Negation, "NOT|!|-"),
            new TokenDefinition<Terminal>(Terminal.Boolean, "TRUE|FALSE"),
            new TokenDefinition<Terminal>(Terminal.Quantifier, "FORALL|EXISTS"),
            new TokenDefinition<Terminal>(Terminal.TimeAttribute, "\\^[0-9]"),
            new TokenDefinition<Terminal>(Terminal.Identifier, "[a-zA-Z]+"),
        };
    }

    protected override void SetUpGrammar()
    {
        var ruleStart = AddProductionRule(SpecialNonTerminal.Start, NonTerminal.LangObject);
        var ruleLangObj = AddProductionRule(NonTerminal.LangObject, NonTerminal.Sentence);

        var ruleSentence = AddProductionRule(NonTerminal.Sentence, Terminal.Open, NonTerminal.Sentence, Terminal.Close);
    }


    
    public override ILanguageObject TryParse(string input)
    {
        var langObj = base.TryParse(input);
        return langObj;
    }
    
    public List<ILanguageObject> TryParse(List<string> inputList)
    {
        var langObjList = new List<ILanguageObject>();
        foreach (var input in inputList) {
            langObjList.Add(TryParse(input));
        }
        return langObjList;
    }
}