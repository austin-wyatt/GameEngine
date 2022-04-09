using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Units
{
    public class VisionGenerator
    {
        public Vector2i Position = new Vector2i();
        
        public float Radius = 6; //in tiles

        public UnitTeam Team = UnitTeam.Unknown;

        public HashSet<TilePoint> VisibleTiles = new HashSet<TilePoint>();

        public HashSet<TileMap> AffectedMaps = new HashSet<TileMap>();

        public object _visibleTilesLock = new object();
        public VisionGenerator() { }

        public VisionGenerator(VisionGenerator gen)
        {
            Position = gen.Position;
            Radius = gen.Radius;
            Team = gen.Team;
        }

        public void SetPosition(TilePoint point) 
        {
            Position = Map.FeatureEquation.PointToMapCoords(point);
        }

        public void SetPosition(Vector2i point)
        {
            Position = point;
        }
    }
}
