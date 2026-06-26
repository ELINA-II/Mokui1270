using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.RAM;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class EMPBlast : AbstractMokui1270Card
{
    private const int energyCost = 0;
    private const int ramCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new CardsVar(1),
        new DynamicVar("Ram", ramCost).WithTooltip("MOKUI1270-RAM"),
        new PowerVar<WeakPower>(1m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    public EMPBlast() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
            Player player = Owner;
            var creature = player?.Creature;
            var combatState = creature!.CombatState;
            if (combatState == null) return;
            var enemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
            int enemyCount = enemies.Count;
        await RAMClass.RecoverRAM(choiceContext,1,player!);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext,CombatState!.HittableEnemies,DynamicVars.Weak.BaseValue,Owner.Creature, this);
        await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Damage.PreviewValue*enemyCount,ValueProp.Move | ValueProp.Unpowered,cardPlay);
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}