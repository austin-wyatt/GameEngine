using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public static class TileEffectManager
    {
        public static Dictionary<TilePoint, HashSet<TileEffect>> TileEffects = new Dictionary<TilePoint,HashSet<TileEffect>>();

        private static HashSet<TileEffect> _emptySet = new HashSet<TileEffect>();

        public static void AddTileEffectToPoint(TileEffect effect, TilePoint point)
        {
            if (TileEffects.TryGetValue(point, out var result))
            {
                result.Add(effect);
            }
            else
            {
                var set = new HashSet<TileEffect>();
                set.Add(effect);

                TileEffects.Add(point, set);
            }

            effect.AddedToTile(point);
        }

        public static void RemoveTileEffect(TileEffect effect, TilePoint point)
        {
            if(TileEffects.TryGetValue(point, out var result))
            {
                if (result.Remove(effect))
                {
                    effect.RemovedFromTile(point);
                }
            }
        }

        public static void ClearTileEffects()
        {
            foreach(var item in TileEffects)
            {
                foreach(var effect in item.Value)
                {
                    effect.RemovedFromTile(item.Key);
                }
            }
        }


        public static HashSet<TileEffect> GetTileEffectsOnTilePoint(TilePoint point)
        {
            if(TileEffects.TryGetValue(point, out var result))
            {
                return result;
            }
            else
            {
                return _emptySet;
            }
        }
    }
}
