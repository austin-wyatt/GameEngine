using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Map;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    public struct FeatureLoadInfo
    {
        public FeaturePoint Origin;
        public int LoadRadius;
        public string FilePath;
    }


    /// <summary>
    /// Stores all of the possible feature origin points and load radii in memory. <para/>
    /// When maps are loaded this list will be checked. If any of the points are within 
    /// the load radius then that feature will be added to the tile map controller's loaded features. <para/>
    /// Similarly, if a loaded feature is outside of the load radius then it will be unloaded.
    /// </summary>
    public static class FeatureManager
    {
        private static List<FeatureLoadInfo> AllFeatures = new List<FeatureLoadInfo>();

        public static Dictionary<FeaturePoint, FeatureEquation> LoadedFeatures = new Dictionary<FeaturePoint, FeatureEquation>();

        public static void Initialize()
        {
            string[] files = Directory.GetFiles("Data/");

            foreach (string file in files)
            {
                if (file.Contains(".f"))
                {
                    string temp = file.Substring(file.LastIndexOf('/') + 1);
                    temp = temp.Replace(".f", "").Replace(" ", "");

                    string[] values = temp.Split('_');

                    try 
                    {
                        Vector2i origin = Feature.UnhashCoordinates(long.Parse(values[0]));
                        int loadRadius = int.Parse(values[1]);

                        AllFeatures.Add(new FeatureLoadInfo()
                        {
                            Origin = new FeaturePoint(origin),
                            LoadRadius = loadRadius,
                            FilePath = file
                        });
                    }
                    catch { }
                }
            }
        }
        //when loading a feature, apply any passed state values as well.
        public static void EvaluateLoadedFeatures(FeaturePoint currPosition)
        {
            List<FeaturePoint> featuresToRemove = new List<FeaturePoint>();

            foreach(var eq in LoadedFeatures)
            {
                int loadRadius = eq.Value.LoadRadius < 150 ? 150 : eq.Value.LoadRadius;

                loadRadius += 200; //we don't want to unload the feature as soon as we leave the load radius since that could
                                   //cause some issues if the user walks back and forth between 2 maps repeatedly.

                if (!CheckDistance(eq.Value.Origin, currPosition, loadRadius))
                {
                    featuresToRemove.Add(eq.Value.Origin);
                }
            }

            foreach (var eq in featuresToRemove)
            {
                LoadedFeatures.Remove(eq);
            }


            for(int i = 0; i < AllFeatures.Count; i++)
            {
                if (LoadedFeatures.ContainsKey(AllFeatures[i].Origin))
                {
                    continue;
                }

                if (CheckDistance(AllFeatures[i].Origin, currPosition, AllFeatures[i].LoadRadius))
                {
                    var feature = FeatureSerializer.LoadFeatureFromFile(AllFeatures[i].FilePath);

                    if(feature != null)
                    {
                        var equation = feature.CreateFeatureEquation();

                        if(equation.StateValues.Count > 0)
                        {
                            Ledgers.ApplyStateValues(equation.StateValues);
                        }

                        LoadedFeatures.Add(AllFeatures[i].Origin, equation);
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
