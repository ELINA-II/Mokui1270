using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class EmergencyFix : AbstractMokui1270Card
{
    private const double HP_THRESHOLD = 0.5;  // 50%
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(22, ValueProp.Move),
        new PowerVar<RegenPower>(5m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
        CardKeyword.Exhaust,
    ];
    
    public EmergencyFix() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);  
        bool isLowHealth = IsHealthBelowThreshold();
        if (isLowHealth)
        {
            await PowerCmd.Apply<RegenPower>(Owner.Creature,DynamicVars["RegenPower"].BaseValue,Owner.Creature, this);
        }
    }
    
    private bool IsHealthBelowThreshold()
    {
        int currentHp = Owner.Creature.CurrentHp;
        int maxHp = Owner.Creature.MaxHp;
        return currentHp < maxHp * HP_THRESHOLD;
    }

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (CombatState == null) return false;
            return Owner.Creature.CurrentHp < Owner.Creature.MaxHp / 2;
        }
    }
    
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}