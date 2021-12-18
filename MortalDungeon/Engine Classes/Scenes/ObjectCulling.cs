using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Scenes
{
    internal static class ObjectCulling
    {
        internal static Frustum Frustum;

        internal static void Initialize() 
        {
            Frustum = new Frustum();
        }

        internal static void UpdateValues(Camera camera) 
        {
            Frustum.CalculateFrustum(camera.ProjectionMatrix, camera.GetViewMatrix());
        }



        private static Vector3 _localPos = new Vector3(0, 0, 0);
        internal static void CullListOfGameObjects<T>(List<T> objList) where T : GameObject 
        {
            objList.ForEach(obj =>
            {
                _localPos.X = obj.Position.X;
                _localPos.Y = obj.Position.Y;
                _localPos.Z = obj.Position.Z;

                float scaleMax = 1;

                if (obj.BaseObjects.Count > 0) 
                {
                    Vector3 scale = obj.BaseObjects[0].BaseFrame.Scale.ExtractScale();

                    scaleMax = scale.X;
                    scaleMax = scaleMax < scale.Y ? scale.Y : scaleMax;
                    scaleMax = scaleMax < scale.Z ? scale.Z : scaleMax;
                }

                scaleMax *= 0.333f; //magic number that seems to pretty accurately determine the edges of a quad in conjuntion with the scale 

                WindowConstants.ConvertGlobalToLocalCoordinatesInPlace(ref _localPos);
                if (Frustum.TestSphere(_localPos.X, _localPos.Y, _localPos.Z, scaleMax))
                {
                    obj.Cull = false;
                }
                else
                {
                    obj.Cull = true;
                }

                obj.OnCull();
            });
        }

        internal static int _culledChunks = 0;
        internal static void CullTileChunk(TileChunk obj)
        {
            if (obj.Tiles.Count == 0)
                return;

            _localPos.X = obj.Center.X;
            _localPos.Y = obj.Center.Y;
            _localPos.Z = obj.Center.Z;

            float scaleMax;

            Vector3 scale = obj.Tiles[0].BaseObjects[0].BaseFrame.CurrentScale;

            scaleMax = scale.X;
            scaleMax = scaleMax < scale.Y ? scale.Y : scaleMax;
            scaleMax = scaleMax < scale.Z ? scale.Z : scaleMax;

            scaleMax *= 0.5f; //another magic number that works pretty well

            scaleMax *= obj.LocalRadius;

            WindowConstants.ConvertGlobalToLocalCoordinatesInPlace(ref _localPos);

            bool prevCull = obj.Cull;

            if (Frustum.TestSphere(_localPos.X, _localPos.Y, _localPos.Z, scaleMax))
            {
                obj.Cull = false;
            }
            else
            {
                obj.Cull = true;
                _culledChunks++;
            }

            if (prevCull != obj.Cull) 
            {
                obj.OnCull();
            }
        }

        internal static void CullListOfParticles(List<ParticleGenerator> objList)
        {
            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].Particles.ForEach(obj =>
                {
                    _localPos.X = obj.Position.X;
                    _localPos.Y = obj.Position.Y;
                    _localPos.Z = obj.Position.Z;

                    float scaleMax = 1;


                    Vector3 scale = obj.Scale.ExtractScale();

                    scaleMax = scale.X;
                    scaleMax = scaleMax < scale.Y ? scale.Y : scaleMax;
                    scaleMax = scaleMax < scale.Z ? scale.Z : scaleMax;

                    scaleMax *= 0.333f; //magic number that seems to pretty accurately determine the edges of a quad in conjuntion with the scale 


                    WindowConstants.ConvertGlobalToLocalCoordinatesInPlace(ref _localPos);

                    if (Frustum.TestSphere(_localPos.X, _localPos.Y, _localPos.Z, scaleMax))
                    {
                        obj.Cull = false;
                    }
                    else
                    {
                        obj.Cull = true;
                    }
                });
            }
        }
    }
}
