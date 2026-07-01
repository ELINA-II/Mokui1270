using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Mokui1270.Scripts.Patches;

public class BloodCostEntry : CombatHistoryEntry
{
    public CardModel Card { get; }
    public int BorrowedEnergy { get; }
    public int HealthCostPaid { get; }
    public string Phase { get; }  // "Borrow", "PayHealth", "Complete"

    public override string Description
    {
        get
        {
            string id = GetId(Actor);
            return $"{id} used blood card {Card.Id.Entry}: borrowed {BorrowedEnergy} energy, paid {HealthCostPaid} HP [{Phase}]";
        }
    }

    public BloodCostEntry(
        Creature actor,
        CardModel card,
        int borrowedEnergy,
        int healthCostPaid,
        string phase,
        int roundNumber,
        CombatSide currentSide,
        CombatHistory history,
        IEnumerable<Player> players)
        : base(actor, roundNumber, currentSide, history, players)
    {
        Card = card;
        BorrowedEnergy = borrowedEnergy;
        HealthCostPaid = healthCostPaid;
        Phase = phase;
    }

    private static string GetId(Creature creature)
    {
        if (!creature.IsPlayer) return creature.Monster!.Id.Entry;
        return creature.Player!.Character.Id.Entry;
    }
}