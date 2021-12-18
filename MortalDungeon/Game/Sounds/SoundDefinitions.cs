using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game
{
    internal static class Sounds 
    {
        internal static AudioBuffer Test = new AudioBuffer("Test", "Resources/Sound/test.ogg");
        internal static AudioBuffer Select = new AudioBuffer("Select", "Resources/Sound/select.ogg");
        internal static AudioBuffer Walk = new AudioBuffer("Walk", "Resources/Sound/walk.ogg");
        internal static AudioBuffer UnitHurt = new AudioBuffer("Unit_Hurt", "Resources/Sound/hurt.ogg");
        internal static AudioBuffer ArmorHit = new AudioBuffer("Armor_Hit", "Resources/Sound/armor_hit.ogg");
        internal static AudioBuffer Shoot = new AudioBuffer("Shoot", "Resources/Sound/shoot.ogg");
        internal static AudioBuffer Die = new AudioBuffer("Shoot", "Resources/Sound/die.ogg");

        internal static AudioBuffer[] Walk_Grass = new AudioBuffer[]
        {
            new AudioBuffer("Walk_Grass_1", "Resources/Sound/leaves01.ogg"),
            new AudioBuffer("Walk_Grass_2", "Resources/Sound/leaves02.ogg"),
        };

        internal static AudioBuffer[] Walk_Wood = new AudioBuffer[]
        {
            new AudioBuffer("Walk_Wood_1", "Resources/Sound/wood01.ogg"),
            new AudioBuffer("Walk_Wood_2", "Resources/Sound/wood02.ogg"),
            new AudioBuffer("Walk_Wood_3", "Resources/Sound/wood03.ogg"),
        };

        internal static AudioBuffer[] Walk_Stone = new AudioBuffer[]
        {
            new AudioBuffer("Walk_Stone_1", "Resources/Sound/footstep00.ogg"),
            new AudioBuffer("Walk_Stone_2", "Resources/Sound/footstep01.ogg"),
            new AudioBuffer("Walk_Stone_3", "Resources/Sound/footstep02.ogg"),
        };


        internal static Sound FootstepSound(this SimplifiedTileType type)
        {
            switch (type)
            {
                case SimplifiedTileType.Grass:
                    return new Sound(Walk_Grass[GlobalRandom.Next() % 2]) { Gain = 0.1f};
                case SimplifiedTileType.Stone:
                    return new Sound(Walk_Stone[GlobalRandom.Next() % 3]) { Gain = 0.015f };
                case SimplifiedTileType.Wood:
                    return new Sound(Walk_Wood[GlobalRandom.Next() % 3]) { Gain = 0.1f };
                case SimplifiedTileType.Water:
                default:
                    return new Sound(Walk) { Gain = 0.4f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            }
        }
    }

    internal class Music 
    {
        internal static AudioBuffer FieldOfDreams = new AudioBuffer("Field_Of_Dreams", "Resources/Sound/Music/field_of_dreams.ogg");
        internal static AudioBuffer HopefulMusic = new AudioBuffer("Hopeful_Music", "Resources/Sound/Music/hopeful_music.ogg");
    }
}
