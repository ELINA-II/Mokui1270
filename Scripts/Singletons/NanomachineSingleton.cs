using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Singletons;

/// <summary>
/// 纳米机器关键词处理器
/// 当打出带有 Nanomachine 关键词的卡牌时，给所有队友恢复 1 点生命值
/// </summary>
public class NanomachineHandler : CustomSingletonModel
{
    private static readonly Logger _logger = new Logger("NanomachineHandler", LogType.GameSync);

    public NanomachineHandler() : base(HookType.Combat)
    {
         _logger.Info("NanomachineHandler initialized");
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var creature = card.Owner?.Creature;
        if (creature == null) return;

        // 检查卡牌是否有 Nanomachine 关键词
        if (!card.Keywords.Contains(MyKeyWords.Nanomachine)) return;

        var player = card.Owner;
        if (player?.Creature?.CombatState == null) return;

        // 获取所有队友（包括自己）
        var teammates = player.Creature.CombatState.Players
            .Where(p => p.Creature != null && p.Creature.IsAlive && p.Creature.Side == player.Creature.Side)
            .Select(p => p.Creature)
            .ToList();

         _logger.Info($"Nanomachine card played: {card.Id.Entry}, healing {teammates.Count} teammates");

        // 给每个队友恢复 1 点生命值
        foreach (var teammate in teammates)
        {
            await CreatureCmd.Heal(teammate, 1);
        }
    }
}