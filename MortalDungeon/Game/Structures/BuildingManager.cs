using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Serializers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    public static class BuildingManager
    {
        public static Dictionary<int, Type> Buildings = new Dictionary<int, Type>();

        static BuildingManager()
        {
            var itemTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.IsClass && t.Namespace == "MortalDungeon.Definitions.Buildings" && !t.IsSealed &&
                            t.IsSubclassOf(typeof(Building))
                            select t;

            var list = itemTypes.ToList();

            foreach (var type in list)
            {
                var prop = type.GetField("GlobalID");
                if (prop != null)
                {
                    Buildings.Add((int)prop.GetValue(null), type);
                }
            }
        }

        public static Building GetBuildingByID(int id)
        {
            if (!Buildings.ContainsKey(id))
                return null;

            var item = (Building)Activator.CreateInstance(Buildings[id]);

            return item;
        }
    }
}
