using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Powers;

public class EnhancedSelfDestructPower : CustomPowerModel
{
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/EnhancedSelfDestructPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/EnhancedSelfDestructPower.png";

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (orb.Owner == Owner.Player){
        Creature creature = Owner.Player.RunState.Rng.CombatTargets.NextItem(Owner.CombatState!.HittableEnemies)!;
			if (creature != null)
			{
				await CreatureCmd.Damage(choiceContext, creature,Amount, ValueProp.Unpowered,Owner);
			}
    }}
}