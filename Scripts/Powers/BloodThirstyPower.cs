using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Mokui1270.Scripts.Powers;

public class BloodThirstyPower : CustomPowerModel
{
    // 借鉴：内部数据存储要处理的卡牌
    private class LifeStealData
    {
        public readonly Dictionary<CardModel, int> pendingHeals = new Dictionary<CardModel, int>();
    }
    
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;  // 可叠加

    public override string? CustomPackedIconPath => "res://Mokui1270/images/powers/BloodThirstyPower.png";
    public override string? CustomBigIconPath => "res://Mokui1270/images/powers/BloodThirstyPower.png";
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new HealVar(1)  // 每层回复1血
    };
    
    protected override object InitInternalData()
    {
        return new LifeStealData();
    }
    
    // 借鉴：在卡牌打出前记录
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // 只关心自己的卡牌
        if (cardPlay.Card.Owner.Creature != Owner) return Task.CompletedTask;
        
        // 只关心攻击牌
        if (cardPlay.Card.Type != CardType.Attack) return Task.CompletedTask;
        
        // 计算总回复量 = 每层回复量 × 层数
        int totalHeal = (int)DynamicVars.Heal.BaseValue * Amount;
        
        if (totalHeal > 0)
        {
            GetInternalData<LifeStealData>().pendingHeals.Add(cardPlay.Card, totalHeal);
        }
        
        return Task.CompletedTask;
    }
    
    // 借鉴：在卡牌打出后执行回复
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 取出之前记录的回复量
        if (GetInternalData<LifeStealData>().pendingHeals.Remove(cardPlay.Card, out var healAmount) && healAmount > 0)
        {
            // 回复生命
            await CreatureCmd.Heal(Owner, healAmount);
        }
    }
}