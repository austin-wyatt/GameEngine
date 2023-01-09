using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Scenes
{
    public static class ObjectCulling
    {
        public static Frustum Frustum;

        public static void Initialize() 
        {
            Frustum = new Frustum();
        }

        public static void UpdateValues(Camera camera) 
        {
            Frustum.CalculateFrustum(camera.ProjectionMatrix, camera.GetViewMatrix());
        }



        private static Vector3 _localPos = new Vector3(0, 0, 0);
        public static void CullListOfGameObjects<T>(List<T> objList) where T : GameObject 
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

        public static void CullListOfUnits(List<Unit> objList)
        {
            for(int i = 0; i < objList.Count; i++)
            {
                if(!(objList[i].Info.TileMapPosition != null && objList[i].Info.TileMapPosition.TileMap.Visible))
                {
                    objList[i].Cull = true;
                    objList[i].OnCull();
                    continue;
                }

                _localPos.X = objList[i].Position.X;
                _localPos.Y = objList[i].Position.Y;
                _localPos.Z = objList[i].Position.Z;

                float scaleMax = 1;

                if (objList[i].BaseObjects.Count > 0)
                {
                    Vector3 scale = objList[i].BaseObjects[0].BaseFrame.CurrentScale;

                    scaleMax = scale.X;
                    scaleMax = scaleMax < scale.Y ? scale.Y : scaleMax;
                    scaleMax = scaleMax < scale.Z ? scale.Z : scaleMax;
                }

                scaleMax *= 0.333f; //magic number that seems to pretty accurately determine the edges of a quad in conjunction with the scale 

                WindowConstants.ConvertGlobalToLocalCoordinatesInPlace(ref _localPos);
                if (Frustum.TestSphere(_localPos.X, _localPos.Y, _localPos.Z, scaleMax))
                {
                    objList[i].Cull = false;
                }
                else
                {
                    objList[i].Cull = true;
                }

                objList[i].OnCull();
            }
        }

        public static void CullUnit(Unit obj)
        {
            if (!(obj.Info.TileMapPosition != null && obj.Info.TileMapPosition.TileMap.Visible))
            {
                obj.Cull = true;
                obj.OnCull();
                return;
            }

            _localPos.X = obj.Position.X;
            _localPos.Y = obj.Position.Y;
            _localPos.Z = obj.Position.Z;

            float scaleMax = 1;

            if (obj.BaseObjects.Count > 0)
            {
                Vector3 scale = obj.BaseObjects[0].BaseFrame.CurrentScale;

                scaleMax = scale.X;
                scaleMax = scaleMax < scale.Y ? scale.Y : scaleMax;
                scaleMax = scaleMax < scale.Z ? scale.Z : scaleMax;
            }

            scaleMax *= 0.333f; //magic number that seems to pretty accurately determine the edges of a quad in conjunction with the scale 

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
        }

        public static int _culledChunks = 0;
        public static void CullTileChunk(TileChunk obj)
        {
            if (obj.Tiles.Count == 0)
                return;

            bool prevCull = obj.Cull;

            if (!obj.Tiles[0].TileMap.Visible)
            {
                obj.Cull = true;

                if(prevCull != obj.Cull)
                {
                    obj.OnCull();
                }
                return;
            }


            _localPos.X = obj.Center.X;
            _localPos.Y = obj.Center.Y;
            _localPos.Z = obj.Center.Z;

            //slightly increase the actual radius just to make sure no chunks pop in on the edges of the screen
            float radius = 1.1f * obj.LocalRadius;

            if (Frustum.TestSphere(_localPos.X, _localPos.Y, _localPos.Z, radius))
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

        public static void CullListOfParticles(List<ParticleGenerator> objList)
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
