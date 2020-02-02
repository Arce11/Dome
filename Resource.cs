using Godot;
using System;
using System.Collections.Generic;

public class Resource : Node2D
{
    public static List<string> avaliableResourcesInMap = new List<string>();
    public override void _Ready()
    {
        string res = AssetFinder.asignNonUsedResource(avaliableResourcesInMap);
        avaliableResourcesInMap.Add(res);
        GetNode<Sprite>("Sprite").Texture = (Texture)ResourceLoader.Load(AssetFinder.GetPathByResourceName(res)); 
        (GetNode<Area2D>("Area2D") as ResourceInfo).Kind = res;
    }
}
