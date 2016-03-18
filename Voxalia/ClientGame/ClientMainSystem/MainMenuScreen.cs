﻿using OpenTK.Graphics.OpenGL4;
using System.Linq;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class MainMenuScreen: Screen
    {
        public UIMenu Menus;
        
        public Texture Backg;

        public override void Init()
        {
            Menus = new UIMenu(TheClient);
            FontSet font = TheClient.FontSets.SlightlyBigger;
            UITextLink quit = new UITextLink(null, "^%Q^7uit", "^%Q^e^7uit", "^7^e^%Q^0uit", () => {
                TheClient.Window.Close();
            }, () => 600, () => 300, font);
            quit.XGet = () => TheClient.Window.Width - 100 - font.font_default.MeasureString("Singleplayer");
            quit.YGet = () => TheClient.Window.Height - 100 - quit.GetHeight();
            Menus.Add(quit);
            UITextLink sp = new UITextLink(null, "^%S^7ingleplayer", "^%S^e^7ingleplayer", "^7^e^%S^0ingleplayer", () => {
                TheClient.ShowSingleplayer();
            }, () => quit.GetX(), () => quit.GetY() - quit.GetHeight(), font);
            Menus.Add(sp);
            UITextLink mp = new UITextLink(null, "^%M^7ultiplayer", "^%M^e^7ultiplayer", "^7^e^%M^0ultiplayer", () => {
                UIConsole.WriteLine("Multiplayer menu coming soon!");
            }, () => quit.GetX(), () => sp.GetY() - sp.GetHeight(), font);
            Menus.Add(mp);
            Backg = TheClient.Textures.GetTexture("ui/menus/menuback");
        }

        public override void Tick()
        {
            Menus.TickAll();
        }

        public override void SwitchTo()
        {
            MouseHandler.ReleaseMouse();
        }

        public override void Render()
        {
            TheClient.Establish2D();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0.5f, 0.5f, 1 });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1 });
            Backg.Bind();
            TheClient.Rendering.RenderRectangle(0, 0, TheClient.Window.Width, TheClient.Window.Height);
            Menus.RenderAll(TheClient.gDelta);
        }
    }
}
