//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace RagdollDesigner
{
    public partial class RagdollDesignerForm : Form
    {
        public RagdollDesigner Designer;

        public GLControl GLCont;

        public Timer MainLoop;

        public RagdollDesignerForm(RagdollDesigner rd)
        {
            Designer = rd;
            InitializeComponent();
            MainLoop = new Timer();
            MainLoop.Interval = 16;
            MainLoop.Tick += MainLoop_Tick;
            Size size = panel1.Size;
            Point position = panel1.Location;
            panel1.Hide();
            GLCont = new GLControl(GraphicsMode.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            GLCont.Location = position;
            GLCont.Size = panel1.Size;
            GLCont.Load += GLCont_Load;
            Controls.Add(GLCont);
        }

        private void GLCont_Load(object sender, EventArgs e)
        {
            GL.Viewport(GLCont.DisplayRectangle);
            MainLoop.Start();
        }

        private void MainLoop_Tick(object sender, EventArgs e)
        {
            GLCont.MakeCurrent();
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1 });
            GLCont.SwapBuffers();
        }
    }
}
