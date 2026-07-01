// RAMChangedEntry.cs
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Mokui1270.Scripts.RAM
{
    public class RAMChangedEntry : CombatHistoryEntry
{
    public int NewRAM { get; }
    public int MaxRAM { get; }
    public int Delta { get; }  // 正=增加，负=消耗
    public string ChangeReason { get; }  // Recover, Consume, Overload, Set, Reset, CardPlayed
    public CardModel? CardSource { get; }  // ✅ 添加这个属性

    public override string Description
    {
        get
        {
            string id = GetId(Actor);
            string cardInfo = CardSource != null ? $" via {CardSource.Id.Entry}" : "";
            return $"{id} RAM: {NewRAM}/{MaxRAM} ({(Delta >= 0 ? "+" : "")}{Delta}) [{ChangeReason}]{cardInfo}";
        }
    }

    public RAMChangedEntry(
        Creature actor,
        int newRAM,
        int maxRAM,
        int delta,
        string changeReason,
        CardModel? cardSource,
        int roundNumber,
        CombatSide currentSide,
        CombatHistory history,
        IEnumerable<Player> players)
        : base(actor, roundNumber, currentSide, history, players)
    {
        NewRAM = newRAM;
        MaxRAM = maxRAM;
        Delta = delta;
        ChangeReason = changeReason;
        CardSource = cardSource;
    }

    private static string GetId(Creature creature)
    {
        if (!creature.IsPlayer) return creature.Monster!.Id.Entry;
        return creature.Player!.Character.Id.Entry;
    }
}
}