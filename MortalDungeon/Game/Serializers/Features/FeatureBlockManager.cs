using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Serializers
{
    public static class FeatureBlockManager
    {
        public static Dictionary<int, DataBlock<Feature>> LoadedInfoBlocks = new Dictionary<int, DataBlock<Feature>>();

        public static Feature GetFeature(int id)
        {
            int blockId = id / FeatureBlockSerializer.BLOCK_SIZE;

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
                DataBlock<Feature> newBlock = FeatureBlockSerializer.LoadFeatureBlockFromFile(blockId);

                if (newBlock != null)
                {
                    LoadedInfoBlocks.Add(blockId, newBlock);

                    if (newBlock.LoadedItems.TryGetValue(id, out var feature))
                    {
                        return feature;
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

        public static void WriteFeatureToInfoBlock(Feature info)
        {
            DataBlock<Feature> block;

            info.CalculateLoadRadius();

            int blockId = info.Id / FeatureBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out DataBlock<Feature> foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = FeatureBlockSerializer.LoadFeatureBlockFromFile(blockId);

                if (block == null)
                {
                    block = new DataBlock<Feature>()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.LoadedItems.AddOrSet(info.Id, info);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            FeatureBlockSerializer.WriteFeatureBlockToFile(block);

            FeatureSerializer.CreateFeatureListFile();
        }

        public static void DeleteFeature(int id)
        {
            DataBlock<Feature> block;

            int blockId = id / FeatureBlockSerializer.BLOCK_SIZE;

            if (LoadedInfoBlocks.TryGetValue(blockId, out DataBlock<Feature> foundBlock))
            {
                block = foundBlock;
            }
            else
            {
                block = FeatureBlockSerializer.LoadFeatureBlockFromFile(blockId);

                if (block == null)
                {
                    block = new DataBlock<Feature>()
                    {
                        BlockId = blockId,
                    };
                }
            }

            block.LoadedItems.Remove(id);

            LoadedInfoBlocks.AddOrSet(blockId, block);

            FeatureBlockSerializer.WriteFeatureBlockToFile(block);
            FeatureSerializer.CreateFeatureListFile();
        }

        public static void LoadAllFeatureBlocks(bool force = false)
        {
            var featureBlocks = FeatureBlockSerializer.LoadAllFeatureBlocks();

            foreach (var block in featureBlocks)
            {
                if (!LoadedInfoBlocks.ContainsKey(block.BlockId) || force)
                {
                    LoadedInfoBlocks.AddOrSet(block.BlockId, block);
                }
            }
        }

        public static List<Feature> GetAllLoadedFeatures()
        {
            List<Feature> features = new List<Feature>();

            foreach(var block in LoadedInfoBlocks)
            {
                foreach(var item in block.Value.LoadedItems)
                {
                    features.Add(item.Value);
                }
            }

            return features;
        }
    }
}
