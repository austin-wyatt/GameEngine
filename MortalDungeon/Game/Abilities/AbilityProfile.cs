using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class AbilityProfile
    {
        public string Name;
        public Func<Unit, Ability> CreateAbility;
    }

    public static class AbilityProfiles
    {
        public static List<AbilityProfile> Profiles = new List<AbilityProfile>();

        static AbilityProfiles()
        {
            AbilityProfile Bleed = new AbilityProfile() { Name = "Bleed" };
            Bleed.CreateAbility = (unit) => new Bleed(unit);
            Profiles.Add(Bleed);

            AbilityProfile Channel = new AbilityProfile() { Name = "Channel" };
            Channel.CreateAbility = (unit) => new Channel(unit, "", "");
            Profiles.Add(Channel);

            AbilityProfile Hide = new AbilityProfile() { Name = "Hide" };
            Hide.CreateAbility = (unit) => new Hide(unit);
            Profiles.Add(Hide);

            AbilityProfile Move = new AbilityProfile() { Name = "Move" };
            Move.CreateAbility = (unit) => new Move(unit);
            Profiles.Add(Move);

            AbilityProfile Shoot = new AbilityProfile() { Name = "Shoot" };
            Shoot.CreateAbility = (unit) => new Shoot(unit);
            Profiles.Add(Shoot);

            AbilityProfile Slow = new AbilityProfile() { Name = "Slow" };
            Slow.CreateAbility = (unit) => new Slow(unit);
            Profiles.Add(Slow);

            AbilityProfile SpawnSkeleton = new AbilityProfile() { Name = "SpawnSkeleton" };
            SpawnSkeleton.CreateAbility = (unit) => new SpawnSkeleton(unit);
            Profiles.Add(SpawnSkeleton);

            AbilityProfile Strike = new AbilityProfile() { Name = "Strike" };
            Strike.CreateAbility = (unit) => new Strike(unit);
            Profiles.Add(Strike);
        }
    }
}
