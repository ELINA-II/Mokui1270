using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Powers;

public class TerminatorPower : CustomPowerModel
{
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;  // 可叠加

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/TerminatorPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/TerminatorPower.png";
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != Owner && !Owner.Pets.Contains<Creature>(dealer!))
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		if (cardSource == null)
		{
			return 1m;
		}
		return 3m;
	}
}