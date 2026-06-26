using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class BufferProtocol : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<BufferProtocolPower>(1m)  // 1层
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.Nanomachine];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.FromPower<BufferProtocolPower>()
    };
    
    public BufferProtocol() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<BufferProtocolPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BufferProtocolPower"].BaseValue,
            Owner.Creature,
            this
        );
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}