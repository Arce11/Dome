using Godot;
using System;
using System.Collections.Generic;

public class Root : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Player p = GetNode<Player>("Player");
        p.map = GetNode<Navigation2D>("NavMap");
        p.line = GetNode<Line2D>("Line2D");
        GUI gui = GetNode<GUI>("Player/GUI");
        gui.Connect("GameOver", this, "_OnGameOver");
    }


    public void _OnGameOver()
    {
        foreach(Node2D node in GetChildren())
        {
            node.QueueFree();
        }
        MainScreen main = new MainScreen();
        main.Show();
        main.Position = new Vector2(0,0);
        AddChild(main);
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

public static class AssetFinder
{
    public readonly static string[] name = {"Llave","Tablas"};
    public readonly static string[] path = {"res://assets/iconos/iconos/Llave.png","res://assets/iconos/iconos/Tabla.png"};

    public static string GetPathByResourceName(string resName)
    {
        for(int i = 0; i< name.Length; i++)
        {
            if(name[i] == resName) return path[i];
        }
        return "";
    }
    public static string asignNonUsedResource(List<string> avaliableResources)
    {
        foreach(string resource in name)
        {
            if (!avaliableResources.Contains(resource)) return resource;
        }
        
        Random random = new Random();
        int randomIndex = random.Next(0, name.Length);
        return name[randomIndex];
    }

    public static string getRandomResourceName()
    {
        Random random = new Random();
        int randomIndex = random.Next(0, name.Length);
        return name[randomIndex];
    }

}
