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
    private const int BASE_DAMAGE = 8;
    private const int DAMAGE_PER_CARD = 8;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(BASE_DAMAGE, ValueProp.Move)
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
        
        // 获取弃牌堆
        var discardPile = PileType.Discard.GetPile(Owner);
        
        // 检查弃牌堆中是否有可消耗的牌（排除自身）
        var availableCards = discardPile.Cards
            .Where(c => c != this)
            .ToList();
        
        if (availableCards.Count == 0)
        {
            // 弃牌堆为空，只造成基础伤害
            await DamageCmd.Attack(BASE_DAMAGE)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
            return;
        }
        
        // 让玩家从弃牌堆选择要消耗的牌（最多4张）
        var selectedCards = await SelectCardsFromDiscardPile(choiceContext, availableCards);
        
        if (selectedCards == null || selectedCards.Count == 0)
        {
            // 没选任何牌，只造成基础伤害
            await DamageCmd.Attack(BASE_DAMAGE)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
            return;
        }
        
        // 计算总伤害
        int extraDamage = selectedCards.Count * DAMAGE_PER_CARD;
        int totalDamage = BASE_DAMAGE + extraDamage;
        
        // 消耗选中的牌
        foreach (var card in selectedCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
        
        // 造成伤害
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }
    
    /// <summary>
    /// 让玩家从弃牌堆选择要消耗的牌（最多4张）
    /// 使用 FromCombatPile API
    /// </summary>
    private async Task<List<CardModel>> SelectCardsFromDiscardPile(PlayerChoiceContext choiceContext, List<CardModel> availableCards)
    {
        // 获取弃牌堆的引用
        var discardPile = PileType.Discard.GetPile(Owner);
        
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.ExhaustSelectionPrompt,
            1,
            4
        )
        {
            Cancelable = false
        };
        
        // 使用 FromCombatPile 从弃牌堆选择
        var selected = await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: discardPile,
            player: Owner,
            prefs: prefs,
            filter: card => card != this  // 排除自身
        );
        
        return selected.ToList();
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}