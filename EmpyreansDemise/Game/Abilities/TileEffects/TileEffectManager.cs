using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public static class TileEffectManager
    {
        public static Dictionary<TilePoint, HashSet<TileEffect>> TileEffects = new Dictionary<TilePoint,HashSet<TileEffect>>();

        private static HashSet<TileEffect> _emptySet = new HashSet<TileEffect>();

        public static object _tileEffectLock = new object();

        public static void AddTileEffectToPoint(TileEffect effect, TilePoint point)
        {
            lock (_tileEffectLock)
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
        }

        public static void RemoveTileEffectImmediately(TileEffect effect, TilePoint point)
        {
            lock (_tileEffectLock)
            {
                if (TileEffects.TryGetValue(point, out var result))
                {
                    if (result.Remove(effect))
                    {
                        effect.RemovedFromTile(point);
                    }
                }
            }
        }

        public static void RemoveTileEffectOnRoundEnd(TileEffect effect, TilePoint point)
        {
            if(_effectsToRemove.Count == 0)
            {
                TileMapManager.Scene.RoundEnd += RemoveTileEffects;
            }

            _effectsToRemove.Add((point, effect));
        }

        private static List<(TilePoint point, TileEffect effect)> _effectsToRemove = new List<(TilePoint, TileEffect)>();
        private static void RemoveTileEffects(object s, EventArgs e)
        {
            lock (_tileEffectLock)
            {
                foreach(var tuple in _effectsToRemove)
                {
                    if (TileEffects.TryGetValue(tuple.point, out var result))
                    {
                        if (result.Remove(tuple.effect))
                        {
                            tuple.effect.RemovedFromTile(tuple.point);
                        }
                    }
                }

                _effectsToRemove.Clear();
            }
        }
        

        public static void ClearTileEffects()
        {
            lock (_tileEffectLock)
            {
                foreach (var item in TileEffects)
                {
                    foreach (var effect in item.Value)
                    {
                        effect.RemovedFromTile(item.Key);
                    }
                }
            }
        }


        public static HashSet<TileEffect> GetTileEffectsOnTilePoint(TilePoint point)
        {
            lock (_tileEffectLock)
            {
                if (TileEffects.TryGetValue(point, out var result))
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
}
