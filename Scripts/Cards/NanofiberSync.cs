using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class NanofiberSync : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.AllAllies;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(12),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
        CardKeyword.Exhaust,
    ];


    public NanofiberSync() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isNanomachine = true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        var allPlayers = CombatState!.Players
            .Where(p => p.Creature.IsAlive)
            .Select(p => p.Creature)
            .ToList();      
        // 给所有玩家回复血量
        foreach (var player in allPlayers)
        {
            await CreatureCmd.Heal(player,DynamicVars.Heal.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}