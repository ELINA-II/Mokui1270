using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ValueProps;

namespace Mokui1270.Scripts.RAM;

public static class RAMClass
{
    private class PlayerRAMState
    {
        public int CurrentRAM = 10;
        public int MaxRAM = 10;
        public int RAMUsedThisTurn;
        public int RAMRecoveredThisTurn;
        public int RAMOverloadThisCombat;
        public int RAMClearedThisCombat;
        public int SlotAUsedThisCombat;
        public int SlotBUsedThisCombat;
        public CardModel? LastRAMCardPlayed;
    }

    private static int _defaultMaxRAM = 10;
    private static readonly Dictionary<Player, PlayerRAMState> _states = new();
    private static readonly PlayerRAMState _defaultState = new();
    private static readonly List<Func<PlayerChoiceContext, Task>> _pendingTriggers = new();

    private static bool _recoveryEnabled = true;//控制能否恢复RAM的开关

    // 生命替代配置：1 RAM = 6 生命
    private const int HealthPerRAM = 6;

    public static Player? CurrentRAMGainer { get; private set; }

    // 事件系统
    public static event Action<int>? OnRAMGained;
    public static event Action<int>? OnRAMUsed;
    public static event Action? OnRAMRecover;
    public static event Action<Player, int, int>? OnChanged;
    public static event Action<Player, int, int>? OnHealthSubstitution; // player, ramAmount, healthCost

    private static PlayerRAMState GetState(Player? player)
    {
        if (player == null)
            return _defaultState;

        if (!_states.TryGetValue(player, out var state))
        {
            state = new PlayerRAMState();
            _states[player] = state;
        }
        return state;
    }

    // ==================== 基础 GET 方法 ====================

    public static int GetCurrentRAM(Player? player)
    {
        return GetState(player).CurrentRAM;
    }

    public static int GetMaxRAM(Player? player)
    {
        return GetState(player).MaxRAM;
    }

    public static int GetRAMUsedThisTurn(Player? player)
    {
        return GetState(player).RAMUsedThisTurn;
    }

    public static int GetRAMRecoverCountThisTurn(Player? player)
    {
        return GetState(player).RAMRecoveredThisTurn;
    }

    public static int GetRAMOverloadCount(Player? player)
    {
        return GetState(player).RAMOverloadThisCombat;
    }

    public static int GetSlotAUsedCount(Player? player)
    {
        return GetState(player).SlotAUsedThisCombat;
    }

    public static int GetSlotBUsedCount(Player? player)
    {
        return GetState(player).SlotBUsedThisCombat;
    }

    public static CardModel? GetLastRAMCardPlayed(Player? player)
    {
        return GetState(player).LastRAMCardPlayed;
    }

    public static float GetRAMPercent(Player player)
    {
        var state = GetState(player);
        if (state.MaxRAM <= 0) return 0f;
        return (float)state.CurrentRAM / state.MaxRAM;
    }

    public static int GetUsedRAM(Player player)
    {
        var state = GetState(player);
        return state.MaxRAM - state.CurrentRAM;
    }

    public static bool IsRecoveryEnabled()
    {
        return _recoveryEnabled;
    }

    // ==================== 基础 SET 方法 ====================

    public static void SetLastRAMCardPlayed(Player player, CardModel? card)
    {
        GetState(player).LastRAMCardPlayed = card;
    }

    public static void AddRAMOverloadCount(Player? player)
    {
        GetState(player).RAMOverloadThisCombat++;
    }

    public static int SetMaxRAM(Player? player, int maxRAM)
    {
        if (maxRAM > 20)
            maxRAM = 20;
        if (maxRAM < 1)
            maxRAM = 1;

        var state = GetState(player);
        state.MaxRAM = maxRAM;
        
        if (state.CurrentRAM > maxRAM)
            state.CurrentRAM = maxRAM;

        if (player != null)
            OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);

        return GetMaxRAM(player);
    }

    public static void IncreaseMaxRAM(Player? player, int increment)
    {
        int newMax = GetMaxRAM(player) + increment;
        SetMaxRAM(player, newMax);
    }

    public static void DoRAMRecover(Player? player, bool isPassive = false)
    {
        if (!isPassive)
            GetState(player).RAMRecoveredThisTurn++;
        
        OnRAMRecover?.Invoke();
    }

    public static void SetRecoveryEnabled(bool enabled)
    {
        _recoveryEnabled = enabled;
    }

    // ==================== 检查方法 ====================

    public static bool HasRAM(int amount, Player player)
    {
        if (amount <= 0)
            return true;
        
        return GetState(player).CurrentRAM >= amount;
    }

    public static bool IsEmptyRAM(Player player)
    {
        return !HasRAM(1, player);
    }

    public static bool IsFullRAM(Player player)
    {
        var state = GetState(player);
        return state.CurrentRAM >= state.MaxRAM;
    }

    // ==================== 生命替代相关 ====================

    public static int GetHealthCostForRAM(int ramAmount, Player player)
    {
        int currentRAM = GetCurrentRAM(player);
        if (currentRAM >= ramAmount)
            return 0;
        
        int ramShortage = ramAmount - currentRAM;
        return ramShortage * HealthPerRAM;
    }

    public static bool CanPay(Player player, int ramAmount)
    {
        int currentRAM = GetCurrentRAM(player);
        if (currentRAM >= ramAmount)
            return true;
        
        int ramShortage = ramAmount - currentRAM;
        int healthCost = ramShortage * HealthPerRAM;
        int currentHealth = player.Creature.CurrentHp;
        
        return currentHealth > healthCost;
    }

    public static (int ramCost, int healthCost) GetActualCost(Player player, int requiredRAM)
    {
        int currentRAM = GetCurrentRAM(player);
        int ramCost = Math.Min(currentRAM, requiredRAM);
        int healthCost = 0;
        
        if (currentRAM < requiredRAM)
        {
            healthCost = (requiredRAM - currentRAM) * HealthPerRAM;
        }
        
        return (ramCost, healthCost);
    }

    // ==================== 同步操作（非战斗场景）====================

    public static void ModifyRAMDirect(Player player, int delta)
    {
        var state = GetState(player);
        int newValue = Math.Clamp(state.CurrentRAM + delta, 0, state.MaxRAM);
        
        if (newValue != state.CurrentRAM)
        {
            state.CurrentRAM = newValue;
            OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);
            
            if (delta > 0)
                OnRAMGained?.Invoke(delta);
            else if (delta < 0)
                OnRAMUsed?.Invoke(-delta);
        }
    }

    public static void RecoverDirect(Player player, int amount, bool isPassive = false)
    {
        var state = GetState(player);
        int newValue = Math.Clamp(state.CurrentRAM + amount, 0, state.MaxRAM);
        int actualGain = newValue - state.CurrentRAM;
        
        if (actualGain > 0)
        {
            state.CurrentRAM = newValue;
            if (!isPassive)
                state.RAMRecoveredThisTurn++;
            
            OnRAMGained?.Invoke(actualGain);
            OnRAMRecover?.Invoke();
            OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);
        }
    }

    // ==================== 异步操作（战斗场景）====================

    public static async Task SetRAM(PlayerChoiceContext choiceContext, int amount, bool isRecover, Player player)
    {
        var state = GetState(player);
        CurrentRAMGainer = player;
        int oldRAM = state.CurrentRAM;
        
        state.CurrentRAM = amount;
        
        if (state.CurrentRAM > state.MaxRAM)
            state.CurrentRAM = state.MaxRAM;
        if (state.CurrentRAM < 0)
            state.CurrentRAM = 0;

        if (amount - oldRAM > 0)
        {
            OnRAMGained?.Invoke(amount - oldRAM);
            if (isRecover)
                OnRAMRecover?.Invoke();
            OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);
        }
        
        if (amount != oldRAM)
        {
            OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);
        }
        
        CurrentRAMGainer = null;
    }

    public static async Task RecoverRAM(PlayerChoiceContext choiceContext, int amount, Player player, bool isPassive = false)
    {
        // 检查是否可以恢复
        if (!_recoveryEnabled)
            return;

        if (amount <= 0)
            return;

        var state = GetState(player);
        int newAmount = state.CurrentRAM + amount;
        await SetRAM(choiceContext, newAmount, true, player);
        
        if (!isPassive)
            DoRAMRecover(player, false);
    }

    /// <summary>
    /// 消耗 RAM（自动使用生命替代）
    /// </summary>
    public static async Task<bool> ConsumeRAM(PlayerChoiceContext choiceContext, int amount, Player player)
    {
        int currentRAM = GetCurrentRAM(player);
        
        if (currentRAM >= amount)
        {
            // RAM 足够，正常消耗
            await LoseRAMInternal(choiceContext, amount, player);
            return true;
        }
        else
        {
            // RAM 不足，用生命替代
            int ramShortage = amount - currentRAM;
            int healthCost = ramShortage * HealthPerRAM;
            int currentHealth = player.Creature.CurrentHp;
            
            if (currentHealth > healthCost)
            {
                // 消耗所有剩余 RAM
                if (currentRAM > 0)
                {
                    await LoseRAMInternal(choiceContext, currentRAM, player);
                }
                
                // 扣血
                try
                {   
                    // 造成伤害（不可格挡、无力量加成、移动类型）
                    await CreatureCmd.Damage(
                        choiceContext, 
                        player.Creature, 
                        healthCost, 
                        ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, 
                        player.Creature
                    );
                }
                catch
                {
                    // 如果伤害失败，回退到直接扣血
                    player.Creature.SetCurrentHpInternal(player.Creature.CurrentHp - healthCost);
                }
                AddRAMOverloadCount(player);
                OnHealthSubstitution?.Invoke(player, ramShortage, healthCost);
                
                return true;
            }
            
            return false;
        }
    }

    // 内部方法：实际执行 RAM 消耗（不检查是否足够）
    private static async Task LoseRAMInternal(PlayerChoiceContext choiceContext, int amount, Player player)
    {
        if (amount <= 0)
            return;

        var state = GetState(player);
        CurrentRAMGainer = player;
        int oldRAM = state.CurrentRAM;
        
        state.CurrentRAM -= amount;
        
        int actualLoss = amount;
        if (state.CurrentRAM < 0)
        {
            actualLoss = amount + state.CurrentRAM;
            state.CurrentRAM = 0;
        }
        
        if (actualLoss > 0)
        {
            for (int i = oldRAM; i > state.CurrentRAM; i--)
            {
                if (i == state.MaxRAM - 1)
                    state.SlotBUsedThisCombat++;
                if (i == state.MaxRAM)
                    state.SlotAUsedThisCombat++;
            }
            
            state.RAMUsedThisTurn += actualLoss;
            OnRAMUsed?.Invoke(actualLoss);
            OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);
        }
        
        CurrentRAMGainer = null;
    }

    // ==================== 回合/战斗重置 ====================

    public static void ResetForTurnStart(Player player)
    {
        var state = GetState(player);
         _recoveryEnabled = true;
        state.RAMUsedThisTurn = 0;
        state.RAMRecoveredThisTurn = 0;
        OnChanged?.Invoke(player, state.CurrentRAM, state.MaxRAM);
    }

    public static void ResetFull(Player player)
    {
        var state = GetState(player);
        state.CurrentRAM = _defaultMaxRAM;
        state.MaxRAM = _defaultMaxRAM;
        state.RAMUsedThisTurn = 0;
        state.RAMRecoveredThisTurn = 0;
        state.RAMOverloadThisCombat = 0;
        state.RAMClearedThisCombat = 0;
        state.SlotAUsedThisCombat = 0;
        state.SlotBUsedThisCombat = 0;
        state.LastRAMCardPlayed = null;
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