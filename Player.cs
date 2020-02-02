using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Player : Node2D
{
    [Signal]
    public delegate void InventoryChange();
    public AnimationPlayer _animator;

    //recoleccion
    public bool CanColect {get;set;}
    public string ColectableResource {get;set;}

    //reparacion
    public bool CanRepare {get;set;}
    public Crack crack;
    public Vector2[] path;
    public Navigation2D map;
    public Line2D line;



    

    public Vector2 dir = new Vector2(0,0);
    public List<string> inventory = new List<string>();
    public List<string> traits = new List<string>();

    public PlayerState state;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _animator = GetNode<AnimationPlayer>("AnimationPlayer");
        _animator.Play("run");
        state = new PlayerIdle(this);
        GUI gui =  GetNode<GUI>("GUI");
        gui.player = this;
        Connect("InventoryChange", gui, "_OnInventoryChange");

    }

    public override void _PhysicsProcess(float delta)
    {
        state._StatePhysicsProcess(delta);
    }
    public override void _Process(float delta)
    {
        state._StateProcess(delta);
    }

    public override void _UnhandledInput(InputEvent @event){
        if (@event is InputEventMouseButton click && @event.IsPressed())
        {
            if (click.ButtonIndex == (int)ButtonList.Right)
            {
                path = map.GetSimplePath(GlobalPosition, GetGlobalMousePosition(), true);
                path = path.Skip(1).ToArray();
                // line.Points = path;
                // foreach (Vector2 item in path)
                // {
                //     GD.Print(item);
                // }
            }
        }
        // state._StateUnhandledInput(@event);
    }

    public void _On_Timer_Timeout()
    {
        state._On_Timer_Timeout();
    }

    public void _On_Proximity_Event(Area2D area)
    {
        if(area is ResourceInfo) // ha tocado un generador de recursos
        {
            GD.Print("Can Colect");
            CanColect = true;
            ColectableResource = (area as ResourceInfo).Kind;
        }
        else if (area is RepareInfo) //ha tocado grieta
        {
            GD.Print("Oh no!!! Una grieta!!!");
            CanRepare = true;
            crack = ((RepareInfo)area).crack;
        }
    }
    public void _Not_Proximity_Event(Area2D area)
    {
        if(area is ResourceInfo) // ha tocado un generador de recursos
        {
            GD.Print("Can Not Colect");
            CanColect = false;
            ColectableResource = "";
        }
        else if(area is RepareInfo)
        {
            GD.Print("Abandonando la grieta sin reparar");
            crack = null;
            CanRepare = false;
        }
    }
}

public abstract class PlayerState
{
    
    protected Player player;
    public PlayerState(Player player)
    {
        this.player = player;
        player.GetNode<Timer>("Timer").Stop();
    }
    public virtual void _StatePhysicsProcess(float delta){}
    public virtual void _StateProcess(float delta){}
    public virtual void _StateUnhandledInput(InputEvent @event){}
    
    // Interfaces the timeout event between the player object and the states so each one can use the timer for it's needs
    public virtual void _On_Timer_Timeout(){}

}

class PlayerRuning : PlayerState
{
    private float _speed = 100;

    public PlayerRuning(Player player) :base(player)
    {
        _speed = _speed*10/(player.inventory.Count + 5);
    }
    public override void _StatePhysicsProcess(float delta)
    {
        player.Position += player.dir*_speed*delta; 
    }

    public override void _StateProcess(float delta)
    {
        if (player.path != null)
        {
            if (player.path.Length > 0)
            {
                Vector2 velocity = (player.path[0] - player.Position).Normalized();

                if (player.path[0].DistanceTo(player.GlobalPosition) < delta*_speed*1.3f)
                {
                    player.GlobalPosition = player.path[0];
                    player.path = player.path.Skip(1).ToArray();
                } else {
                
                    // GD.Print("Velocity:", velocity);
                    player.GlobalPosition += velocity*delta*_speed;
                }

            } else {
                player._animator.Play("idle");
                player.state = new PlayerIdle(player);
            }
        }

    }
}

class PlayerIdle : PlayerState
{
    public PlayerIdle(Player player):base(player){GD.Print("Descansando :)");}

    public override void _StateProcess(float delta)
    {
        if(Input.IsActionJustPressed("ui_start_action"))
        {
            if(player.CanColect)
            {
                player._animator.Play("colect");
                player.state = new PlayerColecting(player);
                return;
            }
            if(player.CanRepare)
            {
                player._animator.Play("repare");
                player.state = new PlayerReparing(player);
                return;
            }
        }
        if(Input.IsMouseButtonPressed((int)ButtonList.Right))
        {
            player._animator.Play("run");
            player.state = new PlayerRuning(player);
        }
    }
}

class PlayerReparing : PlayerState
{
    private float _repairTime = 2;
    private float _progress;
    private string materialUnderUse = "";
    public PlayerReparing(Player player):base(player)
    {
        if(player.traits.Contains("Hungry"))
        {
            _repairTime = 4;
        }

        foreach(string material in player.crack.resourceList)
        {
            foreach(string ownedMaterial in player.inventory)
            {
                if(material == ownedMaterial)
                {
                    GD.Print("         Empiezan las reparaciones");
                    materialUnderUse = ownedMaterial;
                    player.GetNode<Timer>("Timer").WaitTime = _repairTime;
                    player.GetNode<Timer>("Timer").Start();
                    var progrBar = player.GetNode<TextureProgress>("TextureProgress");
                    _progress = 0;
                    progrBar.Value = 0;
                    progrBar.Show();
                    break;
                }
            }
        }


    }

    public override void _StateProcess(float delta)
    {
        var progrBar = player.GetNode<TextureProgress>("TextureProgress");

        if(Input.IsActionJustPressed("ui_cancel_action"))
        {
            player._animator.Play("run");
            player.state = new PlayerRuning(player);
            progrBar.Hide();
            _progress = 0;
        }

        if(materialUnderUse == "")
        {
            GD.Print("Sin materiales");
            //Error sound
            player._animator.Play("idle");
            player.state = new PlayerIdle(player);
        }

        _progress += delta/_repairTime*100;
        progrBar.Value = (double) _progress;
        progrBar.Update();
    }

    public override void _On_Timer_Timeout()
    {
        GD.Print(" Repared with " + materialUnderUse);
        player.inventory.Remove(materialUnderUse);
        player.crack.resourceList.Remove(materialUnderUse);
        player.crack.reSprite(player.crack.resourceList.Count);
        var progrBar = player.GetNode<TextureProgress>("TextureProgress");
        progrBar.Hide();
        player.EmitSignal("InventoryChange");

        //Si se acaba con la lista, la grieta desaparece
        if(player.crack.resourceList.Count <= 0)
        {
            player.crack.QueueFree();
        }
        player._animator.Play("repare");
        player.state = new PlayerReparing(player);
    }
    
}

class PlayerColecting : PlayerState
{
    private float _colectTime = 1;
    private float _progress;
    private const int maxItems = 5;
    public PlayerColecting(Player player):base(player)
    {
        if(player.inventory.Count >= maxItems)
        {
            //sacar algun tipo de sonidito de error y vuelve a idle
            player.state = new PlayerIdle(player);
            return;
        }
        if(player.traits.Contains("Hungry"))
        {
            _colectTime = 2;
        }
        GD.Print("Colecting " + player.ColectableResource +" ...");
        player.GetNode<Timer>("Timer").WaitTime = _colectTime;
        player.GetNode<Timer>("Timer").Start();
        var progrBar = player.GetNode<TextureProgress>("TextureProgress");
        _progress = 0;
        progrBar.Value = 0;
        progrBar.Show();

    }

    public override void _StateProcess(float delta)
    {
        var progrBar = player.GetNode<TextureProgress>("TextureProgress");

        if(Input.IsActionJustPressed("ui_cancel_action"))
        {

            player._animator.Play("run");
            player.state = new PlayerRuning(player);
            progrBar.Hide();
            _progress = 0;
            return;
        }
        _progress += delta/_colectTime*100;
        progrBar.Value = (double) _progress;
        progrBar.Update();
    }
    public override void _On_Timer_Timeout()
    {
        var progrBar = player.GetNode<TextureProgress>("TextureProgress");
        progrBar.Hide();
        _progress = 0;
        GD.Print(player.ColectableResource +" Colected!!!");
        player.inventory.Add(player.ColectableResource);
        player._animator.Play("idle");
        player.state = new PlayerIdle(player);
        player.EmitSignal("InventoryChange");
    }
}

class PlayerSleeping : PlayerState
{
    private float _sleepTime = 10;
    public PlayerSleeping(Player player):base(player)
    {
        player.GetNode<Timer>("Timer").WaitTime = _sleepTime;
        player.GetNode<Timer>("Timer").Start();
    }

    public override void _On_Timer_Timeout()
    {
        GD.Print("Good Morning!!!");
        player._animator.Play("idle");
        player.state = new PlayerIdle(player);
    }

}