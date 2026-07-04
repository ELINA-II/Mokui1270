using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class MassiveUAVBuilder : AbstractMokui1270Card
{
    private const int ENERGY_COST = 2;
    private const CardType TYPE = CardType.Skill;
    private const CardRarity RARITY = CardRarity.Rare;
    private const TargetType TARGET_TYPE = TargetType.Self;

    // ✅ 添加 CalculatedHits 到 DynamicVars
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CalculatedHits", 0m),  // 默认值，实际会在 OnPlay 中计算
    ];

    public MassiveUAVBuilder() : base(ENERGY_COST, TYPE, RARITY, TARGET_TYPE, true)
    {
        isNanomachine = true;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取抽牌堆中的状态牌/诅咒牌/任务牌
        var statusCards = GetStatuses(Owner).ToList();
        int statusCount = statusCards.Count;  // ✅ 直接使用 Count，不需要 DynamicVar

        // 消耗所有找到的牌
        foreach (CardModel item in statusCards)
        {
            await CardCmd.Exhaust(choiceContext, item);
        }

        // 根据消耗的数量抽牌和获得能量
        await CardPileCmd.Draw(choiceContext, statusCount, Owner);
        await PlayerCmd.GainEnergy(statusCount, Owner);
    }

    private static IEnumerable<CardModel> GetStatuses(Player owner)
    {
        return owner.PlayerCombatState!.AllCards
            .Where(c => (c.Type == CardType.Status || c.Type == CardType.Curse || c.Type == CardType.Quest) 
                        && c.Pile?.Type == PileType.Draw);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}