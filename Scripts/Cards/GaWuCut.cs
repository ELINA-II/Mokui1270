using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class GaWuCut : AbstractMokui1270Card
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.BloodAttack];


    protected override IEnumerable<IHoverTip> ExtraHoverTips =>[
    HoverTipFactory.FromPower<HardToKillPower>(),  
    HoverTipFactory.FromPower<HardenedShellPower>(),
    ];

    public GaWuCut() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isBlood = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        if (cardPlay.Target!.HasPower<HardToKillPower>())
		{
			await PowerCmd.Remove<HardToKillPower>(cardPlay.Target);
		}
		if (cardPlay.Target.HasPower<HardenedShellPower>())
		{
			await PowerCmd.Remove<HardenedShellPower>(cardPlay.Target);
		}
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}