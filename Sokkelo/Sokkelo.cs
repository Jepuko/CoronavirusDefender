using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Jesse Korolainen & Teemu Nieminen
/// @version 18.10.2020
/// <summary>
/// Luodaan tietyt koordinaatit, mitä pitkin fysiikkaobjekti pääsee etenemään. 
/// </summary>
public class Sokkelo : PhysicsGame
{
    PhysicsObject vasenReuna;
    PhysicsObject oikeaReuna;
    PhysicsObject ylaReuna;
    PhysicsObject alaReuna;
    PhysicsObject virus;
    PhysicsObject virus2;
    IntMeter ElamaLaskuri;

    public override void Begin()
    {
        LuoKentta();
        LuoVirus();
        PolkuaivoVirus();
        LisaaOhjaimet();


        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    void LuoKentta()
    {
        Camera.ZoomToLevel();
        Image taustaKuva = LoadImage("taustakuva");
        Level.Background.Image = taustaKuva;

        vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.IsVisible = false;

        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.IsVisible = false;

        ylaReuna = Level.CreateTopBorder();
        ylaReuna.Restitution = 1.0;
        ylaReuna.IsVisible = false;

        alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;
    }
    void LuoVirus()
    {
        virus = new PhysicsObject(2 * 10.0, 2 * 10.0, Shape.Circle);
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
    void PolkuaivoVirus()
    {
        virus2 = new PhysicsObject(2 * 25.0, 2 * 25.0, Shape.Circle);
        virus2.X = -350.0;
        virus2.Y = -350.0;
        Image viruksenKuva = LoadImage("korona");
        virus2.Image = viruksenKuva;
        virus2.Restitution = 1.0;

        Add(virus2);

        PathFollowerBrain polkuAivot = new PathFollowerBrain();

        List<Vector> polku = new List<Vector>();

        polku.Add(new Vector(-350, -350));
        polku.Add(new Vector(0, 0));
        polku.Add(new Vector(350, 350));


        polkuAivot.Path = polku;

        polkuAivot.Loop = true;

        polkuAivot.Speed = 100;

        virus2.Brain = polkuAivot;
    }
    void LisaaOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Poistu");
        Keyboard.Listen(Key.F1, ButtonState.Pressed,
                        ShowControlHelp, "Näytä näppäinohjeet");

        Keyboard.Listen(Key.Up, ButtonState.Pressed, LiikutaVirusta, "Lyö virusta ylöspäin", virus, new Vector(0, 1000));
        Keyboard.Listen(Key.Right, ButtonState.Pressed, LiikutaVirusta, "Lyö virusta oikealle", virus, new Vector(1000, 0));
        Keyboard.Listen(Key.Left, ButtonState.Pressed, LiikutaVirusta, "Lyö virusta vasemmalle", virus, new Vector(-1000, 0));
        Keyboard.Listen(Key.Down, ButtonState.Pressed, LiikutaVirusta, "Lyö virusta alaspäin", virus, new Vector(0, -1000));
    }
    private void LiikutaVirusta(PhysicsObject virus, Vector suunta)
    {
        virus.Hit(suunta);
    }
}