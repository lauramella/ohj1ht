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
        TileMap kentta = TileMap.FromLevelAsset("kentta2");
        kentta.SetTileMethod('x', LuoLattia);
        kentta.SetTileMethod('t', LuoTaso);
        kentta.SetTileMethod('T', LuoTaso);
        kentta.SetTileMethod('a', LuoTaso);
        kentta.SetTileMethod('p', LuoPelaaja);
        kentta.SetTileMethod('3', LuoVihu, 2, 3);
        kentta.SetTileMethod('4', LuoVihu, 2, 3);
        kentta.SetTileMethod('v', LuoVihu, 2, 3);
        kentta.Execute(BLOKIN_LEVEYS, BLOKIN_KORKEUS);

        Level.Background.CreateStars();
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
        //AddCollisionHandler(pelaaja, "vihunAmmus", CollisionHandler.DestroyObject);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", pelaaja, HYPPYVOIMA);
        Keyboard.Listen(Key.Space, ButtonState.Pressed, Heita, "Heita ammus", pelaaja, "pelaajanAmmus");
    }

    public void LuoVihu(Vector paikka, double leveys, double korkeus, int liikemaara, int hp)
    {
     
        Vihu vihu = new Vihu(leveys, korkeus, hp);
        vihu.Position = paikka;
        vihu.Image = LoadImage("pallo1");
        vihu.Shape = Shape.Circle;
        vihu.CanRotate = false;
        Add(vihu);

        PathFollowerBrain pfb = new PathFollowerBrain();
        List<Vector> reitti = new List<Vector>();
        reitti.Add(vihu.Position);
        Vector seuraavaPiste = vihu.Position + new Vector(liikemaara * BLOKIN_LEVEYS, 0);
        reitti.Add(seuraavaPiste);
        pfb.Path = reitti;
        pfb.Loop = true;
        vihu.Brain = pfb;

        Timer heittoajastin = new Timer();
        heittoajastin.Interval = 3.0;
        heittoajastin.Timeout += delegate () { Heita(vihu, "vihunAmmus"); };
        heittoajastin.Start();
        AddCollisionHandler(vihu, "pelaajanAmmus", CollisionHandler.DestroyTarget);
            AddCollisionHandler(vihu, "pelaajanAmmus", CollisionHandler.AddMeterValue(vihu.HP, -1));
            vihu.Destroyed += delegate () { heittoajastin.Stop(); };
    }


 




    public void Heita(PhysicsObject heittavaOlio, string tagi)
    {
        PhysicsObject heitettava = new PhysicsObject(BLOKIN_LEVEYS / 2, BLOKIN_KORKEUS / 2, Shape.Heart);
        heitettava.Position = heittavaOlio.Position + new Vector(BLOKIN_LEVEYS/2, BLOKIN_KORKEUS/2);
        heitettava.Hit(new Vector(800, 200));
        heitettava.Tag = "vihunAmmus";
        heitettava.MaximumLifetime = TimeSpan.FromSeconds(3.0);
        Add(heitettava, 1);
    }


    public void Hyppaa(PlatformCharacter hahmo, double voima)
    {
        hahmo.Jump(voima);
    }
    public void Liikuta(PlatformCharacter vihu, double suunta)
    {
        vihu.Walk(suunta);
    }

    public void LuoTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = new PhysicsObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = LoadImage("taso");
        taso.Image = LoadImage("taso2");
        taso.Image = LoadImage("taso3");
        taso.MakeStatic();
        Add(taso);
    }
    public void LuoLattia(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject lattia = new PhysicsObject(leveys, korkeus);
        lattia.Position = paikka;
        lattia.Image = LoadImage("taso");
        lattia.MakeStatic();
        Add(lattia);
    }
}