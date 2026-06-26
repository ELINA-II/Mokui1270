using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class Terminator : AbstractMokui1270Card
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targrtType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    int bloodcost = 0;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<TerminatorPower>(1m),
        ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<TerminatorPower>()
    ];
    

    public Terminator() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
       bloodcost = Owner.Creature.CurrentHp/2;
       await CreatureCmd.Damage(choiceContext, Owner.Creature,bloodcost, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
       await PowerCmd.Apply<TerminatorPower>(choiceContext,Owner.Creature,DynamicVars["TerminatorPower"].BaseValue,Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

}