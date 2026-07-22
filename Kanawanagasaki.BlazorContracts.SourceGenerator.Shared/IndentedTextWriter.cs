namespace Kanawanagasaki.BlazorContracts.SourceGenerator.Shared;

using System.Text;

public class IndentedTextWriter : TextWriter
{
    private bool _needIndent;
    private TextWriter _writer;

    public int IndentLevel { get; set; } = 0;
    public string Indent { get; set; } = "    ";

    public override Encoding Encoding => _writer.Encoding;

    public IndentedTextWriter(TextWriter writer)
    {
        if (writer is null)
            throw new ArgumentNullException();
        _writer = writer;
        _needIndent = false;
    }

    public void IncreaseAndWriteLine(string value)
    {
        IndentLevel++;
        WriteLine(value);
    }

    public void WriteLineAndIncrease(string value)
    {
        WriteLine(value);
        IndentLevel++;
    }

    public void DecreaseAndWriteLine(string value)
    {
        IndentLevel--;
        WriteLine(value);
    }

    public void WriteLineAndDecrease(string value)
    {
        WriteLine(value);
        IndentLevel--;
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
