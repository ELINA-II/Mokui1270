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

        private static bool IsHackCard(CardModel card)
        {
            if (card == null) return false;
            var prop = card.GetType().GetProperty("isHack");
            if (prop == null) return false;
            return (bool)prop.GetValue(card)!;
        }

        // ==================== Harmony Patches ====================

        /// <summary>
        /// 在 CanPlay 阶段检查血战卡牌是否有足够的能量和血量
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCombatState), "HasEnoughResourcesFor")]
        public static bool HasEnoughResourcesForPrefix(
            PlayerCombatState __instance,
            CardModel card,
            ref UnplayableReason reason,
            ref bool __result)
        {
            _logger.Info($"=== HasEnoughResourcesForPrefix: {card?.Id?.Entry}, IsBlood: {IsBloodCard(card)}");

            // Hack 卡牌由 RAM 系统处理
            if (IsHackCard(card)) return true;

            // 非血战卡牌走原逻辑
            if (!IsBloodCard(card)) return true;

            var player = card.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return true;

            var combatState = GetCombatState(card);
            if (combatState == null) return true;

            // 使用 GetAmountToSpend() 和 SpendResources 保持一致
            int requiredEnergy = card.EnergyCost.GetAmountToSpend();
            int currentEnergy = __instance.Energy;
            int missingEnergy = requiredEnergy - currentEnergy;

            _logger.Info($"=== BloodCost Debug: {card.Id.Entry} ===");
            _logger.Info($"GetAmountToSpend(): {requiredEnergy}");
            _logger.Info($"GetWithModifiers(All): {card.EnergyCost.GetWithModifiers(CostModifiers.All)}");
            _logger.Info($"Current Energy: {currentEnergy}");
            _logger.Info($"Missing: {missingEnergy}");
            _logger.Info($"Card Pile: {card.Pile?.Type}");

            // ✅ 如果能量足够，清除残留数据，不扣血
            if (missingEnergy <= 0)
            {
                _borrowedEnergy.Remove(card);
                _logger.Info($"BloodCost: {card.Id.Entry} has enough energy, cleared borrowed data");
                return true;
            }

            // 计算需要扣除的血量：每缺1能量扣5血
            int hpToLose = missingEnergy * 5;

            // 血量不足，卡牌无法打出
            if (player.Creature.CurrentHp <= hpToLose)
            {
                _logger.Info($"Not enough HP: {player.Creature.CurrentHp} <= {hpToLose}");
                __result = false;
                reason = UnplayableReason.BlockedByCardLogic;
                return false;
            }

            // 血量充足，允许打出，记录借用信息
            _logger.Info($"Borrowing {missingEnergy} energy, will lose {hpToLose} HP");
            __result = true;
            reason = UnplayableReason.None;

            // ✅ 所有玩家都记录借用信息（ConditionalWeakTable 是每个进程独立的）
            _borrowedEnergy.Remove(card);
            _borrowedEnergy.Add(card, new BorrowedEnergyData { Value = missingEnergy });

            // ✅ 只有本地玩家记录历史（避免重复记录）
            if (LocalContext.IsMe(player))
            {
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
            }

            return false;
        }

        /// <summary>
        /// 在 SpendResources 之前补充缺失的能量
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static void SpendResourcesPrefix(CardModel __instance)
        {
            _logger.Info($"=== SpendResourcesPrefix: {__instance?.Id?.Entry}");

            if (IsHackCard(__instance)) return;
            if (!IsBloodCard(__instance)) return;

            var player = __instance.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return;

            var combatState = GetCombatState(__instance);
            if (combatState == null) return;

            if (!_borrowedEnergy.TryGetValue(__instance, out var borrowedData))
            {
                return;
            }

            int missingEnergy = borrowedData.Value;
            int requiredEnergy = __instance.EnergyCost.GetAmountToSpend();
            int currentEnergy = player.PlayerCombatState!.Energy;

            // 如果当前能量仍然不足，补能
            if (currentEnergy < requiredEnergy)
            {
                _logger.Info($"Gaining {missingEnergy} energy");
                player.PlayerCombatState!.GainEnergy(missingEnergy);

                // 只有本地玩家记录历史
                if (LocalContext.IsMe(player))
                {
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
        }

        /// <summary>
        /// 在 SpendResources 之后执行扣血
        /// 关键修复：使用 CreatureCmd.Damage 进行网络同步扣血
        /// </summary>
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static async void SpendResourcesFinalizer(CardModel __instance, Exception __exception)
        {
            _logger.Info($"=== SpendResourcesFinalizer: {__instance?.Id?.Entry}, Exception: {__exception?.Message ?? "None"}");

            if (__exception != null) return;
            if (IsHackCard(__instance)) return;
            if (!IsBloodCard(__instance)) return;

            var player = __instance.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive) return;

            // ⚠️ 关键修复：移除 LocalContext.IsMe 限制
            // 所有玩家都需要执行扣血，以保持同步
            // if (!LocalContext.IsMe(player)) return;

            // 如果 _borrowedEnergy 中没有记录，说明能量足够，不扣血
            if (!_borrowedEnergy.TryGetValue(__instance, out var borrowedData))
            {
                _logger.Info($"BloodCost: {__instance.Id.Entry} no borrowed data, skipping");
                return;
            }

            int missingEnergy = borrowedData.Value;

            // 如果 missingEnergy <= 0，清除记录并跳过
            if (missingEnergy <= 0)
            {
                _borrowedEnergy.Remove(__instance);
                _logger.Info($"BloodCost: {__instance.Id.Entry} borrowed energy <= 0, skipping");
                return;
            }

            _borrowedEnergy.Remove(__instance);

            int damageAmount = missingEnergy * 5;

            if (player.Creature.CurrentHp <= damageAmount)
            {
                _logger.Info($"BloodCost: {__instance.Id.Entry} not enough HP: {player.Creature.CurrentHp} <= {damageAmount}");
                return;
            }

            _logger.Info($"BloodCost: {__instance.Id.Entry} - Paying {damageAmount} HP (missing {missingEnergy} energy)");

            var combatState = GetCombatState(__instance);
            if (combatState == null) return;

            // 只有本地玩家记录历史
            if (LocalContext.IsMe(player))
            {
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
            }

            // ✅ 关键修复：使用 CreatureCmd.Damage 进行网络同步扣血
            await ApplyBloodDamageWithSync(player, player.Creature, __instance, damageAmount, combatState);
        }

        /// <summary>
        /// 应用血战伤害 - 使用 CreatureCmd.Damage 确保网络同步
        /// </summary>
        private static async Task ApplyBloodDamageWithSync(
            Player player,
            Creature creature,
            CardModel card,
            int damageAmount,
            ICombatState combatState)
        {
            try
            {
                // ✅ 使用 HookPlayerChoiceContext（而不是抽象的 PlayerChoiceContext）
                var choiceContext = new HookPlayerChoiceContext(
                    card,
                    player.NetId,
                    combatState,
                    GameActionType.Combat
                );

                _logger.Info($"BloodCost: Applying {damageAmount} damage to {creature.LogName} via CreatureCmd.Damage");

                // ✅ 使用 CreatureCmd.Damage - 这会自动处理网络同步
                var results = await CreatureCmd.Damage(
                    choiceContext,
                    creature,
                    damageAmount,
                    ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                    card
                );

                foreach (var result in results)
                {
                    _logger.Info($"BloodCost: Damage result - Unblocked: {result.UnblockedDamage}, Overkill: {result.OverkillDamage}, Target Killed: {result.WasTargetKilled}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"BloodCost: CreatureCmd.Damage failed - {ex.Message}");
                
                // 降级方案：如果 CreatureCmd.Damage 失败，直接扣血
                // 注意：这可能导致不同步，但总比不扣血好
                try
                {
                    _logger.Warn($"BloodCost: Falling back to direct HP modification");
                    creature.SetCurrentHpInternal(creature.CurrentHp - damageAmount);
                    
                    // 如果玩家死亡，触发死亡事件
                    if (creature.IsDead)
                    {
                        creature.InvokeDiedEvent();
                        if (creature.IsPlayer)
                        {
                            player.DeactivateHooks();
                        }
                    }
                }
                catch (Exception innerEx)
                {
                    _logger.Error($"BloodCost: Direct HP modification also failed - {innerEx.Message}");
                }
            }
        }

        /// <summary>
        /// 战斗结束时清理借用表
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
        public static void OnAfterCombatEnd()
        {
            _logger.Info("BloodCost: Clearing borrowed energy table on combat end");
            _borrowedEnergy.Clear();
        }

        /// <summary>
        /// 在卡牌被打出后清理借用表（防御性清理）
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardModel), "OnPlayWrapper")]
        public static void OnCardPlayedCleanup(CardModel __instance)
        {
            if (IsBloodCard(__instance) && _borrowedEnergy.TryGetValue(__instance, out var data))
            {
                _logger.Warn($"BloodCost: Card {__instance.Id.Entry} still had borrowed data after play, cleaning up");
                _borrowedEnergy.Remove(__instance);
            }
        }

        /// <summary>
        /// 在卡牌被移出战斗时清理借用表
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardPile), "RemoveInternal")]
        public static void OnCardRemovedFromPile(CardModel card)
        {
            if (card != null && IsBloodCard(card) && _borrowedEnergy.TryGetValue(card, out var data))
            {
                _logger.Info($"BloodCost: Card {card.Id.Entry} removed from pile, cleaning up borrowed data");
                _borrowedEnergy.Remove(card);
            }
        }
    }
}