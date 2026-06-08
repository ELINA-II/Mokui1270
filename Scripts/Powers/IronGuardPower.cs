using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Powers;

public class IronGuardPower : CustomPowerModel
{   
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;  // 可叠加

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/IronGuardPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/IronGuardPower.png";
    
    protected override IEnumerable<DynamicVar> CanonicalVars  => [
        new PowerVar<IntangiblePower>(1m),
    ];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != Owner.Side)
		{
			return;
		}
        var allPlayers = CombatState!.Players
            .Where(p => p.Creature.IsAlive)
            .Select(p => p.Creature)
            .ToList();      
        // 给所有玩家施加效果
        foreach (var player in allPlayers)
        {
            await PowerCmd.Apply<IntangiblePower>(
            player,
            1,
            Owner,
            null
        );
        }
	}

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		return Amount;
	}
}