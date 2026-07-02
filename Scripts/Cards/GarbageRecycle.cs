using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class GarbageRecycle : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;  // 修正拼写
    private const bool shouldShowInCardLibrary = true;
    
    private const int BASE_HEAL = 6;      // 每耗能基础治疗量
    private const int X_CARD_HEAL = 12;   // X费卡牌治疗量
    private const int STATUS_CARD_COST = 1; // 状态牌按1耗能计算

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MyKeyWords.GarbageRecycle)
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(BASE_HEAL),
        new EnergyVar(1),
        new CardsVar(2),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack
    ];

    public GarbageRecycle() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        isBlood = true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 让玩家选择一张要消耗的手牌
        var selectedCard = await SelectCardToExhaust(choiceContext);
        
        if (selectedCard == null) return;  // 玩家取消了选择
        
        // 2. 计算消耗卡牌的能量消耗和效果
        int energyGain;
        int healAmount;
        bool isXCard = selectedCard.EnergyCost.CostsX;
        bool isStatusCard = selectedCard.Type == CardType.Status;
        
        if (isXCard)
        {
            // X费卡牌：能量翻倍，回复12血量
            energyGain = Owner.PlayerCombatState?.Energy ?? 0;
            healAmount = X_CARD_HEAL;
        }
        else
        {
            // 普通卡牌：获取能量消耗
            int cost = GetCardEnergyCost(selectedCard, isStatusCard);
            energyGain = cost;
            healAmount = cost * BASE_HEAL;
        }
        
        // 3. 消耗选中的卡牌
        await CardCmd.Exhaust(choiceContext, selectedCard, true);
        
        // 5. 回复能量
        if (energyGain > 0)
        {
            if (isXCard)
            {
                // X费卡牌：能量翻倍（设置能量为当前值的2倍）
                await PlayerCmd.SetEnergy(energyGain * 2, Owner);
            }
            else
            {
                // 普通卡牌：增加对应能量
                await PlayerCmd.GainEnergy(energyGain, Owner);
            }
        }

        // 6. 回复生命
        if (healAmount > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, healAmount);
        }

        if(isStatusCard)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue,Owner);
        }
    }

    /// <summary>
    /// 让玩家选择一张要消耗的卡牌
    /// </summary>
    private async Task<CardModel> SelectCardToExhaust(PlayerChoiceContext choiceContext)
    {
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.ExhaustSelectionPrompt,  // "消耗"提示
            1
        )
        {
            Cancelable = false  // 不允许取消
        };
        
        var selected = await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: Owner,
            filter: null,  // 所有手牌可选
            source: this
        );
        
        return selected.FirstOrDefault()!;
    }

    /// <summary>
    /// 获取卡牌的能量消耗值
    /// </summary>
    private int GetCardEnergyCost(CardModel card, bool isStatusCard)
    {
        if (isStatusCard)
        {
            return STATUS_CARD_COST;  // 状态牌按1耗能计算
        }
        
        return card.EnergyCost.Canonical;  // 普通卡牌的基础费用
    }

    protected override void OnUpgrade()
    {
        // 升级：费用 1 → 0
        EnergyCost.UpgradeBy(-1);
    }
}