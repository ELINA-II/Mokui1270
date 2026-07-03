using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.NanomachineCostSystem;
using Mokui1270.Scripts.RAM;

namespace Mokui1270.Scripts.Cards;

public abstract class AbstractMokui1270Card : CustomCardModel
{
    // ==================== 属性 ====================

    public override string PortraitPath => $"res://Mokui1270/images/cards/{GetType().Name}.png";

    public bool isBlood
    {
        get => _isBlood;
        set
        {
            if (_isBlood == value) return;
            _isBlood = value;
        }
    }
    private bool _isBlood = false;

    public bool isNanomachine
    {
        get => _isNanomachine;
        set
        {
            if (_isNanomachine == value) return;
            _isNanomachine = value;
            this.SetNanomachine(value);
        }
    }
    private bool _isNanomachine = false;

    public bool isHack
    {
        get => _isHack;
        set
        {
            if (_isHack == value) return;
            _isHack = value;
        }
    }
    private bool _isHack = false;

    // ==================== 构造函数 ====================

    protected AbstractMokui1270Card() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected AbstractMokui1270Card(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // ==================== RAM 相关方法 ====================

    protected virtual int GetRAMCost()
    {
    if (DynamicVars.TryGetValue("Ram", out var ramVar))
    {
        return (int)ramVar.BaseValue;
    }
    // 没有定义 Ram 变量，返回默认值（或 0）
    return 0;
    }
    /// <summary>
    /// 检查 RAM 是否充足（用于 IsPlayable）
    /// </summary>
    protected override bool IsPlayable
    {
        get
        {
            if (isHack)
            {
                if (Owner == null) return false;
                return RAMClass.HasRAM(GetRAMCost(), Owner);
            }
            return base.IsPlayable;
        }
    }

    /// <summary>
    /// 消耗 RAM
    /// </summary>
    protected async Task<bool> SpendRAM(PlayerChoiceContext choiceContext)
    {
        if (!isHack) return true;
        return await RAMClass.ConsumeRAM(choiceContext, GetRAMCost(), Owner);
    }

    // ==================== OnPlay（子类可自由重写） ====================

    /// <summary>
    /// 子类自由重写 OnPlay，完全不需要改代码
    /// Hack 卡牌需要在 OnPlay 中手动调用 SpendRAM
    /// </summary>
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Hack 卡牌在这里处理 RAM 消耗
        if (isHack)
        {
            bool success = await SpendRAM(choiceContext);
            if (!success)
            {
                // RAM 不足，不执行效果
                return;
            }
        }

        // 调用子类的核心效果（通过虚方法）
        await OnPlayCore(choiceContext, cardPlay);
    }

    /// <summary>
    /// 子类重写此方法来实现卡牌效果
    /// 与 OnPlay 的区别：RAM 已经消耗完毕
    /// </summary>
    protected virtual Task OnPlayCore(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    // ==================== 辅助方法（委托给 RAMClass） ====================

    protected bool HasEnoughRAM(int amount)
    {
        if (Owner == null) return false;
        return RAMClass.HasRAM(amount, Owner);
    }

    protected int GetCurrentRAM()
    {
        if (Owner == null) return 0;
        return RAMClass.GetCurrentRAM(Owner);
    }

    protected int GetMaxRAM()
    {
        if (Owner == null) return 10;
        return RAMClass.GetMaxRAM(Owner);
    }

    protected int GetUsedRAM()
    {
        if (Owner == null) return 0;
        return RAMClass.GetUsedRAM(Owner);
    }

    protected float GetRAMPercent()
    {
        if (Owner == null) return 0f;
        return RAMClass.GetRAMPercent(Owner);
    }

    protected bool IsFullRAM()
    {
        if (Owner == null) return false;
        return RAMClass.IsFullRAM(Owner);
    }

    protected bool IsEmptyRAM()
    {
        if (Owner == null) return true;
        return RAMClass.IsEmptyRAM(Owner);
    }

    protected int GetRAMUsedThisTurn()
    {
        if (Owner == null) return 0;
        return RAMClass.GetRAMUsedThisTurn(Owner);
    }

    protected int GetRAMRecoverCountThisTurn()
    {
        if (Owner == null) return 0;
        return RAMClass.GetRAMRecoverCountThisTurn(Owner);
    }

    protected int GetRAMOverloadCount()
    {
        if (Owner == null) return 0;
        return RAMClass.GetRAMOverloadCount(Owner);
    }

    protected CardModel? GetLastRAMCardPlayed()
    {
        if (Owner == null) return null;
        return RAMClass.GetLastRAMCardPlayed(Owner);
    }
}