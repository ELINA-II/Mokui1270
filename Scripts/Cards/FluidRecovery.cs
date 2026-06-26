using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class FluidRecovery : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new PowerVar<FluidRecoveryPower>(1m)  // 1层
    };
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FluidRecoveryPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack
        ];
   
    
    public FluidRecovery() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {   
        await PowerCmd.Apply<FluidRecoveryPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["FluidRecoveryPower"].BaseValue,
            Owner.Creature,
            this
        );
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}