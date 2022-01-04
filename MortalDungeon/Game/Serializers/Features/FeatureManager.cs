using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Map;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    /// <summary>
    /// Stores all of the possible feature origin points and load radii in memory. <para/>
    /// When maps are loaded this list will be checked. If any of the points are within 
    /// the load radius then that feature will be added to the tile map controller's loaded features. <para/>
    /// Similarly, if a loaded feature is outside of the load radius then it will be unloaded.
    /// </summary>
    public static class FeatureManager
    {
        public static FeatureList AllFeatures = new FeatureList();

        public static Dictionary<FeaturePoint, FeatureEquation> LoadedFeatures = new Dictionary<FeaturePoint, FeatureEquation>();

        public static void Initialize()
        {
            LoadedFeatures = new Dictionary<FeaturePoint, FeatureEquation>();
            AllFeatures = FeatureSerializer.LoadFeatureListFile();
        }
        //when loading a feature, apply any passed state values as well.
        public static void EvaluateLoadedFeatures(FeaturePoint currPosition, int layer)
        {
            List<FeaturePoint> featuresToRemove = new List<FeaturePoint>();

            foreach(var eq in LoadedFeatures)
            {
                int loadRadius = eq.Value.LoadRadius < 150 ? 150 : eq.Value.LoadRadius;

                loadRadius += 200; //we don't want to unload the feature as soon as we leave the load radius since that could
                                   //cause some issues if the user walks back and forth between 2 maps repeatedly.

                if (layer != eq.Value.Layer || !CheckDistance(eq.Value.Origin, currPosition, loadRadius))
                {
                    featuresToRemove.Add(eq.Value.Origin);
                }
            }

            foreach (var eq in featuresToRemove)
            {
                LoadedFeatures.Remove(eq);
            }


            for(int i = 0; i < AllFeatures.Features.Count; i++)
            {
                if (LoadedFeatures.ContainsKey(AllFeatures.Features[i].Origin))
                {
                    continue;
                }

                if (layer == AllFeatures.Features[i].Layer && CheckDistance(AllFeatures.Features[i].Origin, currPosition, AllFeatures.Features[i].LoadRadius))
                {
                    var feature = FeatureSerializer.LoadFeatureFromFile(AllFeatures.Features[i].Id);

                    if(feature != null)
                    {
                        var equation = feature.CreateFeatureEquation();

                        if(equation.StateValues.Count > 0)
                        {
                            Ledgers.ApplyStateValues(equation.StateValues);
                        }

                        LoadedFeatures.Add(AllFeatures.Features[i].Origin, equation);
                    }
                }
            }
        }


        public static bool CheckDistance(FeaturePoint start, FeaturePoint end, int targetDistance) 
        {
            return ((start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y)) < targetDistance * targetDistance;
        }
    }
}
