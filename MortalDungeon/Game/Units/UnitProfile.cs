using MortalDungeon.Engine_Classes.Scenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{

    public enum UnitProfileType 
    {
        Unknown,
        Guy,
        Skeleton
    }

    public class UnitProfile
    {
        public string Name;
        public Func<CombatScene, Unit> CreateUnit;
        public UnitProfileType Type;
    }

    public static class UnitProfiles 
    {
        public static List<UnitProfile> Profiles = new List<UnitProfile>();

        static UnitProfiles()
        {
            UnitProfile Guy = new UnitProfile() { Name = "Guy", Type = UnitProfileType.Guy};
            Guy.CreateUnit = (scene) => new Guy(scene) { ProfileType = UnitProfileType.Guy };
            Profiles.Add(Guy);

            UnitProfile Skeleton = new UnitProfile() { Name = "Skeleton", Type = UnitProfileType.Skeleton };
            Skeleton.CreateUnit = (scene) => new Skeleton(scene) { ProfileType = UnitProfileType.Skeleton };
            Profiles.Add(Skeleton);
        }
    }
}
