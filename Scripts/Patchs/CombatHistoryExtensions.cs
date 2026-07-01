using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patches;
using Mokui1270.Scripts.RAM;
using System.Reflection;
namespace Mokui1270.Scripts.Patchs
{
    public static class CombatHistoryExtensions
    {
        // ✅ 缓存私有的 Add 方法（提高性能）
        private static MethodInfo? _addMethod;

        /// <summary>
        /// 记录治疗事件到战斗历史
        /// </summary>
        public static void RecordHealthRestored(
            this CombatHistory history,
            ICombatState combatState,
            Creature receiver,
            Creature? dealer,
            int amount,
            CardModel? cardSource)
        {
            if (history == null || combatState == null || receiver == null)
                return;

            // 创建治疗条目
            var entry = new HealthRestoredEntry(
                receiver,
                dealer,
                amount,
                cardSource,
                combatState.RoundNumber,
                combatState.CurrentSide,
                history,
                combatState.Players
            );

            // ✅ 调用基类的私有 Add 方法
            if (_addMethod == null)
            {
                _addMethod = typeof(CombatHistory).GetMethod(
                    "Add",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(ICombatState), typeof(CombatHistoryEntry) },
                    null
                );
            }

            _addMethod?.Invoke(history, new object[] { combatState, entry });
        }

        /// <summary>
        /// 获取指定生物在本回合的治疗次数
        /// </summary>
        public static int GetHealCount(
            this CombatHistory history,
            Creature creature,
            ICombatState? combatState = null,
            bool onlyThisRound = true)
        {
            if (history == null || creature == null)
                return 0;

            var entries = history.Entries.OfType<HealthRestoredEntry>();

            if (onlyThisRound && combatState != null)
            {
                entries = entries.Where(e => 
                    e.Receiver == creature && 
                    e.HappenedThisTurn(combatState)
                );
            }
            else if (onlyThisRound)
            {
                // 如果没有传入 combatState，只过滤接收者
                entries = entries.Where(e => e.Receiver == creature);
            }

            return entries.Count();
        }

        /// <summary>
        /// 获取指定生物在本回合的总治疗量
        /// </summary>
        public static int GetTotalHealAmount(
            this CombatHistory history,
            Creature creature,
            ICombatState? combatState = null,
            bool onlyThisRound = true)
        {
            if (history == null || creature == null)
                return 0;

            var entries = history.Entries.OfType<HealthRestoredEntry>();

            if (onlyThisRound && combatState != null)
            {
                entries = entries.Where(e => 
                    e.Receiver == creature && 
                    e.HappenedThisTurn(combatState)
                );
            }
            else if (onlyThisRound)
            {
                entries = entries.Where(e => e.Receiver == creature);
            }

            return entries.Sum(e => e.Amount);
        }

    //RAM相关的记录
    public static void RecordRAMChanged(
            this CombatHistory history,
            ICombatState combatState,
            Creature actor,
            int newRAM,
            int maxRAM,
            int delta,
            string changeReason,
            CardModel? cardSource)
        {
            if (history == null || combatState == null || actor == null)
                return;

            var entry = new RAMChangedEntry(
                actor,
                newRAM,
                maxRAM,
                delta,
                changeReason,
                cardSource,
                combatState.RoundNumber,
                combatState.CurrentSide,
                history,
                combatState.Players
            );

            // 调用私有的 Add 方法
            if (_addMethod == null)
            {
                _addMethod = typeof(CombatHistory).GetMethod(
                    "Add",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(ICombatState), typeof(CombatHistoryEntry) },
                    null
                );
            }

            _addMethod?.Invoke(history, new object[] { combatState, entry });
        }

    //血战同步需要用到的

            private static void AddEntry(CombatHistory history, ICombatState combatState, CombatHistoryEntry entry)
        {
            if (_addMethod == null)
            {
                _addMethod = typeof(CombatHistory).GetMethod(
                    "Add",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(ICombatState), typeof(CombatHistoryEntry) },
                    null
                );
            }

            _addMethod?.Invoke(history, new object[] { combatState, entry });
        }
        public static void RecordBloodCost(
        this CombatHistory history,
        ICombatState combatState,
        Creature actor,
        CardModel card,
        int borrowedEnergy,
        int healthCostPaid,
        string phase)
    {
        if (history == null || combatState == null || actor == null || card == null)
            return;

        var entry = new BloodCostEntry(
            actor,
            card,
            borrowedEnergy,
            healthCostPaid,
            phase,
            combatState.RoundNumber,
            combatState.CurrentSide,
            history,
            combatState.Players
        );

        AddEntry(history, combatState, entry);
    }
    }
}