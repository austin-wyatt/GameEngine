using MortalDungeon.Engine_Classes.Scenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class UnitProfile
    {
        public string Name;
        public Func<CombatScene, Unit> CreateUnit;
    }

    public static class UnitProfiles 
    {
        public static List<UnitProfile> Profiles = new List<UnitProfile>();

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
