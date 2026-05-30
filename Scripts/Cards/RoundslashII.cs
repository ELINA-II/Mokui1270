using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class RoundslashTwo : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new EnergyVar(1),
        new PowerVar<VulnerablePower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        CardKeyword.Retain,
        MyKeyWords.BloodAttack
    ];

    public RoundslashTwo() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
       await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
       .FromCard(this)
       .TargetingAllOpponents(CombatState!)
       .Execute(choiceContext);
       await PowerCmd.Apply<VulnerablePower>(CombatState!.HittableEnemies,DynamicVars.Vulnerable.BaseValue,Owner.Creature, this);
       await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
       await RoundslashThree.CreateInHand(Owner,CombatState!);
	   await Cmd.Wait(0.25f);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

      public static async Task<CardModel?> CreateInHand(Player owner, CombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }
    
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, CombatState combatState)
    {
        var roundslashii = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            roundslashii.Add(combatState.CreateCard<RoundslashTwo>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(roundslashii, PileType.Hand, addedByPlayer: true);
        return roundslashii;
    }
}