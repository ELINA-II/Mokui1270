using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class Epinephrine : AbstractMokui1270Card
{
    // 使用 CalculatedDamageVar 实现动态伤害显示
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(5m),           // 基础伤害：5
        new ExtraDamageVar(1m),               // 每受到一次伤害增加1
        new CalculatedDamageVar(ValueProp.Move)
         .WithMultiplier((CardModel card, Creature? _) => 
                // ✅ 使用 CombatHistory 查询本场战斗受到的伤害次数
                CombatManager.Instance.History.Entries
                    .OfType<DamageReceivedEntry>()
                    .Count(e => e.Receiver == card.Owner.Creature && e.Result.UnblockedDamage > 0)
            )
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack
    ];
    
    // 金边高亮：血量低于50%时高亮
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (CombatState == null) return false;
            return Owner.Creature.CurrentHp < Owner.Creature.MaxHp / 2;
        }
    }
    
    public Epinephrine() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        bool isLowHp = Owner.Creature.CurrentHp < Owner.Creature.MaxHp / 2;
        
        // 第一次攻击
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        
        // 血量低于50%时额外攻击一次
        if (isLowHp)
        {            
            await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
    }
    
    protected override void OnUpgrade()
    {
        // 升级：基础伤害 5 → 8
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}