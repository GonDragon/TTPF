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

    
    internal class Patch_LoadedModManager_LoadAllMods
    {
        static void Postfix()
        {
            foreach (var customTab in TTPF_Mod.settings.customResearchTabs)
            {
                var researchDef = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(customTab.researchDefName);
                if (researchDef != null)
                {
                    researchDef.tab = DefDatabase<ResearchTabDef>.GetNamedSilentFail(customTab.researchTabDefName);
                    researchDef.researchViewX = customTab.researchViewX;
                    researchDef.researchViewY = customTab.researchViewY;
                }
            }
        }
    }
}
