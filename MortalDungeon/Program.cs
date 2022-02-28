using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Resources;
using MortalDungeon.Engine_Classes.Audio;
using System.Threading;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MortalDungeon
{
    public class Program
    {
        public static Window Window;

        public static void Main(string[] args)
        {
            InitializeSoundPlayer();
            
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1280, 720),
                //Size = new Vector2i(2560, 1440),
                //Size = new Vector2i(800, 800),
                Title = "Test Window",
                WindowBorder = OpenTK.Windowing.Common.WindowBorder.Resizable,
                StartFocused = false,
            };

            var gameWindowSettings = GameWindowSettings.Default;
            //gameWindowSettings.IsMultiThreaded = true;
            //gameWindowSettings.RenderFrequency = 30;
            //gameWindowSettings.RenderFrequency = 60;

            using (var game = new Window(gameWindowSettings, nativeWindowSettings))
            {
                Window = game;
                //Window.Context.MakeCurrent();

                game.VSync = OpenTK.Windowing.Common.VSyncMode.Off;

                game.Run();
            }
        }

        static void InitializeSoundPlayer() 
        {
            Thread soundThread = new Thread(SoundPlayer.Initialize);
            soundThread.Priority = ThreadPriority.Highest;

            soundThread.Start();
        }
    }
}
