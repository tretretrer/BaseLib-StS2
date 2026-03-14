using BaseLib.Abstracts;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Patches.Content;

public class CustomEnergyIconPatches {
    public const char Delimiter = '∴';
    public static string GetEnergyColorName(ModelId id) => id.Category + Delimiter + id.Entry;

    [HarmonyPatch(typeof(EnergyIconHelper), nameof(EnergyIconHelper.GetPath), typeof(string))]
    private static class IconPatch {
        static bool Prefix(string prefix, ref string __result) {
            int index = prefix.IndexOf(Delimiter);
            if (index < 0) return true;
            AbstractModel model = ModelDb.GetById<AbstractModel>(new(prefix[..index], prefix[(index+1)..]));
            if (model is not ICustomEnergyIconPool custom || custom.BigEnergyIconPath is not string path) return true;
            __result = path;
            return false;
        }
    }

    [HarmonyPatch(typeof(EnergyIconsFormatter), "TryEvaluateFormat")]
    private static class TextIconPatch {
        static List<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            return new InstructionPatcher(instructions)
                .Match(new InstructionMatcher()
                    .call(AccessTools.Method(typeof(string), nameof(string.Concat), [typeof(string), typeof(string), typeof(string)]))
                    .stloc_3()
                )
                .Insert([
                    CodeInstruction.LoadLocal(0),
                    CodeInstruction.LoadLocal(3),
                    CodeInstruction.Call(typeof(CustomEnergyIconPatches), nameof(GetTextIcon)),
                    CodeInstruction.StoreLocal(3),
                ]);
        }
    }

    static string GetTextIcon(string prefix, string oldText) {
        int index = prefix.IndexOf(Delimiter);
        if (index < 0) return oldText;
        AbstractModel model = ModelDb.GetById<AbstractModel>(new(prefix[..index], prefix[(index+1)..]));
        if (model is not ICustomEnergyIconPool custom || custom.TextEnergyIconPath is not string path) return oldText;
        return $"[img]{path}[/img]";
    }
}