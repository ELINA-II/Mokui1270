using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Mokui_1270.Patchs;

public static class HealTracker
{
    private static Dictionary<ulong, int> _healCounts = new Dictionary<ulong, int>();
    
    /// <summary>
    /// 记录一次治疗
    /// </summary>
    /// <param name="creature">接受治疗的生物</param>
    /// <param name="delta">治疗量</param>
    public static void RecordHeal(Creature creature, decimal delta)
    {
        if (creature.IsPlayer && delta > 0 && creature.Player != null)
        {
            ulong playerNetId = creature.Player.NetId;
            
            if (!_healCounts.ContainsKey(playerNetId))
                _healCounts[playerNetId] = 0;
            
            _healCounts[playerNetId]++;
        }
    }
    
    /// <summary>
    /// 获取指定玩家的治疗次数
    /// </summary>
    /// <param name="player">玩家对象</param>
    /// <returns>治疗次数</returns>
    public static int GetHealCount(Player player)
    {
        if (player == null) return 0;
        return _healCounts.GetValueOrDefault(player.NetId, 0);
    }
    
    /// <summary>
    /// 清除指定玩家的治疗次数
    /// </summary>
    /// <param name="player">玩家对象</param>
    public static void ClearHealCount(Player player)
    {
        if (player == null) return;
        _healCounts.Remove(player.NetId);
    }
    
    /// <summary>
    /// 清除所有玩家的治疗次数（战斗结束时调用）
    /// </summary>
    public static void ClearAll()
    {
        _healCounts.Clear();
    }
    
    /// <summary>
    /// 获取当前记录的玩家数量（调试用）
    /// </summary>
    public static int GetTrackedPlayerCount()
    {
        return _healCounts.Count;
    }
    
    /// <summary>
    /// 获取指定玩家的治疗次数（直接使用 NetId）
    /// </summary>
    public static int GetHealCountByNetId(ulong netId)
    {
        return _healCounts.GetValueOrDefault(netId, 0);
    }


    
}