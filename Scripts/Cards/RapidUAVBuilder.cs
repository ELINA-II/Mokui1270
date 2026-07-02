using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class RapidUAVBuilder : AbstractMokui1270Card
{
    private const int ENERGY_COST = 1;
    private const CardType TYPE = CardType.Skill;
    private const CardRarity RARITY = CardRarity.Common;
    private const TargetType TARGET_TYPE = TargetType.Self;

    public RapidUAVBuilder() : base(ENERGY_COST, TYPE, RARITY, TARGET_TYPE, true)
    {
        isNanomachine = true;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Swarm>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {        
        var selectedCards = await SelectCardsToTransform(choiceContext);
        
        if (selectedCards == null || selectedCards.Count == 0) return;

        // 2. 检查是否至少有一张可转换的卡牌
        var transformableCards = selectedCards.Where(c => c.IsTransformable).ToList();
        if (transformableCards.Count == 0) return;

        // 3. 循环转换每张选中的卡牌
        foreach (var card in transformableCards)
        {
            // 创建蜂群出击卡牌
            var swarm = CombatState!.CreateCard<Swarm>(Owner);
            
            // 如果卡牌已升级，也升级
            if (IsUpgraded)
            {
                CardCmd.Upgrade(swarm);
            }

            // 转换选中的卡牌为蜂群出击
            await CardCmd.Transform(card, swarm);
        }
    }

    private async Task<List<CardModel>> SelectCardsToTransform(PlayerChoiceContext choiceContext)
    {
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.TransformSelectionPrompt,
            -1  // ✅ -1 表示不限数量
        )
        {
            Cancelable = false  // 不允许取消
        };

        var selected = (await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: Owner,
            filter: card => card.IsTransformable,  // 只选可转换的
            source: this
        )).ToList();

        return selected;
    }


    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}