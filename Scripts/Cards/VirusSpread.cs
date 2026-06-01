using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class VirusSpread : AbstractMokui1270Card
{
    private const int energyCost = 1;
    private const int ramCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new CardsVar(1),
        new EnergyVar(energyCost),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    public VirusSpread() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        Player player = Owner;
        bool couldPlay = await CouldPlay((int)DynamicVars["Ram"].BaseValue, choiceContext, player);
        if (!couldPlay)
        {
            await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue,Owner);
        }else{
            var creature = player?.Creature;
            var combatState = creature!.CombatState;
            if (combatState == null) return;
            var enemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
            int enemyCount = enemies.Count;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Damage.PreviewValue*enemyCount,ValueProp.Move | ValueProp.Unpowered,cardPlay);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}