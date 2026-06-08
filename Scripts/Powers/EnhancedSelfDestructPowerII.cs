using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Cards;

namespace Mokui1270.Scripts.Powers;

public class EnhancedSelfDestructPowerII : CustomPowerModel
{
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/EnhancedSelfDestructPowerII.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/EnhancedSelfDestructPowerII.png";

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (orb.Owner == Owner.Player){
        for (int i = 0; i < Amount; i++)
            {
                await RapidUAVBuilder.CreateInHand(Owner.Player,CombatState);
    }}}
}