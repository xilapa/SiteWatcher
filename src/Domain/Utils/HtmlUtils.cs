using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace SiteWatcher.Domain.Utils;

public static class HtmlUtils
{
    private static readonly HtmlParser _htmlParser = new();
    private static readonly IMarkupFormatter _markupFormatter = new CustomMarkupFormatter();
    private static readonly string[] _tagsToRemove = new[] { "SCRIPT", "NOSCRIPT", "STYLE", "META", "TITLE", "LINK", "IMG" };

    public static async Task<string> ExtractText(Stream html)
    {
        var webPageDocument = await _htmlParser.ParseDocumentAsync(html, CancellationToken.None);

        foreach(var element in webPageDocument.All.ToArray())
        {
            if(_tagsToRemove.Contains(element.TagName))
                element.Remove();
        }

        return webPageDocument.DocumentElement.ToHtml(_markupFormatter);
    }
}

internal class CustomMarkupFormatter : IMarkupFormatter
{
    public string CloseTag(IElement element, bool selfClosing) => string.Empty;
    public string Comment(IComment comment) => string.Empty;
    public string Doctype(IDocumentType doctype) => string.Empty;
    public string LiteralText(ICharacterData text) => text.Data;
    public string OpenTag(IElement element, bool selfClosing) => string.Empty;
    public string Processing(IProcessingInstruction processing) => string.Empty;
    public string Text(ICharacterData text) => text.Data;
}