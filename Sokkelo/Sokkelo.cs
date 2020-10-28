using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Jesse Korolainen & Teemu Nieminen
/// @version 18.10.2020
/// @version 27.10.2020 Muokattu Taso1 vektorit. Arvon palautus ei toimi. Lisätty virukselle tuhoutumispiste oikeaan seinään ja testattu.
/// <summary>
/// Luodaan tietyt koordinaatit, mitä pitkin fysiikkaobjekti pääsee etenemään. 
/// </summary>
public class Sokkelo : PhysicsGame
{
    private PhysicsObject oikeaReuna;
    // IntMeter ElamaLaskuri;

    public override void Begin()
    {
        LuoKentta();
        LuoVirus();
        PolkuaivoVirus();
        Timer ajastin = new Timer();
        ajastin.Interval = 1.5;
        ajastin.Timeout += PolkuaivoVirus;
        ajastin.Start();
        LuoPortti();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    public void LuoKentta()
    {
        Camera.ZoomToLevel();
        Image taustaKuva = LoadImage("taustakuva");
        Level.Background.Image = taustaKuva;

        PhysicsObject vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = false;

        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = false;

        PhysicsObject ylaReuna = Level.CreateTopBorder();
        ylaReuna.Restitution = 1.0;
        ylaReuna.IsVisible = false;

        PhysicsObject alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;
    }


    public void LuoVirus()
    {
        PhysicsObject virus = new PhysicsObject(2 * 10.0, 2 * 10.0, Shape.Circle);
        virus.X = 0.0;
        virus.Y = 0.0;
        virus.Color = Color.Red;
        virus.Restitution = 1.0;

        Add(virus);

        //Kentän yhden "ruudun" koko, käytä samaa lukua kuin jos kenttä luotu esimerkiksi ColorTileMapilla.
        const int RUUDUN_KOKO = 500;

        LabyrinthWandererBrain labyrinttiAivot = new LabyrinthWandererBrain(RUUDUN_KOKO);
        labyrinttiAivot.Speed = 100.0;
        labyrinttiAivot.LabyrinthWallTag = "seina";

        virus.Brain = labyrinttiAivot;
    }

    /// <summary>
    /// Testimielessä tehty objekti, jota vasten voidaan koittaa saada virus tuhoutumaan.
    /// </summary>
    public static PhysicsObject LuoPortti()
    {
        PhysicsObject portti = new PhysicsObject(500, 200, Shape.Rectangle);
        Image portinKuva = LoadImage("turret1");
        portti.Image = portinKuva;
        portti.X = 500;
        portti.Y = -250;

        return portti;
    }


    public void PolkuaivoVirus()
    {
        Virus virus2 = new Virus(2 * 22.0, 2 * 22.0, 5);
        virus2.X = -450.0;
        virus2.Y = 0.0;
        Image viruksenKuva = LoadImage("korona");
        virus2.Image = viruksenKuva;
        virus2.Restitution = 1.0;
        Add(virus2);

        Vector[] polku = {
        new Vector(-100, 0),
        new Vector(-100, 200),
        new Vector(100, 200),
        new Vector(100, -250),
        new Vector(500, -250),
        };

        PathFollowerBrain polkuAivot = new PathFollowerBrain();

        polkuAivot.Path = polku;

        polkuAivot.Loop = true;

        polkuAivot.Speed = 400;

        virus2.Brain = polkuAivot;

        AddCollisionHandler(virus2, VirusTormasi);

    }


    /// <summary>
    /// Pelin ensimmäinen taso.
    /// </summary>
    public static Vector[] Taso1()
    {
        Vector[] polku = {
        new Vector(-100, 0),
        new Vector(-100, 200),
        new Vector(100, 200),
        new Vector(100, -250),
        new Vector(500, -250),
        };

        return polku;

    }


    public void VirusTormasi(PhysicsObject virus, PhysicsObject kohde)
    {
        if (kohde == oikeaReuna) virus.Destroy();
    }


}

class Virus : PhysicsObject
{
    public int Elamat { get; set; }

    public Virus(double leveys, double korkeus, int elamia) // constructor eli muodostaja. luo virustyyppisen olion
        : base(leveys, korkeus)
    {
        Elamat = elamia;
    }
}