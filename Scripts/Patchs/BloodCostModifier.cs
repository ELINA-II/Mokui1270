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
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat.History;
using Mokui1270.Scripts.Patchs;
using MegaCrit.Sts2.Core.Logging;
using Mokui1270.Scripts.Patches;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

#pragma warning disable CS8604, CS0649

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
        private static readonly Logger _logger = new Logger("BloodCost", LogType.GameSync);

        private static CombatHistory? GetHistory() => CombatManager.Instance?.History;

        private static ICombatState? GetCombatState(CardModel card)
        {
            return card.Owner?.Creature?.CombatState;
        }

        private static bool IsBloodCard(CardModel card)
        {
            if (card == null) return false;
            var prop = card.GetType().GetProperty("isBlood");
            if (prop == null) return false;
            return (bool)prop.GetValue(card)!;
        }

        // ✅ 检测是否为 Hack 卡牌（使用 RAM 系统）
        private static bool IsHackCard(CardModel card)
        {
            if (card == null) return false;
            var prop = card.GetType().GetProperty("isHack");
            if (prop == null) return false;
            return (bool)prop.GetValue(card)!;
        }

        private static bool HasUsedThisCombat(CardModel card, ICombatState combatState)
        {
            var history = GetHistory();
            if (history == null) return false;

            return history.Entries
                .OfType<BloodCostEntry>()
                .Any(e => e.Card == card && e.Phase == "Complete" && e.HappenedThisTurn(combatState));
        }

        private static bool HasBorrowedThisTurn(CardModel card, ICombatState combatState)
        {
            var history = GetHistory();
            if (history == null) return false;

            return history.Entries
                .OfType<BloodCostEntry>()
                .Any(e => e.Card == card && e.Phase == "Borrow" && e.HappenedThisTurn(combatState));
        }

        // ==================== Harmony Patches ====================

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCombatState), "HasEnoughResourcesFor")]
        public static bool HasEnoughResourcesForPrefix(
            PlayerCombatState __instance,
            CardModel card,
            ref UnplayableReason reason,
            ref bool __result)
        {
            _logger.Debug($"=== HasEnoughResourcesForPrefix: {card?.Id?.Entry}, IsBlood: {IsBloodCard(card)}");
            
            // ✅ Hack 卡牌由 RAM 系统处理，BloodCost 跳过
            if (IsHackCard(card)) return true;
            
            if (!IsBloodCard(card)) return true;

            var player = card.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return true;

            var combatState = GetCombatState(card);
            if (combatState == null) return true;

            if (HasUsedThisCombat(card, combatState)) return true;

            int originalCost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            int currentEnergy = __instance.Energy;
            int missingEnergy = originalCost - currentEnergy;
            
            _logger.Debug($"Energy: {currentEnergy}/{originalCost}, Missing: {missingEnergy}");

            if (missingEnergy <= 0) return true;

            int hpToLose = missingEnergy * 5;
            if (player.Creature.CurrentHp <= hpToLose) return true;

            if (HasBorrowedThisTurn(card, combatState))
            {
                _logger.Debug($"Already borrowed, allowing play");
                __result = true;
                reason = UnplayableReason.None;
                return false;
            }

            _logger.Debug($"Borrowing {missingEnergy} energy");
            __result = true;
            reason = UnplayableReason.None;

            _borrowedEnergy.Remove(card);
            _borrowedEnergy.Add(card, new BorrowedEnergyData { Value = missingEnergy });

            var history = GetHistory();
            if (history != null)
            {
                history.RecordBloodCost(
                    combatState,
                    player.Creature,
                    card,
                    missingEnergy,
                    0,
                    "Borrow"
                );
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static void SpendResourcesPrefix(CardModel __instance)
        {
            _logger.Debug($"=== SpendResourcesPrefix: {__instance?.Id?.Entry}");
            
            // ✅ Hack 卡牌由 RAM 系统处理，BloodCost 跳过
            if (IsHackCard(__instance)) return;
            
            if (!IsBloodCard(__instance)) return;

            var player = __instance.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return;

            var combatState = GetCombatState(__instance);
            if (combatState == null) return;

            if (HasUsedThisCombat(__instance, combatState)) return;

            if (!_borrowedEnergy.TryGetValue(__instance, out var borrowedData))
            {
                if (HasBorrowedThisTurn(__instance, combatState)) return;
                return;
            }

            int missingEnergy = borrowedData.Value;
            int currentEnergy = player.PlayerCombatState!.Energy;
            int originalCost = __instance.EnergyCost.GetWithModifiers(CostModifiers.All);

            if (currentEnergy < originalCost)
            {
                _logger.Debug($"Gaining {missingEnergy} energy");
                player.PlayerCombatState!.GainEnergy(missingEnergy);

                var history = GetHistory();
                if (history != null)
                {
                    history.RecordBloodCost(
                        combatState,
                        player.Creature,
                        __instance,
                        missingEnergy,
                        0,
                        "EnergyGained"
                    );
                }
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static void SpendResourcesFinalizer(CardModel __instance, Exception __exception)
        {
            _logger.Debug($"=== SpendResourcesFinalizer: {__instance?.Id?.Entry}, Exception: {__exception?.Message ?? "None"}");
            
            if (__exception != null) return;

            // ✅ Hack 卡牌由 RAM 系统处理，BloodCost 跳过
            if (IsHackCard(__instance)) return;

            if (!IsBloodCard(__instance)) return;

            var player = __instance.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return;

            if (!LocalContext.IsMe(player)) return;

            var combatState = GetCombatState(__instance);
            if (combatState == null) return;

            if (HasUsedThisCombat(__instance, combatState))
            {
                _borrowedEnergy.Remove(__instance);
                return;
            }

            if (!_borrowedEnergy.TryGetValue(__instance, out var borrowedData))
            {
                if (HasBorrowedThisTurn(__instance, combatState))
                {
                    _logger.Debug($"Has history borrow, marking used");
                    MarkUsed(__instance, combatState);
                }
                return;
            }

            _borrowedEnergy.Remove(__instance);
            int missingEnergy = borrowedData.Value;
            int damageAmount = missingEnergy * 5;

            if (player.Creature.CurrentHp <= damageAmount) return;

            _logger.Debug($"Paying {damageAmount} HP as blood cost");

            var history = GetHistory();
            if (history != null)
            {
                history.RecordBloodCost(
                    combatState,
                    player.Creature,
                    __instance,
                    missingEnergy,
                    damageAmount,
                    "HealthPaid"
                );
            }

            _ = ApplyBloodDamage(player, player.Creature, __instance, damageAmount, combatState!);
            MarkUsed(__instance, combatState);
        }

        private static void MarkUsed(CardModel card, ICombatState combatState)
        {
            var history = GetHistory();
            if (history == null || combatState == null) return;

            if (HasUsedThisCombat(card, combatState)) return;

            history.RecordBloodCost(
                combatState,
                card.Owner.Creature,
                card,
                0,
                0,
                "Complete"
            );
        }

        private static async Task ApplyBloodDamage(
            Player player,
            Creature creature,
            CardModel card,
            int damageAmount,
            ICombatState combatState)
        {
            try
            {
                ulong localPlayerId = LocalContext.NetId ?? player.NetId;
                var choiceContext = new HookPlayerChoiceContext(
                    card,
                    player.NetId,
                    combatState,
                    GameActionType.Combat
                );

                await CreatureCmd.Damage(
                    choiceContext,
                    creature,
                    damageAmount,
                    ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                    card
                );
            }
            catch
            {
                creature.SetCurrentHpInternal(creature.CurrentHp - damageAmount);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
        public static void OnAfterCombatEnd()
        {
            _borrowedEnergy.Clear();
        }
    }
}