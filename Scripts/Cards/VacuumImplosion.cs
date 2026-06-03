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
public class VacuumImplosion : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const int ramCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    private int targetblock = 0;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new EnergyVar(energyCost),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    public VacuumImplosion() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        Player player = Owner;
        bool couldPlay = await CouldPlay((int)DynamicVars["Ram"].BaseValue, choiceContext, player);
        if (!couldPlay)
        {
            await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
        }else{
        targetblock = cardPlay.Target.Block;
        await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block);
        await DamageCmd.Attack(targetblock)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature,targetblock,ValueProp.Move | ValueProp.Unpowered,cardPlay);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Ram"].UpgradeValueBy(-1);
    }
}