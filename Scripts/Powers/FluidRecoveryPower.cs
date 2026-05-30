using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using BaseLib.Abstracts;

namespace Mokui1270.Scripts.Powers
{
    /// <summary>
    /// 体液回收能力 - 攻击卡牌造成单体伤害时，回复伤害一半的血量
    /// </summary>
    public class FluidRecoveryPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override bool AllowNegative => false;
        public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/FluidRecoveryPower.png";
        public override string? CustomBigIconPath => "res://Mokui1270/images/powers/FluidRecoveryPower.png";

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
        {
            // 只处理玩家造成的伤害
            if (dealer?.IsPlayer != true) return;
            
            // 只处理攻击卡牌
            if (cardSource?.Type != CardType.Attack) return;
            
            // 获取实际造成的伤害
            int damageDealt = result.UnblockedDamage + result.OverkillDamage;
            if (damageDealt <= 0) return;
            
            // 计算回复量（伤害的一半，向下取整）
            int healAmount = damageDealt / 2;
            if (healAmount <= 0) return;
            
            // 回复血量
            await CreatureCmd.Heal(dealer, healAmount, playAnim: true);
        }
    }
}