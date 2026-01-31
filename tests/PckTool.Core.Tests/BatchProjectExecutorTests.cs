using NSubstitute;

using PckTool.Abstractions;
using PckTool.Abstractions.Batch;
using PckTool.Core.Services.Batch;

namespace PckTool.Core.Tests;

public class BatchProjectExecutorTests
{
    private readonly BatchProjectExecutor _executor;
    private readonly IPckFileFactory _mockFactory;
    private readonly IPckFile _mockPckFile;
    private readonly ISoundBankCollection _mockSoundBanks;

    public BatchProjectExecutorTests()
    {
        _mockFactory = Substitute.For<IPckFileFactory>();
        _mockPckFile = Substitute.For<IPckFile>();
        _mockSoundBanks = Substitute.For<ISoundBankCollection>();

        _mockPckFile.SoundBanks.Returns(_mockSoundBanks);
        _mockPckFile.HasModifications.Returns(false);

        _executor = new BatchProjectExecutor(_mockFactory);
    }

#region Constructor Tests

    [Fact]
    public void Constructor_WithNullFactory_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new BatchProjectExecutor(null!));
    }

#endregion

#region DryRun Tests

    [Fact]
    public void Execute_WithDryRun_ShouldNotModifyFiles()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);

            // Act
            var result = _executor.Execute(project, true);

            // Assert
            Assert.Single(result.ActionResults);
            Assert.True(result.ActionResults[0].Success);
            Assert.Contains("[DRY RUN]", result.ActionResults[0].Message);
            _mockPckFile.DidNotReceive().ReplaceWem(Arg.Any<uint>(), Arg.Any<byte[]>(), Arg.Any<bool>());
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

#endregion

    /// <summary>
    ///     Creates a temp file and a project that uses it, saving to set FilePath.
    /// </summary>
    private static BatchProject CreateProjectWithTempFile(string tempFile)
    {
        var projectPath = tempFile + ".batchproj";
        var project = BatchProject.Create("Test");
        project.InputFiles.Add(Path.GetFileName(tempFile)); // Use relative path
        project.GameDir = Path.GetDirectoryName(tempFile);  // Set game dir for input file resolution
        project.Save(projectPath);

        return project;
    }

    private static string CreateTempFileWithContent()
    {
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, [0xDE, 0xAD, 0xBE, 0xEF]);

        return path;
    }

    private static void CleanupTempFile(string tempFile)
    {
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }

        var projectPath = tempFile + ".batchproj";

        if (File.Exists(projectPath))
        {
            File.Delete(projectPath);
        }
    }

#region Event Tests

    [Fact]
    public void Execute_ShouldRaiseActionStartedEvent()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);

            var startedCount = 0;
            _executor.ActionStarted += (_, _) => startedCount++;

            // Act
            _executor.Execute(project, true);

            // Assert
            Assert.Equal(1, startedCount);
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_ShouldRaiseActionCompletedEvent()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);

            var completedCount = 0;
            ActionExecutionResult? capturedResult = null;
            _executor.ActionCompleted += (_, e) =>
            {
                completedCount++;
                capturedResult = e.Result;
            };

            // Act
            _executor.Execute(project, true);

            // Assert
            Assert.Equal(1, completedCount);
            Assert.NotNull(capturedResult);
            Assert.True(capturedResult.Success);
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

#endregion

#region Replace Execution Tests

    [Fact]
    public void Execute_ReplaceWem_ShouldCallReplaceWemOnPckFile()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);
            _mockPckFile.ReplaceWem(Arg.Any<uint>(), Arg.Any<byte[]>(), Arg.Any<bool>())
                        .Returns(new WemReplacementResult { EmbeddedBanksModified = 1 });

            // Act
            _executor.Execute(project);

            // Assert
            _mockPckFile.Received(1).ReplaceWem(0x12345678, Arg.Any<byte[]>());
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_ReplaceWem_WithSkipHircSizeUpdates_ShouldPassFalse()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));
            project.SkipHircSizeUpdates = true;

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);
            _mockPckFile.ReplaceWem(Arg.Any<uint>(), Arg.Any<byte[]>(), Arg.Any<bool>())
                        .Returns(new WemReplacementResult { EmbeddedBanksModified = 1 });

            // Act
            _executor.Execute(project);

            // Assert - should pass false for updateHircSizes when SkipHircSizeUpdates is true
            _mockPckFile.Received(1).ReplaceWem(0x12345678, Arg.Any<byte[]>(), false);
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_WhenWemNotFound_ShouldReturnFailedResult()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);
            _mockPckFile.ReplaceWem(Arg.Any<uint>(), Arg.Any<byte[]>(), Arg.Any<bool>())
                        .Returns(new WemReplacementResult()); // WasReplaced will be false

            // Act
            var result = _executor.Execute(project);

            // Assert
            Assert.Single(result.ActionResults);
            Assert.False(result.ActionResults[0].Success);
            Assert.Contains("not found", result.ActionResults[0].Message);
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_ReplaceBnk_ShouldCallReplaceWithOnEntry()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceBnk(0xABCDEF00, Path.GetFileName(tempFile));

            var mockEntry = Substitute.For<ISoundBankEntry>();
            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);
            _mockSoundBanks[0xABCDEF00].Returns(mockEntry);

            // Act
            _executor.Execute(project);

            // Assert
            mockEntry.Received(1).ReplaceWith(Arg.Any<byte[]>());
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_ShouldDisposeLoadedFiles()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.AddReplaceWem(0x12345678, Path.GetFileName(tempFile));

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);

            // Act
            _executor.Execute(project, true);

            // Assert
            _mockPckFile.Received(1).Dispose();
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_AddAction_ShouldReturnNotImplemented()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.Actions.Add(
                new AddAction
                {
                    TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = Path.GetFileName(tempFile)
                });

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);

            // Act
            var result = _executor.Execute(project);

            // Assert
            Assert.Single(result.ActionResults);
            Assert.False(result.ActionResults[0].Success);
            Assert.Contains("not yet implemented", result.ActionResults[0].Message);
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void Execute_RemoveAction_ShouldReturnNotImplemented()
    {
        // Arrange
        var tempFile = CreateTempFileWithContent();

        try
        {
            var project = CreateProjectWithTempFile(tempFile);
            project.Actions.Add(new RemoveAction { TargetType = TargetType.Wem, TargetId = 0x12345678 });

            _mockFactory.Load(Arg.Any<string>()).Returns(_mockPckFile);

            // Act
            var result = _executor.Execute(project);

            // Assert
            Assert.Single(result.ActionResults);
            Assert.False(result.ActionResults[0].Success);
            Assert.Contains("not yet implemented", result.ActionResults[0].Message);
        }
        finally
        {
            CleanupTempFile(tempFile);
        }
    }

#endregion
}
