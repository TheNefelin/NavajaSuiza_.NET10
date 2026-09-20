using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class CsvParserTests
{
    [Fact]
    public void Parse_BasicFields()
    {
        var rows = CsvParser.Parse("a,b,c\r\n1,2,3\r\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "a", "b", "c" }, rows[0]);
        Assert.Equal(new[] { "1", "2", "3" }, rows[1]);
    }

    [Fact]
    public void Parse_QuotedFieldWithComma()
    {
        var rows = CsvParser.Parse("\"a,b\",c\n\"1\",2\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "a,b", "c" }, rows[0]);
        Assert.Equal(new[] { "1", "2" }, rows[1]);
    }

    [Fact]
    public void Parse_EscapedQuotes()
    {
        var rows = CsvParser.Parse("\"dijo \"\"hola\"\"\",x\n");

        Assert.Single(rows);
        Assert.Equal(new[] { "dijo \"hola\"", "x" }, rows[0]);
    }

    [Fact]
    public void Parse_FieldWithNewLineInsideQuotes()
    {
        var rows = CsvParser.Parse("\"linea1\nlinea2\",x\n");

        Assert.Single(rows);
        Assert.Equal(new[] { "linea1\nlinea2", "x" }, rows[0]);
    }

    [Fact]
    public void Parse_HandlesLfAndCrLf()
    {
        var rows = CsvParser.Parse("a\nb,c\r\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "a" }, rows[0]);
        Assert.Equal(new[] { "b", "c" }, rows[1]);
    }

    [Fact]
    public void Parse_SkipsEmptyRows()
    {
        var rows = CsvParser.Parse("a\n\nb\n");

        Assert.Equal(2, rows.Count);
        Assert.Equal("a", rows[0][0]);
        Assert.Equal("b", rows[1][0]);
    }

    [Fact]
    public void Parse_EmptyFieldsArePreserved()
    {
        var rows = CsvParser.Parse(",b,x,,y\n");

        Assert.Single(rows);
        Assert.Equal(new[] { "", "b", "x", "", "y" }, rows[0]);
    }

    [Fact]
    public void Parse_TrailingCommaKeepsEmptyField()
    {
        var rows = CsvParser.Parse("a,\n");

        Assert.Single(rows);
        Assert.Equal(new[] { "a", "" }, rows[0]);
    }

    [Fact]
    public void Parse_EmptyContent_ReturnsEmpty()
    {
        Assert.Empty(CsvParser.Parse(string.Empty));
        Assert.Empty(CsvParser.Parse(""));
    }

    [Fact]
    public void Parse_WhitespaceInsideFieldIsPreserved()
    {
        var rows = CsvParser.Parse("a, b ,c \n");

        Assert.Single(rows);
        Assert.Equal(new[] { "a", " b ", "c " }, rows[0]);
    }
}