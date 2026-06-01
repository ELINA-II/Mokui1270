using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class Salvage : AbstractMokui1270Card
{    

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
		new PowerVar<FocusPower>(1m),
        new EnergyVar(1),
    ];
    
    public Salvage() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault()!;
		if (cardModel != null)
		{
			await CardCmd.Exhaust(choiceContext, cardModel);
            if(cardModel.Type == CardType.Skill){
            await PowerCmd.Apply<FocusPower>(Owner.Creature,DynamicVars["FocusPower"].BaseValue,Owner.Creature, this);
            await PlayerCmd.GainEnergy(1, Owner);
            }
		}
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}