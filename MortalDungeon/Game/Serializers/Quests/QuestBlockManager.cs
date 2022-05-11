using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Serializers
{
    public static class QuestBlockManager
    {
        public static Dictionary<int, DataBlock<Quest>> LoadedInfoBlocks = new Dictionary<int, DataBlock<Quest>>();

        public static Quest GetQuest(int id)
        {
            int blockId = id / QuestBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out var info))
            {
                if (info.LoadedItems.TryGetValue(id, out var d))
                {
                    return d;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                DataBlock<Quest> newBlock = QuestBlockSerializer.LoadQuestBlockFromFile(blockId);

                if (newBlock != null)
                {
                    LoadedInfoBlocks.Add(blockId, newBlock);

                    if (newBlock.LoadedItems.TryGetValue(id, out var q))
                    {
                        return q;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static void WriteQuestToInfoBlock(Quest info)
        {
            DataBlock<Quest> block;

            int blockId = info.ID / QuestBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out DataBlock<Quest> foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = QuestBlockSerializer.LoadQuestBlockFromFile(blockId);

                if (block == null)
                {
                    block = new DataBlock<Quest>()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.LoadedItems.AddOrSet(info.ID, info);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            QuestBlockSerializer.WriteQuestBlockToFile(block);
        }

        public static void DeleteQuest(int id)
        {
            DataBlock<Quest> block;

            int blockId = id / QuestBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out DataBlock<Quest> foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = QuestBlockSerializer.LoadQuestBlockFromFile(blockId);

                if (block == null)
                {
                    block = new DataBlock<Quest>()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.LoadedItems.Remove(id);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            QuestBlockSerializer.WriteQuestBlockToFile(block);
        }

        public static void LoadAllQuestBlocks()
        {
            var questBlocks = QuestBlockSerializer.LoadAllQuestBlocks();

            foreach (var block in questBlocks)
            {
                if (!LoadedInfoBlocks.ContainsKey(block.BlockId))
                {
                    LoadedInfoBlocks.AddOrSet(block.BlockId, block);
                }
            }
        }
    }
}
