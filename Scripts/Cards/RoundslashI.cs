using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class RoundslashI : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar("RoundslashII", 1),
        new DamageVar(9, ValueProp.Move)
        ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, MyKeyWords.BloodAttack];

    public RoundslashI() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
       await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
       .FromCard(this)
       .TargetingAllOpponents(CombatState!)
       .Execute(choiceContext);
       await RoundslashTwo.CreateInHand(Owner,(CombatState)CombatState!);
	   await Cmd.Wait(0.25f);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    public static async Task<CardModel?> CreateInHand(Player owner, CombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }
    
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, CombatState combatState)
    {
        var roundslashi = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            roundslashi.Add(combatState.CreateCard<RoundslashI>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(roundslashi, PileType.Hand, owner, default);
        return roundslashi;
    }

    public static IEnumerable<RoundslashI> Create(Player owner, int amount, CombatState combatState)
{
    List<RoundslashI> list = new List<RoundslashI>();
    for (int i = 0; i < amount; i++)
    {
        list.Add(combatState.CreateCard<RoundslashI>(owner));
    }
    return list;
}
}