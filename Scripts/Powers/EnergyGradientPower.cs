using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Mokui1270.Scripts.Powers;

public class EnergyGradientPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/EnergyGradientPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/EnergyGradientPower.png";

    
    /// <summary>
    /// 阻止格挡在回合开始时清空
    /// </summary>
    public override bool ShouldClearBlock(Creature creature)
    {
        if (base.Owner == creature)
        {
            return false;  // 不清空格挡
        }
        return true;
    }
}