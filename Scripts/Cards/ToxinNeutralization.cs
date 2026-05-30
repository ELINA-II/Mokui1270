using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;


namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class ToxinNeutralization : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(15)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack,
        CardKeyword.Exhaust
    ];
    
    
    public ToxinNeutralization() : base(energyCost, type, rarity, targetType, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 失去生命
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
        
        // 2. 清除所有负面效果
        await RemoveAllDebuffs(Owner.Creature);
    }
    
    /// <summary>
    /// 清除目标身上的所有负面效果（Type == PowerType.Debuff）
    /// </summary>
    private async Task RemoveAllDebuffs(Creature target)
    {
        // 获取所有 Type 为 Debuff 的 Power
        var debuffs = target.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .ToList();
        
        // 逐个移除
        foreach (var debuff in debuffs)
        {
            await PowerCmd.Remove(debuff);
        }
    }
    
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}