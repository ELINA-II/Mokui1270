using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Powers;

public class DashPower : CustomPowerModel
{
    private Rng _rng = new Rng();
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;  // 不可叠加


    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/DashPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/DashPower.png";
    
    
    /// <summary>
    /// 乘法修改伤害
    /// </summary>
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只对自己生效
        if (target != Owner) return 1m;
        
        // 随机
        int roll = _rng.NextInt(0, 100);  // 0-99
        
        if (roll < 50)
        {
            // 50% 概率：伤害变为0
            return 0m;
        }
        else
        {
            // 50% 概率：伤害降为一半
            return 0.5m;
        }
    }
    
    /// <summary>
    /// 回合结束时移除该能力（本回合有效）
    /// </summary>
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }
}