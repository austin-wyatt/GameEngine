using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    public enum RenderingStates 
    {
        GuassianBlur,
        Fade,
    }
    public static class RenderingQueue
    {
        private static readonly List<Letter> _LettersToRender = new List<Letter>();
        private static readonly List<GameObject> _UIToRender = new List<GameObject>();
        private static readonly List<List<GameObject>> _ObjectsToRender = new List<List<GameObject>>();

        private static readonly List<List<Unit>> _UnitsToRender = new List<List<Unit>>();
        private static readonly List<List<Structure>> _StructuresToRender = new List<List<Structure>>();


        private static readonly List<List<BaseTile>> _TilesToRender = new List<List<BaseTile>>();
        private static readonly List<ParticleGenerator> _ParticleGeneratorsToRender = new List<ParticleGenerator>();
        private static readonly List<GameObject> _TileQuadsToRender = new List<GameObject>();

        private static readonly List<List<GameObject>> _LowPriorityQueue = new List<List<GameObject>>();

        private static readonly List<GameObject> _LightQueue = new List<GameObject>();

        public static ContextManager<RenderingStates> RenderStateManager = new ContextManager<RenderingStates>();

        public static List<InstancedRenderData> StructureRenderData = new List<InstancedRenderData>();

        public static Action RenderSkybox = null;

        /// <summary>
        /// Render all queued objects
        /// </summary>
        public static void RenderQueue()
        {
            RenderSkybox?.Invoke();

            if (RenderStateManager.GetFlag(RenderingStates.Fade))
            {
                RenderFunctions.Fade();
                return;
            }

            //if (RenderStateManager.GetFlag(RenderingStates.GuassianBlur)) 
            //{
            //    Renderer.DrawToFrameBuffer(Renderer.MainFBO); //Framebuffer should only be used when we want to do post processing
            //    Renderer.MainFBO.ClearBuffers();
            //}

            GL.Enable(EnableCap.FramebufferSrgb);


            RenderFunctions.DrawGame();

            GL.Clear(ClearBufferMask.DepthBufferBit);


            //if (RenderStateManager.GetFlag(RenderingStates.GuassianBlur))
            //{
            //    Renderer.RenderFrameBuffer(Renderer.MainFBO);
            //    Renderer.MainFBO.UnbindFrameBuffer();
            //}


            GL.Clear(ClearBufferMask.DepthBufferBit);
            //RenderQueuedLetters();

            GL.Disable(EnableCap.FramebufferSrgb);

            RenderQueuedUI();

            RenderInstancedUIData();
        }


        #region Particle queue
        public static void QueueParticlesForRender(ParticleGenerator generator)
        {
            _ParticleGeneratorsToRender.Add(generator);
        }
        public static void RenderQueuedParticles()
        {
            for (int i = 0; i < _ParticleGeneratorsToRender.Count; i++)
            {
                Renderer.RenderParticlesInstanced(_ParticleGeneratorsToRender[i]);
            }

            _ParticleGeneratorsToRender.Clear();
        }
        #endregion

        #region Text queue
        public static void QueueLettersForRender(List<Letter> letters)
        {
            for (int i = 0; i < letters.Count; i++)
            {
                _LettersToRender.Add(letters[i]);
            }
        }
        public static void QueueTextForRender(List<_Text> text)
        {
            for (int i = 0;i < text.Count; i++)
            {
                if (text[i].Render)
                    QueueLettersForRender(text[i].Letters);
            }
        }
        #endregion

        #region UI queue
        public static void QueueUITextForRender(List<_Text> text, bool scissorFlag = false)
        {
            for (int i = 0; i < text.Count; i++)
            {
                //if (text[i].Render)
                //    QueueUIForRender(text[i].Letters, scissorFlag);
                if (text[i].Render)
                    QueueLettersForRender(text[i].Letters);
            }
        }
        public static void RenderQueuedLetters()
        {
            Renderer.RenderTextInstanced(_LettersToRender, ref Renderer._instancedRenderArray);
            _LettersToRender.Clear();
        }

        public static void QueueNestedUI<T>(List<T> uiObjects, int depth = 0, ScissorData scissorData = null, Action<List<UIObject>> renderAfterParent = null, bool overrideRender = false) where T : UIObject
        {
            List<UIObject> renderAfterParentList = new List<UIObject>();

            try //This is a lazy solution for a random crash. If I can figure out why it's happening then I'll come back to this
            {
                if (uiObjects.Count > 0)
                {
                    for (int i = 0; i < uiObjects.Count; i++)
                    {
                        if (uiObjects[i].Render && !uiObjects[i].Cull)
                        {
                            //if (uiObjects[i].RenderAfterParent && renderAfterParent != null && !overrideRender)
                            //{
                            //    renderAfterParentList.Add(uiObjects[i]);
                            //    continue;
                            //}

                            if (uiObjects[i].ScissorData.Scissor == true)
                            {
                                scissorData = uiObjects[i].ScissorData;
                                scissorData._startingDepth = depth;
                            }

                            bool scissorFlag = false;
                            if (scissorData != null && depth - scissorData._startingDepth <= scissorData.Depth && depth != scissorData._startingDepth)
                            {
                                scissorFlag = true;
                            }
                            else
                            {
                                scissorData = null;
                            }

                            QueueUITextForRender(uiObjects[i].TextObjects, scissorFlag || uiObjects[i].ScissorData.Scissor);

                            List<UIObject> objsToRenderAfterParent = new List<UIObject>();

                            if (uiObjects[i].Children.Count > 0)
                            {
                                QueueNestedUI(uiObjects[i].Children, depth + 1, uiObjects[i].ScissorData.Scissor ? uiObjects[i].ScissorData : scissorData, (list) =>
                                {
                                    objsToRenderAfterParent = list;
                                });
                            }

                            QueueUIForRender(uiObjects[i], scissorFlag || uiObjects[i].ScissorData.Scissor);

                            if (objsToRenderAfterParent.Count > 0)
                            {
                                QueueNestedUI(objsToRenderAfterParent, depth + 1, uiObjects[i].ScissorData.Scissor ? uiObjects[i].ScissorData : scissorData, null, true);
                            }
                        }
                    }
                }

                if (renderAfterParentList.Count > 0 && renderAfterParent != null)
                {
                    renderAfterParent(renderAfterParentList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in QueueNestedUI: " + e.Message);
            }
        }

        public static void QueueUIForRender<T>(List<T> objList, bool scissorFlag = false) where T : GameObject
        {
            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].ScissorData._scissorFlag = scissorFlag;

                _UIToRender.Add(objList[i]);
            }
        }
        public static void QueueUIForRender<T>(T obj, bool scissorFlag = false) where T : GameObject
        {
            obj.ScissorData._scissorFlag = scissorFlag;

            _UIToRender.Add(obj);
        }
        public static void RenderQueuedUI()
        {
            Renderer.RenderObjectsInstancedGeneric(_UIToRender, ref Renderer._instancedRenderArray, null, true, false);
            _UIToRender.Clear();
        }

        #endregion

        #region Object queue
        public static void QueueObjectsForRender(List<GameObject> objList)
        {
            _ObjectsToRender.Add(objList);
        }
        public static void RenderQueuedObjects()
        {
            for(int i = 0; i < _ObjectsToRender.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_ObjectsToRender[i], ref Renderer._instancedRenderArray);
            }
            _ObjectsToRender.Clear();
        }
        #endregion

        #region Unit queue
        public static void QueueUnitsForRender(List<Unit> objList)
        {
            _UnitsToRender.Add(objList);
        }

        public static void RenderQueuedUnits()
        {
            for(int i = 0; i < _UnitsToRender.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_UnitsToRender[i], ref Renderer._instancedRenderArray);
            }

            _UnitsToRender.Clear();
        }
        #endregion

        #region Tile queue
        public static void QueueTileObjectsForRender(List<BaseTile> objList)
        {
            if (objList.Count == 0)
                return;

            _TilesToRender.Add(objList);
        }

        public static void RenderTileQueue()
        {
            for(int i = 0; i < _TilesToRender.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_TilesToRender[i], ref Renderer._instancedRenderArray);
            }

            _TilesToRender.Clear();
        }
        #endregion

        #region Tile quad queue
        public static void QueueTileQuadForRender(GameObject obj)
        {
            if (obj == null)
                return;

            _TileQuadsToRender.Add(obj);
        }

        private static readonly List<GameObject> _tempGameObjList = new List<GameObject>();
        public static void RenderTileQuadQueue()
        {
            for (int i = 0; i < _TileQuadsToRender.Count; i++)
            {
                _tempGameObjList.Add(_TileQuadsToRender[i]);
                Renderer.RenderObjectsInstancedGeneric(_tempGameObjList, ref Renderer._instancedRenderArray);
                _tempGameObjList.Clear();
            }

            _TileQuadsToRender.Clear();
        }
        #endregion

        #region Low priority object queue
        public static void QueueLowPriorityObjectsForRender(List<GameObject> objList)
        {
            if (objList.Count == 0)
                return;

            _LowPriorityQueue.Add(objList);
        }
        public static void RenderLowPriorityQueue()
        {
            for (int i = 0; i < _LowPriorityQueue.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_LowPriorityQueue[i], ref Renderer._instancedRenderArray);
            }

            _LowPriorityQueue.Clear();
        }
        #endregion


        #region Structure instanced render data
        public static void GenerateStructureInstancedRenderData(List<Structure> structures)
        {
            for (int i = 0; i < StructureRenderData.Count; i++)
            {
                StructureRenderData[i].CleanUp();
            }

            StructureRenderData = InstancedRenderData.GenerateInstancedRenderData(structures);
        }

        public static void RenderInstancedStructureData()
        {
            Renderer.RenderInstancedRenderData(StructureRenderData);
        }

        #endregion

        #region Tile instanced render data
        private static List<InstancedRenderData> _tileRenderData = new List<InstancedRenderData>();

        public static void ClearTileInstancedRenderData()
        {
            _tileRenderData.Clear();
            _fogTileRenderData.Clear();
            _tilePillarRenderData.Clear();
        }
        public static void QueueTileInstancedRenderData(InstancedRenderData tileData)
        {
            _tileRenderData.Add(tileData);
        }

        public static void RenderInstancedTileData()
        {
            TileMapController.TileOverlaySpritesheet.Use(TextureUnit.Texture1);

            Renderer.RenderInstancedRenderData(_tilePillarRenderData);

            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            //GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);

            Renderer.RenderInstancedRenderData(_tileRenderData);

            GL.StencilMask(0x00);
            Renderer.RenderInstancedRenderData(_fogTileRenderData);

            GL.StencilMask(0xFF);
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.Disable(EnableCap.DepthTest);
            RenderFogQuad();
            GL.Enable(EnableCap.DepthTest);

            GL.Disable(EnableCap.StencilTest);

            ClearTileInstancedRenderData();
        }

        private static List<InstancedRenderData> _fogTileRenderData = new List<InstancedRenderData>();

        public static void QueueFogTileInstancedRenderData(InstancedRenderData tileData)
        {
            _fogTileRenderData.Add(tileData);
        }

        private static List<InstancedRenderData> _tilePillarRenderData = new List<InstancedRenderData>();
        public static void QueueTilePillarInstancedRenderData(InstancedRenderData pillarData)
        {
            _tilePillarRenderData.Add(pillarData);
        }

        #endregion

        #region UI instanced render data
        private static List<UIInstancedRenderData> _uiRenderData = new List<UIInstancedRenderData>();
        public static void QueueUIInstancedRenderData(UIManager manager)
        {
            for(int i = 0; i < manager.UIRenderGroups.Count; i++)
            {
                for(int j = 0; j < manager.UIRenderGroups[i].RenderBatches.Count; j++)
                {
                    _uiRenderData.Add(manager.UIRenderGroups[i].RenderBatches[j].RenderData);
                }
            }
        }

        public static void RenderInstancedUIData()
        {
            Renderer.RenderInstancedRenderData(_uiRenderData);

            _uiRenderData.Clear();
        }

        #endregion

        #region Fog render data
        private static List<GameObject> _fogQuad = new List<GameObject>();

        public static void SetFogQuad(GameObject quad)
        {
            _fogQuad.Clear();
            _fogQuad.Add(quad);
        }

        public static void RenderFogQuad()
        {
            Renderer.RenderObjectsInstancedGeneric(_fogQuad, ref Renderer._instancedRenderArray);
            _fogQuad.Clear();
        }

        #endregion

    }
}
