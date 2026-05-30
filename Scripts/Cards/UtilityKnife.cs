using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui_1270.Patchs;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class UtilityKnife : AbstractMokui1270Card
{
    private const string HEAL_BONUS_KEY = "HealBonus";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack
    ];
    
    // 使用 CalculatedVar 实现动态伤害显示
    protected override IEnumerable<DynamicVar> CanonicalVars =>[
        new CalculationBaseVar(9m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) => 
            HealTracker.GetHealCount(card.Owner)  // 乘数 = 治疗次数
        )
    ];
    
    // 金边高亮：满血时高亮
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (CombatState == null) return false;
            return Owner.Creature.CurrentHp >= Owner.Creature.MaxHp;
        }
    }
    
    public UtilityKnife() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        
        bool isFullHp = Owner.Creature.CurrentHp >= Owner.Creature.MaxHp;
        
        // 第一次攻击
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        
        // 满血时额外攻击
        if (isFullHp)
        {            
            await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}