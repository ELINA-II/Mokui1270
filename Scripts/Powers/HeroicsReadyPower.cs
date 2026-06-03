using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
namespace Mokui1270.Scripts.Powers
{
    /// <summary>
    /// 免死能力 - 受到致命伤害时触发，激活绝境姿态
    /// </summary>
    public class HeroicsReadyPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override bool AllowNegative => false;

        private bool _hasTriggered = false;

        public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/HeroicsReadyPower.png";
        public override string? CustomBigIconPath => "res://Mokui1270/images/powers/HeroicsReadyPower.png";


        public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner) return;
            if (_hasTriggered) return;
            
            // 检查是否是致命伤害
            if (target.CurrentHp <= amount)
            {
                _hasTriggered = true;
                
                // 锁血为1
                await CreatureCmd.SetCurrentHp(target, 1);
                
                // 移除自身
                RemoveInternal();
                
                // 添加绝境姿态能力（持续3回合）
                await PowerCmd.Apply<HeroicsActivePower>(
                    target,
                    3,  // 持续3回合
                    target,
                    null
                );
            }
        }
    }
}