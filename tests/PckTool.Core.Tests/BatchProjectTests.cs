using PckTool.Abstractions.Batch;
using PckTool.Core.Services.Batch;

namespace PckTool.Core.Tests;

public class BatchProjectTests
{
#region GetBasePath Tests

    [Fact]
    public void GetBasePath_WithoutFilePath_ShouldReturnCurrentDirectory()
    {
        var project = BatchProject.Create();

        var basePath = project.GetBasePath();

        Assert.Equal(Environment.CurrentDirectory, basePath);
    }

#endregion

    private static MemoryStream SaveToMemoryStream(BatchProject project)
    {
        var stream = new MemoryStream();
        project.Save(stream);
        stream.Position = 0;

        return stream;
    }

#region Create Tests

    [Fact]
    public void Create_ShouldCreateProjectWithDefaultName()
    {
        var project = BatchProject.Create();

        Assert.Equal("Untitled Batch Project", project.Name);
        Assert.Null(project.FilePath);
    }

    [Fact]
    public void Create_ShouldCreateProjectWithCustomName()
    {
        var project = BatchProject.Create("My Batch Project");

        Assert.Equal("My Batch Project", project.Name);
    }

    [Fact]
    public void Create_ShouldSetSchemaVersion()
    {
        var project = BatchProject.Create();

        Assert.Equal(BatchProject.CurrentSchemaVersion, project.SchemaVersion);
    }

#endregion

#region Save/Load Tests

    [Fact]
    public void Save_ShouldSaveProjectToStream()
    {
        var project = BatchProject.Create("Test Project");
        project.Description = "Test description";

        using var stream = new MemoryStream();
        project.Save(stream);

        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void Save_WithoutPath_ShouldReturnFalse()
    {
        var project = BatchProject.Create();

        var result = project.Save();

        Assert.False(result);
    }

    [Fact]
    public void Load_ShouldLoadProjectFromStream()
    {
        var originalProject = BatchProject.Create("Loaded Batch Project");
        originalProject.Description = "A test description";
        originalProject.OutputDirectory = @"C:\Output";
        originalProject.AddInputFile("test.pck");
        originalProject.AddReplaceWem(0x12345678, "replacement.wem", "Replace test sound");

        using var stream = SaveToMemoryStream(originalProject);
        var loadedProject = BatchProject.Load(stream);

        Assert.NotNull(loadedProject);
        Assert.Equal("Loaded Batch Project", loadedProject.Name);
        Assert.Equal("A test description", loadedProject.Description);
        Assert.Equal(@"C:\Output", loadedProject.OutputDirectory);
        Assert.Single(loadedProject.InputFiles);
        Assert.Equal("test.pck", loadedProject.InputFiles[0]);
        Assert.Single(loadedProject.Actions);
    }

    [Fact]
    public void Load_WithInvalidPath_ShouldReturnNull()
    {
        var result = BatchProject.Load(@"C:\NonExistent\file.batchproj");

        Assert.Null(result);
    }

    [Fact]
    public void Load_WithInvalidJson_ShouldReturnNull()
    {
        using var stream = new MemoryStream("not valid json {{{"u8.ToArray());
        var result = BatchProject.Load(stream);

        Assert.Null(result);
    }

#endregion

#region JSON Serialization Tests

    [Fact]
    public void Actions_ShouldPersistAfterSaveAndLoad()
    {
        var project = BatchProject.Create();
        project.AddReplaceWem(0x11111111, "sound1.wem", "First sound");
        project.AddReplaceBnk(0x22222222, "bank.bnk", "A bank");
        project.AddReplaceWem(0x33333333, "sound2.wem");

        using var stream = SaveToMemoryStream(project);
        var loadedProject = BatchProject.Load(stream);

        Assert.NotNull(loadedProject);
        Assert.Equal(3, loadedProject.Actions.Count);

        var action1 = loadedProject.Actions[0] as ReplaceAction;
        Assert.NotNull(action1);
        Assert.Equal(TargetType.Wem, action1.TargetType);
        Assert.Equal(0x11111111u, action1.TargetId);
        Assert.Equal("sound1.wem", action1.SourcePath);
        Assert.Equal("First sound", action1.Description);

        var action2 = loadedProject.Actions[1] as ReplaceAction;
        Assert.NotNull(action2);
        Assert.Equal(TargetType.Bnk, action2.TargetType);
        Assert.Equal(0x22222222u, action2.TargetId);
    }

    [Fact]
    public void SkipHircSizeUpdates_ShouldPersistAfterSaveAndLoad()
    {
        var project = BatchProject.Create();
        project.SkipHircSizeUpdates = true;

        using var stream = SaveToMemoryStream(project);
        var loadedProject = BatchProject.Load(stream);

        Assert.NotNull(loadedProject);
        Assert.True(loadedProject.SkipHircSizeUpdates);
    }

    [Fact]
    public void SkipHircSizeUpdates_WhenFalse_ShouldNotAppearInJson()
    {
        var project = BatchProject.Create();
        project.SkipHircSizeUpdates = false;

        using var stream = new MemoryStream();
        project.Save(stream);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        Assert.DoesNotContain("skipHircSizeUpdates", json);
    }

    [Fact]
    public void TargetBank_ShouldPersistAfterSaveAndLoad()
    {
        var project = BatchProject.Create();
        project.Actions.Add(
            new ReplaceAction
            {
                TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = "test.wem", TargetBank = 0xABCDEF00
            });

        using var stream = SaveToMemoryStream(project);
        var loadedProject = BatchProject.Load(stream);

        Assert.NotNull(loadedProject);
        var action = loadedProject.Actions[0] as ReplaceAction;
        Assert.NotNull(action);
        Assert.Equal(0xABCDEF00u, action.TargetBank);
    }

    [Fact]
    public void TargetBank_WhenNull_ShouldNotAppearInJson()
    {
        var project = BatchProject.Create();
        project.AddReplaceWem(0x12345678, "test.wem");

        using var stream = new MemoryStream();
        project.Save(stream);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        Assert.DoesNotContain("targetBank", json);
    }

#endregion

#region Validate Tests

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var project = BatchProject.Create();
        project.Name = "";
        project.InputFiles.Add("test.pck");
        project.AddReplaceWem(0x12345678, "test.wem");

        var result = project.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("name"));
    }

    [Fact]
    public void Validate_WithNoInputFiles_ShouldFail()
    {
        var project = BatchProject.Create("Test");
        project.AddReplaceWem(0x12345678, "test.wem");

        var result = project.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("input file"));
    }

    [Fact]
    public void Validate_WithNoActions_ShouldFail()
    {
        var project = BatchProject.Create("Test");
        project.InputFiles.Add("test.pck");

        var result = project.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("action"));
    }

    [Fact]
    public void Validate_WithInvalidAction_ShouldFail()
    {
        var project = BatchProject.Create("Test");
        project.InputFiles.Add("test.pck");
        project.Actions.Add(new ReplaceAction { TargetId = 0, SourcePath = "test.wem" }); // Invalid: ID = 0

        var result = project.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Action 1"));
    }

#endregion

#region Fluent API Tests

    [Fact]
    public void AddInputFile_ShouldAddFileAndReturnSelf()
    {
        var project = BatchProject.Create();

        var result = project.AddInputFile("test.pck");

        Assert.Same(project, result);
        Assert.Single(project.InputFiles);
        Assert.Equal("test.pck", project.InputFiles[0]);
    }

    [Fact]
    public void AddReplaceWem_ShouldAddActionAndReturnSelf()
    {
        var project = BatchProject.Create();

        var result = project.AddReplaceWem(0x12345678, "replacement.wem", "Test");

        Assert.Same(project, result);
        Assert.Single(project.Actions);
        var action = project.Actions[0] as ReplaceAction;
        Assert.NotNull(action);
        Assert.Equal(TargetType.Wem, action.TargetType);
        Assert.Equal(0x12345678u, action.TargetId);
        Assert.Equal("replacement.wem", action.SourcePath);
        Assert.Equal("Test", action.Description);
    }

    [Fact]
    public void AddReplaceBnk_ShouldAddActionAndReturnSelf()
    {
        var project = BatchProject.Create();

        var result = project.AddReplaceBnk(0xABCDEF00, "bank.bnk");

        Assert.Same(project, result);
        Assert.Single(project.Actions);
        var action = project.Actions[0] as ReplaceAction;
        Assert.NotNull(action);
        Assert.Equal(TargetType.Bnk, action.TargetType);
        Assert.Equal(0xABCDEF00u, action.TargetId);
    }

    [Fact]
    public void FluentApi_ShouldSupportChaining()
    {
        var project = BatchProject.Create("Chained Project")
                                  .AddInputFile("file1.pck")
                                  .AddInputFile("file2.pck")
                                  .AddReplaceWem(0x11111111, "sound.wem")
                                  .AddReplaceBnk(0x22222222, "bank.bnk");

        Assert.Equal(2, project.InputFiles.Count);
        Assert.Equal(2, project.Actions.Count);
    }

#endregion
}
