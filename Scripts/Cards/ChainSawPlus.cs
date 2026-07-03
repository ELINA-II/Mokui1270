using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class ChainSawPlus : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>[
        new HpLossVar(5),
        new DamageVar(2, ValueProp.Move | ValueProp.Unblockable),
        new HealVar(6),  // 保留HealVar，每张牌回复6点
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.BloodAttack];

    public ChainSawPlus() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies, true)
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

        // 2. 让玩家选择要消耗的手牌（和 ChainSaw 一样）
        var selectedCards = await SelectCardsToExhaust(choiceContext);
        
        if (selectedCards == null || selectedCards.Count == 0) 
        {
            // 没选任何牌，只执行伤害和Buff移除
            await ExecuteCombatEffects(choiceContext);
            return;
        }

        // 3. 消耗选中的牌
        foreach (var card in selectedCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // 4. 计算治疗量（每张牌回复6点，使用 DynamicVars.Heal）
        int healAmount = (int)(selectedCards.Count * DynamicVars.Heal.BaseValue);
        await CreatureCmd.Heal(creature, healAmount);
        
        // 5. 执行战斗效果（伤害 + 移除Buff）
        await ExecuteCombatEffects(choiceContext);
        await CardPileCmd.Draw(choiceContext,healAmount,Owner);
    }

    /// <summary>
    /// 执行战斗效果：移除敌人Buff + 造成伤害
    /// </summary>
    private async Task ExecuteCombatEffects(PlayerChoiceContext choiceContext)
    {
        var creature = Owner?.Creature;
        if (creature == null) return;
        
        var combatState = creature.CombatState;
        if (combatState == null) return;
        
        var enemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies.Count == 0) return;

        var rng = combatState.RunState.Rng.Niche;
        
        // 移除每个敌人的随机一个正面效果
        foreach (var enemy in enemies)
        {
            var buffs = enemy.Powers.Where(p => p.Type == PowerType.Buff).ToList();
            
            if (buffs.Count > 0)
            {
                int randomIndex = rng.NextInt(0, buffs.Count);
                var buffToRemove = buffs[randomIndex];
                await PowerCmd.Remove(buffToRemove);
            }
        }
        
        // 对全体敌人造成3次伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(3)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
    }

    /// <summary>
    /// 让玩家选择要消耗的手牌（和 ChainSaw 逻辑一致）
    /// </summary>
    private async Task<List<CardModel>> SelectCardsToExhaust(PlayerChoiceContext choiceContext)
    {
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.ExhaustSelectionPrompt,
            1,
            6
        )
        {
            Cancelable = false  // 不允许取消
        };

        var selected = (await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: Owner,
            filter: card => card != this,  // 排除自身
            source: this
        )).ToList();

        return selected;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        EnergyCost.UpgradeBy(-1);
    }
}