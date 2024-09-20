using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using System;
using System.Text;
using System.Collections;
using Verse.Sound;

namespace TTPF
{
    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadAllActiveMods))]
    internal class Patch_LoadedModManager_LoadAllMods
    {
        internal static void Postfix()
        {
            TTPF.Warning("LoadedModManager.LoadAllActiveMods postfix");
            foreach (var customTab in TTPF_Mod.settings.customResearchTabs)
            {
#if DEBUG
                TTPF.Warning(string.Format("Loading user settings for {0}", customTab.researchDefName));
#endif
                var researchDef = DefDatabase<ResearchProjectDef>.GetNamed(customTab.researchDefName, false);
                if (researchDef != null)
                {
                    researchDef.tab = DefDatabase<ResearchTabDef>.GetNamed(customTab.researchTabDefName, false);
                    researchDef.researchViewX = customTab.researchViewX;
                    researchDef.researchViewY = customTab.researchViewY;
#if DEBUG
                    TTPF.Warning(string.Format("User settings loaded at X:{0} - Y:{1}", customTab.researchViewX, customTab.researchViewY));
#endif
                }
            }
        }
    }
}
