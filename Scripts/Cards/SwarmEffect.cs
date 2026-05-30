using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Cards;
    [Pool(typeof(Mokui1270CardPool))]
    public class SwarmEffect : AbstractMokui1270Card
    {
        private const string _nanomachineCountKey = "NanomachineCount";

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(5m, ValueProp.Move),           // 基础伤害 5
            new CalculationBaseVar(1m),                  // 基础攻击次数（可选）
            new CalculationExtraVar(1m),                 // 额外攻击次数增量
            new CalculatedVar(_nanomachineCountKey).WithMultiplier((CardModel card, Creature? _) =>
            {
                // 计算手牌中有多少张 isNanomachine = true 的卡牌
                var hand = PileType.Hand.GetPile(card.Owner);
                if (hand == null) return 0;
                
                return hand.Cards.Count(c => 
                {
                    if (c is not AbstractMokui1270Card mokuiCard) return false;
                    return mokuiCard.isNanomachine;
                });
            })
        };

        public SwarmEffect() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            
            // 计算攻击次数 = 1（基础）+ 纳米机器卡牌数量
            int nanomachineCount = (int)((CalculatedVar)DynamicVars[_nanomachineCountKey]).Calculate(cardPlay.Target);
            int hitCount = 1 + nanomachineCount;
            
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitCount(hitCount)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            // 升级后基础伤害 +2
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }