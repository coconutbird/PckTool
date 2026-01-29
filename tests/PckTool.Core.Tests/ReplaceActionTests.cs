using PckTool.Abstractions.Batch;
using PckTool.Core.Services.Batch;

namespace PckTool.Core.Tests;

public class ReplaceActionTests
{
#region Validation Tests

    [Fact]
    public void Validate_WithValidData_ShouldSucceed()
    {
        var action = new ReplaceAction
        {
            TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = "replacement.wem"
        };

        var result = action.Validate();

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_WithZeroTargetId_ShouldFail()
    {
        var action = new ReplaceAction { TargetType = TargetType.Wem, TargetId = 0, SourcePath = "replacement.wem" };

        var result = action.Validate();

        Assert.False(result.IsValid);
        Assert.Contains("Target ID", result.ErrorMessage);
    }

    [Fact]
    public void Validate_WithNullSourcePath_ShouldFail()
    {
        var action = new ReplaceAction { TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = null };

        var result = action.Validate();

        Assert.False(result.IsValid);
        Assert.Contains("Source path", result.ErrorMessage);
    }

    [Fact]
    public void Validate_WithEmptySourcePath_ShouldFail()
    {
        var action = new ReplaceAction { TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = "   " };

        var result = action.Validate();

        Assert.False(result.IsValid);
        Assert.Contains("Source path", result.ErrorMessage);
    }

#endregion

#region ValidateWithBasePath Tests

    [Fact]
    public void ValidateWithBasePath_WithNonExistentFile_ShouldFail()
    {
        var action = new ReplaceAction
        {
            TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = "nonexistent.wem"
        };

        var result = action.ValidateWithBasePath(@"C:\NonExistentPath");

        Assert.False(result.IsValid);
        Assert.Contains("Source file not found", result.ErrorMessage);
    }

    [Fact]
    public void ValidateWithBasePath_WithExistingFile_ShouldSucceed()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            var action = new ReplaceAction
            {
                TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = Path.GetFileName(tempFile)
            };

            var result = action.ValidateWithBasePath(Path.GetDirectoryName(tempFile)!);

            Assert.True(result.IsValid);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ValidateWithBasePath_WithAbsolutePath_ShouldSucceed()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            var action = new ReplaceAction
            {
                TargetType = TargetType.Wem, TargetId = 0x12345678, SourcePath = tempFile // Absolute path
            };

            var result = action.ValidateWithBasePath(@"C:\SomeOtherPath");

            Assert.True(result.IsValid);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ValidateWithBasePath_WithInvalidBasicValidation_ShouldReturnBasicError()
    {
        var action = new ReplaceAction
        {
            TargetType = TargetType.Wem,
            TargetId = 0, // Invalid
            SourcePath = "test.wem"
        };

        var result = action.ValidateWithBasePath(@"C:\SomePath");

        Assert.False(result.IsValid);
        Assert.Contains("Target ID", result.ErrorMessage);
    }

#endregion

#region GetFullSourcePath Tests

    [Fact]
    public void GetFullSourcePath_WithRelativePath_ShouldCombineWithBasePath()
    {
        var action = new ReplaceAction { SourcePath = "replacement.wem" };

        var result = action.GetFullSourcePath(@"C:\Projects\MyProject");

        Assert.Equal(@"C:\Projects\MyProject\replacement.wem", result);
    }

    [Fact]
    public void GetFullSourcePath_WithAbsolutePath_ShouldReturnAbsolutePath()
    {
        var action = new ReplaceAction { SourcePath = @"D:\Audio\replacement.wem" };

        var result = action.GetFullSourcePath(@"C:\Projects\MyProject");

        Assert.Equal(@"D:\Audio\replacement.wem", result);
    }

    [Fact]
    public void GetFullSourcePath_WithNullSourcePath_ShouldThrow()
    {
        var action = new ReplaceAction { SourcePath = null };

        Assert.Throws<InvalidOperationException>(() => action.GetFullSourcePath(@"C:\SomePath"));
    }

    [Fact]
    public void GetFullSourcePath_WithEmptySourcePath_ShouldThrow()
    {
        var action = new ReplaceAction { SourcePath = "   " };

        Assert.Throws<InvalidOperationException>(() => action.GetFullSourcePath(@"C:\SomePath"));
    }

#endregion

#region Property Tests

    [Fact]
    public void ActionType_ShouldBeReplace()
    {
        var action = new ReplaceAction();

        Assert.Equal(ProjectActionType.Replace, action.ActionType);
    }

    [Fact]
    public void TargetType_DefaultShouldBeWem()
    {
        var action = new ReplaceAction();

        Assert.Equal(TargetType.Wem, action.TargetType);
    }

    [Fact]
    public void TargetBank_DefaultShouldBeNull()
    {
        var action = new ReplaceAction();

        Assert.Null(action.TargetBank);
    }

    [Fact]
    public void TargetBank_CanBeSet()
    {
        var action = new ReplaceAction { TargetBank = 0xABCDEF00 };

        Assert.Equal(0xABCDEF00u, action.TargetBank);
    }

    [Fact]
    public void Description_CanBeSetAndRetrieved()
    {
        var action = new ReplaceAction { Description = "Replace main menu music" };

        Assert.Equal("Replace main menu music", action.Description);
    }

#endregion
}

