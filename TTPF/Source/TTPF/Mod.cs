using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace TTPF
{
    [StaticConstructorOnStartup]
    public static class TTPF
    {
        public static string Author => "GonDragon";
        public static string Name => Assembly.GetName().Name;
        public static string Id => Author + "." + Name;

        public static string Version => Assembly.GetName().Version.ToString();

        private static Assembly Assembly
        {
            get
            {
                return Assembly.GetAssembly(typeof(TTPF));
            }
        }

        public static readonly Harmony Harmony;

        static TTPF()
        {
            Harmony = new Harmony(Id);
            Harmony.PatchAll();

            foreach (var customTab in TTPF_Mod.settings.customResearchTabs)
            {
#if DEBUG
                TTPF.Warning(string.Format("Loading Custom {0}", customTab.researchDefName));
#endif
                var researchDef = DefDatabase<ResearchProjectDef>.GetNamed(customTab.researchDefName, false);
                if (researchDef != null)
                {
                    researchDef.tab = DefDatabase<ResearchTabDef>.GetNamed(customTab.researchTabDefName, false);
                    researchDef.researchViewX = customTab.researchViewX;
                    researchDef.researchViewY = customTab.researchViewY;

                    Traverse.Create(researchDef).Field("x").SetValue(customTab.researchViewX);
                    Traverse.Create(researchDef).Field("y").SetValue(customTab.researchViewY);
#if DEBUG
                    TTPF.Warning(string.Format("Set at X:{0} - Y:{1}", customTab.researchViewX, customTab.researchViewY));
                    TTPF.Warning(string.Format("It is at X:{0} - Y:{1}", researchDef.ResearchViewX, researchDef.ResearchViewY));
#endif
                }
            }

            //Remove redundant prerequisites
            foreach (ResearchProjectDef researchProject in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (researchProject.prerequisites == null) continue;
                foreach(ResearchProjectDef prerequisite in researchProject.prerequisites)
                {
                    if (TTPF.IsRedundant(researchProject, prerequisite))
                    {
                        researchProject.prerequisites.Remove(prerequisite);
                    }
                }
            }

        }

        private static bool IsRedundant(ResearchProjectDef proyect, ResearchProjectDef other)
        {
            List<ResearchProjectDef> checked_proyects = new List<ResearchProjectDef>();
            List<ResearchProjectDef> tocheck_proyects = new List<ResearchProjectDef>();

            tocheck_proyects.AddRange(proyect.prerequisites);

            if(tocheck_proyects.Contains(other)) tocheck_proyects.Remove(other);

            while (tocheck_proyects.Count > 0)
            {
                ResearchProjectDef tocheck = tocheck_proyects.Pop();

                if (tocheck.prerequisites == null) continue;
                foreach (ResearchProjectDef prerequisite in tocheck.prerequisites)
                {
                    if (prerequisite == other) return true;

                    if (checked_proyects.Contains(prerequisite)) continue;

                    checked_proyects.Add(prerequisite);
                    if (prerequisite.prerequisites == null) continue;
                    tocheck_proyects.AddRange(prerequisite.prerequisites);
                }
            }

            return false;
        }

        public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));

        public static void Warning(string message) => Verse.Log.Warning(PrefixMessage(message));

        public static void Error(string message) => Verse.Log.Error(PrefixMessage(message));

        public static void ErrorOnce(string message, string key) => Verse.Log.ErrorOnce(PrefixMessage(message), key.GetHashCode());

        public static void Message(string message) => Messages.Message(message, MessageTypeDefOf.TaskCompletion, false);

        private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";

    }
}
