using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Mokui_1270.Patchs;

[HarmonyPatch]  // 类级别的特性，Harmony 会自动发现
public static class HealTrackerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCurrentHpChanged))]
    public static void OnAfterCurrentHpChanged(Creature creature, decimal delta)
    {
        HealTracker.RecordHeal(creature, delta);
    }
    
    // 可选：战斗结束时自动清除
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
    public static void AfterCombatEnd(IRunState runState, CombatState? combatState, CombatRoom room)
    {
        HealTracker.ClearAll();  // 需要添加 ClearAll 方法
    }
}