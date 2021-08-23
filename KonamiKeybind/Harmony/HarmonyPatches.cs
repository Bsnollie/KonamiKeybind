using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using System.Reflection.Emit;
using Harmony;

namespace DuckGame.KonamiKeybind
{
    internal static class HarmonyPatches
    {
        [HarmonyPatch(typeof(UIControlConfig))]
        [HarmonyPatch(MethodType.Constructor)]
        internal static class UIControlConfigCtorPatch
        {
            internal static MethodBase TargetMethod()
            {
                return typeof(UIControlConfig).GetConstructor(new Type[] { typeof(UIMenu), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(string), typeof(InputProfile) });
            }

            // Add konami keybind to UIControlConfig menu
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                List<CodeInstruction> original = new List<CodeInstruction>(instructions);
                List<CodeInstruction> patch = new List<CodeInstruction>();

                FieldInfo konamiCEFld = AccessTools.Field(typeof(UIControlConfigCtorPatch), nameof(c_konamiControlElement));
                MethodInfo uiCompAdd = AccessTools.DeclaredMethod(typeof(UIComponent), nameof(UIComponent.Add));

                int index = -1;

                // Find where RAGDOLL keybind is added
                for (int i = 0; i < original.Count; i++)
                {
                    CodeInstruction instruction = original[i];

                    if (index == -1 && instruction.opcode == OpCodes.Ldstr && instruction.operand.Equals("RAGDOLL"))
                    {
                        index = -2;
                        continue;
                    }

                    if (index == -2 && instruction.opcode == OpCodes.Callvirt && instruction.operand.Equals(uiCompAdd))
                    {
                        index = i + 1;
                        break;
                    }
                }

                // Add KONAMI keybind and line skip
                patch.Add(new CodeInstruction(OpCodes.Ldloc_1));
                patch.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(UIControlConfigCtorPatch), nameof(c_lineSkip))));
                patch.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                patch.Add(new CodeInstruction(OpCodes.Callvirt, uiCompAdd));
                patch.Add(new CodeInstruction(OpCodes.Ldloc_1));
                patch.Add(new CodeInstruction(OpCodes.Ldsfld, konamiCEFld));
                patch.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                patch.Add(new CodeInstruction(OpCodes.Callvirt, uiCompAdd));
                patch.Add(new CodeInstruction(OpCodes.Ldarg_0));
                patch.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIControlConfig), "_controlElements")));
                patch.Add(new CodeInstruction(OpCodes.Ldsfld, konamiCEFld));
                patch.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.DeclaredMethod(typeof(List<UIControlElement>), nameof(List<UIControlElement>.Add))));

                if (index > -1)
                {
                    original.InsertRange(index, patch);
                }

                return original.AsEnumerable<CodeInstruction>();
            }

            internal static readonly UIControlElement c_konamiControlElement = new UIControlElement("|DGRED|KONAMI", "KONAMI", new DeviceInputMapping(), null, new FieldBinding(Options.Data, "sfxVolume", 0f, 1f, 0.1f), default(Color));

            internal static readonly UIComponent c_lineSkip = new UIText(" ", Color.White, UIAlign.Center, -6f, null);
        }

        [HarmonyPatch(typeof(Duck))]
        [HarmonyPatch(nameof(Duck.Update))]
        // Makes ducks pop on konami button
        internal static class DuckUpdatepatch
        {
            internal static void Postfix(Duck __instance)
            {
                if (!KonamiCooldownManager.CheckCooldown(__instance) && __instance.isServerForObject && __instance.inputProfile != null && __instance.inputProfile.Pressed("KONAMI", false))
                {
                    __instance.position = __instance.cameraPosition;

                    __instance.Presto();
                    KonamiCooldownManager.AddCooldown(__instance);
                }
            }
        }
    }
}
