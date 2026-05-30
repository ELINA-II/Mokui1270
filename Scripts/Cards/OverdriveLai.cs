using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class OverdriveLai : AbstractMokui1270Card
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override bool HasEnergyCostX => true;

    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FluidRecoveryPower>(),
        HoverTipFactory.FromPower<BloodThirstyPower>()
    ];

    public OverdriveLai() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        int num = ResolveEnergyXValue();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(num)
            .FromCard(this)
			.TargetingAllOpponents(CombatState!)
			.Execute(choiceContext); 
            if (Owner.HasPower<FluidRecoveryPower>() | Owner.HasPower<BloodThirstyPower>())
            {
                await CreatureCmd.Heal(Owner.Creature,num*2);
            }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

}