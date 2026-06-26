using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class RoundslashThree : AbstractMokui1270Card
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move),
        new EnergyVar(2),
        new PowerVar<StrengthPower>(3m),
        new CardsVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        CardKeyword.Retain,
        MyKeyWords.BloodAttack
    ];

    public RoundslashThree() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
       await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
       .FromCard(this)
       .TargetingAllOpponents(CombatState!)
       .Execute(choiceContext);
       await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
       await PowerCmd.Apply<StrengthPower>(choiceContext,Owner.Creature,DynamicVars["StrengthPower"].BaseValue,Owner.Creature, this);
        IEnumerable<RoundslashI> enumerable = RoundslashI.Create(
        Owner,                    // 所有者
        DynamicVars.Cards.IntValue, // 数量（1）
        (CombatState)CombatState!               // 战斗状态
        );
        CardCmd.PreviewCardPileAdd(
        await CardPileCmd.AddGeneratedCardsToCombat(enumerable, PileType.Draw, Owner, default)
        );
        await SpiritHelmBreaker.CreateInHand(Owner,(CombatState)CombatState!);
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
        var roundslashiii = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            roundslashiii.Add(combatState.CreateCard<RoundslashThree>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(roundslashiii, PileType.Hand, owner, default);
        return roundslashiii;
    }
}