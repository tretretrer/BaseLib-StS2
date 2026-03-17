using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Patches.Features;

/// <summary>
/// If player apply debuff to self or ally before turn ends (not from curse cards like doubt),
/// the debuff should tick it's duration at enemy's turn ends
/// </summary>
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), [typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
public static class SelfApplyDebuffPatch
{
    [HarmonyPostfix]
    static void Postfix(ref Task __result, PowerModel power, Creature target)
    {
        // Replace the original Task with our wrapped Task
        __result = WrappedApplyTask(__result, power, target);
    }

    static async Task WrappedApplyTask(Task originalTask, PowerModel power, Creature target)
    {
        // First, wait for all await methods in the original function (including Hook.AfterPowerAmountChanged, etc.) to complete execution.
        await originalTask;

        // At this point, all the logic in the original function has been executed, including the part that sets SkipNextDurationTick to true.
        if (target.Side == CombatSide.Player && power.Type == PowerType.Debuff && power.Applier?.Side == CombatSide.Player)
        {
            // Ensure player-applied debuffs on self/allies tick at current (enemy's) turn end
            power.SkipNextDurationTick = false;
        }
    }
}