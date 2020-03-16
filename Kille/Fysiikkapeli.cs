using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// <summary>
///  TODO: Lisää äänet
///  TODO: Dokumentointi
/// </summary>

public class Kille : PhysicsGame
{    
    const double LIIKUTUSVOIMA = 600;
    const double HYPPYVOIMA = 2000;
    const double BLOKIN_LEVEYS = 80;
    const double BLOKIN_KORKEUS = 80;
    public int pelaajanTerveys = 3;
    
  

    public override void Begin()
    {
        SetWindowSize(1024, 768);
        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('t', LuoTaso, "taso");
        kentta.SetTileMethod('T', LuoTaso, "taso2");
        kentta.SetTileMethod('a', LuoTaso, "taso3");
        kentta.SetTileMethod('p', LuoPelaaja);
        kentta.Execute(BLOKIN_LEVEYS, BLOKIN_KORKEUS);
        LuoPistelaskuri();
        List<PhysicsObject> vihut = new List<PhysicsObject>();

        Timer lisaysAjastin = new Timer();
        lisaysAjastin.Interval = 1.3;
        lisaysAjastin.Timeout += delegate ()
        {
            LuoVihu(vihut, "Vihu");
        };
        lisaysAjastin.Start();
        
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
        pelaaja.Image = LoadImage("pelaaja");
        Add(pelaaja);

        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", pelaaja, HYPPYVOIMA);
        Keyboard.Listen(Key.Space, ButtonState.Pressed, Heita, "Heita ammus", pelaaja, "Ammus");
    }

    public void LuoVihu(List<PhysicsObject> vihut, string tagi)
    {
        PhysicsObject vihu = new PhysicsObject(80, 80, Shape.Circle);
        Vector vihunSijainti;
        vihunSijainti = RandomGen.NextVector(Level.BoundingRect);
        vihu.Position = vihunSijainti;
        vihu.Image = LoadImage("pallo1");
        vihu.CanRotate = false;
        AddCollisionHandler<PhysicsObject, PlatformCharacter>(vihu, PelaajaTormasiVihuun);
        tagi = "Vihu";
        AddCollisionHandler(vihu, "Ammus", CollisionHandler.DestroyObject);
        AddCollisionHandler(vihu, "Ammus", Osuma);

        RandomMoverBrain vihunAivot = new RandomMoverBrain();
        vihunAivot.Speed = 400;
        vihu.Brain = vihunAivot;
        vihunAivot.ChangeMovementSeconds = 1;
        Add(vihu);
    }

    public void Osuma(PhysicsObject vihu, PhysicsObject ammus)
    {
        pisteLaskuri.Value +=1;
    }
    IntMeter pisteLaskuri;
    private void LuoPistelaskuri()
    {
     pisteLaskuri = new IntMeter(0);
     Label pistenaytto = new Label();
        pistenaytto.BindTo(pisteLaskuri);
        pistenaytto.Color = Color.Red; // Taustaväri
        pistenaytto.TextColor = Color.Black; // Tekstin väri
        pistenaytto.Title = "Pisteet";
        pistenaytto.Y = Screen.Top - 10;
        Add(pistenaytto);
    }

    /// <summary> 
    ///  Pelaaja törmäsi vihuun.
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="vihu">Vihu</param>
    public void PelaajaTormasiVihuun(PhysicsObject pelaaja, PhysicsObject vihu)
    {
        PlatformCharacter hahmo = pelaaja as PlatformCharacter;
        pelaajanTerveys--;
        pelaaja.Destroy();
        if (pelaajanTerveys <= 0)
        {
            vihu.Destroy();
            LuoLoppuValikko();
        }
    }
    

    private void LuoLoppuValikko()
    {
        MultiSelectWindow loppuValikko = new MultiSelectWindow("Hienoa! Sait " + pisteLaskuri.Value + " pistettä", "Aloita peli uudestaan", "Lopeta");
        loppuValikko.AddItemHandler(0, AloitaPeliUudestaan);
        loppuValikko.AddItemHandler(1, Exit);
        Add(loppuValikko);
            }
      public void AloitaPeliUudestaan()
        {
        ClearAll();
        pelaajanTerveys += 3;
        Begin();
         }


    public void Heita(PhysicsObject pelaaja, string tagi)
    {
        PhysicsObject ammus = new PhysicsObject(BLOKIN_LEVEYS / 2, BLOKIN_KORKEUS / 2, Shape.Star);
        ammus.Position = pelaaja.Position + new Vector(BLOKIN_LEVEYS/2, BLOKIN_KORKEUS/2);
        ammus.Hit(new Vector(800, 350));
        ammus.Tag = "Ammus";
        ammus.MaximumLifetime = TimeSpan.FromSeconds(2.0);
        Add(ammus, 1);
    }

    public void Hyppaa(PlatformCharacter pelaaja, double voima)
    {
        pelaaja.Jump(voima);
    }
    public void Liikuta(PlatformCharacter pelaaja, double suunta)
    {
        pelaaja.Walk(suunta);
    }

    public void LuoTaso(Vector paikka, double leveys, double korkeus, string tekstuuri)
    {
        PhysicsObject taso = new PhysicsObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = LoadImage(tekstuuri);
        taso.MakeStatic();
        Add(taso);
    }

}