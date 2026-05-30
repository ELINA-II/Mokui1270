using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class NanomachineRecycleI : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20, ValueProp.Move),
        new EnergyVar(3),
        new MaxHpVar(5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.Nanomachine,
    CardKeyword.Exhaust];


    public NanomachineRecycleI() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isNanomachine = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		if ((await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", null, "slash_attack.mp3")
			.Execute(choiceContext)).Results.Any((DamageResult r) => r.WasTargetKilled))
		{
			await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue,Owner);
            await CreatureCmd.GainMaxHp(Owner.Creature,DynamicVars.MaxHp.IntValue);
		}
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}