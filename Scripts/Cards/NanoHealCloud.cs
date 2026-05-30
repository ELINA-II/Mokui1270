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
public class NanoHealCloud : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AnyPlayer;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<RegenPower>(5),
        new EnergyVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<RegenPower>(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
        CardKeyword.Exhaust,
    ];


    public NanoHealCloud() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isNanomachine = true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        await PowerCmd.Apply<RegenPower>(cardPlay.Target!,DynamicVars["RegenPower"].BaseValue,Owner.Creature, this);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}