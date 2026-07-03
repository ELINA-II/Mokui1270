using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class BloodThirsty : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new PowerVar<BloodThirstyPower>(2m)  // 1层
    };
    
    public BloodThirsty() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BloodThirstyPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BloodThirstyPower"].BaseValue,
            Owner.Creature,
            this
        );
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}