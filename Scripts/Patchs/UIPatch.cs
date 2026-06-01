// MokuiUIPatch.cs
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Mokui1270.Scripts.Character;
using Mokui1270.Scripts.RAM;
namespace Mokui1270.Scripts.Patchs;

[HarmonyPatch(typeof(NCreature))]
public static class UIPatch
{
    // 战斗开始时重置 RAM
    [HarmonyPatch(typeof(CombatManager), "StartCombatInternal")]
    [HarmonyPrefix]
    public static void CombatStartPrefix(CombatManager __instance, CombatState? ____state)
    {
        GD.Print("[MokuiUIPatch] Combat start - initializing RAM");
        
        if (____state == null)
        {
            GD.Print("[MokuiUIPatch] _state is null");
            return;
        }
        
        foreach (Player player in ____state.Players)
        {
            // 检查是否是 Mokui 角色（根据你的角色判断逻辑修改）
            if (IsMokuiCharacter(player))
            {
                RAMClass.ResetFull(player);
            }
        }
    }
    
    // 给 NCreature 添加 RAM UI 节点
    [HarmonyPatch("_Ready")]
    [HarmonyPostfix]
    private static void AddRAMUI(NCreature __instance)
    {
        // 检查条件：是玩家、是本地玩家、是 Mokui 角色
        if (!__instance.Entity.IsPlayer)
            return;
            
        if (__instance.Entity.Player == null)
            return;
            
        if (!LocalContext.IsMe(__instance.Entity))
            return;
            
        if (!IsMokuiCharacter(__instance.Entity.Player))
            return;        
        try
        {
            // 创建 RAM UI 节点
            var ramUINode = RAMUICode.Create(__instance.Entity.Player);
            ramUINode.Name = "RAMUI";
            ramUINode.UniqueNameInOwner = true;
            
            // 添加到角色节点
            __instance.AddChildSafely(ramUINode);
            
            // 设置渲染顺序（让 RAM UI 显示在最前）
            __instance.MoveChild(ramUINode, 0);
            
            GD.Print($"[MokuiUIPatch] RAM UI added successfully");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[MokuiUIPatch] Failed to add RAM UI: {e.Message}");
        }
    }
    
    // 判断是否是 Mokui 角色的方法（根据你的实际情况修改）
    private static bool IsMokuiCharacter(Player player)
    {
        // 方法1：通过角色名称判断
        // return player.Character?.GetType().Name == "MokuiCharacter";
        
        // 方法2：通过角色类判断
    return player.Character is Mokui1270Character;
        
        // 方法3：通过玩家标签判断
        // return player.HasTag("mokui");
        
        // 临时方案：为所有玩家添加 RAM UI（用于测试）
        // 注意：正式使用时请替换为实际判断逻辑
        //return true;  // 先对所有玩家显示，测试用
        
        // TODO: 替换为你的实际判断逻辑
    }
}