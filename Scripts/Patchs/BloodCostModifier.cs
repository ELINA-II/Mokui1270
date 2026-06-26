using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Mokui1270.BloodCostSystem
{
    [HarmonyPatch]
    public static class BloodCostPatches
    {
        private class BorrowedEnergyData
        {
            public int Value;
        }
        
        private static readonly ConditionalWeakTable<CardModel, BorrowedEnergyData> _borrowedEnergy = new();

        private static bool IsBloodCard(CardModel card)
        {
            if (card == null) return false;
            var prop = card.GetType().GetProperty("isBlood");
            if (prop == null) return false;
            return (bool)prop.GetValue(card)!;
        }

        private static bool HasUsedThisCombat(CardModel card, ICombatState combatState)
        {
            var field = card.GetType().GetField("_usedInCombat", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return false;
            return field.GetValue(card) == combatState;
        }

        private static void MarkUsed(CardModel card, ICombatState combatState)
        {
            var field = card.GetType().GetField("_usedInCombat", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(card, combatState);
            }
        }

        /// <summary>
        /// 欺骗能量检查，让血系卡牌在能量不足时仍然被认为可玩
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCombatState), "HasEnoughResourcesFor")]
        public static bool HasEnoughResourcesForPrefix(PlayerCombatState __instance, CardModel card, ref UnplayableReason reason, ref bool __result)
        {
            if (!IsBloodCard(card)) return true;

            var player = card.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return true;

            var combatState = player.Creature.CombatState;
            if (combatState == null) return true;

            if (HasUsedThisCombat(card, combatState)) return true;

            int originalCost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            int currentEnergy = __instance.Energy;
            int missingEnergy = originalCost - currentEnergy;

            if (missingEnergy <= 0) return true;

            int hpToLose = missingEnergy * 5;
            if (player.Creature.CurrentHp <= hpToLose) return true;

            __result = true;
            reason = UnplayableReason.None;
            
            _borrowedEnergy.Remove(card);
            _borrowedEnergy.Add(card, new BorrowedEnergyData { Value = missingEnergy });
            
            return false;
        }

        /// <summary>
        /// 临时增加能量，让原方法能够正常扣除
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static void SpendResourcesPrefix(CardModel __instance)
        {
            if (!IsBloodCard(__instance)) return;

            var player = __instance.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return;

            var combatState = player.Creature.CombatState;
            if (combatState == null) return;

            if (HasUsedThisCombat(__instance, combatState)) return;

            if (!_borrowedEnergy.TryGetValue(__instance, out var borrowedData))
                return;
            
            int missingEnergy = borrowedData.Value;
            player.PlayerCombatState!.GainEnergy(missingEnergy);
        }

        /// <summary>
        /// 使用 Finalizer 在 SpendResources 完成后造成伤害
        /// </summary>
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static void SpendResourcesFinalizer(CardModel __instance, Exception __exception)
        {
            // 如果原方法出错，跳过
            if (__exception != null) return;
            
            if (!IsBloodCard(__instance)) return;

            var player = __instance.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return;

            var combatState = player.Creature.CombatState;
            if (combatState == null) return;

            if (!_borrowedEnergy.TryGetValue(__instance, out var borrowedData))
                return;
            
            _borrowedEnergy.Remove(__instance);
            int missingEnergy = borrowedData.Value;

            int damageAmount = missingEnergy * 5;
            if (player.Creature.CurrentHp <= damageAmount) return;

            if (!HasUsedThisCombat(__instance, combatState))
            {
                MarkUsed(__instance, combatState);
            }
             _ = ApplyBloodDamage(player, player.Creature, __instance, damageAmount, combatState);
}
private static async Task ApplyBloodDamage(Player player, Creature creature, CardModel card, int damageAmount, ICombatState combatState)
{
    try
    {
        ulong localPlayerId = LocalContext.NetId ?? player.NetId;
        var choiceContext = new HookPlayerChoiceContext(card, localPlayerId, combatState, GameActionType.Combat);
        
        await CreatureCmd.Damage(choiceContext, creature, damageAmount, 
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
    }
    catch
    {
        creature.SetCurrentHpInternal(creature.CurrentHp - damageAmount);
    }
    }
    }
}