using Empyrean.Engine_Classes;
using Empyrean.Game.Map;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Abilities.TileEffects
{
    [Serializable]
    public class TileEffectsSaveInfo : ISerializable
    {
        public List<FeaturePoint> Keys = new List<FeaturePoint>();
        public List<HashSet<TileEffect>> Values = new List<HashSet<TileEffect>>();

        public const string TILE_EFFECT_NAMESPACE = "Empyrean.Definitions.TileEffects.";

        public void CompleteDeserialization()
        {
            lock (TileEffectManager._tileEffectLock)
            {
                TileEffectManager.ClearTileEffects();

                TileEffectManager.TileEffects.Clear();

                for (int i = 0; i < Keys.Count; i++)
                {
                    var tile = TileMapHelpers.GetTile(Keys[i]);

                    if (tile != null)
                    {
                        HashSet<TileEffect> newEffects = new HashSet<TileEffect>();

                        foreach (var item in Values[i])
                        {
                            item.CompleteDeserialization();

                            Type type = Type.GetType(TILE_EFFECT_NAMESPACE + item._typeName);

                            if (type != null)
                            {
                                var newEffect = Activator.CreateInstance(type, new object[] { item }) as TileEffect;

                                newEffect.OnRecreated(tile);

                                newEffects.Add(newEffect);
                            }
                        }


                        TileEffectManager.TileEffects.AddOrSet(tile, newEffects);
                    }
                }
            }
        }

        public void PrepareForSerialization()
        {
            Keys.Clear();
            Values.Clear();

            lock (TileEffectManager._tileEffectLock)
            {
                foreach (var kvp in TileEffectManager.TileEffects)
                {
                    HashSet<TileEffect> newEffects = new HashSet<TileEffect>();

                    foreach (var item in kvp.Value)
                    {
                        item.PrepareForSerialization();

                        newEffects.Add(new TileEffect(item));
                    }

                    Keys.Add(kvp.Key.ToFeaturePoint());
                    Values.Add(newEffects);
                }
            }
        }
    }
}
