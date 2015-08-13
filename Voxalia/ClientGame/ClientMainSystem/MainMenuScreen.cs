﻿using OpenTK.Graphics.OpenGL4;
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
            Menus.Add(new UITextLink("^%S^7ingleplayer", "^%S^e^7ingleplayer", "^7^e^%S^0ingleplayer", () => {
                TheClient.ShowSingleplayer();
            }, 10, 300, TheClient.FontSets.SlightlyBigger));
            Menus.Add(new UITextLink("^%M^7ultiplayer", "^%M^e^7ultiplayer", "^7^e^%M^0ultiplayer", () => {
                UIConsole.WriteLine("Multiplayer menu coming soon!");
            }, 10, 400, TheClient.FontSets.SlightlyBigger));
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
