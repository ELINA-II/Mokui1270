using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Mokui1270.Scripts.Orbs;

namespace Mokui1270.Scripts.Powers;

public class MassiveUAVBuilderAttackPower : CustomPowerModel
{
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/MassiveUAVBuilderAttackPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/MassiveUAVBuilderAttackPower.png";


    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromOrb<AttackUav>(),
    ];
    public override async Task AfterEnergyReset(Player player)
	{
		if (player == Owner.Player)
		{
			await OrbCmd.Channel<AttackUav>(new ThrowingPlayerChoiceContext(),Owner.Player);
            await OrbCmd.Channel<AttackUav>(new ThrowingPlayerChoiceContext(),Owner.Player);
			await PowerCmd.Decrement(this);
		}
	}
}