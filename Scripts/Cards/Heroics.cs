using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class Heroics : AbstractMokui1270Card
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new PowerVar<HeroicsReadyPower>(1m)  // 免死能力，1层
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack,
        CardKeyword.Ethereal
    ];
   
    public Heroics() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
        isBlood = true;
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {   
        // 添加免死能力
        await PowerCmd.Apply<HeroicsReadyPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["HeroicsReadyPower"].BaseValue,
            Owner.Creature,
            this
        );
    }
    
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}