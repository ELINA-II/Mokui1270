using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.RAM;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class BlackwallGateway : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const int ramCost = 5;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20, ValueProp.Move),
        new CardsVar(1),
        new EnergyVar(energyCost),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
        CardKeyword.Exhaust,
    ];

    public BlackwallGateway() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        Player player = Owner;
        bool couldPlay = await CouldPlay((int)DynamicVars["Ram"].BaseValue, choiceContext, player);
        if (!couldPlay)
        {
            await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
        }else{
        if((await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext))
            .Results.Any((DamageResult r) => r.WasTargetKilled))
            {
                await PlayerCmd.GainEnergy(EnergyCost.GetAmountToSpend()*2,Owner);
                await RAMClass.RecoverRAM(choiceContext,(int)DynamicVars["Ram"].BaseValue*2,Owner);
                await BlackwallGateway.CreateInHand(Owner,CombatState!);
            }
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Damage.PreviewValue,ValueProp.Move | ValueProp.Move,cardPlay);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
        }
    }

    


    protected override void OnUpgrade()
    {
        DynamicVars["Ram"].UpgradeValueBy(-2);
        DynamicVars.Damage.UpgradeValueBy(5);
    }

    public static async Task<CardModel?> CreateInHand(Player owner, CombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }
    
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, CombatState combatState)
    {
        var blackgateway = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            blackgateway.Add(combatState.CreateCard<BlackwallGateway>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(blackgateway, PileType.Hand, addedByPlayer: true);
        return blackgateway;
    }
}