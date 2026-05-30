using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]

/// <summary>
/// 活力选项卡牌
/// </summary>
public class CondenseForm : AbstractMokui1270Card
{
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedInCombat => false;

    // 无参构造函数（游戏系统需要）
    public CondenseForm() : base(-1, CardType.Status, CardRarity.Status, TargetType.Self,shouldShowInCardLibrary)
    {
    }
}