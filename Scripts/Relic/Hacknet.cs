// 注册遗物。如果要写自定义池看添加人物的开头
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.RAM;
namespace Mokui1270.Scripts.Relic;
[Pool(typeof(Mokui1270RelicPool))]
public class Hacknet : CustomRelicModel
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Starter;
    // 小图标（原版85x85）
    public override string PackedIconPath => $"res://Mokui1270/images/relics/{GetType().Name}.png";
    // 轮廓图标（原版85x85）
    protected override string PackedIconOutlinePath => $"res://Mokui1270/images/relics/{GetType().Name}Outline.png";
    // 大图标（原版256x256）
    protected override string BigIconPath => $"res://Mokui1270/images/relics/{GetType().Name}Big.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await RAMClass.RecoverRAM(choiceContext,1, player);
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState){
		if (side == Owner.Creature.Side && combatState.RoundNumber <= 1)
		{
			Flash();
			await PowerCmd.Apply<VulnerablePower>(choiceContext,combatState.HittableEnemies,1,Owner.Creature, null);
		}
	}

    // 初始遗物的升级可以写这里
    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<HacknetPlus>().ToMutable();
}