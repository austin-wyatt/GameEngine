using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class AbilityTreeUI
    {
        public UIObject Window;
        public CombatScene Scene;

        public bool Displayed = false;
        public AbilityTree SelectedTree = null;

        private UIBlock _selectedTreeBlock;
        private ScrollableArea _treeDisplayArea;

        private UIBlock _unitDisplayBlock;
        private UIBlock _abilityDisplayBlock;

        public AbilityTreeUI(CombatScene scene)
        {
            Scene = scene;
        }

        public void CreateWindow()
        {
            RemoveWindow();

            Window = UIHelpers.CreateWindow(new UIScale(2 * WindowConstants.AspectRatio, 2f), "AbilityTree", null, Scene, customExitAction: () =>
            {
                RemoveWindow();
            });

            Window.Draggable = false;

            Window.SetPosition(WindowConstants.CenterScreen);

            Scene.AddUI(Window, 1000);



            Displayed = true;

            PopulateData();

            if (SelectedTree != null)
            {
                PopulateTreeInfo();
            }

            CreateUnitDisplay();
        }

        public void RemoveWindow()
        {
            if (Window != null)
            {
                Scene.UIManager.RemoveUIObject(Window);
            }

            Displayed = false;
        }

        public void PopulateData()
        {
            Text activeQuestsLabel = new Text("Ability Trees", Text.DEFAULT_FONT, 48, Brushes.Black);
            activeQuestsLabel.SetTextScale(0.1f);

            activeQuestsLabel.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(activeQuestsLabel);

            ScrollableArea activeQuestsScrollArea = new ScrollableArea(default, new UIScale(0.5f, 1.5f), default, new UIScale(0.5f, 3f), enableScrollbar: false);
            activeQuestsScrollArea.SetVisibleAreaPosition(activeQuestsLabel.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            Window.AddChild(activeQuestsScrollArea);

            UIList treesList = new UIList(default, new UIScale(0.5f, 0.1f), 0.05f);
            treesList.SetPositionFromAnchor(activeQuestsScrollArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            activeQuestsScrollArea.BaseComponent.AddChild(treesList);

            foreach (var tree in AbilityTrees.Trees)
            {
                treesList.AddItem(tree.TreeType.ToString(), (_) =>
                {
                    SelectedTree = tree;
                    PopulateTreeInfo();
                });
            }

            _selectedTreeBlock = new UIBlock(default, new UIScale(1.4f, 1.55f));
            _selectedTreeBlock.SetColor(_Colors.UIDisabledGray);
            _selectedTreeBlock.SetPositionFromAnchor(activeQuestsScrollArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);
            _selectedTreeBlock.SetAllInline(0);

            ScrollableArea treeDisplayArea = new ScrollableArea(default, new UIScale(1.4f, 1.55f), default, new UIScale(3f, 3f), enableScrollbar: false, setScrollable: false);
            treeDisplayArea.SetVisibleAreaPosition(_selectedTreeBlock.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            _selectedTreeBlock.AddChild(treeDisplayArea);

            treeDisplayArea.BaseComponent.SetPositionFromAnchor(treeDisplayArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
            treeDisplayArea.BaseComponent.SetColor(_Colors.UIDisabledGray);
            treeDisplayArea.BaseComponent.SetAllInline(2);

            _treeDisplayArea = treeDisplayArea;

            treeDisplayArea.VisibleArea.Scrollable = true;
            treeDisplayArea.VisibleArea.Draggable = true;
            treeDisplayArea.VisibleArea.Clickable = true;

            treeDisplayArea.VisibleArea.Scroll += (s, e) =>
            {
                Vector3 localCoord = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(MortalDungeon.Window._cursorCoords));

                UIDimensions prevDimension = _treeDisplayArea.BaseComponent.GetDimensions();

                Vector3 mousePosOnObj = localCoord - _treeDisplayArea.BaseComponent.Position;

                Vector3 ratioFromEdge = new Vector3(mousePosOnObj.X / prevDimension.X, mousePosOnObj.Y / prevDimension.Y, 0);

                mousePosOnObj.Z = 0;

                Vector3 pos = _treeDisplayArea.BaseComponent.Position;

                if (e.ScrollDelta.Y > 0)
                {
                    _treeDisplayArea.BaseComponent.ScaleXY(1.1f, 1.1f);
                }
                else
                {
                    _treeDisplayArea.BaseComponent.ScaleXY(0.9f, 0.9f);
                }

                Vector3 currTopLeft = _treeDisplayArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

                UIDimensions currDimensions = _treeDisplayArea.BaseComponent.GetDimensions();

                Vector3 idealMousePos = new Vector3(currDimensions.X * ratioFromEdge.X, currDimensions.Y * ratioFromEdge.Y, 0);

                Vector3 diff = new Vector3(mousePosOnObj.X - idealMousePos.X, mousePosOnObj.Y - idealMousePos.Y, 0);

                _treeDisplayArea.BaseComponent.SetPosition(_treeDisplayArea.BaseComponent.Position + diff);

                PositionTreeObjects();
            };

            Vector3 dragPos = new Vector3(treeDisplayArea.VisibleArea.Position);
            treeDisplayArea.VisibleArea.PreviewDrag += (s, coord, pos, delta) =>
            {
                treeDisplayArea.BaseComponent.SetPosition(treeDisplayArea.BaseComponent.Position + delta);

                return false;
            };


            Window.AddChild(_selectedTreeBlock);


            _unitDisplayBlock = new UIBlock(default, new UIScale(0.1f, 0.1f));
            _unitDisplayBlock.SetColor(_Colors.Transparent);
            _unitDisplayBlock.SetAllInline(0);
            Window.AddChild(_unitDisplayBlock);

            _abilityDisplayBlock = new UIBlock(default, new UIScale(0.1f, 0.1f));
            _abilityDisplayBlock.SetColor(_Colors.Transparent);
            _abilityDisplayBlock.SetAllInline(0);
            Window.AddChild(_abilityDisplayBlock);
        }

        private class NodeDisplayData
        {
            public UIBlock Obj;
            public Vector2 RelativePosition;
            public int NodeId;
            public AbilityTreeNode TreeNode;

            public NodeDisplayData(UIBlock block, Vector2 pos, int id, AbilityTreeNode node)
            {
                Obj = block;
                RelativePosition = pos;
                NodeId = id;
                TreeNode = node;
            }
        }

        private List<NodeDisplayData> _treeObjects = new List<NodeDisplayData>();
        private List<(int node1, int node2)> _nodeConnections = new List<(int node1, int node2)>();
        public void PopulateTreeInfo()
        {
            _treeDisplayArea.BaseComponent.RemoveChildren();
            _treeObjects.Clear();
            _nodeConnections.Clear();

            HashSet<AbilityTreeNode> visitedNodes = new HashSet<AbilityTreeNode>();

            void queueChildren(AbilityTreeNode parentObject, int depth, int childIndex)
            {
                UIBlock treeNode = new UIBlock(default, new UIScale(0.1f, 0.1f));

                _treeDisplayArea.BaseComponent.AddChild(treeNode);

                var displayData = new NodeDisplayData(treeNode, parentObject.RelativePosition, parentObject.ID, parentObject);

                _treeObjects.Add(displayData);

                treeNode.Draggable = true;
                treeNode.Clickable = true;

                visitedNodes.Add(parentObject);

                for (int i = 0; i < parentObject.ConnectedNodes.Count; i++)
                {
                    if (!visitedNodes.Contains(parentObject.ConnectedNodes[i]))
                    {
                        queueChildren(parentObject.ConnectedNodes[i], depth + 1, i + 1);
                        _nodeConnections.Add((parentObject.ID, parentObject.ConnectedNodes[i].ID));
                    }
                }


                treeNode.RightClick += (s, e) =>
                {
                    Console.WriteLine(displayData.RelativePosition);

                    bool available = true;

                    foreach(var unit in PlayerParty.UnitsInParty)
                    {
                        if(unit.AbilityLoadout.Items.Exists(l => l.NodeID == parentObject.ID && l.AbilityTreeType == SelectedTree.TreeType))
                        {
                            available = false;
                            break;
                        }
                    }

                    bool canSelect = parentObject.Unlocked && available;

                    if (canSelect && _selectedAbilityBlock >= 0 && _selectedUnit != null)
                    {
                        AbilityLoadoutItem newItem = new AbilityLoadoutItem(SelectedTree.TreeType, displayData.TreeNode.ID);

                        if (_selectedAbilityBlock >= _currentAbilities.Count)
                        {
                            _selectedUnit.AddAbility(newItem);
                        }
                        else
                        {
                            var currentAbility = _currentAbilities[_selectedAbilityBlock];
                            _selectedUnit.ReplaceAbility(currentAbility, newItem);
                        }

                        int prevAbilityBlock = _selectedAbilityBlock;

                        CreateAbilityDisplays();

                        _abilityDisplays[prevAbilityBlock].OnClick();
                        CalculateTreeNodeColors();
                    }
                };

                treeNode.Drag += (s, e, m, d) =>
                {
                    CalculateRelativePosition(ref displayData);
                    DrawConnectingLines();
                };

                

                UIHelpers.AddTimedHoverTooltip(treeNode, parentObject.Name, Scene);
            }

            queueChildren(SelectedTree.EntryPoint, 0, 0);

            _treeDisplayArea.BaseComponent.AddChild(_lineBlock, -5);

            CalculateTreeNodeColors();

            PositionTreeObjects();
        }

        public void CalculateTreeNodeColors()
        {
            foreach(var node in _treeObjects)
            {
                if (node.TreeNode.Unlocked)
                {
                    node.Obj.SetColor(_Colors.Green);
                }
                else
                {
                    node.Obj.SetColor(_Colors.Red);
                }


                bool available = true;
                foreach (var unit in PlayerParty.UnitsInParty)
                {
                    if (unit.AbilityLoadout.Items.Exists(l => l.NodeID == node.TreeNode.ID && l.AbilityTreeType == SelectedTree.TreeType))
                    {
                        available = false;
                        break;
                    }
                }

                if (!available)
                {
                    node.Obj.SetColor(_Colors.Purple);
                }
            }
        }

        public void PositionTreeObjects()
        {
            //get the dimensions of the display area
            //position each tree object relative to the dimensions of the display area
            Vector3 botRight = _treeDisplayArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomRight);
            Vector3 topLeft = _treeDisplayArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

            Vector3 dimensions = botRight - topLeft;

            foreach (var node in _treeObjects)
            {
                Vector3 pos = topLeft + new Vector3(node.RelativePosition) * dimensions;

                node.Obj.SetPosition(pos);
            }

            ScaleTreeObjects();
            DrawConnectingLines();
        }

        public void ScaleTreeObjects()
        {
            foreach (var node in _treeObjects)
            {
                node.Obj.SetScale(_treeDisplayArea.BaseComponent.Scale.X * node.Obj.Size.X / WindowConstants.AspectRatio, 
                    _treeDisplayArea.BaseComponent.Scale.Y * node.Obj.Size.Y, 1);
            }
        }

        private void CalculateRelativePosition(ref NodeDisplayData displayData)
        {
            Vector3 botRight = _treeDisplayArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomRight);
            Vector3 topLeft = _treeDisplayArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft);

            Vector3 dimensions = botRight - topLeft;

            displayData.RelativePosition = (displayData.Obj.Position - topLeft).Xy;

            displayData.RelativePosition.X /= dimensions.X;
            displayData.RelativePosition.Y /= dimensions.Y;
        }

        private UIBlock _lineBlock = new UIBlock();
        public void DrawConnectingLines()
        {
            _lineBlock.SetColor(_Colors.Transparent);
            _lineBlock.SetAllInline(0);

            _lineBlock.RemoveChildren();
            

            foreach(var connection in _nodeConnections)
            {
                var nodeA = _treeObjects.Find(o => o.NodeId == connection.node1);
                var nodeB = _treeObjects.Find(o => o.NodeId == connection.node2);

                var hypotenuse = Vector2.Distance(nodeA.Obj.Position.Xy, nodeB.Obj.Position.Xy);

                var sideA = Vector2.Distance(new Vector2(nodeA.Obj.Position.X, nodeB.Obj.Position.Y), nodeA.Obj.Position.Xy);

                //var angle = Vector3.CalculateAngle(nodeA.obj.Position, nodeB.obj.Position);
                float angle = (float)MathHelper.Asin(sideA / hypotenuse);

                UIBlock line = new UIBlock(scaleAspectRatio: false);

                int sign = ((nodeA.Obj.Position.X > nodeB.Obj.Position.X) ? 1 : -1) * ((nodeA.Obj.Position.Y > nodeB.Obj.Position.Y) ? -1 : 1);

                line.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(angle) * sign);

                line.SetPosition((nodeA.Obj.Position + nodeB.Obj.Position) / 2);

                float yVal = 0.02f * (float)Math.Abs((Math.PI - angle) / Math.PI) * _treeDisplayArea.BaseComponent.Scale.Y;
                float xVal = hypotenuse / WindowConstants.ScreenUnits.X * 2;

                line.SetSize(new UIScale(xVal, yVal));

                line.SetAllInline(0);
                line.SetColor(_Colors.Black);

                _lineBlock.AddChild(line);
            }
        }


        private List<UIBlock> _unitDisplays = new List<UIBlock>();
        private Unit _selectedUnit = null;
        public void CreateUnitDisplay()
        {
            _unitDisplayBlock.RemoveChildren();
            _unitDisplays.Clear();

            foreach (var unit in PlayerParty.UnitsInParty)
            {
                UIBlock block = new UIBlock(default, new UIScale(0.25f, 0.25f));
                Scene.Tick += block.Tick;

                block.OnCleanUp += (e) =>
                {
                    Scene.Tick -= block.Tick;
                };

                BaseObject obj = unit.CreateBaseObject();
                obj.EnableLighting = false;
                obj._currentAnimation.Pause();

                obj.BaseFrame.SetScale(block.Size.X / WindowConstants.AspectRatio, block.Size.Y, 1);

                block.BaseObjects.Clear();
                block.BaseObjects.Add(obj);
                block._baseObject = obj;

                block.Hoverable = true;
                block.HoverColor = _Colors.IconHover;

                block.SelectedColor = _Colors.IconSelected;

                if(_unitDisplays.Count == 0)
                {
                    block.SetPositionFromAnchor(_treeDisplayArea.VisibleArea.GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0),
                        UIAnchorPosition.TopLeft);
                }
                else
                {
                    block.SetPositionFromAnchor(_unitDisplays[^1].GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(5, 0, 0),
                        UIAnchorPosition.TopLeft);
                }

                _unitDisplayBlock.AddChild(block);
                _unitDisplays.Add(block);

                block.Clickable = true;

                block.Click += (s, e) =>
                {
                    foreach (var disp in _unitDisplays)
                    {
                        disp.OnSelect(false);
                        disp.BaseObject._currentAnimation.Pause();
                    }

                    block.OnSelect(true);
                    block.BaseObject._currentAnimation.Reset();

                    _selectedUnit = unit;

                    CreateAbilityDisplays();
                };
            }
        }

        private List<UIBlock> _abilityDisplays = new List<UIBlock>();
        private List<AbilityLoadoutItem> _currentAbilities = new List<AbilityLoadoutItem>();
        private int _selectedAbilityBlock = -1;
        public void CreateAbilityDisplays()
        {
            _abilityDisplayBlock.RemoveChildren();
            _currentAbilities.Clear();
            _abilityDisplays.Clear();

            _selectedAbilityBlock = -1;

            List<AbilityLoadoutItem> currentAbilities = new List<AbilityLoadoutItem>();

            int maxAbilities = 4;

            if(_selectedUnit != null)
            {
                foreach(var loadoutItem in _selectedUnit.AbilityLoadout.Items)
                {
                    currentAbilities.Add(loadoutItem);
                }
            }

            _currentAbilities = currentAbilities;

            for (int i = 0; i < maxAbilities; i++)
            {
                UIBlock block = new UIBlock(default, new UIScale(0.25f, 0.25f));
                block.SetColor(_Colors.UIHoveredGray);

                if(i == 0)
                {
                    block.SetPositionFromAnchor(_unitDisplays[0].GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 25, 0), UIAnchorPosition.TopLeft);
                }
                else
                {
                    block.SetPositionFromAnchor(_abilityDisplays[^1].GetAnchorPosition(UIAnchorPosition.TopRight) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);
                }

                block.Hoverable = true;
                block.HoverColor = _Colors.UISelectedGray;

                block.Clickable = true;
                block.SelectedColor = _Colors.IconSelected;

                int index = i;
                block.Click += (s, e) =>
                {
                    _selectedAbilityBlock = index;

                    foreach(var item in _abilityDisplays)
                    {
                        item.OnSelect(false);
                    }

                    block.OnSelect(true);
                };

                if(i < currentAbilities.Count)
                {
                    var ability = currentAbilities[i].GetAbilityFromLoadoutItem(_selectedUnit);

                    var icon = ability.GenerateIcon(new UIScale(0.22f, 0.22f));

                    icon.Hoverable = false;
                    icon.HoverColor = _Colors.IconHover;

                    block.Hover += (s) =>
                    {
                        icon._colorOverride = ColorOverride.Hover;
                        icon.EvaluateColor();
                    };

                    block.HoverEnd += (s) =>
                    {
                        icon._colorOverride = ColorOverride.None;
                        icon.EvaluateColor();
                    };

                    block.RightClick += (s, e) =>
                    {
                        _selectedUnit.RemoveAbility(currentAbilities[index]);
                        CalculateTreeNodeColors();
                        CreateAbilityDisplays();
                    };

                    block.AddChild(icon);

                    icon.SetPosition(block.Position);

                    UIHelpers.AddTimedHoverTooltip(block, ability.Name, Scene);
                }
                else
                {
                    UIHelpers.AddTimedHoverTooltip(block, "Empty", Scene);
                }

                _abilityDisplayBlock.AddChild(block);
                _abilityDisplays.Add(block);
            }
        }
    }
}
