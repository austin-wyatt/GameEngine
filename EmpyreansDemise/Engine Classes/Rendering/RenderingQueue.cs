using Empyrean.Game.Structures;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    public enum RenderingStates 
    {
        GuassianBlur,
        Fade,
    }
    public static class RenderingQueue
    {
        private static readonly List<RenderBatch> _UIToRender = new List<RenderBatch>();
        private static readonly List<List<GameObject>> _ObjectsToRender = new List<List<GameObject>>();

        private static readonly List<List<Unit>> _UnitsToRender = new List<List<Unit>>();
        private static readonly List<List<Structure>> _StructuresToRender = new List<List<Structure>>();


        private static readonly List<List<BaseTile>> _TilesToRender = new List<List<BaseTile>>();
        private static readonly List<ParticleGenerator> _ParticleGeneratorsToRender = new List<ParticleGenerator>();
        private static readonly List<GameObject> _TileQuadsToRender = new List<GameObject>();

        private static readonly List<List<GameObject>> _LowPriorityQueue = new List<List<GameObject>>();

        private static List<IndividualMesh> _IndividualMeshsToRender = new List<IndividualMesh>();

        public static ContextManager<RenderingStates> RenderStateManager = new ContextManager<RenderingStates>();

        public static List<InstancedRenderData> StructureRenderData = new List<InstancedRenderData>();
        public static List<InstancedRenderData> FogStructureRenderData = new List<InstancedRenderData>();

        /// <summary>
        /// Render all queued objects
        /// </summary>
        public static void RenderQueue()
        {
            if(Window.SkyBox != null)
            {
                RenderFunctions.DrawSkybox();
            }

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

            GL.Enable(EnableCap.FramebufferSrgb);
            RenderLowPriorityQueue();
            GL.Disable(EnableCap.FramebufferSrgb);
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

        #region UI queue

        public static void QueueNestedUI(List<UIObject> uiObjects, RenderBatch currentBatch = null)
        {
            if(currentBatch == null)
            {
                currentBatch = RenderBatch.Get();
                _UIToRender.Add(currentBatch);
            }

            try //This is a lazy solution for a random crash. If I can figure out why it's happening then I'll come back to this
            {
                if (uiObjects.Count > 0)
                {
                    for (int i = 0; i < uiObjects.Count; i++)
                    {
                        if (uiObjects[i].Render && !uiObjects[i].Cull)
                        {
                            if (currentBatch.Items.Count == RenderBatch.BATCH_SIZE)
                            {
                                RenderBatch newBatch = RenderBatch.Get();
                                newBatch.CopyParameters(currentBatch);

                                _UIToRender.Add(newBatch);
                                currentBatch = newBatch;
                            }

                            //start scissor
                            if (uiObjects[i].ScissorData.Scissor == true)
                            {
                                RenderBatch scissorBatch = RenderBatch.Get();
                                scissorBatch.RenderBatchType = RenderBatchType.Scissor;
                                scissorBatch.ScissorData = uiObjects[i].ScissorData;

                                if(currentBatch.RenderBatchType == RenderBatchType.Scissor)
                                    scissorBatch.DependentBatches.Add(currentBatch);

                                _UIToRender.Add(scissorBatch);

                                currentBatch = scissorBatch;
                            }

                            if (uiObjects[i].Children.Count > 0)
                            {
                                QueueNestedUI(uiObjects[i].Children, currentBatch);
                            }

                            for(int j = 0; j < uiObjects[i].TextStrings.Count; j++)
                            {
                                currentBatch.TextStrings.Add(uiObjects[i].TextStrings[j]);
                            }

                            currentBatch.Items.Add(uiObjects[i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in QueueNestedUI: " + e.Message);
            }
        }

        public static void RenderQueuedUI()
        {
            for(int i = 0; i < _UIToRender.Count; i++)
            {
                if (_UIToRender[i].Items.Count == 0)
                {
                    _UIToRender[i].Free();
                    continue;
                }

                if(_UIToRender[i].RenderBatchType == RenderBatchType.Default)
                {
                    Renderer.RenderObjectsInstancedGeneric(_UIToRender[i].Items, ref Renderer._instancedRenderArray, null, true, false, deferredShading: false);

                    if(_UIToRender[i].TextStrings.Count > 0)
                    {
                        TextRenderer.RenderTextStrings(_UIToRender[i].TextStrings);
                    }

                    _UIToRender[i].Free();
                }
                else if(_UIToRender[i].RenderBatchType == RenderBatchType.Scissor)
                {
                    int stencilVal = 1;

                    GL.Clear(ClearBufferMask.StencilBufferBit);

                    GL.Enable(EnableCap.StencilTest);
                    GL.StencilFunc(StencilFunction.Always, stencilVal, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                    GL.StencilMask(0xFF);

                    for (int j = 0; j < _UIToRender[i].DependentBatches.Count; j++)
                    {
                        throw new NotImplementedException();
                        if (j == 1)
                        {
                            //intersect each nested scissor area together
                            GL.StencilFunc(StencilFunction.Equal, stencilVal, 0xFF);
                            GL.StencilOp(StencilOp.Incr, StencilOp.Incr, StencilOp.Incr);
                        }
                        _UIToRender[i].DependentBatches[j].DrawScissorQuad();
                    }

                    _UIToRender[i].DrawScissorQuad();

                    GL.StencilFunc(StencilFunction.Equal, stencilVal, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                    Renderer.RenderObjectsInstancedGeneric(_UIToRender[i].Items, ref Renderer._instancedRenderArray, null, true, false, deferredShading: false);

                    if (_UIToRender[i].TextStrings.Count > 0)
                    {
                        TextRenderer.RenderTextStrings(_UIToRender[i].TextStrings);
                    }

                    GL.Disable(EnableCap.StencilTest);

                    _UIToRender[i].Free();
                }
            }
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
                Renderer.RenderObjectsInstancedGeneric(_ObjectsToRender[i], ref Renderer._instancedRenderArray, deferredShading: false);
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
        }

        public static void ClearUnitQueue()
        {
            _UnitsToRender.Clear();
        }
        #endregion

        #region BaseTile queue
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
                Renderer.RenderObjectsInstancedGeneric(_TilesToRender[i], ref Renderer._instancedRenderArray, deferredShading: false);
            }

            _TilesToRender.Clear();
        }
        #endregion

        #region BaseTile quad queue
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
                Renderer.RenderObjectsInstancedGeneric(_LowPriorityQueue[i], ref Renderer._instancedRenderArray, deferredShading: false);
            }

            _LowPriorityQueue.Clear();
        }
        #endregion


        #region Structure instanced render data
        public static void GenerateStructureInstancedRenderData(List<Structure> structures, List<Structure> fogStructures)
        {
            for (int i = 0; i < StructureRenderData.Count; i++)
            {
                StructureRenderData[i].CleanUp();
            }

            for(int i = 0; i < FogStructureRenderData.Count; i++)
            {
                FogStructureRenderData[i].CleanUp();
            }

            StructureRenderData = InstancedRenderData.GenerateInstancedRenderData(structures);
            FogStructureRenderData = InstancedRenderData.GenerateInstancedRenderData(fogStructures);
        }

        public static void RenderInstancedStructureData()
        {
            Renderer.RenderInstancedRenderData(StructureRenderData);
        }

        public static void RenderFogInstancedStructureData()
        {
            Renderer.RenderInstancedRenderData(FogStructureRenderData);
        }

        #endregion

        #region Tile map instanced render data
        public static void ClearTileInstancedRenderData()
        {
            _chunkRenderData.Clear();
        }

        public static void RenderInstancedTileData()
        {
            TileMapController.TileOverlaySpritesheet.Use(TextureUnit.Texture1);

            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            GL.Enable(EnableCap.StencilTest);
            //The reference value is the value that is written into the stencil buffer
            //The mask value gets anded with the ref value and the stencil value and the comparison takes place between those values
            //Ex. StencilFunction.Greater means (3 & 0xFF > [stencil_value] & 0xFF)
            GL.StencilFunc(StencilFunction.Always, 3, 0xFF);
            //The stencil mask limits which values can be drawn into the stencil buffer, 0xFF means all values can be written
            //ie the mask is &'d with the value you are attempting to write
            GL.StencilMask(0xFF);

            //GL.DepthMask(false);
            if (_chunkRenderData.Count > 0)
            {
                _chunkRenderData[0].EnableInstancedShaderAttributes();
                _chunkRenderData[0].Shader.Use();
                for (int i = 0; i < _chunkRenderData.Count; i++)
                {
                    _chunkRenderData[i].Draw(MeshChunkDrawType.Visible);
                }
                _chunkRenderData[0].DisableInstancedShaderAttributes();
            }

            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Zero);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            //GL.StencilMask(1);
            RenderFogInstancedStructureData();

            //GL.StencilMask(3);
            GL.StencilFunc(StencilFunction.Always, 3, 0xFF);
            if (_chunkRenderData.Count > 0)
            {
                _chunkRenderData[0].EnableInstancedShaderAttributes();
                _chunkRenderData[0].Shader.Use();
                for (int i = 0; i < _chunkRenderData.Count; i++)
                {
                    _chunkRenderData[i].Draw(MeshChunkDrawType.Fog);
                }
                _chunkRenderData[0].DisableInstancedShaderAttributes();
            }

            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            //GL.StencilMask(1);

            RenderInstancedStructureData();

            GL.Disable(EnableCap.StencilTest);

            ClearTileInstancedRenderData();
        }


        private static List<MeshChunkInstancedRenderData> _chunkRenderData = new List<MeshChunkInstancedRenderData>();
        public static void QueueChunkRenderData(TileChunk chunk)
        {
            if (chunk?.ChunkRenderData?.IsValid == true)
            {
                _chunkRenderData.Add(chunk.ChunkRenderData);
            }
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
            Renderer.RenderObjectsInstancedGeneric(_fogQuad, ref Renderer._instancedRenderArray, deferredShading: false);
        }

        public static void ClearFogQuad()
        {
            _fogQuad.Clear();
        }

        #endregion


        #region Individual Meshes
        private static bool _renderMeshes = false;
        public static void QueueIndividualMeshesForRender(List<IndividualMesh> meshes)
        {
            _IndividualMeshsToRender = meshes;
            _renderMeshes = true;
        }

        public static void RenderIndividualMeshes()
        {
            if (_renderMeshes)
            {
                _renderMeshes = false;
                for(int i = 0; i < _IndividualMeshsToRender.Count; i++)
                {
                    _IndividualMeshsToRender[i].Draw();
                }
            }
        }
        #endregion
    }
}
