﻿using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Structures
{
    public enum StructureEnum 
    {
        Unknown = 0,

        Tree_1 = 2,
        Tree_2 = 3,
        Grass = 4,

        Rock_1 = 10,
        Rock_2 = 11,
        Rock_3 = 12,

        Grave_1 = 20,
        Grave_2 = 21,
        Grave_3 = 22,

        Wall_1 = 30,
        Wall_Corner = 31,
        Wall_Door = 32,
        Wall_Wood_1 = 40,
        Wall_Wood_Corner = 41,
        Wall_Wood_Door = 42,
        Wall_Iron_1 = 50,
        Wall_Iron_Door = 51,

        Cliff_1 = 90,
        Cliff_2 = 91,
        Cliff_3 = 92,
        Cliff_4 = 93,
        Cliff_5 = 94,
        Cliff_6 = 95,

        Tent = 1000
    }
    public class Structure : Unit
    {
        public StructureEnum Type;

        public bool Pathable = false;
        public bool Passable = false; //when passable the height of the object is not factored into the pathable height

        /// <summary>
        /// This will initialize nothing. Any structures created with this must create a valid GameObject before attempting to be rendered.
        /// </summary>
        public Structure() 
        {
            Name = "Structure";

            Type = StructureEnum.Unknown;

            _createStatusBar = false;
        }
        public Structure(CombatScene scene) : base(scene)
        {
            Name = "Structure";

            Type = StructureEnum.Unknown;

            _createStatusBar = false;
        }
        public Structure(CombatScene scene, Spritesheet spritesheet, int spritesheetPos, Vector3 position = default) : base(scene, spritesheet, spritesheetPos, position)
        {
            Name = "Structure";

            Type = (StructureEnum)spritesheetPos;

            _createStatusBar = false;
        }

        public override void SetTileMapPosition(Tile baseTile)
        {
            Tile prevTile = Info.TileMapPosition;

            if (prevTile != null)
                prevTile.RemoveStructure(this);

            baseTile.AddStructure(this);

            Info.TileMapPosition = baseTile;

            VisionGenerator.SetPosition(baseTile.TilePoint);

            Scene.OnStructureMoved();
        }

        public override void CleanUp()
        {
            CleanUpEvent(this);

            Scene.RemoveVisionGenerator(VisionGenerator);

            //SetTextureLoaded(false);
        }

        public virtual void Removed()
        {

        }
    }
}
