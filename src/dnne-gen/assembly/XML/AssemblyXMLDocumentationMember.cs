using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DNNE.Assembly.XML;

internal partial record class AssemblyXMLDocumentationMember
{
    [GeneratedRegex(@"N:(?<name>.+)")]
    private static partial Regex IDForNamespaceRegex();
    [GeneratedRegex(@"T:(?<namespace>[\w\.]+)\.(?<name>\w+)")]
    private static partial Regex IDForTypeRegex();
    [GeneratedRegex(@"F:(?<namespace>[\w\.]+)\.(?<enclosing_type>\w+)\.(?<name>\w+)")]
    private static partial Regex IDForFieldRegex();
    [GeneratedRegex(@"P:(?<namespace>[\w\.]+)\.(?<enclosing_type>\w+)\.(?<name>\w+)\(?(?<indexer_type>[\w\.,@*\[\]:]+)?\)?")]
    private static partial Regex IDForPropertyRegex();
    [GeneratedRegex(@"M:(?<namespace>[\w\.]+)\.(?<enclosing_type>\w+)\.#?(?<name>\w+)\(?(?<args>[\w\.,@*\[\]:]+)?\)?~?(?<operates_on>[\w\.]+)?")]
    private static partial Regex IDForMethodRegex();
    [GeneratedRegex(@"E:(?<namespace>[\w\.]+)\.(?<enclosing_type>\w+)\.(?<name>\w+)")]
    private static partial Regex IDForEventRegex();
    [GeneratedRegex(@"!:(?<error>.*)")]
    private static partial Regex IDForErrorRegex();

    public string ID { get; private init; }
    public AssemblyXMLDocumentationMemberType Type { get; private init; }
    private Match match;
    public string Name => Type switch
    {
        AssemblyXMLDocumentationMemberType.NAMESPACE => match.Groups["name"].Value.Split('.').Last(),
        _ => match.Groups["name"].Value
    };
    public string Namespace => Type switch
    {
        AssemblyXMLDocumentationMemberType.NAMESPACE => match.Groups["name"].Value[..match.Groups["name"].Value.LastIndexOf('.')],
        _ => match.Groups["namespace"].Value
    };
    public string EnclosingType => Type switch
    {
        AssemblyXMLDocumentationMemberType.FIELD or AssemblyXMLDocumentationMemberType.PROPERTY or AssemblyXMLDocumentationMemberType.METHOD or AssemblyXMLDocumentationMemberType.EVENT => match.Groups["enclosing_type"].Value,
        _ => string.Empty
    };
    public IEnumerable<string> Arguments => Type switch
    {
        AssemblyXMLDocumentationMemberType.METHOD when match.Groups.ContainsKey("args") => match.Groups["args"].Value.Split(','),
        _ => []
    };
    public string IndexerType => Type switch
    {
        AssemblyXMLDocumentationMemberType.PROPERTY when match.Groups.ContainsKey("indexer_type") => match.Groups["indexer_type"].Value,
        _ => string.Empty
    };

    public string? Summary { get; init; }
    public string? Remarks { get; init; }
    public Dictionary<string, string>? Parameters { get; init; }
    public string? Returns { get; init; }

    public bool IsConstructor => Type == AssemblyXMLDocumentationMemberType.METHOD && Name.Equals("ctor", StringComparison.InvariantCultureIgnoreCase);
    public bool IsDestructor => Type == AssemblyXMLDocumentationMemberType.METHOD && Name.Equals("dtor", StringComparison.InvariantCultureIgnoreCase);
    public bool IsExplicitOperator => Type == AssemblyXMLDocumentationMemberType.METHOD && Name.Equals("op_Explicit", StringComparison.InvariantCultureIgnoreCase);
    public bool IsIndexer => Type == AssemblyXMLDocumentationMemberType.PROPERTY && Arguments.Any();

    public AssemblyXMLDocumentationMember(string id)
    {
        ID = id;

        switch (ID[0])
        {
            case 'N' when IDForNamespaceRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.NAMESPACE;
                this.match = match!;
                break;
            case 'T' when IDForTypeRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.TYPE;
                this.match = match!;
                break;
            case 'F' when IDForFieldRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.FIELD;
                this.match = match!;
                break;
            case 'P' when IDForPropertyRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.PROPERTY;
                this.match = match!;
                break;
            case 'M' when IDForMethodRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.METHOD;
                this.match = match!;
                break;
            case 'E' when IDForEventRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.EVENT;
                this.match = match!;
                break;
            case '!' when IDForErrorRegex().IsMatched(ID, out Match? match):
                Type = AssemblyXMLDocumentationMemberType.ERROR;
                this.match = match!;
                break;
            default:
                throw new ArgumentOutOfRangeException("id", "Unknown member type.");
        };
    }
}
