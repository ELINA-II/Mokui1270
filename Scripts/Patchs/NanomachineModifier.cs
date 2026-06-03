using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using Mokui1270.Scripts.Cards;

namespace Mokui1270.NanomachineCostSystem
{
    public static class NanomachineModifier
    {
        private static readonly ConditionalWeakTable<CardModel, NanomachineData> _cardData = new();

        private class NanomachineData
        {
            public bool Enabled { get; set; }
        }

        private static NanomachineData GetData(CardModel card)
        {
            if (card == null) return null!;
            return _cardData.GetOrCreateValue(card);
        }

        public static void SetNanomachine(this CardModel card, bool enabled)
        {
            var data = GetData(card);
            if (data != null) data.Enabled = enabled;
        }

        public static bool IsNanomachine(this CardModel card)
        {
            var data = GetData(card);
            return data != null && data.Enabled;
        }

        internal static bool ShouldEnableNanomachine(CardModel card)
        {
            if (card is not AbstractMokui1270Card mokuiCard) return false;
            return mokuiCard.isNanomachine;
        }
    }

    /// <summary>
    /// 纳米机器系统 - 打出时给自己和所有队友恢复1点生命值
    /// </summary>
    [HarmonyPatch]
    public static class NanomachineHealPatch
    {
        // 使用 Postfix，返回类型为 void（不匹配原方法的 Task）
        // 或者使用 async void，但不推荐
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardModel), "SpendResources")]
        public static void AfterSpendResources(CardModel __instance)
        {
            if (!NanomachineModifier.ShouldEnableNanomachine(__instance)) return;
            
            var player = __instance.Owner;
            if (player?.Creature?.CombatState == null) return;

            // 获取所有队友（包括自己）
            var allTeammates = player.Creature.CombatState.Players
                .Where(p => p.Creature != null && p.Creature.IsAlive && p.Creature.Side == player.Creature.Side)
                .ToList();

            // 同步治疗，不使用 await
            foreach (var teammate in allTeammates)
            {
                var creature = teammate.Creature;
                // 使用同步方法直接治疗
                creature.HealInternal(1);
            }
        }
    }
}