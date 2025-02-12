
using System.Collections.Generic;
using Godot;
public partial class PlayerManager : Node {
    LocalDatabase database = new();
    List<PlayerBase> players = new();
    PlayerRelationship playerRelationship;
    List<Node3D> waypoints = new();

    public List<PlayerBase> Players { get => players; }
    public PlayerRelationship PlayerRelationship { get => playerRelationship; }
    public LocalDatabase Database { get => database; }

    public override void _Ready() {
        players = this.TryGetAllChildOfType<PlayerBase>();
        playerRelationship = new(players);
        PlayerBase player = GetNode<PlayerBase>("Player");
        player.CanvasLayer.Visible = true;

        Server server = GetNode<Server>(StaticNodePaths.Server);
        server.ProvidePlayerManager(this);

        //! Debug 
        // Callable.From(DebugStart).CallDeferred();
        // !EndDebug 
    }

    public PlayerBase FindPlayer(string name) {
        return GetNode<PlayerBase>(name);
    }

    public Unit FindUnit(string playerName, string name) {
        return FindPlayer(playerName).GetNode<Unit>(name);
    }

    async void DebugStart() {
        database.LoadData();
        GD.Print("[DebugStart] Loaded " + database.Units.Count + " units");
        GD.Print("[DebugStart] Loaded " + database.Abilites.Count + " abilities");
        GD.Print("[DebugStart] Loaded " + database.Effects.Count + " effects");
        RealTimeStrategyPlayer player = GetNode<RealTimeStrategyPlayer>("Player");
        var Tsukugi = player.AddUnit(database.Units["Tsukugi"], player.GetNode<Node3D>("Spawn1").GlobalPosition);
        player.AddUnit(database.Units["Healer"], player.GetNode<Node3D>("Spawn1").GlobalPosition.AddToX(1f));
        player.AddUnit(database.Units["Guard"], player.GetNode<Node3D>("Spawn1").GlobalPosition.AddToX(-1f));
        Tsukugi.UnitAttributes.ApplyDamage(100);

        await this.Wait(1);
        playerRelationship.UpdateRelationship("Neutral", "Hostile", RelationshipType.Hostile);
        playerRelationship.UpdateRelationship("Hostile", "Neutral", RelationshipType.Hostile);
        playerRelationship.UpdateRelationship("Player", "Hostile", RelationshipType.Hostile);
        playerRelationship.UpdateRelationship("Player", "Neutral", RelationshipType.Friend);
        playerRelationship.UpdateRelationship("Hostile", "Player", RelationshipType.Hostile);

        Node areas = GetNode("../Terrain/Areas");
        waypoints = areas.TryGetAllChildOfType<Node3D>();
        RealTimeStrategyPlayer Hostile = GetNode<RealTimeStrategyPlayer>("Hostile");
        RealTimeStrategyPlayer Neutral = GetNode<RealTimeStrategyPlayer>("Neutral");
        waypoints.Shuffle();

        for (int i = 0; i < waypoints.Count; i++) {
            if (i % 2 == 0) Hostile.AddUnit(database.Units["Tsuki"], waypoints[i].GlobalPosition);
            else Neutral.AddUnit(database.Units["Tsukita"], waypoints[i].GlobalPosition);
        }
        await this.Wait(1);
        DebugMoveToRandomWaypoints(Neutral, waypoints);
        DebugMoveToRandomWaypoints(Hostile, waypoints);
    }

    void DebugMoveToRandomWaypoints(RealTimeStrategyPlayer player, List<Node3D> waypoints) {
        RealTimeStrategyPlayer firstPlayer = GetNode<RealTimeStrategyPlayer>("Player");
        List<NavigationUnit> allUnits = player.GetAllUnits();
        foreach (NavigationUnit unit in allUnits) {

            Color newColor = unit.OverheadLabel.OutlineModulate;
            if (firstPlayer.IsHostilePlayer(player)) newColor = new Color(1, 0, 0);
            if (firstPlayer.GetRelationship(player) == RelationshipType.Friend) {
                newColor = new Color(0.5f, 1, 0.5f);
            }
            if (firstPlayer.GetRelationship(player) == RelationshipType.Neutral) newColor = new Color(0.5f, 0.5f, 0.5f);
            if (firstPlayer.GetRelationship(player) == RelationshipType.Unknown) newColor = new Color(0, 0, 0);
            unit.OverheadLabel.OutlineModulate = newColor;
            firstPlayer.DebugLog(firstPlayer.GetRelationship(player) + " - " + newColor.ToString());

            waypoints.Shuffle();
            Stack<Node3D> WayPoints = new();
            foreach (Node3D waypoint in waypoints) {
                unit.UnitTask.AddTask(new UnitTaskMove(waypoint.GlobalPosition, unit));
            }
        }
    }
}



