using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.GameObjects;
using Empyrean.Game.Objects;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Structures
{
    class Grass : Structure
    {
        public StructureEnum GrassType = StructureEnum.Grass;

        public Grass(CombatScene scene, Tile tile) : base(scene)
        {
            //BaseObject.BaseFrame.RotateX(25);

            AnimationSet = AnimationSerializer.AllAnimationSets.LazyGet("Grass");

            AddBaseObject(CreateBaseObject());
            //BaseObject.BaseFrame.SetScale(0.1f, 0.1f, 0.05f);

            BaseObject.BaseFrame.CameraPerspective = true;

            //SetPosition(tile.Position + new Vector3(-50 + (float)TileMap._randomNumberGen.NextDouble() * 100,
            //    -300 + (float)TileMap._randomNumberGen.NextDouble() * 200, 0.001f));

            SetPosition(tile.Position + new Vector3(0, 0, 0.001f));

            VisibleThroughFog = true;
            SetTileMapPosition(tile);
            Name = "Grass";
            Pathable = true;

            //HasContextMenu = true;

            SetTeam(UnitTeam.Unknown);
            Info.Height = 0;

            LoadTexture(this);
        }

        //public override void InitializeVisualComponent()
        //{
        //    //base.InitializeVisualComponent();

        //    VisibleThroughFog = true;

        //    SelectionTile = new UnitSelectionTile(this, new Vector3(0, 0, -0.19f));
        //    AddBaseObject(CreateBaseObject());
        //    BaseObject.BaseFrame.SetScale(0.5f, 0.5f, 0.25f);

        //    BaseObject.BaseFrame.CameraPerspective = true;

        //    LoadTexture(this);
        //}

        //public override BaseObject CreateBaseObject()
        //{
        //    BaseObject obj = _3DObjects.CreateBaseObject(new SpritesheetObject((int)StructureEnum.Grass, Spritesheets.StructureSheet), _3DObjects.Grass, default);

        //    obj.BaseFrame.SetBaseColor(Color);

        //    return obj;
        //}
    }
}
