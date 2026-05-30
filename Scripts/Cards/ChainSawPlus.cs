using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
    [Pool(typeof(Mokui1270CardPool))]
    public class ChainSawPlus : AbstractMokui1270Card
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>[
            new HpLossVar(5),
            new DamageVar(2, ValueProp.Move | ValueProp.Unblockable),
            new HealVar(6),
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m)
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeyWords.BloodAttack];

        public ChainSawPlus() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies, true)
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

            var rng = combatState.RunState.Rng.Niche;
        
        
            foreach (var enemy in enemies)
            {
            // 获取敌人身上的所有正面效果（Buff 类型的能力）
            var buffs = enemy.Powers.Where(p => p.Type == PowerType.Buff).ToList();
            
            if (buffs.Count > 0)
            {
                // 使用 NextInt(minInclusive, maxExclusive) 随机选择一个
                int randomIndex = rng.NextInt(0, buffs.Count);
                var buffToRemove = buffs[randomIndex];
                
                // 移除这个正面效果
                await PowerCmd.Remove(buffToRemove);
            }
            }
            
            // 3. 对全体敌人造成3次伤害
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .WithHitCount(3)
                    .FromCard(this)
			        .TargetingAllOpponents(CombatState!)
                    .Execute(choiceContext);
            
            // 4. 每有一个敌人回复6生命
            decimal totalHeal = enemyCount * DynamicVars.Heal.BaseValue;
            await CreatureCmd.Heal(Owner.Creature,totalHeal);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3);
            EnergyCost.UpgradeBy(-1);
        }
    }