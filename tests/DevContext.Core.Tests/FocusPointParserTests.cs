namespace DevContext.Core.Tests;

public sealed class FocusPointParserTests
{
    [Fact]
    public void Parse_FilePath_ReturnsFileFocus()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Orders\OrdersController.cs", "");

        var result = FocusPointParser.Parse(@"src\Orders\OrdersController.cs", fs);

        Assert.NotNull(result);
        Assert.Equal(FocusKind.File, result.Kind);
    }

    [Fact]
    public void Parse_Directory_ReturnsFolderFocus()
    {
        var fs = new FakeFileSystem();
        fs.AddFile(@"src\Orders\file.cs", "");

        var result = FocusPointParser.Parse("src\\Orders", fs);

        Assert.NotNull(result);
        Assert.Equal(FocusKind.Folder, result.Kind);
    }

    [Fact]
    public void Parse_TypeName_ReturnsTypeFocus()
    {
        var fs = new FakeFileSystem();

        var result = FocusPointParser.Parse("OrdersController", fs);

        Assert.NotNull(result);
        Assert.Equal(FocusKind.Type, result.Kind);
        Assert.Equal("OrdersController", result.TypeName);
    }

    [Fact]
    public void Parse_MethodNotation_ReturnsMethodFocus()
    {
        var fs = new FakeFileSystem();

        var result = FocusPointParser.Parse("OrdersController:CreateOrder", fs);

        Assert.NotNull(result);
        Assert.Equal(FocusKind.Method, result.Kind);
        Assert.Equal("OrdersController", result.TypeName);
        Assert.Equal("CreateOrder", result.MethodName);
    }
}
