using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.RAM;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class ThousandDagger : AbstractMokui1270Card
{
    private const int energyCost = 3;
    private const int ramCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targrtType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

     protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<SlipperyPower>(),
    ];

    public ThousandDagger() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
    new DamageVar(1, ValueProp.Move),
    new CardsVar(1),
    new EnergyVar(energyCost),
    new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM"),
    new CalculationBaseVar(0m),      // 基础值（会被乘数乘以）
    new CalculationExtraVar(2m),     // 额外乘数
    new CalculatedVar("HitCount")
        .WithMultiplier(static (card, target) => 
        {
            // 使用静态 lambda，通过 card.Owner 获取玩家
            Player player = card.Owner;
            int ram = RAMClass.GetCurrentRAM(player);
            
            // 如果目标有 SlipperyPower，返回双倍
            if (target != null && target.HasPower<SlipperyPower>())
            {
                return ram * 2;
            }
            return ram;
        })
];

protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    Player player = Owner;
    int currentRAM = RAMClass.GetCurrentRAM(player);
    
    // 获取计算后的攻击次数
    int hitCount = (int)((CalculatedVar)DynamicVars["HitCount"]).Calculate(cardPlay.Target);
    
    // 执行攻击
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .WithHitCount(hitCount)
        .FromCard(this)
        .Targeting(cardPlay.Target!)
        .Execute(choiceContext);
    
    await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    await RAMClass.ConsumeRAM(choiceContext, currentRAM, player);
}

protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(1);
}

}