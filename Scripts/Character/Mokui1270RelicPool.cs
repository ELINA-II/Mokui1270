using BaseLib.Abstracts;

public class TestRelicPool : CustomRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://Mokui_1270/images/Character/mokui_orb_small.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Mokui_1270/images/Character/mokui_orb.png";
}