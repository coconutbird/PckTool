using PckTool.Core.WWise.Util;

namespace PckTool.Core.WWise.Bnk.Enums;

public enum BnkChunkId : uint
{
    HeaderChunk = 1145588546,
    DataIndexChunk = 1480870212,
    DataChunk = 1096040772,
    HierarchyChunk = 1129466184,
    StrMapChunk = 1145656403,
    StateManagerChunk = 1196250195,
    FxParamsChunk = 1380997190,
    EnvSettingsChunk = 1398165061,
    PlatformChunk = 1413565520
}

public static class BnkChunkIds
{
    public static uint BankHeaderChunkId { get; } = Hash.AkmmioFourcc('B', 'K', 'H', 'D');
    public static uint BankDataIndexChunkId { get; } = Hash.AkmmioFourcc('D', 'I', 'D', 'X');
    public static uint BankDataChunkId { get; } = Hash.AkmmioFourcc('D', 'A', 'T', 'A');
    public static uint BankHierarchyChunkId { get; } = Hash.AkmmioFourcc('H', 'I', 'R', 'C');
    public static uint BankStrMapChunkId { get; } = Hash.AkmmioFourcc('S', 'T', 'I', 'D');
    public static uint BankStateMgrChunkId { get; } = Hash.AkmmioFourcc('S', 'T', 'M', 'G');
    public static uint BankFxParamsChunkId { get; } = Hash.AkmmioFourcc('F', 'X', 'P', 'R');
    public static uint BankEnvSettingChunkId { get; } = Hash.AkmmioFourcc('E', 'N', 'V', 'S');
    public static uint BankPlatChunkId { get; } = Hash.AkmmioFourcc('P', 'L', 'A', 'T');
    public static uint BankInitChunkId { get; } = Hash.AkmmioFourcc('I', 'N', 'I', 'T');
}
