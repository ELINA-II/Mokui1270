using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Mokui1270.Scripts.Patchs;

namespace Mokui1270.Scripts.Cards;

[Pool(typeof(Mokui1270CardPool))]
public class SerenePose : AbstractMokui1270Card
{
    private const int ENERGY_COST = 2;
    private const CardType TYPE = CardType.Skill;
    private const CardRarity RARITY = CardRarity.Uncommon;
    private const TargetType TARGET_TYPE = TargetType.Self;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<BufferPower>(1m),
        new PowerVar<VigorPower>(10m),
    ];

    public SerenePose() : base(ENERGY_COST, TYPE, RARITY, TARGET_TYPE, true)
    {
        isBlood = true;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        MyKeyWords.BloodAttack,
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<ScatterQi>(),
        HoverTipFactory.FromCard<CondenseForm>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 添加护盾能力
        await PowerCmd.Apply<BufferPower>(choiceContext,Owner.Creature, DynamicVars["BufferPower"].BaseValue, Owner.Creature, this);
        
        // 2. 创建选项列表
        var options = new List<CardModel>();
        var cardScope = CardScope ?? Owner.RunState;
        
        // 获取所有存活敌人
        var enemies = CombatState?.HittableEnemies?.Where(e => e.IsAlive).ToList();
        
        // 选项1：击晕（如果存在敌人）
        var scatterQi = cardScope.CreateCard<ScatterQi>(Owner);
        options.Add(scatterQi);
        
        // 选项2：活力（始终可用）
        var condenseForm = cardScope.CreateCard<CondenseForm>(Owner);
        options.Add(condenseForm);
        
        // 3. 如果没有可用选项，直接返回
        if (!options.Any())
        {
            return;
        }
        
        // 4. 弹出选择界面
        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            options,
            Owner,
            canSkip: false
        );
        
        // 5. 执行选中的效果
        if (selected is ScatterQi)
        {
            await ExecuteStun();
        }
        else if (selected is CondenseForm)
        {
            await ExecuteVigor(choiceContext);
        }
    }
    
    /// <summary>
    /// 执行击晕效果
    /// </summary>
    private async Task ExecuteStun()
    {
        Creature targets = Owner.RunState.Rng.CombatTargets.NextItem(CombatState!.HittableEnemies)!;
        await CreatureCmd.Stun(targets!);
    }
    
    /// <summary>
    /// 执行活力效果
    /// </summary>
    private async Task ExecuteVigor(PlayerChoiceContext choiceContext)
    {
        // 给玩家增加活力（根据你的设计）
        await PowerCmd.Apply<VigorPower>(choiceContext,Owner.Creature,10, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}