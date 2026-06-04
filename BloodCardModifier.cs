using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;

namespace Mokui1270.BloodCostSystem
{
    /// <summary>
    /// 标记卡牌为血系卡牌的接口
    /// </summary>
    public interface IBloodCard
    {
        /// <summary>
        /// 每点缺失能量消耗的生命值，默认5
        /// </summary>
        int HpPerMissingEnergy => 5;
    }

    /// <summary>
    /// 血系卡牌修饰器 - 核心逻辑
    /// </summary>
    public class BloodCostModifier : CardModifier
    {
        /// <summary>
        /// 本场战斗是否已使用过（用于限制每场战斗一次）
        /// </summary>
        private CombatState? _usedInCombat;
        
        /// <summary>
        /// 本次打出缺少的能量值
        /// </summary>
        private int _missingEnergy;

        // Priority 在父类中是普通属性，不能直接 override
        // 需要在构造函数中设置，或者使用 new 关键字隐藏
        public new int Priority 
        { 
            get => base.Priority;
            set => base.Priority = value;
        }

        public BloodCostModifier()
        {
            base.Priority = -100; // 高优先级，确保在费用计算前生效
        }

        public override void OnInitialApplication()
        {
            // 可以在这里添加额外的初始化逻辑
            // 比如修改卡牌描述，显示血系效果
        }

        public override void ModifyDescription(Creature? target, ref string description)
        {
            description += $"\n[color=yellow]血系：可用生命值替代不足的能量（每点5生命）[/color]";
        }

        /// <summary>
        /// 检查是否可以在本场战斗中再次使用
        /// </summary>
        private bool CanUseInCurrentCombat(CombatState combatState)
        {
            return _usedInCombat != combatState;
        }

        /// <summary>
        /// 标记已在当前战斗中使用
        /// </summary>
        private void MarkUsed(CombatState combatState)
        {
            _usedInCombat = combatState;
        }

        /// <summary>
        /// 计算缺少的能量和需要消耗的生命值
        /// </summary>
        private (int missingEnergy, int hpCost) CalculateBloodCost(PlayerCombatState playerCombatState, CardModel card)
        {
            int originalCost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            int currentEnergy = playerCombatState.Energy;
            int missingEnergy = Math.Max(0, originalCost - currentEnergy);
            
            int hpCost = missingEnergy * GetHpPerEnergy(card);
            return (missingEnergy, hpCost);
        }

        private int GetHpPerEnergy(CardModel card)
        {
            // 如果卡牌实现了 IBloodCard 接口，使用自定义值
            if (card is IBloodCard bloodCard)
                return bloodCard.HpPerMissingEnergy;
            return 5; // 默认值
        }

        /// <summary>
        /// 核心方法：重写资源检查逻辑
        /// </summary>
        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = Owner?.Owner;
            if (player?.Creature == null || !player.Creature.IsAlive)
                return;

            var combatState = player.Creature.CombatState;
            if (combatState == null)
                return;

            // 每场战斗只能触发一次血系效果
            if (!CanUseInCurrentCombat(combatState))
                return;

            var playerCombatState = player.PlayerCombatState;
            if (playerCombatState == null)
                return;

            // 计算血系消耗
            var (missingEnergy, hpCost) = CalculateBloodCost(playerCombatState, Owner!);
            
            if (missingEnergy <= 0)
                return;

            // 检查生命值是否足够
            if (player.Creature.CurrentHp <= hpCost)
            {
                // 生命不足，无法使用血系效果，让正常能量检查失败
                return;
            }

            // 记录缺少的能量（用于后续补充）
            _missingEnergy = missingEnergy;
            
            // 临时增加能量，让 SpendResources 能够正常扣除
            playerCombatState.GainEnergy(missingEnergy);
            
            // 标记已使用
            MarkUsed(combatState);
            
            // 造成生命值伤害
            await ApplyBloodDamage(player, player.Creature, Owner!, hpCost, combatState);
        }

        private static async Task ApplyBloodDamage(Player player, Creature creature, CardModel card, int damageAmount, CombatState combatState)
        {
            try
            {
                ulong localPlayerId = LocalContext.NetId ?? player.NetId;
                var choiceContext = new HookPlayerChoiceContext(card, localPlayerId, combatState, GameActionType.Combat);
                
                await CreatureCmd.Damage(choiceContext, creature, damageAmount, 
                    ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
            }
            catch
            {
                creature.SetCurrentHpInternal(creature.CurrentHp - damageAmount);
            }
        }

        /// <summary>
        /// 保存数据到存档
        /// </summary>
        public override void StoreSaveData(ModifierSave save)
        {
            base.StoreSaveData(save);
            // _usedInCombat 不应该持久化，它是运行时状态
            // 如果需要保存 _missingEnergy，可以在这里添加
            save.IntProperties["missingEnergy"] = _missingEnergy;
        }

        /// <summary>
        /// 从存档加载数据
        /// </summary>
        public override void LoadSaveData(ModifierSave save)
        {
            base.LoadSaveData(save);
            // 加载持久化数据
            if (save.IntProperties.TryGetValue("missingEnergy", out var value))
            {
                _missingEnergy = value;
            }
        }

        /// <summary>
        /// 克隆后的处理
        /// </summary>
        public override void AfterClonedOnCard(CardModel card)
        {
            base.AfterClonedOnCard(card);
            // 重置战斗使用标记（克隆的卡牌是新的实例）
            _usedInCombat = null;
            _missingEnergy = 0;
        }
    }

    /// <summary>
    /// 辅助方法扩展
    /// </summary>
    public static class BloodCardExtensions
    {
        /// <summary>
        /// 将卡牌标记为血系卡牌
        /// </summary>
        public static void MakeBloodCard(this CardModel card, int hpPerMissingEnergy = 5)
        {
            // 检查是否已有血系修饰器
            var existing = CardModifier.Modifiers(card).OfType<BloodCostModifier>().FirstOrDefault();
            if (existing != null)
                return;
            
            CardModifier.AddModifier(card, new BloodCostModifier());
        }

        /// <summary>
        /// 检查卡牌是否为血系卡牌
        /// </summary>
        public static bool IsBloodCard(this CardModel card)
        {
            return CardModifier.Modifiers(card).Any(m => m is BloodCostModifier);
        }
    }
}