using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Mokui1270.Scripts.Powers;


namespace Mokui_1270.Patches;

[HarmonyPatch]
public static class HealOverflowPatch
{
    /// <summary>
    /// 拦截 Creature.HealInternal 方法
    /// 在原始方法执行前修改治疗量，将超量部分转为格挡
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Creature), "HealInternal")]
    public static bool OnHealInternalPrefix(Creature __instance, ref decimal amount)
    {
        // 检查是否有超量治疗能力
        if (__instance.HasPower<EnergyGradientPower>())
        {
            int currentHp = __instance.CurrentHp;
            int maxHp = __instance.MaxHp;
            int spaceLeft = maxHp - currentHp;
            int intAmount = (int)amount;
            
            if (intAmount > spaceLeft && spaceLeft >= 0)
            {
                int overflow = intAmount - spaceLeft;
                int actualHeal = spaceLeft;
                
                // 超量部分转为格挡
                if (overflow > 0)
                {
                    // 同步调用格挡获取
                    __instance.GainBlockInternal(overflow);

                }
                
                // 修改治疗量为实际可治疗的值
                amount = actualHeal;
            }
        }
        
        return true;
    }
}