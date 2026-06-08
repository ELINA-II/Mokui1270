using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class FullSalvo : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move)];

    public FullSalvo() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        IEnumerable<CardModel> enumerable = PileType.Hand.GetPile(Owner).Cards.ToList();
		int handSize = enumerable.Count();
		await CardCmd.Discard(choiceContext, enumerable);
		await Cmd.CustomScaledWait(0f, 0.25f);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(handSize)
            .FromCard(this)
			.Targeting(cardPlay.Target!)
			.Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext,handSize,Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}