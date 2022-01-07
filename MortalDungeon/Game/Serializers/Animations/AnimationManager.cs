using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    public static class AnimationManager
    {
        public static Dictionary<string, AnimationSet> AnimationSets = new Dictionary<string, AnimationSet>();

        static AnimationManager()
        {
            var list = AnimationSerializer.LoadAllAnimations();

            foreach(var animation in list)
            {
                AnimationSets.Add(animation.Name, animation);
            }
        }
    }
}
