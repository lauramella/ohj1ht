using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class KonnaEscape : PhysicsGame
{
    public override void Begin()
    {
        SetWindowSize(1024, 768);
        TileMap kentta = TileMap.FromLevelAsset("kentta");

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
}
