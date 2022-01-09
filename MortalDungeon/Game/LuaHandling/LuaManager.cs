using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MortalDungeon.Game.LuaHandling
{
    public static class LuaManager
    {
        public static Lua State = new Lua();

        public static void Initialize()
        {
            State.LoadCLRPackage();

            State.DoFile("Game/LuaHandling/LuaSource.lua");
        }

        public static string PrepareScript(string script)
        {
            string preparedScript = script;

            int indexOfLambda = preparedScript.IndexOf("() => ");
            int stringSize = 6;

            while (indexOfLambda != -1)
            {
                int semicolonIndex = preparedScript.IndexOf(';', indexOfLambda);

                string temp = preparedScript.Substring(indexOfLambda, semicolonIndex + 1 - indexOfLambda)
                    .Replace("() => ", "function () return ").Replace(";", "end");

                preparedScript = preparedScript.Substring(0, indexOfLambda) + temp + preparedScript.Substring(semicolonIndex + 1);

                indexOfLambda = preparedScript.IndexOf("() => ");
            }

            script = script.Replace(" ", "").Replace("\n", "");

            return preparedScript;
        }

        public static void ApplyScript(string script)
        {
            script = PrepareScript(script);

            if (script.Contains("import(") || script.Contains("require("))
            {
                return;
            }

            var vals = State.DoString(script);

            foreach (var val in vals)
            {
                Console.WriteLine(val);
            }
        }
    }
}
