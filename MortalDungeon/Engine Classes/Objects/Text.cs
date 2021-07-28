using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes
{
    public class Letter : GameObject
    {
        public Character Character;
        public new float Scale = 0.1f;
        private float _scaleX = 1;
        private float _baseLetterOffset = 350f; 
        private float _baseYOffset = 0f;
        public float LetterOffset = 350f; //how wide the character is
        public float YOffset = 0f;

        private float _baseXCorrection = 0f;
        private float _baseYCorrection = 0f;
        public float XCorrection = 0f; //shift by this much in the X direction
        public float YCorrection = 0f;

        public BaseObject LetterObject;

        public new ObjectType ObjectType = ObjectType.Text;

        private bool CameraPerspective = false;
        private RenderableObject _display;
        private bool usingMonospace = true;

        public Letter(Character character, Vector3 position, bool cameraPerspective, int ID = 0, float scale = 0.1f)
        {
            Character = character;

            RenderableObject letterDisplay = new RenderableObject(new SpritesheetObject((int)Character, Spritesheets.CharacterSheet).CreateObjectDefinition(ObjectIDs.CHARACTER), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { letterDisplay },
                Frequency = 0,
                Repeats = -1
            };

            BaseObject letter = new BaseObject(new List<Animation>() { Idle }, ID, "letter", position, EnvironmentObjects.BASE_TILE.Bounds);
            letter.BaseFrame.CameraPerspective = cameraPerspective;
            CameraPerspective = cameraPerspective;
            letter.Clickable = false;
            LetterObject = letter;
            _display = letter.BaseFrame;

            BaseObjects.Add(letter);

            letter.RenderData = new RenderData() { AlphaThreshold = Rendering.RenderingConstants.TextAlphaThreshold };

            SetScale(scale);

            SetKerning();
            SetPosition(position);
        }

        public void ChangeCharacter(Character character) 
        {
            _display.SpritesheetPosition = (int)character;
            Character = character;
        }

        public override void SetPosition(Vector3 position)
        {
            BaseObjects.ForEach(obj =>
            {
                obj.SetPosition(position + PositionalOffset + new Vector3(XCorrection, YCorrection, 0));
            });

            ParticleGenerators.ForEach(particleGen =>
            {
                particleGen.SetPosition(position + PositionalOffset);
            });

            Position = position;
        }

        public void SetScale(float scale) 
        {
            LetterObject.BaseFrame.SetScaleAll(scale);

            Scale = scale;

            if (!CameraPerspective)
            {
                LetterObject.BaseFrame.ScaleX((float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X); //it'll display fine in 3D but 2D will be stretched
                _scaleX = (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X;
            }

            SetKerning();
        }

        public override void SetColor(Vector4 color) 
        {
            _display.Color = color;
        }

        private void SetKerning() 
        {
            if (!usingMonospace)
            {
                switch (Character)
                {
                    case Character.i:
                        _baseLetterOffset = 180f;
                        _baseXCorrection = -60f;
                        break;
                    case Character.t:
                        _baseLetterOffset = 215f;
                        _baseXCorrection = -20f;
                        break;
                    case Character.h:
                        _baseLetterOffset = 275f;
                        _baseXCorrection = 0f;
                        break;
                    case Character.r:
                        _baseLetterOffset = 260f;
                        _baseXCorrection = 0f;
                        break;
                    case Character.l:
                        _baseLetterOffset = 200;
                        _baseXCorrection = -75f;
                        break;
                    case Character.u:
                        break;
                    case Character.NewLine:
                        _baseLetterOffset = 0f;
                        break;
                    case Character.Space:
                        _baseLetterOffset = 150f;
                        break;
                    case Character.e:
                        _baseLetterOffset = 300f;
                        _baseXCorrection = -20f;
                        break;
                    case Character.m:
                        _baseLetterOffset = 360f;
                        _baseXCorrection = 100f;
                        break;
                    case Character.o:
                        _baseLetterOffset = 275f;
                        _baseXCorrection = 0f;
                        break;
                    case Character.f:
                        _baseLetterOffset = 225f;
                        break;
                    case Character.c:
                        _baseLetterOffset = 225f;
                        _baseXCorrection = 30f;
                        break;
                    case Character.w:
                        _baseLetterOffset = 340f;
                        _baseXCorrection = 40f;
                        break;
                    case Character.y:
                        _baseLetterOffset = 280f;
                        _baseXCorrection = -20f;
                        _baseYCorrection = 30f;
                        break;
                    case Character.p:
                        _baseYCorrection = 25f;
                        break;
                    case Character.q:
                        _baseYCorrection = 10f;
                        break;
                    case Character.a:
                        _baseLetterOffset = 250f;
                        _baseXCorrection = 25f;
                        break;
                    case Character.k:
                        _baseLetterOffset = 250f;
                        break;
                    case Character.s:
                        _baseLetterOffset = 275f;
                        _baseXCorrection = 15f;
                        break;
                    case Character.j:
                        _baseLetterOffset = 200f;
                        _baseXCorrection = 50f;
                        break;
                    case Character.d:
                        _baseLetterOffset = 275f;
                        _baseXCorrection = 40f;
                        break;
                    case Character.n:
                        _baseLetterOffset = 270f;
                        _baseXCorrection = 20f;
                        break;
                    case Character.v:
                        _baseLetterOffset = 270f;
                        _baseXCorrection = 20f;
                        break;
                    case Character.Q:
                        _baseLetterOffset = 350f;
                        break;
                    case Character.W:
                        _baseLetterOffset = 375f;
                        _baseXCorrection = 90f;
                        break;
                    case Character.I:
                        _baseLetterOffset = 260f;
                        _baseXCorrection = -40f;
                        break;
                    case Character.T:
                        _baseLetterOffset = 320f;
                        _baseXCorrection = 10f;
                        break;
                    case Character.B:
                        _baseLetterOffset = 320f;
                        _baseXCorrection = 10f;
                        break;
                    case Character.R:
                        _baseLetterOffset = 320f;
                        _baseXCorrection = 10f;
                        break;
                    case Character.U:
                        _baseLetterOffset = 275f;
                        //_baseXCorrection = -80f;
                        break;
                    case Character.M:
                        _baseLetterOffset = 325f;
                        _baseXCorrection = 90f;
                        break;
                    case Character.J:
                        _baseLetterOffset = 250f;
                        //_baseXCorrection = 90f;
                        break;
                    case Character.Z:
                        _baseLetterOffset = 330f;
                        _baseXCorrection = 80f;
                        break;
                    case Character.D:
                        _baseLetterOffset = 360f;
                        //_baseXCorrection = 90f;
                        break;
                    case Character.O:
                        //_baseLetterOffset = 300f;
                        //_baseXCorrection = 90f;
                        break;
                    case Character.G:
                        _baseLetterOffset = 360f;
                        _baseXCorrection = 60f;
                        break;
                    case Character.L:
                        _baseLetterOffset = 280f;
                        _baseXCorrection = 0f;
                        break;
                    case Character.Apostrophe:
                        _baseLetterOffset = 200f;
                        _baseXCorrection = -100f;
                        break;
                    case Character.LeftParenthesis:
                        _baseLetterOffset = 200f;
                        break;
                    case Character.RightParenthesis:
                        _baseLetterOffset = 200f;
                        break;
                    case Character.LessThan:
                        _baseLetterOffset = 275f;
                        break;
                    case Character.GreaterThan:
                        _baseLetterOffset = 275f;
                        break;
                    case Character.LeftBracket:
                        _baseLetterOffset = 200f;
                        break;
                    case Character.RightBracket:
                        _baseLetterOffset = 200f;
                        break;
                    case Character.Comma:
                        _baseLetterOffset = 150;
                        _baseXCorrection = -50f;
                        break;
                    case Character.Period:
                        _baseLetterOffset = 150;
                        _baseXCorrection = -50f;
                        break;
                };
            }

            LetterOffset = _baseLetterOffset * Scale * _scaleX;
            YOffset = _baseYOffset * Scale;

            XCorrection = _baseXCorrection * Scale * _scaleX;
            YCorrection = _baseYCorrection * Scale;
        }
    }

    public class Text
    {
        public List<Letter> Letters = new List<Letter>();
        public string TextString = "";
        public Vector3 Position = new Vector3();

        public float Scale = 0.1f;
        public bool CameraPerspective = false;
        private float _baseLetterOffset = 300f;
        public float LetterOffset = 300f;
        public float YOffset = 0f;

        public float NewLineHeight = 700f;

        public bool Render = true;

        public Text() { }
        public Text(string textString, Vector3 position = new Vector3(), bool cameraPerspective = false) 
        {
            TextString = textString;
            Position = position;
            CameraPerspective = cameraPerspective;


            SetTextString(textString);
        }

        public void SetTextString(string textString) 
        {
            //textString = textString.Replace("\r", "");

            Texture tempTexture = null;

            if (Letters.Count > 0) 
            {
                tempTexture = Letters[0].LetterObject.BaseFrame.TextureReference; //hack, we know this texture is already loaded so we can hot swap characters
            }

            TextString = textString;
            char[] arr = TextString.ToCharArray();
            Vector3 position = new Vector3(Position);

            if (Letters.Count > arr.Length) 
            {
                Letters.RemoveRange(arr.Length, Letters.Count - arr.Length);
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (CharacterConstants._characterMap[arr[i]] == Character.NewLine)
                {
                    position.X = Position.X;
                    position.Y += NewLineHeight * Scale;
                }

                if (i < Letters.Count)
                {
                    Letters[i].ChangeCharacter(CharacterConstants._characterMap[arr[i]]);
                    Letters[i].SetPosition(position);
                }
                else 
                {
                    Letter temp = new Letter(CharacterConstants._characterMap[arr[i]], position, CameraPerspective, i, Scale);

                    if (tempTexture != null)
                    {
                        temp.LetterObject.BaseFrame.TextureReference = tempTexture; //hack, figure out a fix for this problem later (TextureReference of new renderable object is null)
                    }

                    Letters.Add(temp);
                }

                //if (CharacterConstants._characterMap[arr[i]] == Character.NewLine)
                //{
                //    position.X = Position.X;
                //    position.Y += NewLineHeight * Scale;
                //}
                //else
                //{
                //    position.X += Letters[i].LetterOffset + Letters[i].XCorrection;
                //    position.Y += Letters[i].YOffset;
                //}

                if (CharacterConstants._characterMap[arr[i]] != Character.NewLine)
                {
                    position.X += Letters[i].LetterOffset + Letters[i].XCorrection;
                    position.Y += Letters[i].YOffset;
                }
            }
        }
        public void RecalculateTextPosition() 
        {
            Vector3 position = new Vector3(Position);

            for (int i = 0; i < Letters.Count; i++)
            {
                Letters[i].SetPosition(position);

                if (Letters[i].Character == Character.NewLine)
                {
                    position.X = Position.X;
                    position.Y += NewLineHeight * Scale;
                }
                else
                {
                    position.X += Letters[i].LetterOffset + Letters[i].XCorrection;
                    position.Y += Letters[i].YOffset + Letters[i].YCorrection;
                }
            }
        }


        public UIDimensions GetTextDimensions()
        {
            if (Letters.Count == 0)
                return new UIDimensions();

            Vector3 maxPos = new Vector3(Letters[0].Position);
            Vector3 minPos = new Vector3(Letters[0].Position);

            Vector3 letterDimensions = Letters[0].GetDimensions(); //assume all letters have the same dimensions for now

            for (int i = 0; i < Letters.Count; i++)
            {
                maxPos.X = Letters[i].Position.X > maxPos.X ? Letters[i].Position.X : maxPos.X;
                maxPos.Y = Letters[i].Position.Y > maxPos.Y ? Letters[i].Position.Y : maxPos.Y;
                minPos.X = Letters[i].Position.X < minPos.X ? Letters[i].Position.X : minPos.X;
                minPos.Y = Letters[i].Position.Y < minPos.Y ? Letters[i].Position.Y : minPos.Y;
            }

            UIDimensions dimensions = new UIDimensions((maxPos.X + letterDimensions.X / 2) - (minPos.X - letterDimensions.X / 2), 
                (maxPos.Y - letterDimensions.Y / 2) - (minPos.Y + letterDimensions.Y / 2));

            return dimensions;
        }

        public void AddCharacter(Character character, int index = -1)
        {

            if (index > 0 || index >= TextString.Length)
            {
                Vector3 position = new Vector3(Position) + new Vector3(GetOffsetAtIndex(TextString.Length - 1), GetYOffsetAtIndex(TextString.Length - 1), 0);
                Letter temp = new Letter(character, position, CameraPerspective, TextString.Length, Scale);
                TextString += CharacterConstants._characterMapToChar[character];

                Letters.Add(temp);
            }
            else 
            {
                Vector3 position = new Vector3(Position) + new Vector3(GetOffsetAtIndex(index), GetYOffsetAtIndex(index), 0);
                Letter temp = new Letter(character, position, CameraPerspective, index, Scale);

                char[] arr = TextString.ToCharArray();
                string newStr = "";
                for (int i = 0; i < TextString.Length; i++) 
                {
                    if (i == index)
                    {
                        newStr += CharacterConstants._characterMapToChar[character];
                        Letters.Insert(i, temp);
                    }
                    else if(i < index)
                    {
                        position.X = Letters[i + 1].LetterOffset * (i + 1);
                        position.Y = Letters[i + 1].YOffset * (i + 1);

                        Letters[i + 1].SetPosition(position);
                        Letters[i + 1].BaseObjects[0].ID++;
                    }

                    newStr += arr[i];
                }
            }
        }
        public void AddCharacter(char character, int index = -1)
        {
            if (index < 0 || index >= TextString.Length)
            {
                Vector3 position = new Vector3(Position) + new Vector3(GetOffsetAtIndex(TextString.Length - 1), GetYOffsetAtIndex(TextString.Length - 1), 0);
                Letter temp = new Letter(CharacterConstants._characterMap[character], position, CameraPerspective, TextString.Length, Scale);
                TextString += character;

                Letters.Add(temp);
            }
            else
            {
                Vector3 position = new Vector3(Position) + new Vector3(GetOffsetAtIndex(index), GetYOffsetAtIndex(index), 0);
                Letter temp = new Letter(CharacterConstants._characterMap[character], position, CameraPerspective, index, Scale);

                char[] arr = TextString.ToCharArray();
                string newStr = "";
                for (int i = 0; i < TextString.Length; i++)
                {
                    if (i == index)
                    {
                        newStr += character;
                        Letters.Insert(i, temp);
                    }
                    else if (i < index)
                    {
                        position.X = Letters[i + 1].LetterOffset * (i + 1);
                        position.Y = Letters[i + 1].YOffset * (i + 1);

                        Letters[i + 1].SetPosition(position);
                        Letters[i + 1].BaseObjects[0].ID++;
                    }

                    newStr += arr[i];
                }
            }

        }
        public void RemoveCharacter(int index = -1)
        {
            if (index < 0 || index >= TextString.Length)
            {
                Letters.RemoveAt(TextString.Length - 1);
                TextString = TextString.Remove(TextString.Length - 1, 1);
            }
            else
            {

                for (int i = 0; i < TextString.Length; i++)
                {
                    if (i > index) 
                    {
                        Letters[i].Position.X = Position.X + GetOffsetAtIndex(i - 1);
                        Letters[i].Position.Y = Position.Y + GetYOffsetAtIndex(i - 1);

                        Letters[i].SetPosition(Letters[i].Position);
                        Letters[i].BaseObjects[0].ID--;
                    }
                }

                Letters.RemoveAt(index);
                TextString = TextString.Remove(index);
            }
        }

        public void SetScale(float scale) 
        {
            Scale = scale;
            LetterOffset = Scale * _baseLetterOffset;

            Vector3 position = Position;
            Letters.ForEach(letter =>
            {
                letter.SetScale(Scale);
            });

            RecalculateTextPosition();
        }
        public void SetColor(Vector4 color) 
        {
            Letters.ForEach(letter =>
            {
                letter.SetColor(color);
            });
        }
        public void SetPosition(Vector3 position)
        {
            Position = position;
            RecalculateTextPosition();
        }
        private float GetOffsetAtIndex(int index) 
        {
            float offset = 0;
            if (index < TextString.Length)
                for (int i = 0; i < index; i++) 
                {
                    offset += Letters[i].LetterOffset;
                    if (Letters[i].Character == Character.NewLine) 
                    {
                        offset = 0;
                    }
                }

            return offset;
        }
        private float GetYOffsetAtIndex(int index)
        {
            float offset = 0;
            if (index < TextString.Length)
                for (int i = 0; i < index; i++)
                {
                    offset += Letters[i].YOffset;
                    if (Letters[i].Character == Character.NewLine)
                    {
                        offset += NewLineHeight * Scale;
                    }
                }

            return offset;
        }
    }
}
