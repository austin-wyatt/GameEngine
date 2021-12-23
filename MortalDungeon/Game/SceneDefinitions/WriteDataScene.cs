using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.SceneDefinitions
{
    class WriteDataScene : Scene
    {
        public WriteDataScene()
        {
            InitializeFields();
        }


        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null)
        {
            base.Load(camera, cursorObject, mouseRay);

            Dialogue dialogue = new Dialogue();

            DialogueNode entry = new DialogueNode(0, "Hello traveler");

            var option = entry.AddResponse(message: "Oh, hello there")
                .AddNext(0, "What brings you to this neck of the woods?");

            var hostilePath = option.AddResponse(message: "None of your beeswax")
                .AddNext(1, "But the question I have is what are you doing here?")
                .AddResponse()
                .AddNext(0, "Frig off buddy")
                .AddResponse(message:"Rude.", outcome: 1);

            var amiablePath = option.AddResponse(message: "Just taking a stroll")
                .AddNext(0, "Nice")
                .AddResponse()
                .AddNext(0, "Well... See you around.")
                .AddResponse(type: ResponseType.Ok, outcome: 2);

            var longPath = option.AddResponse(message: "lots of responses")
                .AddNext(0, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(1, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(0, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(1, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(0, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(1, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(1, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(1, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse()
                .AddNext(1, "This is a long message meant to take up lots of space in the conversation")
                .AddResponse(type: ResponseType.Ok, outcome: 3);



            dialogue.EntryPoint = entry;

            DialogueSerializer.WriteDialogueToFile(dialogue);


        }

        public override void OnUpdateFrame(FrameEventArgs args)
        {
            
        }

        public override bool OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                Window.CloseWindow();
            }

            return base.OnKeyUp(e);
        }
    }
}
