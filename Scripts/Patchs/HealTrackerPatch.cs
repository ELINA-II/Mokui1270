using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
namespace Mokui1270.Scripts.Patchs
{
    [HarmonyPatch]
    public static class HealTrackerPatch
    {

        /// <summary>
        /// 在血量变化后检测是否为治疗，并记录到 CombatHistory
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCurrentHpChanged))]
        public static void OnAfterCurrentHpChanged(Creature creature, decimal delta)
        {
             if (delta <= 0 || !creature.IsPlayer)
        return;

        // ✅ 直接用 CombatManager 的 History
        var history = CombatManager.Instance.History;
        if (history == null) return;

        var combatState = creature.CombatState;
        if (combatState == null) return;

        history.RecordHealthRestored(
        combatState,
        creature,
        null,
        (int)delta,
        null
        );
        }
    }
}