using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetriON.Skins;

public class Skin {

    private static readonly Dictionary<string, string> Skins = new() {
        ["default"] = "skins/default/",
        ["dark"] = "skins/dark/"
    };

    private string _currentSkin = "default";

    public void SetSkin(string skinName)
    {
        if (Skins.ContainsKey(skinName))
        {
            _currentSkin = skinName;
        }
        else throw new ArgumentException($"Skin '{skinName}' does not exist.");
    }

    public string GetCurrentSkinPath()
    {
        return Skins[_currentSkin];
    }
    
    public void ReloadSkins() {
        // Placeholder for future skin reloading logic
    }
}


