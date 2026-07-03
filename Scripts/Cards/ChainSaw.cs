using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class ChainSaw : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>[
        new HpLossVar(5),
        new DamageVar(2, ValueProp.Move | ValueProp.Unblockable),
        new CalculationBaseVar(0m),
        new HealVar(5),
        new CalculationExtraVar(1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.BloodAttack];

    public ChainSaw() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies, true)
    {
        isBlood = true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner;
        var creature = player?.Creature;
        
        if (creature == null) return;
        
        // 1. 对自己造成伤害
        if (creature.CurrentHp > 5)
        {
            await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
        }
        else
        {
            return;
        }
        
        // 2. 让玩家选择要消耗的手牌（借鉴 RapidUAVBuilder 的模式）
        var selectedCards = await SelectCardsToExhaust(choiceContext);
        
        if (selectedCards == null || selectedCards.Count == 0) 
        {
            // 没选任何牌，只触发伤害效果
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(3)
                .FromCard(this)
                .TargetingAllOpponents(CombatState!)
                .Execute(choiceContext);
            return;
        }

        // 3. 消耗选中的牌
        foreach (var card in selectedCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // 4. 计算治疗量（固定值，完全同步安全）
        int healAmount = selectedCards.Count * 5;
        await CreatureCmd.Heal(creature, healAmount);
        
        // 5. 对全体敌人造成3次伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(3)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    /// <summary>
    /// 让玩家选择要消耗的手牌（借鉴 RapidUAVBuilder 的选牌逻辑）
    /// </summary>
    private async Task<List<CardModel>> SelectCardsToExhaust(PlayerChoiceContext choiceContext)
    {
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.ExhaustSelectionPrompt,
            1,
            3
        )
        {
            Cancelable = false  // 不允许取消
        };

        var selected = (await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: Owner,
            filter: card => card != this,  // 排除自身（已经在使用了）
            source: this
        )).ToList();

        return selected;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    public CardModel GetTranscendenceTransformedCard()
    {
        return (CardModel)(object)ModelDb.Card<ChainSawPlus>();
    }
}