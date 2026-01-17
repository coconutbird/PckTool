using PckTool.Core.Package;

namespace PckTool.Core.Tests;

public class ProjectFileTests
{
    [Fact]
    public void Create_ShouldCreateProjectWithDefaultName()
    {
        var project = ProjectFile.Create();

        Assert.Equal("Untitled Project", project.Name);
        Assert.True(project.IsDirty);
        Assert.Null(project.FilePath);
    }

    [Fact]
    public void Create_ShouldCreateProjectWithCustomName()
    {
        var project = ProjectFile.Create("My Custom Project");

        Assert.Equal("My Custom Project", project.Name);
        Assert.True(project.IsDirty);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtAndModifiedAt()
    {
        var before = DateTime.UtcNow;
        var project = ProjectFile.Create();
        var after = DateTime.UtcNow;

        Assert.InRange(project.CreatedAt, before, after);
        Assert.InRange(project.ModifiedAt, before, after);
    }

    [Fact]
    public void Save_ShouldSaveProjectToStream()
    {
        var project = ProjectFile.Create("Test Project");
        project.PackagePath = @"C:\Games\HaloWars\sounds.pck";
        project.SoundTablePath = @"C:\Games\HaloWars\soundtable.xml";

        using var stream = new MemoryStream();
        project.Save(stream);

        Assert.True(stream.Length > 0);
        Assert.False(project.IsDirty);
    }

    [Fact]
    public void Save_WithoutPath_ShouldReturnFalse()
    {
        var project = ProjectFile.Create();

        var result = project.Save();

        Assert.False(result);
    }

    [Fact]
    public void Load_ShouldLoadProjectFromStream()
    {
        var originalProject = ProjectFile.Create("Loaded Project");
        originalProject.PackagePath = @"C:\Games\HaloWars\sounds.pck";
        originalProject.SoundTablePath = @"C:\Games\HaloWars\soundtable.xml";
        originalProject.GameDirectory = @"C:\Games\HaloWars";
        originalProject.OutputDirectory = @"C:\Output";
        originalProject.Notes = "Test notes";

        using var stream = SaveToMemoryStream(originalProject);
        var loadedProject = ProjectFile.Load(stream);

        Assert.NotNull(loadedProject);
        Assert.Equal("Loaded Project", loadedProject.Name);
        Assert.Equal(@"C:\Games\HaloWars\sounds.pck", loadedProject.PackagePath);
        Assert.Equal(@"C:\Games\HaloWars\soundtable.xml", loadedProject.SoundTablePath);
        Assert.Equal(@"C:\Games\HaloWars", loadedProject.GameDirectory);
        Assert.Equal(@"C:\Output", loadedProject.OutputDirectory);
        Assert.Equal("Test notes", loadedProject.Notes);
        Assert.False(loadedProject.IsDirty);
    }

    [Fact]
    public void Load_WithInvalidPath_ShouldReturnNull()
    {
        var result = ProjectFile.Load(@"C:\NonExistent\file.pckproj");

        Assert.Null(result);
    }

    [Fact]
    public void Load_WithInvalidJson_ShouldReturnNull()
    {
        using var stream = new MemoryStream("not valid json {{{"u8.ToArray());
        var result = ProjectFile.Load(stream);

        Assert.Null(result);
    }

    [Fact]
    public void MarkDirty_ShouldSetIsDirtyToTrue()
    {
        var project = ProjectFile.Create();
        using var stream = new MemoryStream();
        project.Save(stream);

        Assert.False(project.IsDirty);

        project.MarkDirty();

        Assert.True(project.IsDirty);
    }

    [Fact]
    public void AddEditingBank_ShouldAddBankToList()
    {
        var project = ProjectFile.Create();
        using var stream = new MemoryStream();
        project.Save(stream);

        project.AddEditingBank(0x12345678);

        Assert.Contains(0x12345678u, project.EditingBanks);
        Assert.True(project.IsDirty);
    }

    [Fact]
    public void AddEditingBank_ShouldNotAddDuplicate()
    {
        var project = ProjectFile.Create();
        project.AddEditingBank(0x12345678);
        project.AddEditingBank(0x12345678);

        Assert.Single(project.EditingBanks);
    }

    [Fact]
    public void RemoveEditingBank_ShouldRemoveBankFromList()
    {
        var project = ProjectFile.Create();
        project.AddEditingBank(0x12345678);
        using var stream = new MemoryStream();
        project.Save(stream);

        project.RemoveEditingBank(0x12345678);

        Assert.DoesNotContain(0x12345678u, project.EditingBanks);
        Assert.True(project.IsDirty);
    }

    [Fact]
    public void RemoveEditingBank_WithNonExistentBank_ShouldNotMarkDirty()
    {
        var project = ProjectFile.Create();
        using var stream = new MemoryStream();
        project.Save(stream);

        project.RemoveEditingBank(0x12345678);

        Assert.False(project.IsDirty);
    }

    [Fact]
    public void AddEditingSound_ShouldAddSoundToList()
    {
        var project = ProjectFile.Create();
        using var stream = new MemoryStream();
        project.Save(stream);

        project.AddEditingSound(0xABCDEF00);

        Assert.Contains(0xABCDEF00u, project.EditingSounds);
        Assert.True(project.IsDirty);
    }

    [Fact]
    public void AddEditingSound_ShouldNotAddDuplicate()
    {
        var project = ProjectFile.Create();
        project.AddEditingSound(0xABCDEF00);
        project.AddEditingSound(0xABCDEF00);

        Assert.Single(project.EditingSounds);
    }

    [Fact]
    public void RemoveEditingSound_ShouldRemoveSoundFromList()
    {
        var project = ProjectFile.Create();
        project.AddEditingSound(0xABCDEF00);
        using var stream = new MemoryStream();
        project.Save(stream);

        project.RemoveEditingSound(0xABCDEF00);

        Assert.DoesNotContain(0xABCDEF00u, project.EditingSounds);
        Assert.True(project.IsDirty);
    }

    [Fact]
    public void EditingBanks_ShouldPersistAfterSaveAndLoad()
    {
        var project = ProjectFile.Create();
        project.AddEditingBank(0x11111111);
        project.AddEditingBank(0x22222222);
        project.AddEditingBank(0x33333333);

        using var stream = SaveToMemoryStream(project);
        var loadedProject = ProjectFile.Load(stream);

        Assert.NotNull(loadedProject);
        Assert.Equal(3, loadedProject.EditingBanks.Count);
        Assert.Contains(0x11111111u, loadedProject.EditingBanks);
        Assert.Contains(0x22222222u, loadedProject.EditingBanks);
        Assert.Contains(0x33333333u, loadedProject.EditingBanks);
    }

    [Fact]
    public void EditingSounds_ShouldPersistAfterSaveAndLoad()
    {
        var project = ProjectFile.Create();
        project.AddEditingSound(0xAAAAAAAA);
        project.AddEditingSound(0xBBBBBBBB);

        using var stream = SaveToMemoryStream(project);
        var loadedProject = ProjectFile.Load(stream);

        Assert.NotNull(loadedProject);
        Assert.Equal(2, loadedProject.EditingSounds.Count);
        Assert.Contains(0xAAAAAAAAu, loadedProject.EditingSounds);
        Assert.Contains(0xBBBBBBBBu, loadedProject.EditingSounds);
    }

    private static MemoryStream SaveToMemoryStream(ProjectFile project)
    {
        var stream = new MemoryStream();
        project.Save(stream);
        stream.Position = 0;

        return stream;
    }
}
