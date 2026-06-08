using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Mokui1270.Scripts.Patchs;
using Mokui1270.Scripts.Powers;

namespace Mokui1270.Scripts.Cards;
[Pool(typeof(Mokui1270CardPool))]
public class WatchSelfDestruct : AbstractMokui1270Card
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targrtType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    private bool isHardtokill = false;
    private bool isHardenedShell = false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.Hacking,
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<HardToKillPower>(),
        HoverTipFactory.FromPower<HardenedShellPower>(),
        HoverTipFactory.FromPower<WatchSelfDestructPower>(),
    ];

    public WatchSelfDestruct() : base(energyCost,type,rarity,targrtType,shouldShowInCardLibrary)
    {
        isHack = true;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext,CardPlay cardPlay)
    {
        await PowerCmd.Apply<WatchSelfDestructPower>(Owner.Creature,3,Owner.Creature, this);
		foreach (Creature hittableEnemy in CombatState!.HittableEnemies)
		{
            isHardenedShell = false;
            isHardtokill = false;
			if (hittableEnemy.HasPower<HardToKillPower>())
		{
			await PowerCmd.Remove<HardToKillPower>(hittableEnemy);
            isHardtokill = true;
		}
		if (hittableEnemy.HasPower<HardenedShellPower>())
		{
			await PowerCmd.Remove<HardenedShellPower>(hittableEnemy);
            isHardenedShell = true;
		}
        await CreatureCmd.Damage(
            choiceContext,
            hittableEnemy,      // 目标生物
            hittableEnemy.CurrentHp/2,                  // 伤害值
            ValueProp.Unblockable, // 属性
            this       // 来源生物
            );
        if (!hittableEnemy.IsDead)
        {
            if (isHardtokill)
            {
                await PowerCmd.Apply<HardToKillPower>(hittableEnemy,1,Owner.Creature, this);
            }
            if (isHardenedShell)
            {
                await PowerCmd.Apply<HardenedShellPower>(hittableEnemy,1,Owner.Creature, this);
            }
        }
		}
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        EnergyCost.UpgradeBy(-1);
    }
}