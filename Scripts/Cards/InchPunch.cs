using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
    
    // 存储待处理的数据
    private CardModel? _pendingCard = null;
    private Creature? _pendingTarget = null;
    private int _pendingDamage = 0;
    private int _pendingHeal = 0;
    
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
        
        // 存储目标
        _pendingTarget = cardPlay.Target;
        
        // 选择要消耗的卡牌
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1)
        {
            Cancelable = true,
        };
        
        var selectedCard = (await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this
        )).FirstOrDefault();
        
        if (selectedCard == null) return;
        
        // 计算伤害和治疗量
        (_pendingDamage, _pendingHeal) = CalculateValues(selectedCard);
        _pendingCard = selectedCard;
        
        // 消耗卡牌
        await CardCmd.Exhaust(choiceContext, selectedCard);
        
        // 注意：不在 OnPlay 中执行伤害，让 AfterCardPlayed 来处理
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);
        
        if (_pendingCard == null || _pendingTarget == null) return;
        
        // 造成伤害
        await DamageCmd.Attack(_pendingDamage)
            .FromCard(this)
            .Targeting(_pendingTarget)
            .Execute(choiceContext);
        
        // 回复生命
        if (_pendingHeal > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, _pendingHeal);
        }
        
        // 清理数据
        _pendingCard = null;
        _pendingTarget = null;
        _pendingDamage = 0;
        _pendingHeal = 0;
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