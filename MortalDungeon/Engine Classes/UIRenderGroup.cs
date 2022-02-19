using MortalDungeon.Engine_Classes.Rendering;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class UIRenderGroup
    {
        public UIObject Root;

        public List<UIRenderBatch> RenderBatches = new List<UIRenderBatch>();

        public UIRenderGroup(UIObject root)
        {
            Root = root;

            GenerateGroups();
        }

        public void GenerateGroups()
        {
            ClearBatches();

            UIRenderBatch currentBatch = new UIRenderBatch();

            void handleObject(UIObject obj) 
            {
                if(obj.ScissorData.Scissor)
                {
                    HandleScissorObject(obj);
                    return;
                }
                else
                {
                    if(currentBatch.TextureHandles.Count >= 8)
                    {
                        RenderBatches.Add(currentBatch);
                        currentBatch = new UIRenderBatch();
                    }

                    if (obj.Render && obj.TextureLoaded && !obj.Cull)
                    {
                        currentBatch.ObjectsToRender.Add(obj);
                        if (obj._baseObject != null && obj.TextureLoaded)
                        {
                            currentBatch.TextureHandles.Add(obj._baseObject.BaseFrame.Material.Diffuse.Handle);
                        }

                        foreach (var child in obj.Children)
                        {
                            handleObject(child);
                        }
                    }
                }
            }

            handleObject(Root);

            if(RenderBatches.Count > 0 && RenderBatches[^1] != currentBatch && currentBatch.ObjectsToRender.Count != 0)
            {
                RenderBatches.Add(currentBatch);
            }

            FillBatches();
        }

        public void HandleScissorObject(UIObject root)
        {
            UIRenderBatch currentBatch = new UIRenderBatch
            {
                ScissorData = root.ScissorData
            };

            void handleObject(UIObject obj)
            {
                if (currentBatch.TextureHandles.Count >= 8)
                {
                    RenderBatches.Add(currentBatch);
                    currentBatch = new UIRenderBatch
                    {
                        ScissorData = root.ScissorData
                    };
                }

                if (obj.Render && obj.TextureLoaded && !obj.Cull)
                {
                    currentBatch.ObjectsToRender.Add(obj);

                    if (obj._baseObject != null)
                    {
                        currentBatch.TextureHandles.Add(obj._baseObject.BaseFrame.Material.Diffuse.Handle);
                    }

                    foreach (var child in obj.Children)
                    {
                        handleObject(child);
                    }
                }
            }

            handleObject(Root);

            if (RenderBatches.Count > 0 && RenderBatches[^1] != currentBatch && currentBatch.ObjectsToRender.Count != 0)
            {
                RenderBatches.Add(currentBatch);
            }
        }

        public void FillBatches()
        {
            foreach(var batch in RenderBatches)
            {
                var renderData = UIInstancedRenderData.GenerateInstancedRenderData(batch.ObjectsToRender);
                
                if(renderData.Count > 0)
                {
                    batch.RenderData = renderData[0];
                    batch.RenderData.ScissorData = batch.ScissorData;

                    if(renderData.Count > 1)
                    {
                        throw new Exception("This shouldn't be generating more than one set of instanced render data.");
                    }
                }
            }
        }

        public void ClearBatches()
        {
            foreach(var batch in RenderBatches)
            {
                if(batch.RenderData != null)
                {
                    batch.RenderData.CleanUp();
                }
            }

            RenderBatches.Clear();
        }

        public void CleanUp()
        {
            ClearBatches();
        }
    }

    public class UIRenderBatch
    {
        public UIInstancedRenderData RenderData;

        public ScissorData ScissorData = new ScissorData();

        public List<UIObject> ObjectsToRender = new List<UIObject>();
        public HashSet<int> TextureHandles = new HashSet<int>();
    }
}
