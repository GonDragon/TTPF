using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using System;
using System.Text;

namespace TTPF
{
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawRightRect")]
    internal class MainTabWindow_Research_InjectPatchButton
    {
        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction prev = instructions.First();
            bool patchButton = false;
            int ldargUsed = 0;

            foreach (var code in instructions)
            {
                if (!patchButton)
                {
                    if (prev.opcode == OpCodes.Ldarg_1)
                    {
                        ldargUsed++;

                        if (ldargUsed == 3)
                        {
                            patchButton = true;

                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MainTabWindow_Research_InjectPatchButton), nameof(DoPatchButton)));
                            continue;
                        }
                    }
                    yield return code;
                    prev = code;
                }
                else
                {
                    yield return code;
                }
            }
        }
        private static void DoPatchButton(RectAggregator rightOutRect, MainTabWindow_Research researchWindow)
        {
            Rect rect = rightOutRect;
            rect.yMax = rect.yMin + 20f;
            Rect butRect = rect.RightPartPixels(30f);
            butRect.x -= 150f;
            butRect.width = 90f;
            if (Widgets.ButtonText(butRect, "Copy Patch", true, false, true))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                stringBuilder.AppendLine("<Patch>");

                Dictionary<string, List<ResearchProjectDef>> researchProjectsDict = new Dictionary<string, List<ResearchProjectDef>>();
                foreach (ResearchProjectDef researchProjectDef in researchWindow.VisibleResearchProjects.Where<ResearchProjectDef>((Func<ResearchProjectDef, bool>)(def => def.Debug_IsPositionModified())))
                {
                    string modName = researchProjectDef.modContentPack.Name;

                    if (researchProjectsDict.ContainsKey(modName))
                    {
                        researchProjectsDict[modName].Add(researchProjectDef);
                    }
                    else
                    {
                        researchProjectsDict.Add(modName, new List<ResearchProjectDef> { researchProjectDef });
                    }
                }

                foreach(String modname in researchProjectsDict.Keys)
                {
                    stringBuilder.AppendLine("  <Operation Class=\"PatchOperationFindMod\">");
                    stringBuilder.AppendLine("    <mods>");
                    stringBuilder.AppendLine(string.Format("      <li>{0}</li>", modname));
                    stringBuilder.AppendLine("    </mods>");
                    stringBuilder.AppendLine("    <match Class=\"PatchOperationSequence\">");
                    stringBuilder.AppendLine("      <success>Always</success>");
                    stringBuilder.AppendLine("      <operations>");

                    foreach (ResearchProjectDef researchProjectDef in researchProjectsDict[modname])
                    {
                        stringBuilder.AppendLine("        <li Class=\"VESSP.PatchOperationEditResearch\">");
                        stringBuilder.AppendLine("          <success>Always</success>");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine(string.Format("          <xpath>Defs/ResearchProjectDef[defName=\"{0}\"]</xpath>", researchProjectDef.defName));
                        stringBuilder.AppendLine(string.Format("          <researchViewX>{0:F2}</researchViewX>", (object)researchProjectDef.ResearchViewX));
                        stringBuilder.AppendLine(string.Format("          <researchViewY>{0:F2}</researchViewY>", (object)researchProjectDef.ResearchViewY));
                        stringBuilder.AppendLine(string.Format("          <tab>{0}</tab>", (object)researchProjectDef.tab));
                        stringBuilder.AppendLine("        </li>");
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendLine("      </operations>");
                    stringBuilder.AppendLine("    </match>");
                    stringBuilder.AppendLine("  </Operation>");
                }
                stringBuilder.AppendLine("</Patch>");
                GUIUtility.systemCopyBuffer = stringBuilder.ToString();
                TTPF.Message("Patched research tree data copied to clipboard.");
            }
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawProjectInfo")]
    internal class MainTabWindow_Research_InjectTabsFloatMenu
    {
        private static void Postfix(MainTabWindow_Research __instance, Rect rect, ref ResearchTabDef ___curTabInt, ref ResearchProjectDef ___selectedProject, ref bool ___editMode)
        {
            Rect buttonRect = new Rect(rect.x, rect.yMax - ((!ModsConfig.AnomalyActive || ___curTabInt != ResearchTabDefOf.Anomaly) ? 100f : 180f) - 30f, rect.width, 28f);
            ResearchProjectDef selectedProject = ___selectedProject;

            if (Prefs.DevMode && ___editMode)
            {
                Verse.Text.Font = GameFont.Tiny;
                if (Widgets.ButtonText(new Rect(rect.xMin, buttonRect.y, 120f, 25f), "Debug: Change Tab"))
                {
                    List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                    foreach (ResearchTabDef researchTabDef in DefDatabase<ResearchTabDef>.AllDefs)
                    {
                        floatMenuOptions.Add(new FloatMenuOption(researchTabDef.label, (Action)(() =>
                        {
                            selectedProject.tab = researchTabDef;

                        }), MenuOptionPriority.Default));

                        Find.WindowStack.Add((Window)new FloatMenu(floatMenuOptions));
                    }
                }
                Verse.Text.Font = GameFont.Small;
            }
        }

    }
}
