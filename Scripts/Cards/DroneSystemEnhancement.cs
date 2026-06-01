using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class DroneSystemEnhancement : AbstractMokui1270Card
{    

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Nanomachine,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Focus>(),
        HoverTipFactory.FromCard<Slots>()
    ];
    
    public DroneSystemEnhancement() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
        isNanomachine = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {     
        var options = new List<CardModel>();
        var cardScope = CardScope ?? Owner.RunState;
        
        var focus = cardScope.CreateCard<Focus>(Owner);
        options.Add(focus);
        var slots = cardScope.CreateCard<Slots>(Owner);
        options.Add(slots);
        
        if (!options.Any())
        {
            return;
        }
        
        // 4. 弹出选择界面
        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            options,
            Owner,
            canSkip: false
        );
        
        // 5. 执行选中的效果
        if (selected is Focus)
        {
            await PowerCmd.Apply<FocusPower>(Owner.Creature,1,Owner.Creature, this);
        }
        else if (selected is Slots)
        {
            await OrbCmd.AddSlots(Owner,1);   
        }

        EnergyCost.AddThisCombat(1);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}