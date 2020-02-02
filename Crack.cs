using Godot;
using System;
using System.Collections.Generic;

public class Crack : Node2D
{
    [Export]
    public int iconSize = 64;
    [Export]
    public int iconMargin = 3;
    //private PackedScene main_inst = new PackedScene()

    public List<string> resourceList;
    private List<Sprite> sprites;
    private Node2D _resourcesPanel;  
    
    
    public override void _Ready()
    {
        resourceList = new List<string>();
        sprites = new List<Sprite>();
        Random random = new Random();
        int resourceCount = random.Next(1, 3);

        _resourcesPanel = GetNode<Node2D>("ResourcesNeeded");

        for(int i = 0; i<resourceCount;i++)
        {
            sprites.Add( new Sprite());
            resourceList.Add(AssetFinder.getRandomResourceName());
            sprites[i].Texture = ResourceLoader.Load<Texture>(AssetFinder.GetPathByResourceName(resourceList[i]));
            sprites[i].Scale = new Vector2(0.7f, 0.7f);
            _resourcesPanel.AddChild(sprites[i]);
            sprites[i].Position = new Vector2(iconMargin + (iconSize + iconMargin) * i, iconMargin);
        }

        (GetNode<Area2D>("Area2D") as RepareInfo).crack = this; //pasarle la info a el objeto que colisiona para que la comparta
                                                                                 //en el evento 
    }

    public void reSprite(int size)
    {
        for(int i = 0; i < sprites.Count;i++)
            sprites[i].QueueFree();
        for(int i = 0; i<size;i++)
        {
            var sprite = new Sprite();
            sprite.Texture = ResourceLoader.Load<Texture>(AssetFinder.GetPathByResourceName(resourceList[i]));

            _resourcesPanel.AddChild(sprite);
            sprite.Position = new Vector2(iconMargin + (iconSize + iconMargin) * i, iconMargin);
        }
    }

    public override void _Process(float delta)
    {

    }
}
