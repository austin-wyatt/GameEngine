using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Units
{
    public enum SelectionType
    {
        Select,
        Target
    }

    public static class SelectionIndicatorManager
    {
        private static ObjectPool<IndividualMesh> _indicatorPool = new ObjectPool<IndividualMesh>();
        public static Dictionary<Unit, IndividualMesh> SelectedUnits = new Dictionary<Unit, IndividualMesh>();

        private static Dictionary<IndividualMesh, HashSet<PropertyAnimation>> _indicatorPropertyAnimations = new Dictionary<IndividualMesh, HashSet<PropertyAnimation>>();

        private const string _selectionFilePath = "Resources/Textures/SelectionCircle.png";
        private const string _targetFilePath = "Resources/Textures/TargetIndicator.png";

        private static Dictionary<string, int> _fileTextureIds = new Dictionary<string, int>()
        {
            { _selectionFilePath, 20000},
            { _targetFilePath, 20001},
        };

        public static void SelectUnit(Unit unit, string filePath = _selectionFilePath)
        {
            IndividualMesh indicator = _indicatorPool.GetObject();

            List<Tile> tiles = unit.Info.TileMapPosition.TileMap.GetTilesInRadius(unit.Info.TileMapPosition, 1);

            indicator.FillFromTiles(tiles);

            Vector3 localPos = WindowConstants.ConvertGlobalToLocalCoordinates(unit._actualPosition);
            localPos.Z = unit.Info.TileMapPosition.Properties.Height + 0.001f;

            indicator.SetTranslation(localPos);

            indicator.TextureTransformations.SetScale(new Vector2(2.5f, 2.5f), new Vector2(0.5f, 0.5f));


            indicator.Color = GetTeamColorFromUnit(unit);

            if (indicator.Texture == null || (indicator.Texture.FileName != filePath))
            {
                indicator.Texture = new SimpleTexture(filePath, _fileTextureIds[filePath]) 
                { 
                    WrapType = TextureWrapType.ClampToEdge 
                };
            }
            indicator.Texture.TextureLoaded = false;
            indicator.LoadTexture();

            Window.Scene.IndividualMeshes.Add(indicator);
            
            if(SelectedUnits.TryGetValue(unit, out var mesh))
            {
                DeselectUnit(unit);
            }

            SelectedUnits.AddOrSet(unit, indicator);
            unit.SelectionIndicator = indicator;

            AddPulsingAnimationToIndicator(indicator);
        }

        public static void DeselectUnit(Unit unit)
        {
            //release the selected unit's indicator back into the indicator pool after removing it from the scene
            if (SelectedUnits.TryGetValue(unit, out var mesh))
            {
                Window.Scene.IndividualMeshes.Remove(mesh);
                SelectedUnits.Remove(unit);
                unit.SelectionIndicator = null;

                _indicatorPool.FreeObject(mesh);

                if(_indicatorPropertyAnimations.TryGetValue(mesh, out var animations))
                {
                    foreach(var anim in animations)
                    {
                        Window.Scene.Tick -= anim.Tick;
                    }

                    _indicatorPropertyAnimations.Remove(mesh);
                }
            }
        }

        public static void TargetUnit(Unit unit)
        {
            SelectUnit(unit, _targetFilePath);
        }

        public static void UntargetUnit(Unit unit)
        {
            DeselectUnit(unit);
        }


        public static void UpdateIndicatorPosition(Unit unit)
        {
            Vector3 posOffset = unit._actualPosition - unit.Info.TileMapPosition._position;
            posOffset.Z = 0;

            //posOffset.X *= X_FACTOR;
            //posOffset.Y *= Y_FACTOR;

            posOffset.X *= -0.0008f;
            posOffset.Y *= 0.00078f;

            unit.SelectionIndicator.TextureTransformations.SetTranslation(posOffset.Xy);
        }

        public static void UpdateIndicatorTilePosition(Unit unit)
        {
            List<Tile> tiles = unit.Info.TileMapPosition.TileMap.GetTilesInRadius(unit.Info.TileMapPosition, 1);

            unit.SelectionIndicator.FillFromTiles(tiles);
            Vector3 localPos = WindowConstants.ConvertGlobalToLocalCoordinates(unit._actualPosition);
            localPos.Z = unit.Info.TileMapPosition.Properties.Height + 0.01f;

            unit.SelectionIndicator.SetTranslation(localPos);
            UpdateIndicatorPosition(unit);
        }

        private static Vector4 GetTeamColorFromUnit(Unit unit)
        {
            Relation relation = unit.AI.Team.GetRelation(UnitTeam.PlayerUnits);
            switch (relation)
            {
                case Relation.Friendly:
                    return _Colors.Green;
                case Relation.Neutral:
                    return _Colors.Tan;
                case Relation.Hostile:
                    return _Colors.LessAggressiveRed;
            }

            return _Colors.Purple;
        }

        public static void UpdateIndicatorColor(Unit unit)
        {
            unit.SelectionIndicator.Color = GetTeamColorFromUnit(unit);
        }

        private static void AddPulsingAnimationToIndicator(IndividualMesh indicator)
        {
            PropertyAnimation anim = new PropertyAnimation();

            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 pulseIn = new Vector2(0.02f, 0.02f);
            Vector2 pulseOut = new Vector2(-0.02f, -0.02f);

            for (int i = 0; i < 10; i++)
            {
                int capturedIndex = i;

                Keyframe temp = new Keyframe(capturedIndex * 8, () =>
                {
                    if (capturedIndex < 5)
                    {
                        indicator.TextureTransformations.ScaleBy(pulseIn, center);
                    }
                    else
                    {
                        indicator.TextureTransformations.ScaleBy(pulseOut, center);
                    }
                });

                anim.Keyframes.Add(temp);
            }

            anim.Play();
            anim.Repeat = true;

            if(_indicatorPropertyAnimations.TryGetValue(indicator, out var animations))
            {
                animations.Add(anim);
            }
            else
            {
                _indicatorPropertyAnimations.Add(indicator, new HashSet<PropertyAnimation>() { anim });
            }

            Window.Scene.Tick += anim.Tick;
        }
    }
}
