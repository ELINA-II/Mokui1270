using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class BioHack : AbstractMokui1270Card
{
    private const int energyCost = 0;
    private const int ramCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new CardsVar(1),
        new EnergyVar(energyCost),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    public BioHack() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
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
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Damage.PreviewValue,ValueProp.Move | ValueProp.Move,cardPlay);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Ram"].UpgradeValueBy(-1);
    }
}