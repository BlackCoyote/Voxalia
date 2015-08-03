﻿using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.UISystem.MenuSystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public class MainMenuScreen: Screen
    {
        public UIMenu Menus;
        
        public Texture Topper;

        public override void Init()
        {
            Menus = new UIMenu(TheClient);
            Menus.Add(new UIMenuButton("ui/menus/buttons/basic", "Singleplayer", () => {
                TheClient.ShowSingleplayer();
            }, 10, 300, 350, 70, TheClient.FontSets.SlightlyBigger));
            Menus.Add(new UIMenuButton("ui/menus/buttons/basic", "Multiplayer", () => {
                UIConsole.WriteLine("MP!");
            }, 10, 400, 350, 70, TheClient.FontSets.SlightlyBigger));
            Topper = TheClient.Textures.GetTexture("ui/menus/voxalia_topper");
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
            Topper.Bind();
            TheClient.Rendering.RenderRectangle(0, 0, 600, 300);
            Menus.RenderAll(TheClient.gDelta);
        }
    }
}
