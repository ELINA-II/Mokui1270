using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;
using Mokui1270.Scripts.Patchs;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class FissionCore : AbstractMokui1270Card
{    

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromOrb<PlasmaOrb>(),
    ];
    
    public FissionCore() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyPlayer, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await OrbCmd.Channel<PlasmaOrb>(choiceContext, cardPlay.Target.Player!);
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}