using Godot;
using System;

public class GUI : MarginContainer
{
    public Player player;
    [Export]
    public float _drainRate = 1;  // porportion/s
    private TextureProgress bar;
    [Signal]
    public delegate void GameOver();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        bar = GetNode<TextureProgress>("Bars/Bar/TextureProgress");
        bar.Value = 100;
    }

// Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }

    public void _OnHPTimerTimeout()
    {
        bar.Value = bar.Value - _drainRate;
        bar.Update();
        if (bar.Value <= 0)
        {
            GetNode<Timer>("HP_Timer").Stop();
            EmitSignal("GameOver");
        }
    }
    public void _OnInventoryChange()
    {
       int count = 0;
        Sprite sprite;
        string path;
        foreach (string item in player.inventory){
            count++;
            sprite = GetNode<Sprite>("Bars/Bar/Inventario/Background/HBoxContainer/Sprite"+count.ToString());
            if (item == "Llave")
            {
                path = "res://assets/iconos/iconos/Llave.png";
            } else if (item == "Tablas")
            {
                path = "res://assets/iconos/iconos/Tabla.png";
            } else{
                path = "res://assets/iconos/iconos/Inventario.png";
            }
            sprite.Texture = ResourceLoader.Load<Texture>(path);
        }
        path = "res://assets/iconos/iconos/Inventario.png";
        for (int i=count+1; i<6; i++)
        {
            sprite = GetNode<Sprite>("Bars/Bar/Inventario/Background/HBoxContainer/Sprite"+i.ToString());
            sprite.Texture = ResourceLoader.Load<Texture>(path);
        }

    }
}
