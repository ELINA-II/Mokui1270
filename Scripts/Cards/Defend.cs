using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class Defend : AbstractMokui1270Card
{

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targrtType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];


    public Defend() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {}

    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}