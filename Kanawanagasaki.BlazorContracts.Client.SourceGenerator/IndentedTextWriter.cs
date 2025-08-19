namespace Kanawanagasaki.BlazorContracts.Client.SourceGenerator;
using System;
using System.Text;

internal class IndentedTextWriter : TextWriter
{
    private bool _needIndent;
    private TextWriter _writer;

    internal int IndentLevel { get; set; } = 0;
    internal string Indent { get; set; } = "    ";

    public override Encoding Encoding => _writer.Encoding;

    internal IndentedTextWriter(TextWriter writer)
    {
        if (writer is null)
            throw new ArgumentNullException();
        _writer = writer;
        _needIndent = false;
    }

    internal void IncreaseAndWriteLine(string value)
    {
        IndentLevel++;
        WriteLine(value);
    }

    internal void DecreaseAndWriteLine(string value)
    {
        IndentLevel--;
        WriteLine(value);
    }

    public override void Write(char value)
    {
        if (_needIndent)
        {
            _writer.Write(GetIndentStr(IndentLevel));
            _needIndent = false;
        }
        if (value == '\n')
        {
            _needIndent = true;
            _writer.Write("\n");
        }
        else
            _writer.Write(value);
    }

    private string GetIndentStr(int level)
    {
        if (level <= 0) return "";
        var len = level * Indent.Length;
        var sb = new StringBuilder(len, len);
        for (var i = 0; i < level; ++i)
        {
            sb.Append(Indent);
        }
        return sb.ToString();
    }
}