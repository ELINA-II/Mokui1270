using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]

public class Focus : AbstractMokui1270Card
{
    public override bool CanBeGeneratedInCombat => false;

    private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

    public Focus() : base(-1, CardType.Status, CardRarity.Status, TargetType.None,shouldShowInCardLibrary)
    {
    }
}