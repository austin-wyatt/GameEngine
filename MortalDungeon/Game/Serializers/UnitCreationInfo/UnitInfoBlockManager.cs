using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class UnitInfoBlockManager
    {
        public static Dictionary<int, UnitInfoBlock> LoadedInfoBlocks = new Dictionary<int, UnitInfoBlock>();

        public static UnitCreationInfo GetUnit(int id)
        {
            int blockId = id / UnitInfoBlockSerializer.UNIT_BLOCK_SIZE;

            if(LoadedInfoBlocks.TryGetValue(blockId, out var info))
            {
                if(info.Units.TryGetValue(id, out var unit))
                {
                    return unit;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                UnitInfoBlock newBlock = UnitInfoBlockSerializer.LoadUnitBlockInfoFromFile(blockId);

                if(newBlock != null)
                {
                    LoadedInfoBlocks.Add(blockId, newBlock);

                    if (newBlock.Units.TryGetValue(id, out var unit))
                    {
                        return unit;
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

        public static void WriteUnitToInfoBlock(UnitCreationInfo info)
        {
            UnitInfoBlock block;

            int blockId = info.Id / UnitInfoBlockSerializer.UNIT_BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out UnitInfoBlock foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = UnitInfoBlockSerializer.LoadUnitBlockInfoFromFile(blockId);

                if(block == null)
                {
                    block = new UnitInfoBlock()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.Units.AddOrSet(info.Id, info);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            UnitInfoBlockSerializer.WriteUnitBlockInfoToFile(block);
        }

        public static void DeleteUnit(int id)
        {
            UnitInfoBlock block;

            int blockId = id / UnitInfoBlockSerializer.UNIT_BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out UnitInfoBlock foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = UnitInfoBlockSerializer.LoadUnitBlockInfoFromFile(blockId);

                if (block == null)
                {
                    block = new UnitInfoBlock()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.Units.Remove(id);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            UnitInfoBlockSerializer.WriteUnitBlockInfoToFile(block);
        }

        public static void LoadAllUnitBlocks()
        {
            var unitBlocks = UnitInfoBlockSerializer.LoadAllUnitBlockInfo();

            foreach(var block in unitBlocks)
            {
                if (!LoadedInfoBlocks.ContainsKey(block.BlockId))
                {
                    LoadedInfoBlocks.AddOrSet(block.BlockId, block);
                }
            }
        }
    }
}
