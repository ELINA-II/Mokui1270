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
using StrengthPower = MegaCrit.Sts2.Core.Models.Powers.StrengthPower;
using IHoverTip = MegaCrit.Sts2.Core.HoverTips.IHoverTip;
using HoverTipFactory = MegaCrit.Sts2.Core.HoverTips.HoverTipFactory;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class SpiritHelmBreaker : AbstractMokui1270Card
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new PowerVar<StrengthPower>(-3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
        MyKeyWords.BloodAttack
    ];

    public SpiritHelmBreaker() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(5).FromCard(this)
			.TargetingAllOpponents(CombatState!)
			.Execute(choiceContext);
        await PowerCmd.Apply<StrengthPower>(choiceContext,Owner.Creature,DynamicVars["StrengthPower"].BaseValue,Owner.Creature, this);
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
        var SpiritHelmBreaker = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            SpiritHelmBreaker.Add(combatState.CreateCard<SpiritHelmBreaker>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(SpiritHelmBreaker, PileType.Hand, owner, CardPilePosition.Bottom);
        return SpiritHelmBreaker;
    }

}