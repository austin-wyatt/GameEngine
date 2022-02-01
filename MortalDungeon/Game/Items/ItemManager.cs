using MortalDungeon.Definitions.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Items
{
    public static class ItemManager
    {
        public static Dictionary<int, Type> Items = new Dictionary<int, Type>();

        static ItemManager()
        {
            Items.Add(0, typeof(Item));
            Items.Add(1, typeof(Dagger_1));
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
