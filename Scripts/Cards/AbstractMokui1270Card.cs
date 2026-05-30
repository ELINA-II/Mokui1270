using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using Mokui1270.BloodCostSystem;
using Mokui1270.NanomachineCostSystem;

namespace Mokui1270.Scripts.Cards;

public abstract class AbstractMokui1270Card : CustomCardModel
{
    public override string PortraitPath => $"res://Mokui1270/images/cards/{GetType().Name}.png";

    public bool isBlood
    {
        get => _isBlood;
        set
        {
            if(_isBlood == value) return;
            _isBlood = value;
        }
    }
    private bool _isBlood = false;
    
    public bool isNanomachine 
    { 
        get => _isNanomachine;
        set
        {
            if(_isNanomachine == value) return;
            _isNanomachine = value;
            this.SetNanomachine(value);
        }
    }
    private bool _isNanomachine = false;
    
    // 无参构造函数（游戏系统需要）
    protected AbstractMokui1270Card() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }
    
    // 带参数的构造函数
    protected AbstractMokui1270Card(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) 
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}