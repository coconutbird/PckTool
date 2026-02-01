using PckTool.Core.WWise.Bnk.Enums;
using PckTool.Core.WWise.Bnk.Hirc.Items;
using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Tests;

/// <summary>
///     Round-trip tests for Halo Wars 2 PCK files.
/// </summary>
public class Hw2RoundTripTests
{
    private const string Hw2GameDir =
        @"C:\Users\dev\AppData\Local\Packages\Microsoft.HoganThreshold_8wekyb3d8bbwe\AC\Packages\Microsoft.HoganThreshold_8wekyb3d8bbwe\TempState\DUMP";

    private const string Hw2LaunchPckPath = Hw2GameDir + @"\data\sound\AMB_Caldera.bnk";
    private const string Hw2DialoguePckPath = Hw2GameDir + @"\data\sound\AMB_Boneyard.bnk";

    [SkippableFact]
    public void Hw2_LaunchPck_FullRoundTrip()
    {
        Skip.IfNot(File.Exists(Hw2LaunchPckPath), $"HW2 AMB_Caldera.bnk not found at {Hw2LaunchPckPath}");

        var tempPath = Path.GetTempFileName();

        try
        {
            using (var original = PckFile.Load(Hw2LaunchPckPath))
            {
                Assert.NotNull(original);
                original.Save(tempPath);

                using var reloaded = PckFile.Load(tempPath);
                Assert.NotNull(reloaded);

                Assert.True(original == reloaded, "Round-trip produced different data");
            }
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [SkippableFact]
    public void Hw2_DialoguePck_FullRoundTrip()
    {
        Skip.IfNot(File.Exists(Hw2DialoguePckPath), $"HW2 AMB_Boneyard.bnk not found at {Hw2DialoguePckPath}");

        var tempPath = Path.GetTempFileName();

        try
        {
            using (var original = PckFile.Load(Hw2DialoguePckPath))
            {
                Assert.NotNull(original);
                original.Save(tempPath);

                using var reloaded = PckFile.Load(tempPath);
                Assert.NotNull(reloaded);

                Assert.True(original == reloaded, "Round-trip produced different data");
            }
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [SkippableFact]
    public void Hw2_LaunchPck_HircItemsRoundTrip()
    {
        Skip.IfNot(File.Exists(Hw2LaunchPckPath), $"HW2 AMB_Caldera.bnk not found at {Hw2LaunchPckPath}");

        using var pck = PckFile.Load(Hw2LaunchPckPath);
        Assert.NotNull(pck);

        var totalItems = 0;
        var roundTripSuccesses = 0;
        var roundTripFailures = new List<(uint BankId, uint ItemId, HircType Type, string Error)>();

        foreach (var entry in pck.SoundBanks)
        {
            var bank = entry.Parse();

            if (bank?.HircChunk?.Items is null) continue;

            var hasFeedback = bank.HasFeedback;

            foreach (var item in bank.HircChunk.Items)
            {
                totalItems++;

                try
                {
                    using var writeStream = new MemoryStream();
                    using var writer = new BinaryWriter(writeStream);
                    item.Write(writer);

                    var writtenData = writeStream.ToArray();

                    using var readStream = new MemoryStream(writtenData);
                    using var reader = new BinaryReader(readStream);
                    var rereadItem = HircItem.Read(reader, hasFeedback);

                    using var rewriteStream = new MemoryStream();
                    using var rewriter = new BinaryWriter(rewriteStream);
                    rereadItem!.Write(rewriter);

                    var rewrittenData = rewriteStream.ToArray();

                    if (writtenData.SequenceEqual(rewrittenData))
                    {
                        roundTripSuccesses++;
                    }
                    else
                    {
                        roundTripFailures.Add(
                            (bank.Id, item.Id, item.Type,
                             $"Data mismatch: {writtenData.Length} vs {rewrittenData.Length} bytes"));
                    }
                }
                catch (Exception ex)
                {
                    roundTripFailures.Add((bank.Id, item.Id, item.Type, ex.Message));
                }
            }
        }

        var failuresByType = roundTripFailures
                             .GroupBy(f => f.Type)
                             .Select(g => $"{g.Key}: {g.Count()}")
                             .ToList();

        Assert.True(
            roundTripFailures.Count == 0,
            $"Round-trip failures: {roundTripFailures.Count}/{totalItems}\nBy Type: {string.Join(", ", failuresByType)}");
    }
}
