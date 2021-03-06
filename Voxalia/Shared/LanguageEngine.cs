//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore.Files;
using FreneticDataSyntax;
using FreneticGameCore;

namespace Voxalia.Shared
{
    public class LanguageEngine
    {
        public Dictionary<string, FDSSection> EnglishDocuments = new Dictionary<string, FDSSection>();

        public Dictionary<string, FDSSection> LanguageDocuments = new Dictionary<string, FDSSection>();

        public string CurrentLanguage = "en_us";

        public void SetLanguage(string language)
        {
            CurrentLanguage = language.ToLowerFast();
            LanguageDocuments.Clear();
        }

        public FDSSection GetLangDoc(string id, FileHandler Files, string lang = null, Dictionary<string, FDSSection> confs = null)
        {
            if (lang == null)
            {
                lang = CurrentLanguage;
            }
            if (confs == null)
            {
                confs = LanguageDocuments;
            }
            string idlow = id.ToLowerFast();
            if (LanguageDocuments.TryGetValue(idlow, out FDSSection doc))
            {
                return doc;
            }
            string path = "info/text/" + idlow + "_" + lang + ".fds";
            if (Files.Exists(path))
            {
                try
                {
                    string dat = Files.ReadText(path);
                    doc = new FDSSection(dat);
                    LanguageDocuments[idlow] = doc;
                    return doc;
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    SysConsole.Output("Reading language documents", ex);
                }
            }
            LanguageDocuments[idlow] = null;
            return null;
        }

        const string badkey = "common.languages.badkey";

        public string Handle(string info, string[] pathAndVars)
        {
            info = info.Replace('\r', '\n').Replace("\n", "");
            for (int i = 2; i < pathAndVars.Length; i++)
            {
                info = info.Replace("{{" + (i - 1).ToString() + "}}", pathAndVars[i]);
            }
            return info;
        }

        public List<string> HandleList(List<string> infolist, string[] pathAndVars)
        {
            for (int i = 0; i < infolist.Count; i++)
            {
                infolist[i] = Handle(infolist[i], pathAndVars);
            }
            return infolist;
        }

        public List<string> GetTextList(FileHandler Files, params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetTextList(Files, "voxalia", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            FDSSection lang = GetLangDoc(category, Files);
            FDSSection langen = GetLangDoc(category, Files, "en_us", EnglishDocuments);
            List<string> str = null;
            if (lang != null)
            {
                str = lang.GetStringList(defPath);
                if (str != null)
                {
                    return HandleList(str, pathAndVars);
                }
            }
            if (langen != null)
            {
                str = langen.GetStringList(defPath);
                if (str != null)
                {
                    return HandleList(str, pathAndVars);
                }
            }
            if (defPath == badkey)
            {
                return new List<string>() { "((Invalid key!))" };
            }
            return GetTextList(Files, "voxalia", badkey);
        }

        public string GetText(FileHandler Files, params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetText(Files, "voxalia", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            FDSSection lang = GetLangDoc(category, Files);
            FDSSection langen = GetLangDoc(category, Files, "en_us", EnglishDocuments);
            string str = null;
            if (lang != null)
            {
                str = lang.GetString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (langen != null)
            {
                str = langen.GetString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (defPath == badkey)
            {
                return "((Invalid key!))";
            }
            return GetText(Files, "voxalia", badkey);
        }
    }
}
