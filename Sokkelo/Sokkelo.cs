using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Jesse Korolainen & Teemu Nieminen
/// @version 18.10.2020
/// @version 27.10.2020 Muokattu Taso1 vektorit. Arvon palautus ei toimi. Lisätty virukselle tuhoutumispiste oikeaan seinään ja testattu.
/// @version 19.11.2020 Torni ampuu nyt oikein. Poistettu tornin aivot ja ratkaistu ongelma poistamalla PhysicsObject ja laitettu tilalle AssaultRifle.
/// @version 20.11.2020 Tornin ennakointi ja OstaTykki-aliohjelma korjattu.
/// @version 9.12.2020 Koodi siistitty ja homma toimii.
/// <summary>
/// Peli on tornipuolustustyylinen peli, jossa pelaajan on parannettava jatkuvasti immuunipuolustustaan sanytolilla taistelussa viheliäistä koronavirusta vastaan.
/// Ohjelma piirtää tekstitiedostosta kartan, jota pitkin syntyvät virukset kulkevat kohti maalia. 
/// Maaliin päästessään virus tuhoutuu ja tekee vahinkoa pelaajaan. Peli päättyy, kun ihminen sairastuu koronaan eli ei ole enää elämiä jäljellä.
/// Viruksia tuhotaan asettamalla sanytol-tykkitorneja polun varrelle, jotka automaattisesti ampuvat viruksia.
/// Kun virus tuhoutuu, pelaaja saa yhden rahan lisää ja virus voimistuu sekä kooltaan että elämiltään. Viidellä rahalla voi ostaa uuden tykkitornin.
/// </summary>
public class Sokkelo : PhysicsGame
{
    private PhysicsObject oikeaReuna;
    private int aloitusRahat = 10;
    private IntMeter rahaLaskuri;
    const int ruudunLeveys = 50;
    const int ruudunKorkeus = 50;
    List<Koordinaatti> koordinaatit = new List<Koordinaatti>();
    private Vector alku;
    private List<Vector> polku = new List<Vector>();
    private List<Virus> virukset = new List<Virus>();
    private IntMeter tappoLaskuri;
    /// <summary>
    /// Pelaajan hp. Yksi virus tekee 1 vahinkoa osuessaan maaliin.
    /// </summary>
    private int pelaajanElama = 1;


    public override void Begin()
    {
        ClearAll();
        polku.Clear();
        koordinaatit.Clear();
        virukset.Clear();
        aloitusRahat = 10;
        pelaajanElama = 1;
        tappoLaskuri = null;
        rahaLaskuri = null;
        MultiSelectWindow valikko = new MultiSelectWindow("Tervetuloa peliin", "Aloita peli", "Lopeta");
        valikko.ItemSelected += PainettiinValikonNappia;
        Add(valikko);
        LuoKentta();
        LuoAivot();
        AsetaOhjaimet();
        Mouse.IsCursorVisible = true;
        LuoRahaLaskuri();
        LuoTappoLaskuri();
    }


    /// <summary>
    /// Luodaan alkuvalikko.
    /// </summary>
    /// <param name="valinta"></param>
    void PainettiinValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                Timer ajastin = new Timer();
                ajastin.Interval = 1.5;
                ajastin.Timeout += delegate { PolkuaivoVirus(); };
                ajastin.Start();
                break;
            case 1:
                Exit();
                break;
        }
    }


    /// <summary>
    /// Luodaan kenttä, jossa virukset kulkevat.
    /// </summary>
    public void LuoKentta()
    {
        TileMap ruudut = TileMap.FromLevelAsset("kentta1.txt");
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
    }


    /// <summary>
    /// Paikka, johon uusi virus syntyy.
    /// </summary>
    /// <param name="sijainti"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
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


    /// <summary>
    /// Tyhjä ruutu, johon voi luoda tykkitorneja.
    /// </summary>
    /// <param name="sijainti"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    void LuoRuutu(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject ruutu = PhysicsObject.CreateStaticObject(leveys, korkeus);
        ruutu.Position = sijainti;
        ruutu.Shape = Shape.Rectangle;
        ruutu.Tag = "tyhjä ruutu";
        ruutu.Color = Color.Transparent;
        Add(ruutu);
    }


    /// <summary>
    /// Maali, johon virus yrittää mennä.
    /// </summary>
    /// <param name="sijainti"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="jarjestysnumero"></param>
    void LuoMaali(Vector sijainti, double leveys, double korkeus, int jarjestysnumero)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        Koordinaatti koordinaatti = new Koordinaatti(jarjestysnumero, sijainti);
        koordinaatit.Add(koordinaatti);
        polku.Tag = "maali";
        Add(polku, -3);
    }


    /// <summary>
    /// Viruksen polulle koordinaatit.
    /// </summary>
    /// <param name="sijainti"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="jarjestysnumero"></param>
    /// <param name="tag"></param>
    void LuoKoordinaatti(Vector sijainti, double leveys, double korkeus, int jarjestysnumero, string tag)
    {
        PhysicsObject polku = PhysicsObject.CreateStaticObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        Koordinaatti koordinaatti = new Koordinaatti(jarjestysnumero, sijainti);
        koordinaatit.Add(koordinaatti);
        polku.Tag = tag;
        polku.CollisionIgnoreGroup = 1;
        Add(polku, -3);
    }


    /// <summary>
    /// Erilaiset grafiikat polulle. Ei vaikuta viruksen liikkeisiin, eikä tykkitornia voi asettaa polulle.
    /// </summary>
    /// <param name="sijainti"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    void LuoPolku(Vector sijainti, double leveys, double korkeus)
    {
        PhysicsObject polku = new PhysicsObject(leveys, korkeus);
        polku.Shape = Shape.Rectangle;
        polku.Position = sijainti;
        Image polunKuva = LoadImage("polku");
        polku.Image = polunKuva;
        polku.IgnoresCollisionResponse = true;
        Add(polku, -3);
    }


    /// <summary>
    /// Käy läpi koordinaattien järjestyksen.
    /// </summary>
    void LuoAivot()
    {
        koordinaatit.Sort(new Comparison<Koordinaatti>(VertaaKoordinaatteja));
        for (int i = 0; i < koordinaatit.Count; i++)
        {
            polku.Add(koordinaatit[i].sijainti);
        }
    }


    /// <summary>
    /// Varmistetaan, että polku toimii ja virus osaa mennä oikeaa koordinaattia kohti.
    /// </summary>
    /// <param name="k1"></param>
    /// <param name="k2"></param>
    /// <returns></returns>
    int VertaaKoordinaatteja(Koordinaatti k1, Koordinaatti k2)
    {
        if (k1.jarjestysnumero > k2.jarjestysnumero) return 1;
        if (k1.jarjestysnumero < k2.jarjestysnumero) return -1;
        return 0;
    }


    /// <summary>
    /// Luodaan rahalaskuri.
    /// </summary>
    public void LuoRahaLaskuri()
    {
        rahaLaskuri = new IntMeter(0);

        Label rahaNaytto = new Label();
        rahaNaytto.X = Screen.Left + 150;
        rahaNaytto.Y = Screen.Top - 25;
        rahaNaytto.TextColor = Color.White;
        rahaNaytto.Color = Color.Transparent;

        rahaNaytto.BindTo(rahaLaskuri);
        rahaLaskuri.Value = aloitusRahat;
        Add(rahaNaytto);

        Label rahat = new Label("Rahat");
        rahat.X = rahaNaytto.X;
        rahat.Y = rahaNaytto.Y - 25;
        rahat.TextColor = Color.White;
        rahat.Color = Color.Transparent;
        Add(rahat);
    }


    /// <summary>
    /// Luodaan tappolaskuri.
    /// </summary>
    public void LuoTappoLaskuri()
    {
        tappoLaskuri = new IntMeter(0);

        Label tappoNaytto = new Label();
        tappoNaytto.X = Screen.Right - 150;
        tappoNaytto.Y = Screen.Top - 25;
        tappoNaytto.TextColor = Color.White;
        tappoNaytto.Color = Color.Transparent;

        tappoNaytto.BindTo(tappoLaskuri);
        tappoLaskuri.Value = 0;
        Add(tappoNaytto);

        Label paivaLaskuri = new Label("Päivää ilman koronaa");
        paivaLaskuri.X = tappoNaytto.X;
        paivaLaskuri.Y = tappoNaytto.Y - 25;
        paivaLaskuri.TextColor = Color.White;
        paivaLaskuri.Color = Color.Transparent;
        Add(paivaLaskuri);
    }


    /// <summary>
    /// Annetaan pelaajalle ohjaimet.
    /// </summary>
    void AsetaOhjaimet()
    {
        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, OstaTykki, "Osta käsidesiä klikkaamalla tyhjää ruutua. Käsidesi maksaa 5 rahaa.");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
    }


    /// <summary>
    /// Annetaan pelaajalle mahdollisuus ostaa uusia tykkejä.
    /// </summary>
    public void OstaTykki() 
    {
        if (rahaLaskuri.Value < 5) return;
        Vector sijainti = Mouse.PositionOnWorld;
        foreach (GameObject olio in GetObjectsAt(sijainti, "tyhjä ruutu"))
        {
            AssaultRifle ase = new AssaultRifle(80, 40);
            LuoTykkitorni(ase, olio.Position);
            rahaLaskuri.AddValue(-5);
            olio.Tag = "käytetty ruutu";
        }
    }


    /// <summary>
    /// Luodaan virus ja sille aivot.
    /// </summary>
    /// <returns></returns>
    public PhysicsObject PolkuaivoVirus()
    {
        Virus virus = new Virus(ruudunKorkeus / 2 + tappoLaskuri.Value / 5, ruudunLeveys / 2 + tappoLaskuri.Value / 5, 3 + tappoLaskuri.Value / 5);
        virus.Position = alku;
        virus.CollisionIgnoreGroup = 1;
        virus.IgnoresCollisionResponse = true;
        Image viruksenKuva = LoadImage("korona");
        virus.Image = viruksenKuva;
        virus.Restitution = 1.0;
        virus.Tag = "virus";
        virukset.Add(virus);
        Add(virus);

        PathFollowerBrain polkuAivot = new PathFollowerBrain();
        polkuAivot.Path = polku;
        polkuAivot.Loop = false;
        polkuAivot.Speed = 150;
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
    public AssaultRifle LuoTykkitorni(AssaultRifle ase, Vector sijainti)
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 1.0;
        ajastin.Timeout += delegate { TorniAmpuu(virukset, ase); };
        ajastin.Start();
        ase.Position = sijainti;
        ase.ProjectileCollision = AmmusOsui;
        ase.InfiniteAmmo = true;
        ase.Power.DefaultValue = 100;
        ase.FireRate = 100.0;
        ase.AmmoIgnoresGravity = true;
        ase.CanHitOwner = false;
        ase.AmmoIgnoresExplosions = true;
        ase.AttackSound = null;
        Image torninKuva = LoadImage("turret1");
        ase.Image = torninKuva;
        Add(ase);
        return ase;
    }


   /// <summary>
   /// Torni ampuu viruksia. Kun virus pääsee maaliin, alkaa torni ampua jostain syystä joka toisella yli.
   /// </summary>
   /// <param name="kohteet">Mitä ammutaan</param>
   /// <param name="ase">Millä ammutaan</param>
    public void TorniAmpuu(List<Virus> kohteet, AssaultRifle ase)
    {
        if (kohteet.Count == 0) return;
        Virus kohde = HeikoinLenkki(kohteet, ase.Position);
        if (kohde == null) return;
        Vector suunta = (kohde.Position + (kohde.Velocity * (Vector.Distance(kohde.Position, ase.Position) / 500)) - ase.AbsolutePosition).Normalize();
        ase.Angle = suunta.Angle;
        PhysicsObject ammus = ase.Shoot();
        if (ammus != null)
            ammus.IgnoresCollisionResponse = true;
    }


    /// <summary>
    /// Tykki valitsee aina eniten osumaa saaneen viruksen, max range 300.
    /// </summary>
    /// <param name="kohteet"></param>
    /// <param name="sijainti"></param>
    /// <returns></returns>
    public Virus HeikoinLenkki(List<Virus> kohteet, Vector sijainti)
    {
        int heikoin = -1;
        for (int i = 0; i < kohteet.Count; i++)
            if ((heikoin == -1 || kohteet[i].Elamat <= kohteet[heikoin].Elamat) && Vector.Distance(sijainti, kohteet[i].Position) < 300)
                heikoin = i;
        if (heikoin == -1)
        { 
            return null;
        }
        return kohteet[heikoin];
    }


    /// <summary>
    /// Tuhotaan virukseltä elämä aina, kun ammus osuu siihen ja poistetaan ammus listasta. Mikäli virus tuhoutuu, lisätään rahalaskuriin rahaa.
    /// </summary>
    /// <param name="ammus"></param>
    /// <param name="kohde"></param>
    public void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.IgnoresCollisionResponse = true;
        if (kohde.Tag.ToString() == "virus") 
        {
            Virus virus = (Virus)kohde;
            virus.Elamat -= 1;
            ammus.Destroy();
            if (virus.Elamat <= 0)
            {
                rahaLaskuri.AddValue(1);
                kohde.Destroy();
                virukset.Remove((Virus)kohde);
                tappoLaskuri.Value++;
            }
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
        if (kohde.Tag.ToString() == "maali")
        {
            virus.Destroy();
            virukset.Remove((Virus)virus);
            pelaajanElama--;
            if (pelaajanElama == 0)
            {
                Label tekstikentta = new Label("Koronavirus! Karanteeniin siitä! Selvisit " + tappoLaskuri + " päivää ilman koronaa.");
                tekstikentta.Color = Color.Transparent;
                tekstikentta.TextColor = Color.Black;
                Add(tekstikentta);
                Explosion rajahdys = new Explosion(5000);
                SoundEffect rajahdysAani = LoadSoundEffect(RandomGen.SelectOne<string>("lepakkokeitto", "nuha", "koronaviirus"));
                rajahdys.Sound = rajahdysAani;
                Add(rajahdys);
                Timer.SingleShot(7.0, Begin);
            }
        }
    }
}


/// <summary>
/// Viruksen luokka.
/// </summary>
public class Virus : PhysicsObject
{
    /// <summary>
    /// Viruksen hp.
    /// </summary>
    public int Elamat { get; set; }

    public Virus(double leveys, double korkeus, int elamia)
        : base(leveys, korkeus)
    {
        Elamat = elamia;
    }
}


/// <summary>
/// Kentän koordinaatit.
/// </summary>
public class Koordinaatti
{
    /// <summary>
    /// Järjestysnumero kertoo virukselle, että mihin koordinaattiin sen pitää seuraavaksi mennä.
    /// </summary>
    public int jarjestysnumero;
    /// <summary>
    /// Missä virus on.
    /// </summary>
    public Vector sijainti;
    public Koordinaatti(int jarjestysnumero, Vector sijainti)
    {
        this.jarjestysnumero = jarjestysnumero;
        this.sijainti = sijainti;
    }
}