using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.RAM;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class ActOfRescue : AbstractMokui1270Card
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const int RAM_COST = 1;
    private const int BLOCK_AMOUNT = 6;
    private const int DRAW_AMOUNT = 1;
    private const int ENERGY_GAIN = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(DRAW_AMOUNT),
        new BlockVar(BLOCK_AMOUNT, ValueProp.Move),
        new EnergyVar(ENERGY_GAIN),
        new DynamicVar("Ram", RAM_COST).WithTooltip("MOKUI1270-RAM")
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    public ActOfRescue() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        isHack = true;
    }

    protected override int GetRAMCost() => RAM_COST;

    // ✅ 使用 OnPlay，手动处理 RAM
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 消耗 RAM
        bool success = await SpendRAM(choiceContext);
        if (!success)
        {
            // RAM 不足，卡牌无法打出（IsPlayable 已经阻止了这种情况）
            return;
        }

        // 2. 执行效果：获得格挡，抽牌
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block, cardPlay);
        await CardPileCmd.Draw(choiceContext, DRAW_AMOUNT, Owner);
        await Cmd.Wait(0.25f);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType result = base.GetResultPileTypeForCardPlay();
        if (result != PileType.Discard)
        {
            return result;
        }
        return PileType.Hand;
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}