using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class InchPunch : AbstractMokui1270Card
{
    private const int BASE_DAMAGE = 20;
    private const int X_CARD_DAMAGE = 40;
    private const int X_CARD_HEAL = 8;
    private const float DAMAGE_MULTIPLIER = 5f;
    private const float HEAL_MULTIPLIER = 4f;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(BASE_DAMAGE, ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MyKeyWords.InchPunch)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack
    ];

    public InchPunch() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        
        // 选择要消耗的卡牌
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1)
        {            
        };
        
        var selectedCards = await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this
        );
        
        var selectedCard = selectedCards.FirstOrDefault();
        
        if (selectedCard == null) return;
        
        // 计算伤害和治疗量
        var (damage, heal) = CalculateValues(selectedCard);
        
        // 先消耗卡牌
        await CardCmd.Exhaust(choiceContext, selectedCard);
        
        // 造成伤害
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        
        // 回复生命
        if (heal > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, heal);
        }
    }
    
    private (int damage, int heal) CalculateValues(CardModel card)
    {
        if (card.EnergyCost.CostsX)
        {
            return (X_CARD_DAMAGE, X_CARD_HEAL);
        }
        
        int cost = card.EnergyCost.Canonical;
        int damage = BASE_DAMAGE + (int)(cost * DAMAGE_MULTIPLIER);
        int heal = (int)(cost * HEAL_MULTIPLIER);
        
        return (damage, heal);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}