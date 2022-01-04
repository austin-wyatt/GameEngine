using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MortalDungeon.Game.Serializers;

namespace MortalDungeon.Game
{
    public class DialogueWindow
    {
        public UIObject Window;
        public CombatScene Scene;

        public Action OnDialogueEnded = null;

        public DialogueWindow(CombatScene scene)
        {
            Scene = scene;

            Window = UIHelpers.CreateWindow(new UIScale(1.25f, 1.5f), "DialogueWindow", null, Scene, createExitButton: false);
            Window.SetRender(false);

            Window.Draggable = false;
        }


        private DialogueNode _currentNode;
        private DialogueNode _prevNode;
        private TextComponent _prevDialogueText;
        private UIObject _buttonParent;
        private UIObject _dialogueParent;
        private UIObject _speakerParent;
        private List<Func<BaseObject>> _participantObjects;
        private List<BaseObject> _createdObjects;

        private ScrollableArea _scrollableArea;

        public void StartDialogue(Dialogue dialogue, List<Unit> participants)
        {
            Scene.ExclusiveUIObjects.Add(Window);

            Window.RemoveChildren();
            Window.SetPosition(WindowConstants.CenterScreen);

            Window.SetRender(true);

            _participantObjects = new List<Func<BaseObject>>();

            _createdObjects = new List<BaseObject>();

            foreach (Unit participant in participants)
            {
                BaseObject createObj()
                {
                    BaseObject obj = participant.CreateBaseObject();

                    obj._currentAnimation.Reset();
                    obj._currentAnimation.Pause();

                    obj.BaseFrame.ScaleX(0.18f / WindowConstants.AspectRatio);
                    obj.BaseFrame.ScaleY(0.18f);

                    obj.SetPosition(new Vector3(0, 0, 0));

                    return obj;
                }
                

                _participantObjects.Add(createObj);
            }

            _currentNode = dialogue.EntryPoint;
            _prevNode = null;
            _prevDialogueText = null;

            _scrollableArea = new ScrollableArea(default, new UIScale(1.22f, 1.47f), default, new UIScale(1.25f, 1.47f), 0.1f);
            _scrollableArea.SetVisibleAreaPosition(Window.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
            Window.AddChild(_scrollableArea);

            _buttonParent = new UIBlock();
            _buttonParent.MultiTextureData.MixTexture = false;
            _buttonParent.SetColor(Colors.Transparent);
            _buttonParent.SetAllInline(0);

            //Window.AddChild(_buttonParent);
            _scrollableArea.BaseComponent.AddChild(_buttonParent);

            _dialogueParent = new UIBlock();
            _dialogueParent.MultiTextureData.MixTexture = false;
            _dialogueParent.SetColor(Colors.Transparent);
            _dialogueParent.SetAllInline(0);

            //Window.AddChild(_dialogueParent);
            _scrollableArea.BaseComponent.AddChild(_dialogueParent);

            _speakerParent = new UIBlock();
            _speakerParent.MultiTextureData.MixTexture = false;
            _speakerParent.SetColor(Colors.Transparent);
            _speakerParent.SetAllInline(0);

            //Window.AddChild(_speakerParent);
            _scrollableArea.BaseComponent.AddChild(_speakerParent);

            Task.Run(() => AdvanceDialogueText(dialogue));
        }

        public void EndDialogue()
        {
            Scene.ExclusiveUIObjects.Remove(Window);

            Window.RemoveChildren();
            Window.SetRender(false);

            OnDialogueEnded?.Invoke();
        } 

        public void AdvanceDialogueText(Dialogue dialogue)
        {
            UIObject uiObj;

            uiObj = new UIBlock();

            //foreach(var obj in _createdObjects)
            //{
            //    obj._currentAnimation.Stop();
            //}

            if (_currentNode.Speaker != -1)
            {
                BaseObject obj = _participantObjects[_currentNode.Speaker].Invoke();
                _createdObjects.Add(obj);

                uiObj.AddBaseObject(obj);

                obj._currentAnimation.Play();
            }

            uiObj.SetColor(Colors.Transparent);
            uiObj.MultiTextureData.MixTexture = false;
            uiObj.SetScale(0.18f / WindowConstants.AspectRatio, 0.18f, 0.18f);
            uiObj.SetAllInline(0);

            TextComponent dialogueText = new TextComponent();
            dialogueText.SetTextScale(0.04f);
            dialogueText._textField.SetColor(Colors.Tan);

            string dialogueMessage = EventLog.WrapString(_currentNode.GetMessage(), textWrapLength);

            dialogueText.SetText(dialogueMessage);


            if (_prevDialogueText == null)
            {
                Vector3 windowPos = Window.GetAnchorPosition(UIAnchorPosition.TopLeft);
                Vector3 pos = new Vector3(windowPos.X + 75, windowPos.Y + 30, Window.Position.Z - 0.0000001f);
                dialogueText.SetPositionFromAnchor(pos, UIAnchorPosition.TopLeft);
            }
            else
            {
                dialogueText.SetPositionFromAnchor(_prevDialogueText.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            }

            //in the future unit types can have "voices" and this would determine what sound plays when they speak
            new Sound(Sounds.Select) { Gain = 0.25f, Pitch = GlobalRandom.NextFloat(0.75f, 0.9f) }.Play();

            _dialogueParent.AddChild(dialogueText);

            #region create speaker
            if (_prevDialogueText == null)
            {
                Vector3 pos = Window.GetAnchorPosition(UIAnchorPosition.TopLeft);

                uiObj.SetPositionFromAnchor(Window.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 0, 0), UIAnchorPosition.TopLeft);
                _speakerParent.AddChild(uiObj);
            }
            else if (_prevNode != null && _prevNode.Speaker != _currentNode.Speaker && _currentNode.Speaker == 1)
            {
                Vector3 pos = _prevDialogueText.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 0, 0);

                pos = new Vector3(Window.GetAnchorPosition(UIAnchorPosition.TopRight).X - 10, pos.Y, pos.Z);

                uiObj.SetPositionFromAnchor(pos, UIAnchorPosition.TopRight);
                _speakerParent.AddChild(uiObj);
            }
            else if (_prevNode != null && _prevNode.Speaker != _currentNode.Speaker)
            {
                Vector3 windowPos = Window.GetAnchorPosition(UIAnchorPosition.TopLeft);
                Vector3 pos = new Vector3(windowPos.X + 10, 0, windowPos.Z - 0.00000001f);

                pos.Y = _prevDialogueText.GetAnchorPosition(UIAnchorPosition.BottomLeft).Y;

                uiObj.SetPositionFromAnchor(pos, UIAnchorPosition.TopLeft);
                _speakerParent.AddChild(uiObj);
            }
            #endregion

            foreach (var text in dialogueText.TextObjects)
            {
                text.SetScissorData(_scrollableArea.BaseComponent.ScissorData);
            }

            CheckTextPlacement();

            _prevDialogueText = dialogueText;
            _prevNode = _currentNode;
            Task.Run(() => CreateResponses(dialogue));
        }


        public void CreateResponses(Dialogue dialogue)
        {
            if(_currentNode.Responses.Count == 0)
            {
                EndDialogue();
            }

            if (_currentNode.Responses[0].ResponseType == ResponseType.None)
            {
                if(_currentNode.Responses[0].Outcome > 0)
                {
                    StateIDValuePair updatedState = new StateIDValuePair()
                    {
                        Type = (int)LedgerUpdateType.Dialogue,
                        StateID = dialogue.ID,
                        ObjectHash = 0,
                        Data = _currentNode.Responses[0].Outcome,
                    };

                    DialogueLedger.SetStateValue(updatedState);
                }

                if(_currentNode.Responses[0].StateValues.Count > 0)
                {
                    Ledgers.ApplyStateValues(_currentNode.Responses[0].StateValues);
                }

                dialogue.DialogueOutcome = _currentNode.Responses[0].Outcome;


                if (_currentNode.Responses[0].Next == null)
                {
                    EndDialogue();
                    return;
                }

                _currentNode = _currentNode.Responses[0].Next;

                Thread.Sleep(1000);

                AdvanceDialogueText(dialogue);
                return;
            }
            else
            {
                //create buttons
                foreach (var res in _currentNode.Responses)
                {
                    string responseString = EventLog.WrapString(res.ToString(), textWrapLength);

                    Button button = new Button(default, new UIScale(0.2f, 0.1f), responseString, 0.043f, Colors.UILightGray, Colors.UITextBlack);
                    button.BaseComponent.MultiTextureData.MixTexture = false;

                    button.BaseComponent.SetSize(button.TextBox.BaseComponent.Size + new UIScale(0.05f, 0.05f));
                    button.BaseComponent.SetPosition(button.TextBox.BaseComponent.GetAnchorPosition(UIAnchorPosition.Center));

                    button.Click += (s, e) =>
                    {
                        _buttonParent.RemoveChildren();

                        if (res.Outcome > 0)
                        {
                            StateIDValuePair updatedState = new StateIDValuePair()
                            {
                                Type = (int)LedgerUpdateType.Dialogue,
                                StateID = dialogue.ID,
                                ObjectHash = 0,
                                Data = res.Outcome,
                            };

                            DialogueLedger.SetStateValue(updatedState);
                        }

                        if (res.StateValues.Count > 0)
                        {
                            Ledgers.ApplyStateValues(res.StateValues);
                        }

                        dialogue.DialogueOutcome = res.Outcome;


                        if (res.Next == null)
                        {
                            EndDialogue();
                            return;
                        }
                        else
                        {
                            _currentNode = res.Next;
                            AddResponseText(dialogue, res.ToString());
                            return;
                        }
                    };

                    Vector3 windowPosition = Window.GetAnchorPosition(UIAnchorPosition.TopCenter);

                    Vector3 pos = new Vector3();

                    if (_buttonParent.Children.Count == 0)
                    {
                        pos = _prevDialogueText.GetAnchorPosition(UIAnchorPosition.BottomLeft);
                        pos.X = windowPosition.X;
                        pos.Y += 10;

                        button.SetPositionFromAnchor(pos, UIAnchorPosition.TopCenter);
                    }
                    else
                    {
                        button.SetPositionFromAnchor(
                            _buttonParent.Children[^1].GetAnchorPosition(UIAnchorPosition.BottomCenter) + new Vector3(0, 10, 0), UIAnchorPosition.TopCenter);
                    }

                    foreach (var text in button.TextBox.TextObjects)
                    {
                        text.SetScissorData(_scrollableArea.BaseComponent.ScissorData);
                    }

                    _buttonParent.AddChild(button);
                }

                CheckTextPlacement();
            }
        }

        private const int textWrapLength = 25;
        public void AddResponseText(Dialogue dialogue, string response)
        {
            TextComponent dialogueText = new TextComponent();
            dialogueText.SetTextScale(0.04f);
            dialogueText._textField.SetColor(Colors.Green);

            string dialogueMessage = EventLog.WrapString(response, textWrapLength);

            dialogueText.SetText(dialogueMessage);


            if (_prevDialogueText == null)
            {
                Vector3 windowPos = Window.GetAnchorPosition(UIAnchorPosition.TopLeft);
                Vector3 pos = new Vector3(windowPos.X + 75, windowPos.Y + 30, Window.Position.Z - 0.0000001f);
                dialogueText.SetPositionFromAnchor(pos, UIAnchorPosition.TopLeft);
            }
            else
            {
                dialogueText.SetPositionFromAnchor(_prevDialogueText.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 10, 0), UIAnchorPosition.TopLeft);
            }

            _prevDialogueText = dialogueText;
            _dialogueParent.AddChild(dialogueText);

            foreach(var text in dialogueText.TextObjects)
            {
                text.SetScissorData(_scrollableArea.BaseComponent.ScissorData);
            }

            CheckTextPlacement();

            AdvanceDialogueText(dialogue);
        }


        private int _scrollableAreaExpansionCount = 1;
        public void CheckTextPlacement()
        {
            //float YDiff = _dialogueParent.Children[^1].GetAnchorPosition(UIAnchorPosition.BottomCenter).Y - Window.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y + 40;
            float YDiff = _dialogueParent.Children[^1].GetAnchorPosition(UIAnchorPosition.BottomCenter).Y - 
                _scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y + 40;

            if(YDiff <= 0 && _buttonParent.Children.Count > 0)
            {
                YDiff = _buttonParent.Children[^1].GetAnchorPosition(UIAnchorPosition.BottomCenter).Y -
                _scrollableArea.BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomCenter).Y + 40;
            }

            if (YDiff > 0)
            {
                _scrollableAreaExpansionCount++;
                //_scrollableArea.SetBaseAreaSize(_scrollableArea._baseAreaSize + new UIScale(0, 0.2f));
                _scrollableArea.SetBaseAreaSize(_scrollableArea._baseAreaSize + new UIScale(0, 1f));
                _scrollableArea.Scrollbar.ScrollByPercentage(1);
                //_scrollableArea.Scrollbar.ScrollByPercentage((float)1 / _scrollableAreaExpansionCount);
            }
        }
    }
}
