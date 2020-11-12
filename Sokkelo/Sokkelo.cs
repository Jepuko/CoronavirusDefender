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
    private IntMeter ElamaLaskuri;
    // private Vector[] Taso1;
    private AssaultRifle torninAse;
    private int aloitusRahat = 5;
    private IntMeter rahaLaskuri;
    const int ruudunLeveys = 50;
    const int ruudunKorkeus = 50;
    List<Koordinaatti> koordinaatit = new List<Koordinaatti>();
    private Vector alku;
    private List<Vector> polku = new List<Vector>();

    public override void Begin()
    {
        LuoKentta();
        LuoAivot();
        // LuoVirus(); PathWandererBrain
        LuoTykkitorni(30, 30, torninAse, new Vector(0, 100));
        Timer ajastin = new Timer();
        ajastin.Interval = 1.5;
        ajastin.Timeout += delegate { PolkuaivoVirus(); };
        ajastin.Start();
        // OstaTykki(new Vector(OstaTykki)); 
        Mouse.IsCursorVisible = true;
        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, OstaTykki, "Osta Tykki");
        // LuoTaso1();
        LuoRahaLaskuri();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    public void LuoKentta()
    {
        TileMap ruudut = TileMap.FromLevelAsset("kentta1.txt");
        // ruudut.SetTileMethod('=', LuoYlareuna);
        // ruudut.SetTileMethod('a', LuoAlareuna);
        // ruudut.SetTileMethod('o', LuoOikeareuna);
        // ruudut.SetTileMethod('v', LuoVasenreuna);
        // ruudut.SetTileMethod('>', LuoOikea);
        // ruudut.SetTileMethod('V', LuoAlas);
        // ruudut.SetTileMethod('^', LuoYlos);
        ruudut.SetTileMethod('A', LuoAlku);
        ruudut.SetTileMethod('#', LuoPolku);
        ruudut.SetTileMethod('.', LuoRuutu);
        ruudut.SetTileMethod('M', LuoMaali, 9);
        ruudut.SetTileMethod('B', LuoKoordinaatti, 1, "B");
        ruudut.SetTileMethod('C', LuoKoordinaatti, 2, "C");
        ruudut.SetTileMethod('D', LuoKoordinaatti, 3, "D");
        ruudut.SetTileMethod('E', LuoKoordinaatti, 4, "E");
        ruudut.SetTileMethod('F', LuoKoordinaatti, 5, "F");
        ruudut.SetTileMethod('G', LuoKoordinaatti, 6, "G");
        ruudut.SetTileMethod('H', LuoKoordinaatti, 7, "H");
        ruudut.SetTileMethod('I', LuoKoordinaatti, 8, "I");

        ruudut.Execute(ruudunLeveys, ruudunKorkeus);

        Camera.ZoomToLevel();
        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Tag = "maali";
        Image taustaKuva = LoadImage("taustakuva");
        Level.Background.Image = taustaKuva;

        // muuta lista taulukoksi -> taulukko PolkuAivovirukselle
    }


    void LuoAlku(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject ruutu = PhysicsObject.CreateStaticObject(leveys, korkeus);
        ruutu.Position = sijainti;
        ruutu.Shape = Shape.Rectangle;
        ruutu.Tag = "alku";
        ruutu.Color = Color.Transparent;
        alku = sijainti;
        Add(ruutu);
    }


    void LuoRuutu(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject ruutu = PhysicsObject.CreateStaticObject(leveys, korkeus);
        ruutu.Position = sijainti;
        ruutu.Shape = Shape.Rectangle;
        ruutu.Tag = "tyhjä ruutu";
        ruutu.Color = Color.Transparent;
        Add(ruutu);
    }


    void LuoMaali(Vector sijainti, double leveys, double korkeus, int jarjestysnumero)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        // polku.IgnoresCollisionResponse = true;
        // Vector korjattuSijainti = new Vector(leveys, korkeus - 200);
        // koordinaatit.Add(sijainti);
        Koordinaatti koordinaatti = new Koordinaatti(jarjestysnumero, sijainti);
        koordinaatit.Add(koordinaatti);
        polku.Tag = "maali";
        Add(polku, -3);
    }


    void LuoKoordinaatti(Vector sijainti, double leveys, double korkeus, int jarjestysnumero, string tag)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        // polku.IgnoresCollisionResponse = true;
        // Vector korjattuSijainti = new Vector(leveys, korkeus - 200);
        // koordinaatit.Add(sijainti);
        Koordinaatti koordinaatti = new Koordinaatti(jarjestysnumero, sijainti);
        koordinaatit.Add(koordinaatti);
        polku.Tag = tag;
        polku.CollisionIgnoreGroup = 1;
        Add(polku, -3);
    }

    /*
    void LuoOikea(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        // polku.IgnoresCollisionResponse = true;
        // Vector korjattuSijainti = new Vector(leveys, korkeus - 200);
        koordinaatit.Add(sijainti);
        polku.Tag = ">";
        polku.CollisionIgnoreGroup = 1;
        Add(polku, -3);
    }

    void LuoAlas(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        // polku.IgnoresCollisionResponse = true;
        // Vector korjattuSijainti = new Vector(leveys, korkeus - 200);
        koordinaatit.Add(sijainti);
        polku.Tag = "V";
        polku.CollisionIgnoreGroup = 1;
        Add(polku, -3);
    }

    void LuoYlos(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        // polku.IgnoresCollisionResponse = true;
        // Vector korjattuSijainti = new Vector(leveys, korkeus - 200);
        koordinaatit.Add(sijainti);
        polku.Tag = "^";
        polku.CollisionIgnoreGroup = 1;
        Add(polku, -3);
    }
    */
 
    void LuoPolku(Vector sijainti, double leveys, double korkeus)
    {
        // PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        GameObject polku = new GameObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        // polku.IgnoresCollisionResponse = true;
        // Vector korjattuSijainti = new Vector(leveys, korkeus - 200);
        // koordinaatit.Add(sijainti);
        Add(polku, -3);
    }
    /// <summary>
    /// Järjestetään koordinaatit suuruusjärjestykseen ja luodaan näillä aivoille polku.
    /// </summary>
    void LuoAivot()
    {
        koordinaatit.Sort(new Comparison<Koordinaatti>(VertaaKoordinaatteja));
        for (int i = 0; i < koordinaatit.Count; i++)
        {
            polku.Add(koordinaatit[i].sijainti);
        }
    }

    int VertaaKoordinaatteja(Koordinaatti k1, Koordinaatti k2)
    {
        if (k1.jarjestysnumero > k2.jarjestysnumero) return 1;
        if (k1.jarjestysnumero < k2.jarjestysnumero) return -1;
        return 0;
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

    public void LuoRahaLaskuri()

    {
        rahaLaskuri = new IntMeter(0);

        Label rahaNaytto = new Label();
        rahaNaytto.X = Screen.Left + 100;
        rahaNaytto.Y = Screen.Top - 100;
        rahaNaytto.TextColor = Color.Black;
        rahaNaytto.Color = Color.White;

        rahaNaytto.BindTo(rahaLaskuri);
        rahaLaskuri.Value = aloitusRahat;
        Add(rahaNaytto);
    }


    public void OstaTykki() // korjaa tykkien ostaminen
    {
        if (rahaLaskuri.Value < 5) return;
        Vector sijainti = Mouse.PositionOnWorld;
        foreach (GameObject olio in GetObjectsAt(sijainti))
        {
            if (olio.Tag.ToString() != "tyhjä ruutu") return; // declare "tyhjä ruutu" myöhemmin, kun luodaan kenttä

            AssaultRifle ase = new AssaultRifle(5, 5);
            LuoTykkitorni(30, 30, ase, olio.Position);
            rahaLaskuri.AddValue(-5);
            olio.Tag = "käytetty ruutu";
        }
    }


    public PhysicsObject PolkuaivoVirus()
    {
        Virus virus = new Virus(ruudunKorkeus / 2, ruudunLeveys / 2, 5);
        virus.Position = alku;
        virus.CollisionIgnoreGroup = 1;
        virus.IgnoresCollisionResponse = true;
        Image viruksenKuva = LoadImage("korona");
        virus.Image = viruksenKuva;
        virus.Restitution = 1.0;
        virus.Tag = "virus"; // Virukselle luodaan tag.
        Add(virus);

        PathFollowerBrain polkuAivot = new PathFollowerBrain();

        polkuAivot.Path = polku;

        polkuAivot.Loop = false;

        polkuAivot.Speed = 100;

        virus.Brain = polkuAivot;

        polkuAivot.Active = true;

        AddCollisionHandler(virus, VirusTormasi);

        return virus;
    }


    /// <summary>
    /// Tykkitorni, joka tuhoaa viruksia. Aseena AssaultRifle, jossa loppumattomat ammukset.
    /// </summary>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="ase"></param>
    /// <returns></returns>
    public Torni LuoTykkitorni(double leveys, double korkeus, AssaultRifle ase, Vector sijainti)
    {

        Torni tykkitorni = new Torni(leveys, korkeus, ase);
        tykkitorni.Shape = Shape.Circle;
        tykkitorni.Position = sijainti;
        tykkitorni.Image = LoadImage("turret1");

        FollowerBrain torninAivot = new FollowerBrain(PolkuaivoVirus()); // korjaa, että tykki ampuu oikeaan osoitteeseen ja kääntyy virusta kohti
        torninAivot.Speed = 0;
        torninAivot.DistanceClose = 500;
        torninAivot.DistanceFar = 1000;
        torninAivot.TargetClose += delegate { TorniAmpuu(torninAivot.CurrentTarget, ase); };
        // torninAivot.DistanceToTarget.AddTrigger(500, TriggerDirection.Down, VirusTormasi);
        // IGameObject kohde = CurrentTarget.Tag.ToString("virus");


        ase = new AssaultRifle(50, 100);
        ase.ProjectileCollision = AmmusOsui;
        ase.InfiniteAmmo = true;
        ase.Power.DefaultValue = 1;
        ase.FireRate = 5.0;
        ase.AmmoIgnoresGravity = true;
        ase.CanHitOwner = false;
        ase.AmmoIgnoresExplosions = true;
        Image torninKuva = LoadImage("turret1");
        ase.Image = torninKuva;

        tykkitorni.Add(ase);

        tykkitorni.Brain = torninAivot;

        Add(tykkitorni);

        return tykkitorni;
    }


    /// <summary>
    /// Mitä tapahtuu kun virus tulee tarpeeksi lähelle tornia.
    /// </summary>
    /// <param name="torni"></param>
    /// <param name="ase"></param>


    public void TorniAmpuu(IGameObject kohde, AssaultRifle ase)
    {
        // torninAse.Angle = (virus.Position - torninAse.Position).Angle;
        PhysicsObject ammus = ase.Shoot();
    }


    public void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        if (kohde.Tag.ToString() == "virus") 
        {
            kohde.Destroy(); 
            rahaLaskuri.AddValue (1);
        }
    }


    /// <summary>
    /// Ohjelma tuhoaa viruksen, kun se osuu seinään.
    /// </summary>
    /// <param name="virus"></param>
    /// <param name="kohde"></param>
    /// 

    public void VirusTormasi(PhysicsObject virus, PhysicsObject kohde)
    {
        /*
        if (kohde.Tag.ToString() == "alku")
        {
            virus.StopMoveTo();
            virus.MoveTo(kohde.Position + new Vector(ruudunLeveys, 0), 100, null);
        }

        if (kohde.Tag.ToString() == "V")
        {
            virus.StopMoveTo();
            virus.MoveTo(kohde.Position + new Vector(0, -ruudunKorkeus), 100, null);
        }

        if (kohde.Tag.ToString() == "^")
        {
            virus.StopMoveTo();
            virus.MoveTo(kohde.Position + new Vector(0, ruudunKorkeus), 100, null);
        }

        if (kohde.Tag.ToString() == ">")
        {
            virus.StopMoveTo();
            virus.MoveTo(kohde.Position + new Vector(ruudunLeveys, 0), 100, null);
        }
        */
        if (kohde.Tag.ToString() == "maali") virus.Destroy();
       // pelaaja.elamaLaskuri =- 1;

       // if (kohde == ) virus
    }
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


public class Pelaaja : PhysicsObject
{
    private IntMeter elamaLaskuri = new IntMeter(3, 0, 3);
    public IntMeter ElamaLaskuri { get { return elamaLaskuri; } }

    public Pelaaja(double leveys, double korkeus)
        : base(leveys, korkeus)
    {
        elamaLaskuri.LowerLimit += delegate { this.Destroy(); };
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
/// <summary>
/// Luodaan koordinaatti-luokka.
/// </summary>
public class Koordinaatti
{
    public int jarjestysnumero;
    public Vector sijainti;
    public Koordinaatti(int jarjestysnumero, Vector sijainti)
    {
        this.jarjestysnumero = jarjestysnumero;
        this.sijainti = sijainti;
    }
}