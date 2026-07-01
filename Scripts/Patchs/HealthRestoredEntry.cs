using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Mokui1270.Scripts.Patchs
{
    /// <summary>
    /// 治疗历史记录条目
    /// </summary>
    public class HealthRestoredEntry : CombatHistoryEntry
    {
        // ✅ 治疗量（只读属性，在构造函数中设置）
        public int Amount { get; }
        
        // ✅ 治疗来源（谁造成的治疗，可以是卡牌、药水等）
        public Creature? Dealer { get; }
        
        // ✅ 治疗来源的卡牌（如果是卡牌治疗）
        public CardModel? CardSource { get; }
        
        // ✅ 接受治疗的生物（就是 Actor）
        // 基类已经有 Actor 属性，所以我们不需要再定义 Receiver
        public Creature Receiver => Actor;

        // ✅ 实现抽象属性 Description
        public override string Description
        {
            get
            {
                string id = GetId(Receiver);
                if (Dealer == null && CardSource == null)
                {
                    return $"{id} healed {Amount} HP";
                }
                if (CardSource != null)
                {
                    return $"{id} healed {Amount} HP from {CardSource.Id.Entry}";
                }
                return $"{GetId(Dealer!)} healed {Amount} HP to {id}";
            }
        }

        /// <summary>
        /// 构造函数：创建治疗记录
        /// </summary>
        /// <param name="receiver">接受治疗的生物</param>
        /// <param name="dealer">治疗来源（可以是生物或null）</param>
        /// <param name="amount">治疗量</param>
        /// <param name="cardSource">来源卡牌（可选）</param>
        /// <param name="roundNumber">当前回合数</param>
        /// <param name="currentSide">当前阵营</param>
        /// <param name="history">战斗历史</param>
        /// <param name="players">所有玩家</param>
        public HealthRestoredEntry(
            Creature receiver,
            Creature? dealer,
            int amount,
            CardModel? cardSource,
            int roundNumber,
            CombatSide currentSide,
            CombatHistory history,
            IEnumerable<Player> players)
            : base(receiver, roundNumber, currentSide, history, players)
        {
            Amount = amount;
            Dealer = dealer;
            CardSource = cardSource;
        }

        /// <summary>
        /// 获取生物显示名称
        /// </summary>
        private static string GetId(Creature creature)
        {
            if (!creature.IsPlayer)
            {
                return creature.Monster!.Id!.Entry;
            }
            return creature.Player!.Character!.Id!.Entry;
        }
    }
}