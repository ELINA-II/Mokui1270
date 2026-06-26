using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Mokui1270.Scripts.Orbs;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class MassiveUAVBuilder : AbstractMokui1270Card
{
    private const int ENERGY_COST = 1;
    private const CardType TYPE = CardType.Skill;
    private const CardRarity RARITY = CardRarity.Uncommon;
    private const TargetType TARGET_TYPE = TargetType.Self;

    public MassiveUAVBuilder() : base(ENERGY_COST, TYPE, RARITY, TARGET_TYPE, true)
    {
        isNanomachine = true;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromOrb<AttackUav>(),
        HoverTipFactory.FromOrb<DefendUav>(),
        HoverTipFactory.FromOrb<HealUav>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {        
        // 2. 创建选项列表
        var options = new List<CardModel>();
        var cardScope = CardScope ?? Owner.RunState;
        
        var attack = cardScope.CreateCard<AttackUavChoice>(Owner);
        options.Add(attack);
        var defend = cardScope.CreateCard<DefendUavChoice>(Owner);
        options.Add(defend);
        var heal = cardScope.CreateCard<HealUavChoice>(Owner);
        options.Add(heal);
        
        // 4. 弹出选择界面
        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            options,
            Owner,
            canSkip: false
        );
        
        // 5. 执行选中的效果
        if (selected is AttackUavChoice)
        {
            await PowerCmd.Apply<MassiveUAVBuilderAttackPower>(choiceContext,Owner.Creature,1,Owner.Creature, this);
            }
        else if (selected is DefendUavChoice)
        {
            await PowerCmd.Apply<MassiveUAVBuilderDefendPower>(choiceContext,Owner.Creature,1,Owner.Creature, this);
        }else if(selected is HealUavChoice){
            await PowerCmd.Apply<MassiveUAVBuilderHealPower>(choiceContext,Owner.Creature,1,Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}