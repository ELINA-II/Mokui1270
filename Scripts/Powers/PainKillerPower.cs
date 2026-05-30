using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Mokui1270.Scripts.Powers;

public class PainKillerPower : CustomPowerModel
{
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;  // 可叠加层数

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/PainKillerPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/PainKillerPower.png";

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // 只关心自己
        if (creature != Owner) return;
        
        // 只关心失去生命（delta < 0）
        if (delta >= 0) return;
        
        // 计算抽牌数量 = 每层抽牌数 × 层数
        int drawCount = Amount;
        
        if (drawCount > 0 && Owner.CombatState != null)
        {
            // 抽牌
            await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player!);
            await PowerCmd.Apply<SetupStrikePower>(Owner,Amount,Owner, null);
        }
    }
}