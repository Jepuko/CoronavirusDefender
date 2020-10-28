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
    // private Vector[] Taso1;
    AssaultRifle torninAse;

    public override void Begin()
    {
        LuoKentta();
        // LuoVirus(); PathWandererBrain
        PolkuaivoVirus();
        // LuoTykkitorni();
        Timer ajastin = new Timer();
        ajastin.Interval = 1.5;
        ajastin.Timeout += PolkuaivoVirus;
        ajastin.Start();
        
        // LuoTaso1();

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

    /*
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
    */


    /// <summary>
    /// Tykkitorni, joka tuhoaa viruksia. Aseena AssaultRifle, jossa loppumattomat ammukset.
    /// </summary>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="ase"></param>
    /// <returns></returns>
    public Torni LuoTykkitorni(double leveys, double korkeus, AssaultRifle ase)
    {

        Torni tykkitorni = new Torni(leveys, korkeus, ase);
        tykkitorni.Shape = Shape.Circle;
        tykkitorni.X = 0.0;
        tykkitorni.Y = 100.0;
        tykkitorni.Image = LoadImage("turret1");


        FollowerBrain torninAivot = new FollowerBrain(10, 10, Shape.Rectangle);
        torninAivot.Speed = 0;
        torninAivot.DistanceClose = 50;
        torninAivot.DistanceFar = 500;
        torninAivot.TargetClose += delegate { TorniAmpuu(virus, torninAse); };

        torninAse = new AssaultRifle(50, 100);
        torninAse.ProjectileCollision = AmmusOsui;
        torninAse.InfiniteAmmo = true;
        torninAse.Power.DefaultValue = 1;
        torninAse.FireRate = 5.0;
        torninAse.AmmoIgnoresGravity = true;
        torninAse.CanHitOwner = false;
        torninAse.AmmoIgnoresExplosions = true;
        Image torninKuva = LoadImage("turret1");
        torninAse.Image = torninKuva;
        
        tykkitorni.Add(torninAse);

        tykkitorni.Brain = torninAivot;

        Add(tykkitorni);

        return tykkitorni;
    }


    /// <summary>
    /// Mitä tapahtuu kun virus tulee tarpeeksi lähelle tornia.
    /// </summary>
    /// <param name="torni"></param>
    /// <param name="ase"></param>

    public void TykkitorninLahella(PhysicsObject tykkitorni, AssaultRifle torninAse)
    {
        tykkitorni.Angle = (virus.Position - tykkitorni.Position).Angle;
        TorniAmpuu(tykkitorni, torninAse);
    }

    public void TorniAmpuu(PhysicsObject tykkitorni, AssaultRifle ase)
    {
        PhysicsObject ammus = torninAse.Shoot();
    }


    public void PolkuaivoVirus()
    {
        Virus virus = new Virus(2 * 22.0, 2 * 22.0, 5);
        virus.X = -450.0;
        virus.Y = 0.0;
        Image viruksenKuva = LoadImage("korona");
        virus.Image = viruksenKuva;
        virus.Restitution = 1.0;
        Add(virus);

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

        polkuAivot.Speed = 100;

        virus.Brain = polkuAivot;

        AddCollisionHandler(virus, VirusTormasi);

        AddCollisionHandler(virus, AmmusOsui);

    }

    /*
    /// <summary>
    /// Pelin ensimmäinen taso.
    /// </summary>
    public void LuoTaso1()
    {
        Taso1 = new Vector[] polku;
        
        polku= {
        new Vector(-100, 0);
        new Vector(-100, 200);
        new Vector(100, 200);
        new Vector(100, -250);
        new Vector(500, -250);
        };

    }
    */

    public void VirusTormasi(PhysicsObject virus, PhysicsObject kohde)
    {
        if (kohde == oikeaReuna) virus.Destroy();
    }


    public void AmmusOsui(PhysicsObject virus, PhysicsObject ammus)
    {
        if (ammus == virus) virus.Destroy();
    }
}

public class Virus : PhysicsObject
{
    public int Elamat { get; set; }

    public Virus(double leveys, double korkeus, int elamia) // constructor eli muodostaja. luo virustyyppisen olion
        : base(leveys, korkeus)
    {
        Elamat = elamia;

    }
}


/// <summary>
/// Tykkitornin tiedot muille aliohjelmille.
/// </summary>
public class Torni : PhysicsObject
{
    public AssaultRifle torninAse;
    public Torni(double leveys, double korkeus, AssaultRifle ase)
        : base(leveys, korkeus)
    {
        torninAse = ase;
    }
}