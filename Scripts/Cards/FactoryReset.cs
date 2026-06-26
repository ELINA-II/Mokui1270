using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class FactoryReset : AbstractMokui1270Card
{    

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("OrbSlots", 1m),
		new PowerVar<StrengthPower>(3m),
		new PowerVar<FocusPower>(1m),
        new EnergyVar(1),
    ];
    
    public FactoryReset() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
		OrbCmd.RemoveSlots(Owner,DynamicVars["OrbSlots"].IntValue);
		await PowerCmd.Apply<StrengthPower>(choiceContext,Owner.Creature,DynamicVars.Strength.BaseValue,Owner.Creature, this);
		await PowerCmd.Apply<FocusPower>(choiceContext,Owner.Creature,DynamicVars["FocusPower"].BaseValue,Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}