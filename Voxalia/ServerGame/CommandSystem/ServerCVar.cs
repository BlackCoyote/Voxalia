﻿using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.Shared.Files;
using Voxalia.Shared;

namespace Voxalia.ServerGame.CommandSystem
{
    /// <summary>
    /// Handles the serverside CVar system.
    /// </summary>
    public class ServerCVar
    {
        /// <summary>
        /// The CVar System the client will use.
        /// </summary>
        public CVarSystem system;

        // System CVars
        public CVar s_filepath;

        // Game CVars
        public CVar g_timescale, g_fps, g_maxheight, g_minheight, g_maxdist;

        /// <summary>
        /// Prepares the CVar system, generating default CVars.
        /// </summary>
        public void Init(Outputter output)
        {
            system = new CVarSystem(output);

            // System CVars
            s_filepath = Register("s_filepath", Program.Files.BaseDirectory, CVarFlag.Textual | CVarFlag.ReadOnly, "The current system environment filepath (The directory of /data).");
            // Game CVars
            g_timescale = Register("g_timescale", "1", CVarFlag.Numeric, "The current game time scaling value.");
            g_fps = Register("g_fps", "30", CVarFlag.Numeric, "What framerate to use.");
            g_maxheight = Register("g_maxheight", "5000", CVarFlag.Numeric, "What the highest possible Z coordinate should be (for building)."); // TODO: Also per-world?
            g_minheight = Register("g_minheight", "-5000", CVarFlag.Numeric, "What the lowest possible Z coordinate should be (for building)."); // TODO: Also per-world?
            g_maxdist = Register("g_maxdist", "50000", CVarFlag.Numeric, "How far on the X or Y axis a player may travel from the origin."); // TODO: Also per-world?
        }

        CVar Register(string name, string value, CVarFlag flags, string desc)
        {
            return system.Register(name, value, flags, desc);
        }
    }
}
