// RAMUINode.cs
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Mokui1270.Scripts.RAM;

public partial class RAMUICode : Control
{
    private Player? _player;
    private int _max = 10;
    private int _current = 10;
    
    private const float RamSpacing = 18f;
    private const float DrawScale = 0.8f;
    private const float VerticalOffset = -30f;
    
    private Texture2D? _ramEmptyTexture;
    private Texture2D? _ramFullTexture;
    
    private static string RamEmptyPath => "res://Mokui1270/images/character/ram_empty.png";
    private static string RamFullPath => "res://Mokui1270/images/character/ram_full.png";
    
    public static RAMUICode Create(Player player)
    {
        var ramUINode = new RAMUICode();
        ramUINode._player = player;
        ramUINode._ramEmptyTexture = GD.Load<Texture2D>(RamEmptyPath);
        ramUINode._ramFullTexture = GD.Load<Texture2D>(RamFullPath);
        
        ramUINode._current = RAMClass.GetCurrentRAM(player);
        ramUINode._max = RAMClass.GetMaxRAM(player);
        
        ramUINode.UpdateLayoutSize();
        ramUINode.QueueRedraw();
        return ramUINode;
    }
    
    public override void _Ready()
    {
        UpdateLayoutSize();
        QueueRedraw();
        RAMClass.OnChanged += HandleRAMChanged;
    }
    
    public override void _ExitTree()
    {
        RAMClass.OnChanged -= HandleRAMChanged;
    }
    
    private void HandleRAMChanged(Player player, int current, int max)
    {
        if (_player != null && player == _player)
        {
            SetRAM(current, max);
        }
    }
    
    public void SetRAM(int current, int max)
    {
        _current = Mathf.Clamp(current, 0, max);
        _max = Mathf.Max(0, max);
        UpdateLayoutSize();
        QueueRedraw();
    }
    
    private void UpdateLayoutSize()
    {
        Texture2D? texture = _ramFullTexture ?? _ramEmptyTexture;
        if (texture != null && _max > 0)
        {
            Vector2 size = texture.GetSize() * DrawScale;
            float width = size.X + (_max - 1) * RamSpacing;
            CustomMinimumSize = new Vector2(width, size.Y);
            Size = CustomMinimumSize;
        }
    }
    
    private float GetRAMDrawX(int index, float ramWidth)
    {
        float centerOffset = CalculateCenterOffset();
        float slotOffset = index * RamSpacing;
        return centerOffset + slotOffset;
    }
    
    private float CalculateCenterOffset()
    {
        float headCenterX = 0f;
        
        float targetCenterOffset;
        
        if (_max % 2 == 1)
        {
            int centerIndex = (_max - 1) / 2;
            targetCenterOffset = centerIndex * RamSpacing + GetSlotWidth() / 2f;
        }
        else
        {
            int leftCenterIndex = _max / 2 - 1;
            int rightCenterIndex = _max / 2;
            float leftCenterPos = leftCenterIndex * RamSpacing + GetSlotWidth() / 2f;
            float rightCenterPos = rightCenterIndex * RamSpacing + GetSlotWidth() / 2f;
            targetCenterOffset = (leftCenterPos + rightCenterPos) / 2f;
        }
        
        return headCenterX - targetCenterOffset;
    }
    
    private float GetRAMDrawY(float ramHeight)
    {
        var creature = this.GetParentOrNull<NCreature>();
        
        if (creature != null)
        {
            float headY = GetCharacterHeadY(creature);
            if (headY != float.MinValue)
            {
                return headY - ramHeight + VerticalOffset;
            }
            
            if (creature.Visuals?.OrbPosition != null)
            {
                float scaleY = ((Node2D)creature.Visuals).Scale.Y;
                float orbY = ((Node2D)creature.Visuals.OrbPosition).Position.Y * scaleY;
                return orbY - ramHeight - 20f + VerticalOffset;
            }
        }
        
        return Size.Y - ramHeight - 350f + VerticalOffset;
    }
    
    private float GetCharacterHeadY(NCreature creature)
    {
        var bounds = creature.GetNodeOrNull<Control>("Bounds");
        if (bounds != null)
        {
            return bounds.Position.Y + bounds.GetRect().Position.Y;
        }
        
        var intentPos = creature.GetNodeOrNull<Marker2D>("IntentPos");
        if (intentPos != null)
        {
            return intentPos.Position.Y - 20f;
        }
        
        var centerPos = creature.GetNodeOrNull<Marker2D>("CenterPos");
        if (centerPos != null)
        {
            return centerPos.Position.Y - 180f;
        }
        
        return float.MinValue;
    }
    
    private float GetSlotWidth()
    {
        if (_ramFullTexture != null)
            return _ramFullTexture.GetSize().X * DrawScale;
        return 24f;
    }
    
    private float GetSlotHeight()
    {
        if (_ramFullTexture != null)
            return _ramFullTexture.GetSize().Y * DrawScale;
        return 24f;
    }
    
    public override void _Draw()
    {
        if (_player == null || _ramEmptyTexture == null || _ramFullTexture == null || _max <= 0)
            return;
            
        Vector2 textureSize = _ramFullTexture.GetSize() * DrawScale;
        float drawY = GetRAMDrawY(textureSize.Y);
        
        for (int i = 0; i < _max; i++)
        {
            Texture2D texture = i < _current ? _ramFullTexture : _ramEmptyTexture;
            float drawX = GetRAMDrawX(i, textureSize.X);
            
            DrawTextureRect(texture, new Rect2(drawX, drawY, textureSize.X, textureSize.Y), false);
        }
    }
}