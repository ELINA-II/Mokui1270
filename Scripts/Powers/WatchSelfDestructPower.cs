using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Mokui1270.Scripts.RAM;

namespace Mokui1270.Scripts.Powers;

public class WatchSelfDestructPower : CustomPowerModel
{
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/WatchSelfDestructPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/WatchSelfDestructPower.png";

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == Owner.Side && !Owner.IsDead)
		{
			Flash();
            if(Amount <= 1)
            {
                RAMClass.SetRecoveryEnabled(true);
            }
			await PowerCmd.Decrement(this);
		}
	}

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        RAMClass.SetRecoveryEnabled(true);
    }

	}