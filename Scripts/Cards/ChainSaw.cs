using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
    [Pool(typeof(Mokui1270CardPool))]
    public class ChainSaw : AbstractMokui1270Card
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>[
            new HpLossVar(5),
            new DamageVar(2, ValueProp.Move | ValueProp.Unblockable),
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m)
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.BloodAttack];

        public ChainSaw() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies, true)
    {
        isBlood = true;
    }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = Owner;
            var creature = player?.Creature;
            
            if (creature == null) return;
            
            // 1. 对自己造成伤害
            if (creature.CurrentHp > 5)
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);
            }
            else
            {
                return;
            }
            
            // 2. 获取所有敌人
            var combatState = creature.CombatState;
            if (combatState == null) return;
            
            var enemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
            int enemyCount = enemies.Count;
            
            if (enemyCount == 0) return;
            
            // 3. 对全体敌人造成3次伤害
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .WithHitCount(3)
                    .FromCard(this)
			        .TargetingAllOpponents(CombatState!)
                    .Execute(choiceContext);
            
            // 4. 每有一个敌人回复生命
            int totalHeal = enemyCount * 5;
            await CreatureCmd.Heal(Owner.Creature,totalHeal);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }

        public CardModel GetTranscendenceTransformedCard()
	    {
		return (CardModel)(object)ModelDb.Card<ChainSawPlus>();
	    }
    }