using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class CurtainCrack : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StrengthLoss", 99m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PiercingWailPower>(),
        HoverTipFactory.FromPower<ArtifactPower>(),

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
        CardKeyword.Exhaust,
    ];


    public CurtainCrack() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isNanomachine = true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (cardPlay.Target.HasPower<ArtifactPower>())
		{
			await PowerCmd.Remove<ArtifactPower>(cardPlay.Target);
		}
        await PowerCmd.Apply<PiercingWailPower>(cardPlay.Target!,DynamicVars["StrengthLoss"].BaseValue,Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
        EnergyCost.UpgradeBy(-1);
    }
}