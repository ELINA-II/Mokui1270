using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class IronGuard : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    private int teammateCount = 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.FromPower<IronGuardPower>()
    };
    
    public IronGuard() : base(energyCost, type, rarity, targetType, true)
    {
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        teammateCount = Owner.Creature.CombatState!.Players
    .Count(p => p.Creature != null && p.Creature.IsAlive && p.Creature.Side == Owner.Creature.Side);
        await PowerCmd.Apply<DashPower>(Owner.Creature,teammateCount, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}