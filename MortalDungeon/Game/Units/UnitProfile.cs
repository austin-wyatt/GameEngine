using MortalDungeon.Engine_Classes.Scenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    internal class UnitProfile
    {
        internal string Name;
        internal Func<CombatScene, Unit> CreateUnit;
    }

    internal static class UnitProfiles 
    {
        internal static List<UnitProfile> Profiles = new List<UnitProfile>();

        static UnitProfiles()
        {
            UnitProfile Guy = new UnitProfile() { Name = "Guy" };
            Guy.CreateUnit = (scene) => new Guy(scene);
            Profiles.Add(Guy);

            UnitProfile Skeleton = new UnitProfile() { Name = "Skeleton" };
            Skeleton.CreateUnit = (scene) => new Skeleton(scene);
            Profiles.Add(Skeleton);


        }
    }
}
