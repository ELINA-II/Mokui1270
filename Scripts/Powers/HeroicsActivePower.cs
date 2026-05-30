using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Mokui1270.Scripts.Powers
{
    /// <summary>
    /// 火场怪力（激活状态） - 持续3回合，期间：
    /// - 所受伤害降为0（相当于无敌）
    /// - 每回合获得3能量和3力量
    /// - 层数显示剩余回合数
    /// </summary>
    public class HeroicsActivePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool AllowNegative => false;

        public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/HeroicsActivePower.png";
        public override string? CustomBigIconPath => "res://Mokui1270/images/powers/HeroicsActivePower.png";

        /// <summary>
        /// 修改受到的伤害，降为0（参考 IntangiblePower 的实现）
        /// </summary>
        public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            // 只保护自己
            if (target != Owner) return amount;
            
            // 如果没有剩余回合，不修改伤害
            if (Amount <= 0) return amount;
            
            // 将伤害降为0（无敌）
            return 0;
        }

        /// <summary>
        /// 伤害被修改后闪烁提示
        /// </summary>
        public override Task AfterModifyingHpLostAfterOsty()
        {
            if (Amount > 0)
            {
                Flash();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 修改伤害上限（保险起见，也设为0）
        /// </summary>
        public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner) return decimal.MaxValue;
            if (Amount <= 0) return decimal.MaxValue;
            
            // 伤害上限设为0
            return 0;
        }

        /// <summary>
        /// 每回合开始时给予增益
        /// </summary>
        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            var creature = player.Creature;
            if (creature != Owner) return;
            
            // 检查是否还有剩余回合
            if (Amount <= 0)
            {
                await Cleanup();
                return;
            }
            
            // 增加能量
            await PlayerCmd.GainEnergy(3,player);
        
            // 增加临时力量
            await PowerCmd.Apply<SetupStrikePower>(Owner,3,Owner, null);
            
            // 减少剩余回合计数（层数会减少）
            SetAmount(Amount - 1);
        }

        /// <summary>
        /// 战斗结束时清理
        /// </summary>
        public override async Task AfterCombatEnd(CombatRoom room)
        {
            await Cleanup();
        }

        /// <summary>
        /// 清理临时力量和自身
        /// </summary>
        private async Task Cleanup()
        {     
            // 移除自身
            RemoveInternal();
        }
    }
}