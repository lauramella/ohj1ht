using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Kille : PhysicsGame
{

    const double LIIKUTUSVOIMA = 500;
    const double HYPPYVOIMA = 2000;
    const double BLOKIN_LEVEYS = 80;
    const double BLOKIN_KORKEUS = 80;

    public override void Begin()
    {
        SetWindowSize(1024, 768);
        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('x', LuoLattia);
        kentta.SetTileMethod('t', LuoTaso);
        kentta.SetTileMethod('p', LuoPelaaja);
        kentta.Optimize('t');
        kentta.Execute(BLOKIN_LEVEYS, BLOKIN_KORKEUS);



        Level.CreateBorders();
        Camera.ZoomToLevel();

        Gravity = new Vector(0, -2500);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    public void LuoPelaaja(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter pelaaja = new PlatformCharacter(leveys, korkeus);
        pelaaja.Position = paikka;
        pelaaja.Image = LoadImage("kille1");
        Add(pelaaja);

        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", pelaaja, HYPPYVOIMA);

    }
    public void Hyppaa(PlatformCharacter hahmo, double voima)
    {
        hahmo.Jump(voima);
    }
    public void Liikuta(PlatformCharacter liikuteltavaOlio, double suunta)
    {
        liikuteltavaOlio.Walk(suunta);
    }

    public void LuoTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = new PhysicsObject(leveys, korkeus);
        taso.Position = paikka;
        taso.MakeStatic();
        Add(taso);
    }
    public void LuoLattia(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lattia = new PhysicsObject(leveys, korkeus);
        lattia.Position = paikka;
        lattia.MakeStatic();
        Add(lattia);
    }
}