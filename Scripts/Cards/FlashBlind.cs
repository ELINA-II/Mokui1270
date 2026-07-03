using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class FlashBlind : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const int ramCost = 5;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new EnergyVar(energyCost),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
        CardKeyword.Exhaust,
    ];

    public FlashBlind() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        // 1. 消耗 RAM
        bool success = await SpendRAM(choiceContext);
        if (!success)
        {
            // RAM 不足，卡牌无法打出（IsPlayable 已经阻止了这种情况）
            return;
        }
        await CreatureCmd.Stun(cardPlay.Target!);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Ram"].UpgradeValueBy(-2);
    }
}