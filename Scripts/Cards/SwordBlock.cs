using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class SwordBlock : AbstractMokui1270Card
{
    private const int BASE_BLOCK = 20;
    private const double HP_THRESHOLD = 0.3;  // 30%
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new BlockVar(BASE_BLOCK, ValueProp.Move),
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack
    ];
    
    public SwordBlock() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);  
        bool isLowHealth = IsHealthBelowThreshold();
        if (isLowHealth)
        {
            await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);  
        }
        // 结束玩家回合（参考 VoidForm）
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }
    
    private bool IsHealthBelowThreshold()
    {
        int currentHp = Owner.Creature.CurrentHp;
        int maxHp = Owner.Creature.MaxHp;
        return currentHp < maxHp * HP_THRESHOLD;
    }
    
    protected override void OnUpgrade()
    {
        // 升级：增加基础格挡 20 → 25
        DynamicVars.Block.UpgradeValueBy(5);
    }
}