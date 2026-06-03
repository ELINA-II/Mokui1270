using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Mokui1270.Scripts.Orbs;

public class HealUav : CustomOrbModel
{
    // 被动效果数值，ModifyOrbValue表示是否吃集中等
    public override decimal PassiveVal => ModifyOrbValue(3);

    // 激发效果数值
    public override decimal EvokeVal => ModifyOrbValue(10);

    // 暗色，使用球的主体色的暗色调
    public override Color DarkenedColor => new(0.0f, 0.502f, 1.0f);

    // 不出现在随机球池中
    public override bool IncludeInRandomPool => false;

    // 提示图标路径
    public override string? CustomIconPath => "res://Mokui1270/images/orbs/HealUav.png";
    // 球的场景的路径。如果你使用这个，你必须要有一个名称为SpineSkeleton并且是SpineSprite类型的节点
    // public override string? CustomSpritePath => "res://test/scenes/test_orb.tscn";

    // 可以继承这个并自行搭建场景，只需父节点是Node2D即可。这样就没有上述限制。代码上优先使用这个
    public override Node2D? CreateCustomSprite()
    {
        return PreloadManager.Cache.GetScene("res://Mokui1270/images/orbs/HealUav.tscn").Instantiate<Node2D>();
    }

    // 回合开始时触发被动
    public override async Task AfterTurnStartOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    // 触发被动
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target != null)
		{
			throw new InvalidOperationException("HEALUAV cannot target creatures.");
		}
		Trigger();
		await CreatureCmd.Heal(Owner.Creature, PassiveVal);
    }

    // 触发激发，返回受影响的角色
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
		var allPlayers = CombatState.Players
            .Where(p => p.Creature.IsAlive)
            .Select(p => p.Creature)
            .ToList();
        
        if (allPlayers.Count == 0)
        {
            return Array.Empty<Creature>();
        }
        
        // 给所有玩家回复血量
        foreach (var player in allPlayers)
        {
            await CreatureCmd.Heal(player,EvokeVal);
        }
        
        // 返回受到影响的生物（被治疗的玩家）
        return allPlayers;
    }
}