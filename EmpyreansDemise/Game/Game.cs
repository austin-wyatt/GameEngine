﻿using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game
{
    //Main entrypoint for the game. All position logic, shader info (just the names of the shaders to be used), 
    //texture info, etc are handled here and passed to the renderer
    class Game
    {
        public List<Scene> Scenes = new List<Scene>(); //list of all scenes, might need to differentiate between "game" scenes and "menu" scenes or some such. 
                                                       //Might want to further generalize a "scene" to be a collection of objects and their placement + function so
                                                       //that multiple scenes can be used in tandem (hud scene overlaying an environment for example)
        //TODO: environment, item, character, etc objects that extend the BaseObject and provide more specific (and simple) handling
    }
}
