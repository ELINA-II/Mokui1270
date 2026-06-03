using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Orbs;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class RandomBuild : AbstractMokui1270Card
{
    private const int ENERGY_COST = 0;
    private const CardType TYPE = CardType.Skill;
    private const CardRarity RARITY = CardRarity.Common;
    private const TargetType TARGET_TYPE = TargetType.Self;

    public RandomBuild() : base(ENERGY_COST, TYPE, RARITY, TARGET_TYPE, true)
    {
        isNanomachine = true;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromOrb<AttackUav>(),
        HoverTipFactory.FromOrb<DefendUav>(),
        HoverTipFactory.FromOrb<HealUav>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {        
         var rng = CombatState?.RunState.Rng.Niche;
        if (rng == null) return;
        
        // 随机选择一个选项（0, 1, 2）
        int randomIndex = rng.NextInt(0, 3);
        
        // 根据随机结果执行效果
        switch (randomIndex)
        {
            case 0:
                await AttackBuild(choiceContext);
                break;
            case 1:
                await DefendBuild(choiceContext);
                break;
            case 2:
                await HealBuild(choiceContext);
                break;
        }
    }
    private async Task AttackBuild(PlayerChoiceContext choiceContext)
    {
        await OrbCmd.Channel<AttackUav>(choiceContext,Owner);
    }
    private async Task DefendBuild(PlayerChoiceContext choiceContext)
    {
        await OrbCmd.Channel<DefendUav>(choiceContext,Owner);
    }
    private async Task HealBuild(PlayerChoiceContext choiceContext)
    {
        await OrbCmd.Channel<HealUav>(choiceContext,Owner);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

     public static async Task<CardModel?> CreateInHand(Player owner, CombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }
    
    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, CombatState combatState)
    {
        var randombuild = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            randombuild.Add(combatState.CreateCard<RandomBuild>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(randombuild, PileType.Hand, addedByPlayer: true);
        return randombuild;
    }
}