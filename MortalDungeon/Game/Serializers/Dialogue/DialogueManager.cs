using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    public static class DialogueManager
    {
        public static Dictionary<int, DataBlock<Dialogue>> LoadedInfoBlocks = new Dictionary<int, DataBlock<Dialogue>>();

        public static Dialogue GetDialogue(int id)
        {
            int blockId = id / DialogueBlockSerializer.BLOCK_SIZE;

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
                DataBlock<Dialogue> newBlock = DialogueBlockSerializer.LoadDialogueBlockFromFile(blockId);

                if (newBlock != null)
                {
                    LoadedInfoBlocks.Add(blockId, newBlock);

                    if (newBlock.LoadedItems.TryGetValue(id, out var dialogue))
                    {
                        return dialogue;
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

        public static void WriteDialogueToInfoBlock(Dialogue info)
        {
            DataBlock<Dialogue> block;

            int blockId = info.ID / DialogueBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out DataBlock<Dialogue> foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = DialogueBlockSerializer.LoadDialogueBlockFromFile(blockId);

                if (block == null)
                {
                    block = new DataBlock<Dialogue>()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.LoadedItems.AddOrSet(info.ID, info);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            DialogueBlockSerializer.WriteDialogueBlockToFile(block);
        }

        public static void DeleteDialogue(int id)
        {
            DataBlock<Dialogue> block;

            int blockId = id / DialogueBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out DataBlock<Dialogue> foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = DialogueBlockSerializer.LoadDialogueBlockFromFile(blockId);

                if (block == null)
                {
                    block = new DataBlock<Dialogue>()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.LoadedItems.Remove(id);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            DialogueBlockSerializer.WriteDialogueBlockToFile(block);
        }

        public static void LoadAllDialogueBlocks()
        {
            var dialogueBlocks = DialogueBlockSerializer.LoadAllDialogueBlocks();

            foreach (var block in dialogueBlocks)
            {
                if (!LoadedInfoBlocks.ContainsKey(block.BlockId))
                {
                    LoadedInfoBlocks.AddOrSet(block.BlockId, block);
                }
            }
        }
    }
}
