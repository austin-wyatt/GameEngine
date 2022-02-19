using MortalDungeon.Definitions.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MortalDungeon.Game.Items
{
    public static class ItemManager
    {
        public static Dictionary<int, Type> Items = new Dictionary<int, Type>();

        static ItemManager()
        {
            Items.Add(0, typeof(Item));


            var itemTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                       where t.IsClass && t.Namespace == "MortalDungeon.Definitions.Items" && !t.IsSealed &&
                       t.IsSubclassOf(typeof(Item))
                       select t;

            var list = itemTypes.ToList();

            foreach (var type in list)
            {
                var prop = type.GetField("ID");
                if (prop != null)
                {
                    Items.Add((int)prop.GetValue(null), type);
                }
            }
        }

        public static Item GetItemByID(int id)
        {
            if (!Items.ContainsKey(id))
                return null;

            var item = (Item)Activator.CreateInstance(Items[id]);

            return item;
        }
    }
}
