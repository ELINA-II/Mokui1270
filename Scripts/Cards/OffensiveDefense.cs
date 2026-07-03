using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class OffensiveDefense : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const int ramCost = 4;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targrtType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private int StrengthNum = 0;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new EnergyVar(energyCost),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM"),
        new PowerVar<VulnerablePower>(2),
        ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public OffensiveDefense() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
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
        StrengthNum = Owner.Creature.Block/8;
        await CreatureCmd.LoseBlock(Owner.Creature, Owner.Creature.Block);
        await PowerCmd.Apply<StrengthPower>(choiceContext,Owner.Creature,StrengthNum,Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext,Owner.Creature,DynamicVars.Vulnerable.BaseValue,Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);        
    }
}