using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Resources;

namespace MortalDungeon
{
    class Program
    {
        public static Window Window;

        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1280, 720),
                //Size = new Vector2i(2560, 1440),
                //Size = new Vector2i(800, 800),
                Title = "Test Window",
                WindowBorder = OpenTK.Windowing.Common.WindowBorder.Resizable
            };

            var gameWindowSettings = GameWindowSettings.Default;


            using (var game = new Window(gameWindowSettings, nativeWindowSettings))
            {
                Window = game;

                game.VSync = OpenTK.Windowing.Common.VSyncMode.Off;
                game.Run();
            }
        }
    }
}
