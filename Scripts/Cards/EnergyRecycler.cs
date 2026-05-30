using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class EnergyRecycler : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(2m)];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>[
        CardKeyword.Unplayable,
        MyKeyWords.BloodAttack
    ];
    
    public EnergyRecycler() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
        isBlood = true;
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (!IsInHand() || card == this) return;
        await HealPlayer();
    }
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (!IsInHand() || cardPlay.Card == this) return;
        await HealPlayer();
    }
    
    /// <summary>
    /// 回合结束时，如果这张牌还在手牌中，将其弃掉
    /// </summary>
    public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        if (!IsInHand()) return;
        
        // 弃掉这张牌
        var discardPile = PileType.Discard.GetPile(Owner);
        await CardPileCmd.Add(this, discardPile);
    }
    
    private async Task HealPlayer()
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
    }
    
    private bool IsInHand() => Pile?.Type == PileType.Hand;
    
    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(1m);
    }
}