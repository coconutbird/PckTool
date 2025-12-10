using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Chunks;

public class HircChunk : BaseChunk
{
    public override bool IsValid => LoadedItems is not null;

    public List<LoadedItem>? LoadedItems { get; private set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var loadedItems = new List<LoadedItem>();

        var numberOfReleasableHircItem = reader.ReadUInt32();

        for (var i = 0; i < numberOfReleasableHircItem; ++i)
        {
            var loadedItem = new LoadedItem();

            Console.WriteLine("Idx: " + i);

            if (!loadedItem.Read(reader))
            {
                return false;
            }

            loadedItems.Add(loadedItem);
        }

        LoadedItems = loadedItems;

        return true;
    }

    public IEnumerable<uint> ResolveSoundFileIds(SoundBank startBank, uint itemId)
    {
        var item = LoadedItems?.FirstOrDefault(x => x.Id == itemId);

        if (item is null)
        {
            yield break;
        }

        foreach (var soundFileId in ResolveSoundFileIds(startBank, item))
        {
            yield return soundFileId;
        }
    }

    public IEnumerable<uint> ResolveSoundFileIds(SoundBank startBank, LoadedItem item)
    {
        switch (item.Type)
        {
            case HircType.Event:
            {
                var actions = item.EventInitialValues?.Actions ?? [];

                foreach (var action in actions)
                {
                    foreach (var soundFileId in ResolveSoundFileIds(startBank, action))
                    {
                        yield return soundFileId;
                    }
                }

                yield break;
            }

            case HircType.Action:
            {
                var actionType = item.ActionType;

                switch (actionType)
                {
                    case ActionType.Play:
                    {
                        var initialValues = item.ActionInitialValues;

                        if (initialValues is null)
                        {
                            yield break;
                        }

                        var playActionParams = initialValues.PlayActionParams!;

                        var targetBank = playActionParams.FileId;

                        if (targetBank != startBank.SoundbankId)
                        {
                            throw new NotImplementedException("Cross-bank references are not implemented");
                        }

                        var targetItemId = initialValues.Ext;

                        // TODO: resolve in context with every bank
                        foreach (var soundFileId in ResolveSoundFileIds(startBank, targetItemId))
                        {
                            yield return soundFileId;
                        }

                        yield break;
                    }

                    default:
                        yield break;
                }
            }

            case HircType.RanSeqCntr:
            {
                var children = item.RanSeqCntrInitialValues?.Children;

                if (children is null)
                {
                    yield break;
                }

                foreach (var child in children.ChildIds)
                {
                    foreach (var soundFileId in ResolveSoundFileIds(startBank, child))
                    {
                        yield return soundFileId;
                    }
                }

                yield break;
            }

            case HircType.Sound:
            {
                var streamType = item.SoundValues?.BankSourceData.StreamType;

                if (streamType == StreamType.DataBnk)
                {
                    yield return item.SoundValues!.BankSourceData.MediaInformation.SourceId;
                }

                yield break;
            }

            default:
                yield break;
        }
    }
}
