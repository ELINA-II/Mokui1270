using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Mokui1270.Scripts.Patchs;

public static class DamageTracker
{
    private static Dictionary<ulong, int> _damageCounts = new Dictionary<ulong, int>();
    
    /// <summary>
    /// 记录一次受到伤害
    /// </summary>
    public static void RecordDamage(Creature creature, decimal delta)
    {
        if (creature.IsPlayer && delta > 0 && creature.Player != null)
        {
            ulong playerNetId = creature.Player.NetId;
            
            if (!_damageCounts.ContainsKey(playerNetId))
                _damageCounts[playerNetId] = 0;
            
            _damageCounts[playerNetId]++;
        }
    }
    
    /// <summary>
    /// 获取指定玩家的受伤次数
    /// </summary>
    public static int GetDamageCount(Player player)
    {
        if (player == null) return 0;
        return _damageCounts.GetValueOrDefault(player.NetId, 0);
    }
    
    /// <summary>
    /// 清除指定玩家的受伤次数
    /// </summary>
    public static void ClearDamageCount(Player player)
    {
        if (player == null) return;
        _damageCounts.Remove(player.NetId);
    }
    
    /// <summary>
    /// 清除所有玩家的受伤次数（战斗结束时调用）
    /// </summary>
    public static void ClearAll()
    {
        _damageCounts.Clear();
    }
    
    /// <summary>
    /// 获取当前记录的玩家数量（调试用）
    /// </summary>
    public static int GetTrackedPlayerCount()
    {
        return _damageCounts.Count;
    }
    
    /// <summary>
    /// 获取指定玩家的受伤次数（直接使用 NetId）
    /// </summary>
    public static int GetDamageCountByNetId(ulong netId)
    {
        return _damageCounts.GetValueOrDefault(netId, 0);
    }
}

// ==================== Patches ====================

[HarmonyPatch]
public static class DamageTrackerPatches
{
    /// <summary>
    /// 记录受伤 - 在 LoseHpInternal 方法后执行
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
    public static void OnDamageReceived(Creature __instance, DamageResult __result)
    {
        // 使用 DamageResult 中的 UnblockedDamage（实际伤害量）
        if (__result.UnblockedDamage > 0)
        {
            DamageTracker.RecordDamage(__instance, __result.UnblockedDamage);
        }
    }
    
    /// <summary>
    /// 战斗结束时清除记录 - Patch EndCombatInternal 方法
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CombatManager), "EndCombatInternal")]
    public static void OnCombatEnd(CombatManager __instance)
    {
        DamageTracker.ClearAll();
    }
}