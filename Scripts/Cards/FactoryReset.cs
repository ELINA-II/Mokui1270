using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class FactoryReset : AbstractMokui1270Card
{    

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new MaxHpVar(1m),
        new CardsVar(2),
    ];
    
    public FactoryReset() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature,DynamicVars.MaxHp.BaseValue, isFromCard: true);
        // 1. 获取消耗牌堆中的卡牌
        var exhaustPile = PileType.Exhaust.GetPile(Owner);
        if (exhaustPile == null || exhaustPile.Cards.Count == 0)
        {
            // 消耗牌堆为空，无法选择
            return;
        }

        // 2. 让玩家从消耗牌堆中选择至多3张牌
        var selectedCards = await SelectCardsFromExhaust(choiceContext);
        
        if (selectedCards == null || selectedCards.Count == 0) return;

        // 3. 将选中的卡牌移回抽牌堆
        await CardPileCmd.Add(selectedCards, PileType.Draw);
    }

    private async Task<List<CardModel>> SelectCardsFromExhaust(PlayerChoiceContext choiceContext)
    {
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.ExhaustSelectionPrompt,
            1,  // min: 至少选1张
            3   // max: 最多选3张
        )
        {
            Cancelable = true,
        };

        var selected = await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Exhaust.GetPile(Owner),
            Owner,
            prefs,
            filter: card => card is not RoundslashI
                         && card is not RoundslashTwo
                         && card is not RoundslashThree
        );

        return selected.ToList();
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}