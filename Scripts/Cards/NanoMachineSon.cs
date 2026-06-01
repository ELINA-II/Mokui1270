using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class NanoMachineSon : AbstractMokui1270Card
{    

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0m),
        new CalculationExtraVar(2m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.Owner.PlayerCombatState!.AllCards.Count((CardModel c) => c.Keywords.Contains(MyKeyWords.Nanomachine))),
    ];
    
    public NanoMachineSon() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.CalculatedBlock.PreviewValue,ValueProp.Move,cardPlay);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.CalculationExtra.UpgradeValueBy(1m);
    }
}