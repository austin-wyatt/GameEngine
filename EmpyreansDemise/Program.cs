using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Resources;
using Empyrean.Engine_Classes.Audio;
using System.Threading;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace Empyrean
{
    public class Program
    {
        public static Window Window;

        public static Stopwatch ProgramTimer = new Stopwatch();

        public static void Main(string[] args)
        {
            ProgramTimer.Start();

            InitializeSoundPlayer();

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1280, 720),
                //Size = new Vector2i(2560, 1440),
                //Size = new Vector2i(800, 800),
                Title = "Test Window",
                //WindowBorder = OpenTK.Windowing.Common.WindowBorder.Fixed,
                WindowBorder = OpenTK.Windowing.Common.WindowBorder.Resizable,
                StartFocused = false,
                NumberOfSamples = 4,
                //WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen
            };

            //nativeWindowSettings.Profile = OpenTK.Windowing.Common.ContextProfile.Core;

            var gameWindowSettings = GameWindowSettings.Default;
            //gameWindowSettings.RenderFrequency = 30;
            //gameWindowSettings.RenderFrequency = 60;

            gameWindowSettings.RenderFrequency = 200;

            using (var game = new Window(gameWindowSettings, nativeWindowSettings))
            {
                Window = game;
                //Window.Context.MakeCurrent();

                game.VSync = OpenTK.Windowing.Common.VSyncMode.Off;
                //game.VSync = OpenTK.Windowing.Common.VSyncMode.On;

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
