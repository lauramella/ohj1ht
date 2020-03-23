using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Laura Mella
/// @version 17.03.2020


/// <summary>
/// Tasohyppelypeli
/// Keltaiset pallot yrittävät valloittaa ruudun! Pelin tarkoitus on väistellä keltaisia palloja sekä kerätä pisteitä ampumalla niitä. 
/// Pelin alussa kilpikonnalla eli pelaajalla on kolme elämää ja se menettää yhden elämän osuessaan viholliseen eli palloon. Pelajaan kerätessä tarpeeksi monta pistettä, pelikentälle 
/// ilmestyy bonuksia, joita keräämällä pelaaja saa jokaisesta pisteen ja 1/4 elämän.
/// </summary>


public class Kille : PhysicsGame
{    
    const double LIIKUTUSVOIMA = 600;
    const double HYPPYVOIMA = 2000;
    const double BLOKIN_LEVEYS = 80;
    const double BLOKIN_KORKEUS = 80;
    const int RUUDUN_LEVEYS = 1024;
    const int RUUDUN_KORKEUS = 768;
    const int PAINOVOIMA_X = 0;
    const int PAINOVOIMA_Y = -2500;
    public double pelaajanElama = 3.0;
    public IntMeter pisteLaskuri;
   

    /// <summary>
    /// Luodaan pistelaskuri ja näyttö.
    /// </summary>
    public void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);
        Label pistenaytto = new Label();
        pistenaytto.BindTo(pisteLaskuri);
        pistenaytto.Color = Color.Red;
        pistenaytto.TextColor = Color.Black;
        pistenaytto.Title = "Pisteet";
        pistenaytto.Y = Screen.Top - 10;
        Add(pistenaytto);
    }


    /// <summary>
    /// Aloitetaan peli, luodaan kenttä sekä käynnistetään ajastin, joka kutsuu aliohjelmaa, joka luo vihuja.
    /// </summary>
    public override void Begin()
    {
        SetWindowSize(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('t', LuoTaso, "taso");
        kentta.SetTileMethod('T', LuoTaso, "taso2");
        kentta.SetTileMethod('a', LuoTaso, "taso3");
        kentta.SetTileMethod('p', LuoPelaaja);
        kentta.Execute(BLOKIN_LEVEYS, BLOKIN_KORKEUS);
        LuoPistelaskuri(); //Aliohjelmakutsu pistelaskurille
      
        Timer lisaysAjastin = new Timer();
        lisaysAjastin.Interval = 1.2;
        lisaysAjastin.Timeout += delegate ()
        {
            LuoVihu();

        };
        lisaysAjastin.Start();
        
        Level.Background.CreateStars();
        Level.CreateBorders();
        Camera.ZoomToLevel();

        Gravity = new Vector(PAINOVOIMA_X, PAINOVOIMA_Y); 
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Luodaan pelaaja sekä lisätään näppäimet, joilla sitä liikutetaan sekä ammutaan.
    /// </summary>
    /// <param name="paikka">Mihin pelaaja luodaan</param>
    /// <param name="leveys">Pelaajan leveys</param>
    /// <param name="korkeus">Pelaajan korkeus</param>
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


    /// <summary>
    /// Aliohjelma, jossa luodaan vihut ja lisätään niille tekoäly.
    /// </summary>
    public void LuoVihu()
    {
        PhysicsObject vihu = LuoKohde(80, RandomGen.NextVector(Level.BoundingRect), LoadImage("pallo1"));
        vihu.CanRotate = false;   
        AddCollisionHandler<PhysicsObject, PlatformCharacter>(vihu, PelaajaTormasiVihuun);
        AddCollisionHandler(vihu, "Ammus", CollisionHandler.DestroyObject);
        AddCollisionHandler(vihu, "Ammus", Osuma);

        RandomMoverBrain vihunAivot = new RandomMoverBrain();
        vihunAivot.Speed = 400;
        vihu.Brain = vihunAivot;
        vihunAivot.ChangeMovementSeconds = 1;
        Add(vihu);
    }
 

    /// <summary>
    /// Lisätään pistelaskuriin piste, kun ammus osuu vihuun.
    /// Äänitehoste pisteen saamisesta.
    /// Kutsutaan silmukassa aliohjelmaa "LuoBonus"
    /// </summary>
    /// <param name="vihu">vihollinen</param>
    /// <param name="ammus">pelaajan ammus</param>
    public void Osuma(PhysicsObject vihu, PhysicsObject ammus)
    {
        SoundEffect pisteAani = LoadSoundEffect("piste");
        pisteAani.Play();
        pisteLaskuri.Value += 1;
        if (pisteLaskuri.Value % 9 == 0)
        {
            for (int i = 0; i < 3; i++) LuoBonus();           
        }
    }
 
    
     /// <summary>
     /// Luodaan bonus satunnaiseen paikkaan pelikentällä.
     /// </summary>     
    public void LuoBonus()
    {
        PhysicsObject bonus = LuoKohde(40, RandomGen.NextVector(Level.BoundingRect), LoadImage("bonus"));
        AddCollisionHandler<PhysicsObject, PlatformCharacter>(bonus, BonusPisteet);
        Add(bonus);      
    }


    /// <summary>
    /// Funktio, joka palauttaa parametreina koon, paikan ja kuvan. Näitä voidaan hyödyntää luodessa kohteita pelikentälle.
    /// </summary>
    /// <param name="koko">Kohteen koko</param>
    /// <param name="paikka">Kohteen paikka</param>
    /// <param name="kuva">Kohteen kuva</param>
    /// <returns></returns>
    public PhysicsObject LuoKohde(double koko, Vector paikka, Image kuva)
    {
       PhysicsObject kohde = new PhysicsObject(koko, koko);
       kohde.Position = paikka;
       kohde.Image = kuva;
        return kohde;
    }


    /// <summary>
    ///  Pelaaja saa pisteen ja lisää elämää kerätessä bonuksen.
    /// </summary>
    /// <param name="bonus">bonus</param>
    /// <param name="pelaaja">pelaaja</param>
    public void BonusPisteet(PhysicsObject bonus, PhysicsObject pelaaja)
    {
        pelaajanElama += 0.25;
        PlatformCharacter hahmo = pelaaja as PlatformCharacter;
        pisteLaskuri.Value += 1;
        SoundEffect osumaAani = LoadSoundEffect("piste");
        osumaAani.Play();
        bonus.Destroy();
    }


    /// <summary> 
    ///  Pelaaja törmäsi vihuun. Kolmannesta törmäyksestä peliloppuu.
    ///  Törmäykselle äänitehoste.
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="vihu">Vihu</param>
    public void PelaajaTormasiVihuun(PhysicsObject pelaaja, PhysicsObject vihu)
    {
        PlatformCharacter hahmo = pelaaja as PlatformCharacter;
        pelaajanElama--;
        SoundEffect osumaAani = LoadSoundEffect("osuma");
        osumaAani.Play();
        pelaaja.Destroy();
        if (pelaajanElama <= 0)
        {
            vihu.Destroy();
            LuoLoppuValikko();
        }
    }
  
    
    /// <summary>
    /// Luodaan loppuvalikko, jossa taustamusiikki.
    /// Loppuvalikosta näkyy pelissä kerätyt pisteet ja vaihtoehtoina on aloittaa peli uudestaan tai lopettaa peli.
    /// </summary>
    public void LuoLoppuValikko()
    {
        MultiSelectWindow loppuValikko = new MultiSelectWindow("Hienoa! Sait " + pisteLaskuri.Value + " pistettä.", "Aloita peli uudestaan", "Lopeta");
        loppuValikko.AddItemHandler(0, AloitaPeliUudestaan);
        loppuValikko.AddItemHandler(1, Exit);
        Add(loppuValikko);

    }


    /// <summary>
    /// Aliohjelma, joka aloittaa pelin uudestaan ja nollaa laskurit
    /// </summary>
      public void AloitaPeliUudestaan()
      {
        ClearAll();          
        pelaajanElama += 3;
        Begin();
      }


    /// <summary>
    /// Pelaaja heittää ammuksen
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="tagi">Ammus</param>
    public void Heita(PhysicsObject pelaaja, string tagi)
    {
        PhysicsObject ammus = new PhysicsObject(BLOKIN_LEVEYS / 2, BLOKIN_KORKEUS / 2, Shape.Star);
        ammus.Position = pelaaja.Position + new Vector(BLOKIN_LEVEYS/2, BLOKIN_KORKEUS/2);
        ammus.Hit(new Vector(800, 350));
        ammus.Tag = "Ammus";
        ammus.MaximumLifetime = TimeSpan.FromSeconds(2.0);
        Add(ammus, 1);
    }


    /// <summary>
    /// Pelaaja hyppää
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="voima">Hyppyvoima, määritelty ohjelman alussa vakiona</param>
    public void Hyppaa(PlatformCharacter pelaaja, double voima)
    {
        pelaaja.Jump(voima);
        SoundEffect hyppyAani = LoadSoundEffect("jump");
        hyppyAani.Play();
    }


    /// <summary>
    /// Pelaajan liikutus
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    /// <param name="suunta">Suunta</param>
    public void Liikuta(PlatformCharacter pelaaja, double suunta)
    {
        pelaaja.Walk(suunta);
    }


    /// <summary>
    /// Luodaan tasot
    /// </summary>
    /// <param name="paikka">Mihin taso luodaan</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    /// <param name="tekstuuri">Tasojen tekstuuri</param>
    public void LuoTaso(Vector paikka, double leveys, double korkeus, string tekstuuri)
    {
        PhysicsObject taso = new PhysicsObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = LoadImage(tekstuuri);
        taso.MakeStatic();
        Add(taso);
    }
}