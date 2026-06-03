using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.Orbs;

public class AttackUav : CustomOrbModel
{
    // 被动效果数值
    public override decimal PassiveVal => ModifyOrbValue(5);

    // 激发效果数值
    public override decimal EvokeVal => ModifyOrbValue(11);

    // 暗色
    public override Color DarkenedColor => new(0.0f, 0.502f, 1.0f);

    // 不出现在随机球池中
    public override bool IncludeInRandomPool => false;

    // 提示图标路径
    public override string? CustomIconPath => "res://Mokui1270/images/orbs/AttackUav.png";

    // 自定义场景
    public override Node2D? CreateCustomSprite()
    {
        return PreloadManager.Cache.GetScene("res://Mokui1270/images/orbs/AttackUav.tscn").Instantiate<Node2D>();
    }

    // 回合开始时触发被动（无目标，随机/自动选择）
    public override async Task AfterTurnStartOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    // 触发被动（支持指定目标）
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();
        
        // 获取目标：有目标则用指定目标，否则自动选择最弱的敌人
        Creature attackTarget = target ?? GetWeakestEnemy()!;
        
        if (attackTarget == null) return;
        
        await CreatureCmd.Damage(choiceContext, attackTarget, PassiveVal, ValueProp.Unpowered, Owner.Creature);
    }

    // 触发激发，返回受影响的角色
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        IReadOnlyList<Creature> hittableEnemies = CombatState.HittableEnemies;
		if (hittableEnemies.Count == 0)
		{
			return Array.Empty<Creature>();
		}
		Creature weakestEnemy = hittableEnemies.MinBy((Creature c) => c.CurrentHp)!;
		await CreatureCmd.Damage(playerChoiceContext, weakestEnemy, EvokeVal, ValueProp.Unpowered,Owner.Creature);
		return [weakestEnemy];
    }

    /// <summary>
    /// 获取当前最弱的敌人（血量最低）
    /// </summary>
    private Creature? GetWeakestEnemy()
    {
        IReadOnlyList<Creature> hittableEnemies = CombatState.HittableEnemies;
        return hittableEnemies.Count == 0 ? null : hittableEnemies.MinBy(c => c.CurrentHp);
    }
}