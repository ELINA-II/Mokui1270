using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class ActOfRescue : AbstractMokui1270Card
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const int ramCost = 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new BlockVar(6, ValueProp.Move),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM")
        ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    public ActOfRescue() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlayInternal(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        Player player = Owner;
        bool couldPlay = await CouldPlay((int)DynamicVars["Ram"].BaseValue, choiceContext, player);
        if (!couldPlay)
        {
            await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
        }else{
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
        await Cmd.Wait(0.25f);
        }
    }

    protected override PileType GetResultPileTypeForCardPlay()
	{
		PileType resultPileTypeForCardPlay = base.GetResultPileTypeForCardPlay();
		if (resultPileTypeForCardPlay != PileType.Discard)
		{
			return resultPileTypeForCardPlay;
		}
		return PileType.Hand;
	}

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}