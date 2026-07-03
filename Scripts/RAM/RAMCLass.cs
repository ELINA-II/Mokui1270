using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat.History;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.RAM;

public static class RAMClass
{
    // ==================== 常量 ====================
    private const int DefaultMaxRAM = 10;

    // ==================== 本地状态 ====================
    private static bool _recoveryEnabled = true;
    private static readonly List<Func<PlayerChoiceContext, Task>> _pendingTriggers = new();

    // ==================== 事件 ====================
    public static event Action<int>? OnRAMGained;
    public static event Action<int>? OnRAMUsed;
    public static event Action? OnRAMRecover;
    public static event Action<Player, int, int>? OnChanged;

    // ==================== 辅助方法 ====================

    private static CombatHistory? GetHistory() => CombatManager.Instance?.History;

    private static ICombatState? GetCombatState(Player player) => player.Creature?.CombatState;

    private static RAMChangedEntry? GetLastRAMEntry(Player player)
    {
        var history = GetHistory();
        if (history == null || player?.Creature == null) return null;

        return history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature)
            .LastOrDefault();
    }

    // ✅ 内联查询：直接从 CombatHistory 获取当前 RAM
    private static int QueryCurrentRAM(Player player)
    {
        var history = GetHistory();
        if (history == null || player?.Creature == null) return DefaultMaxRAM;

        var lastEntry = history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature)
            .LastOrDefault();

        return lastEntry?.NewRAM ?? DefaultMaxRAM;
    }

    // ✅ 内联查询：直接从 CombatHistory 获取最大 RAM
    private static int QueryMaxRAM(Player player)
    {
        var history = GetHistory();
        if (history == null || player?.Creature == null) return DefaultMaxRAM;

        var lastEntry = history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature)
            .LastOrDefault();

        return lastEntry?.MaxRAM ?? DefaultMaxRAM;
    }

    // ✅ 内联查询：本回合 RAM 使用量
    private static int QueryRAMUsedThisTurn(Player player)
    {
        var history = GetHistory();
        var combatState = GetCombatState(player);
        if (history == null || combatState == null || player?.Creature == null) return 0;

        return history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature && e.HappenedThisTurn(combatState))
            .Sum(e => e.Delta < 0 ? -e.Delta : 0);
    }

    // ✅ 内联查询：本回合 RAM 恢复次数
    private static int QueryRAMRecoverCountThisTurn(Player player)
    {
        var history = GetHistory();
        var combatState = GetCombatState(player);
        if (history == null || combatState == null || player?.Creature == null) return 0;

        return history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature && e.HappenedThisTurn(combatState))
            .Count(e => e.Delta > 0 && e.ChangeReason != "Set");
    }

    // ✅ 内联查询：超载次数
    private static int QueryRAMOverloadCount(Player player)
    {
        var history = GetHistory();
        if (history == null || player?.Creature == null) return 0;

        return history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature && e.ChangeReason == "Overload")
            .Count();
    }

    // ✅ 内联查询：最后打出的 RAM 卡牌
    private static CardModel? QueryLastRAMCardPlayed(Player player)
    {
        var history = GetHistory();
        if (history == null || player?.Creature == null) return null;

        var lastEntry = history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature && e.ChangeReason == "CardPlayed")
            .LastOrDefault();

        return lastEntry?.CardSource;
    }

    // ==================== 公共 GET 方法 ====================

    public static int GetCurrentRAM(Player? player)
    {
        if (player == null) return DefaultMaxRAM;
        return QueryCurrentRAM(player);
    }

    public static int GetMaxRAM(Player? player)
    {
        if (player == null) return DefaultMaxRAM;
        return QueryMaxRAM(player);
    }

    public static int GetRAMUsedThisTurn(Player? player)
    {
        if (player == null) return 0;
        return QueryRAMUsedThisTurn(player);
    }

    public static int GetRAMRecoverCountThisTurn(Player? player)
    {
        if (player == null) return 0;
        return QueryRAMRecoverCountThisTurn(player);
    }

    public static int GetRAMOverloadCount(Player? player)
    {
        if (player == null) return 0;
        return QueryRAMOverloadCount(player);
    }

    public static int GetSlotAUsedCount(Player? player)
    {
        if (player?.Creature == null) return 0;

        var history = GetHistory();
        if (history == null) return 0;

        return history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature && e.ChangeReason == "SlotA")
            .Count();
    }

    public static int GetSlotBUsedCount(Player? player)
    {
        if (player?.Creature == null) return 0;

        var history = GetHistory();
        if (history == null) return 0;

        return history.Entries
            .OfType<RAMChangedEntry>()
            .Where(e => e.Actor == player.Creature && e.ChangeReason == "SlotB")
            .Count();
    }

    public static CardModel? GetLastRAMCardPlayed(Player? player)
    {
        if (player == null) return null;
        return QueryLastRAMCardPlayed(player);
    }

    public static float GetRAMPercent(Player player)
    {
        int max = GetMaxRAM(player);
        if (max <= 0) return 0f;
        return (float)GetCurrentRAM(player) / max;
    }

    public static int GetUsedRAM(Player player)
    {
        return GetMaxRAM(player) - GetCurrentRAM(player);
    }

    public static bool IsRecoveryEnabled() => _recoveryEnabled;

    /// <summary>
    /// 检查玩家是否有足够的RAM
    /// </summary>
    public static bool HasRAM(int amount, Player player)
    {
        if (amount <= 0) return true;
        return GetCurrentRAM(player) >= amount;
    }

    public static bool IsEmptyRAM(Player player) => !HasRAM(1, player);

    public static bool IsFullRAM(Player player)
    {
        return GetCurrentRAM(player) >= GetMaxRAM(player);
    }

    // ==================== SET 方法 ====================

    public static void SetLastRAMCardPlayed(Player player, CardModel? card)
    {
        if (player?.Creature == null) return;

        var combatState = GetCombatState(player);
        var history = GetHistory();
        if (combatState != null && history != null)
        {
            history.RecordRAMChanged(
                combatState,
                player.Creature,
                GetCurrentRAM(player),
                GetMaxRAM(player),
                0,
                "CardPlayed",
                card
            );
        }
    }

    public static void AddRAMOverloadCount(Player? player)
    {
        // 保留但不使用
    }

    public static int SetMaxRAM(Player? player, int maxRAM)
    {
        if (player?.Creature == null) return DefaultMaxRAM;

        maxRAM = Math.Clamp(maxRAM, 1, 20);
        int currentRAM = GetCurrentRAM(player);

        if (currentRAM > maxRAM)
        {
            currentRAM = maxRAM;
        }

        var combatState = GetCombatState(player);
        var history = GetHistory();
        if (combatState != null && history != null)
        {
            history.RecordRAMChanged(
                combatState,
                player.Creature,
                currentRAM,
                maxRAM,
                0,
                "SetMax",
                null
            );
        }

        OnChanged?.Invoke(player, currentRAM, maxRAM);
        return maxRAM;
    }

    public static void IncreaseMaxRAM(Player? player, int increment)
    {
        if (player == null) return;
        int newMax = GetMaxRAM(player) + increment;
        SetMaxRAM(player, newMax);
    }

    public static void SetRecoveryEnabled(bool enabled)
    {
        _recoveryEnabled = enabled;
    }

    public static void DoRAMRecover(Player? player, bool isPassive = false)
    {
        // 通过 RecoverRAM 方法处理
    }

    // ==================== 核心操作 ====================

    public static async Task SetRAM(PlayerChoiceContext choiceContext, int amount, bool isRecover, Player player)
    {
        if (player?.Creature == null) return;

        int maxRAM = GetMaxRAM(player);
        int oldRAM = GetCurrentRAM(player);
        int newRAM = Math.Clamp(amount, 0, maxRAM);
        int delta = newRAM - oldRAM;

        var combatState = GetCombatState(player);
        var history = GetHistory();
        if (combatState != null && history != null)
        {
            string reason = isRecover ? "Recover" : "Set";
            history.RecordRAMChanged(
                combatState,
                player.Creature,
                newRAM,
                maxRAM,
                delta,
                reason,
                null
            );
        }

        if (delta > 0)
        {
            OnRAMGained?.Invoke(delta);
            if (isRecover) OnRAMRecover?.Invoke();
        }
        else if (delta < 0)
        {
            OnRAMUsed?.Invoke(-delta);
        }

        OnChanged?.Invoke(player, newRAM, maxRAM);
    }

    public static async Task RecoverRAM(PlayerChoiceContext choiceContext, int amount, Player player, bool isPassive = false)
    {
        if (!_recoveryEnabled || amount <= 0 || player?.Creature == null) return;

        int currentRAM = GetCurrentRAM(player);
        int maxRAM = GetMaxRAM(player);
        int newRAM = Math.Min(currentRAM + amount, maxRAM);

        await SetRAM(choiceContext, newRAM, true, player);
    }

    /// <summary>
    /// 消耗RAM，如果RAM不足则返回false（卡牌无法打出）
    /// </summary>
    public static async Task<bool> ConsumeRAM(PlayerChoiceContext choiceContext, int amount, Player player)
    {
        if (player?.Creature == null) return false;

        int currentRAM = GetCurrentRAM(player);

        // 如果RAM不足，直接返回false
        if (currentRAM < amount)
        {
            return false;
        }

        // RAM充足，正常消耗
        await LoseRAMInternal(choiceContext, amount, player);
        return true;
    }

    public static void ModifyRAMDirect(Player player, int delta)
    {
        if (player?.Creature == null) return;

        int currentRAM = GetCurrentRAM(player);
        int maxRAM = GetMaxRAM(player);
        int newRAM = Math.Clamp(currentRAM + delta, 0, maxRAM);
        int actualDelta = newRAM - currentRAM;

        if (actualDelta == 0) return;

        var combatState = GetCombatState(player);
        var history = GetHistory();
        if (combatState != null && history != null)
        {
            string reason = actualDelta > 0 ? "Recover" : "Consume";
            history.RecordRAMChanged(
                combatState,
                player.Creature,
                newRAM,
                maxRAM,
                actualDelta,
                reason,
                null
            );
        }

        if (actualDelta > 0)
            OnRAMGained?.Invoke(actualDelta);
        else
            OnRAMUsed?.Invoke(-actualDelta);

        OnChanged?.Invoke(player, newRAM, maxRAM);
    }

    // ==================== 内部方法 ====================

    private static async Task LoseRAMInternal(PlayerChoiceContext choiceContext, int amount, Player player)
    {
        if (amount <= 0 || player?.Creature == null) return;

        int currentRAM = GetCurrentRAM(player);
        int maxRAM = GetMaxRAM(player);
        int newRAM = Math.Max(0, currentRAM - amount);
        int delta = newRAM - currentRAM;

        var combatState = GetCombatState(player);
        var history = GetHistory();
        if (combatState != null && history != null)
        {
            history.RecordRAMChanged(
                combatState,
                player.Creature,
                newRAM,
                maxRAM,
                delta,
                "Consume",
                null
            );
        }

        OnRAMUsed?.Invoke(-delta);
        OnChanged?.Invoke(player, newRAM, maxRAM);
    }

    // ==================== 回合/战斗重置 ====================

    public static void ResetForTurnStart(Player player)
    {
        _recoveryEnabled = true;
        OnChanged?.Invoke(player, GetCurrentRAM(player), GetMaxRAM(player));
    }

    public static void ResetFull(Player player)
    {
        if (player?.Creature == null) return;

        _recoveryEnabled = true;
        _pendingTriggers.Clear();

        var combatState = GetCombatState(player);
        var history = GetHistory();
        if (combatState != null && history != null)
        {
            history.RecordRAMChanged(
                combatState,
                player.Creature,
                DefaultMaxRAM,
                DefaultMaxRAM,
                0,
                "Reset",
                null
            );
        }

        OnChanged?.Invoke(player, DefaultMaxRAM, DefaultMaxRAM);
    }

    public static void ClearAll()
    {
        _recoveryEnabled = true;
        _pendingTriggers.Clear();
    }

    // ==================== 触发器队列 ====================

    public static void QueueCountdownTrigger(Func<PlayerChoiceContext, Task> trigger)
    {
        _pendingTriggers.Add(trigger);
    }

    public static async Task ProcessPendingTriggers(PlayerChoiceContext choiceContext)
    {
        foreach (var trigger in _pendingTriggers)
        {
            await trigger(choiceContext);
        }
        _pendingTriggers.Clear();
    }
}