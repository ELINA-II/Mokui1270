using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Powers;

public class DashPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/DashPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/DashPower.png";
    
    /// <summary>
    /// 乘法修改伤害
    /// 将伤害降低为原本的 75%
    /// </summary>
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只对自己生效
        if (target != Owner) return 1m;
        
        // 伤害降为原本的 75%（即 0.75 倍）
        return 0.75m;
    }
    
    /// <summary>
    /// 回合结束时移除该能力（本回合有效）
    /// </summary>
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }
}