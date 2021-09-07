using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Tiles.HelperTiles
{
    public class HeightIndicatorTile : GameObject
    {
        public enum Animations 
        {
            Up,
            Down
        }

        public BaseTile AttachedTile;

        public HeightIndicatorTile(BaseTile attachedTile){
            Name = "HeightIndicator";
            AttachedTile = attachedTile;

            RenderableObject tileUp = new RenderableObject(new SpritesheetObject(31, Spritesheets.TileSheet).CreateObjectDefinition(ObjectIDs.BASE_TILE, EnvironmentObjects.BaseTileBounds, true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            RenderableObject tileDown = new RenderableObject(new SpritesheetObject(32, Spritesheets.TileSheet).CreateObjectDefinition(ObjectIDs.BASE_TILE, EnvironmentObjects.BaseTileBounds, true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            Animation Up = new Animation()
            {
                Frames = new List<RenderableObject>() { tileUp },
                Frequency = 0,
                Repeats = 0,
                GenericType = (int)Animations.Up
            };

            Animation Down = new Animation()
            {
                Frames = new List<RenderableObject>() { tileDown },
                Frequency = 0,
                Repeats = 0,
                GenericType = (int)Animations.Down
            };

            BaseObject tile = new BaseObject(new List<Animation>() { Up, Down }, ObjectID, "Height Indicator " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            tile.BaseFrame.CameraPerspective = true;
            tile.BaseFrame.TextureReference = attachedTile.BaseObjects[0].BaseFrame.TextureReference;

            AddBaseObject(tile);

            MultiTextureData.MixTexture = false;

            SetPosition(attachedTile.Position + new Vector3(0, 0, 0.05f));
        }
    }
}
