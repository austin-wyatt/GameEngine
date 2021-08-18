using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
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


        /// <summary>
        /// Render all queued objects
        /// </summary>
        public static void RenderQueue()
        {
            //DrawToFrameBuffer(MainFBO); //Framebuffer should only be used when we want to 
            RenderQueuedUI();

            //RenderFrameBuffer(MainFBO);

            //MainFBO.UnbindFrameBuffer();
            //MainFBO.ClearColorBuffer(false);

            //DrawToFrameBuffer(MainFBO); 

            RenderQueuedLetters();

            RenderQueuedParticles();
            RenderTileQueue();

            RenderTileQuadQueue();

            RenderQueuedStructures();
            RenderQueuedUnits();
            RenderQueuedObjects();

            RenderLowPriorityQueue();


            //RenderFrameBuffer(MainFBO);
        }


        #region Particle queue
        public static void QueueParticlesForRender(ParticleGenerator generator)
        {
            _ParticleGeneratorsToRender.Add(generator);
        }
        public static void RenderQueuedParticles()
        {
            _ParticleGeneratorsToRender.ForEach(gen =>
            {
                Renderer.RenderParticlesInstanced(gen);
            });

            _ParticleGeneratorsToRender.Clear();
        }
        #endregion

        #region Text queue
        public static void QueueLettersForRender(List<Letter> letters)
        {
            letters.ForEach(letter =>
            {
                _LettersToRender.Add(letter);
            });
        }
        public static void QueueTextForRender(List<Text> text)
        {
            text.ForEach(obj =>
            {
                if (obj.Render)
                    QueueLettersForRender(obj.Letters);
            });
        }
        #endregion

        #region UI queue
        public static void QueueUITextForRender(List<Text> text, bool scissorFlag = false)
        {
            text.ForEach(obj =>
            {
                if (obj.Render)
                    QueueUIForRender(obj.Letters, scissorFlag);
            });
        }
        public static void RenderQueuedLetters()
        {
            Renderer.RenderObjectsInstancedGeneric(_LettersToRender, ref Renderer._instancedRenderArray);
            _LettersToRender.Clear();
        }

        public static void QueueNestedUI<T>(List<T> uiObjects, int depth = 0, ScissorData scissorData = null) where T : UIObject
        {
            if (uiObjects.Count > 0)
            {
                for (int i = 0; i < uiObjects.Count; i++)
                {
                    if (uiObjects[i].Render)
                    {
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

                        if (uiObjects[i].Children.Count > 0)
                        {
                            QueueNestedUI(uiObjects[i].Children, depth + 1, uiObjects[i].ScissorData.Scissor ? uiObjects[i].ScissorData : scissorData);
                        }

                        QueueUIForRender(uiObjects[i], scissorFlag || uiObjects[i].ScissorData.Scissor);
                    }
                }


                //RenderableObject display = uiObjects[0].GetDisplay();

                //RenderObjectsInstancedGeneric(uiObjects, display);
                //QueueUIForRender(uiObjects);
            }
        }
        public static void QueueUIForRender<T>(List<T> objList, bool scissorFlag = false) where T : GameObject
        {
            objList.ForEach(obj =>
            {
                obj.ScissorData._scissorFlag = scissorFlag;

                _UIToRender.Add(obj);
            });
        }
        public static void QueueUIForRender<T>(T obj, bool scissorFlag = false) where T : GameObject
        {
            obj.ScissorData._scissorFlag = scissorFlag;

            _UIToRender.Add(obj);
        }
        public static void RenderQueuedUI()
        {
            Renderer.RenderObjectsInstancedGeneric(_UIToRender, ref Renderer._instancedRenderArray);
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
            _ObjectsToRender.ForEach(obj =>
            {
                Renderer.RenderObjectsInstancedGeneric(obj, ref Renderer._instancedRenderArray);
            });
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
            _UnitsToRender.ForEach(obj =>
            {
                Renderer.RenderObjectsInstancedGeneric(obj, ref Renderer._instancedRenderArray);
            });
            _UnitsToRender.Clear();
        }
        #endregion

        #region Structure queue
        public static void QueueStructuresForRender(List<Structure> objList)
        {
            if (objList.Count == 0)
                return;

            _StructuresToRender.Add(objList);
        }
        public static void RenderQueuedStructures()
        {
            int i = 0;
            bool instantiate = false;
            _StructuresToRender.ForEach(obj =>
            {
                if (i == 0 || i == _StructuresToRender.Count - 1)
                {
                    instantiate = true;
                }
                else 
                {
                    instantiate = false;
                }

                Renderer.RenderObjectsInstancedGeneric(obj, ref Renderer._instancedRenderArray, null, instantiate);

                i++;
            });
            _StructuresToRender.Clear();
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

            _TilesToRender.ForEach(list =>
            {
                Renderer.RenderObjectsInstancedGeneric(list, ref Renderer._instancedRenderArray);
            });

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
            _LowPriorityQueue.ForEach(list =>
            {
                Renderer.RenderObjectsInstancedGeneric(list, ref Renderer._instancedRenderArray);
            });

            _LowPriorityQueue.Clear();
        }
        #endregion
    }
}
