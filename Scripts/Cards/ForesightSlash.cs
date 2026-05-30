using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class ForesightSlash : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.BloodAttack];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(16m, ValueProp.Move),
        new EnergyVar(2),
        new BlockVar(16m, ValueProp.Move),
    ];

    protected override bool ShouldGlowGoldInternal
	{
		get
		{
			if (CombatState == null)
			{
				return false;
			}
			return CombatState.HittableEnemies.Any((Creature e) => e.Monster?.IntendsToAttack ?? false);
		}
	}

    public ForesightSlash() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        if (cardPlay.Target!.Monster?.IntendsToAttack == true)
		{
            await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
		}
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}