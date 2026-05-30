using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
namespace Mokui1270.Scripts.Patchs;

public class MyKeyWords
{
    // 自定义枚举的名字。最终会变成{前缀}-{枚举值大写}的形式，例如TEST-UNIQUE
    [CustomEnum("BloodAttack")]
    // 放在原版卡牌描述的位置，这里是卡牌描述的前面
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword BloodAttack;


    [CustomEnum("Hacking")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Hacking;


    [CustomEnum("Nanomachine")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Nanomachine;

    [CustomEnum("InchPunch")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword InchPunch;

    [CustomEnum("GarbageRecycle")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword GarbageRecycle;
}