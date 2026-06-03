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
public class NanomachineRecycleII : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<SneakyPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.FromPower<SneakyPower>()
    };

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;


    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.Nanomachine];
    
    public NanomachineRecycleII() : base(2, CardType.Power, CardRarity.Rare, TargetType.AllAllies, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var allPlayers = CombatState!.Players
            .Where(p => p.Creature.IsAlive)
            .Select(p => p.Creature)
            .ToList();      
        // 给所有玩家施加效果
        foreach (var player in allPlayers)
        {
            await PowerCmd.Apply<SneakyPower>(
            player,
            DynamicVars["SneakyPower"].BaseValue,
            Owner.Creature,
            this
        );
        }
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}