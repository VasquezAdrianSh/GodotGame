using System.Collections.Generic;
using Godot;

public partial class RTSNavigation : Node {
    RealTimeStrategyPlayer player;

    // Navigation
    float navigationGroupGapDistance = 1;
    public readonly NavigationInputHandler NavigationInputHandler = new();
    public readonly NavigationBase NavigationBase = new();

    public override void _Ready() {
        base._Ready();
        player = this.TryFindParentNodeOfType<RealTimeStrategyPlayer>();
    }

    public override void _Input(InputEvent @event) {
        base._Input(@event);
        if (!player.IsFirstPlayer()) return;
        if (!player.CanInteract(PlayerInteractionType.UnitControl)) return;
        if (@event is InputEventMouseButton eventMouseButton
            && eventMouseButton.ButtonIndex == MouseButton.Right
            && eventMouseButton.Pressed) {

            Vector3? targetPosition = NavigationBase.GetNavigationTargetPosition(player.Camera);
            if (targetPosition is not Vector3 targetPositionInWorld) return;

            Vector2 mousePosition = player.Camera.GetViewport().GetMousePosition();
            if (player.RTSSelection.SimpleSelect(mousePosition) is List<Unit> units && units[0].Player.IsHostilePlayer(player)) {
                player.RTSSelection.SelectedUnits.ForEach(unit => {
                    if (unit is not NavigationUnit navUnit) return;
                    navUnit.UnitCombat.StartCombatTask(units[0]);
                });
            } else {
                NavigateTo(targetPositionInWorld);
            }
        }
    }

    void NavigateTo(Vector3 targetPositionInWorld) {
        SelectionUtils.ApplyCommandToGroupPosition(
            player.RTSSelection.SelectedUnits,
            targetPositionInWorld,
            navigationGroupGapDistance,
            (float)System.Math.Floor(System.Math.Sqrt(player.RTSSelection.SelectedUnits.Count)),
            ApplyNavigation);
    }

    static void ApplyNavigation(Unit unit, Vector3 targetPosition) {
        if (unit is not NavigationUnit navigationUnit) return;
        // TODO Improve me to accumulate tasks instead
        if (navigationUnit.UnitSelection.IsSelected) {
            navigationUnit.NavigationAgent.StartNewNavigation(targetPosition);
        } else {
            navigationUnit.UnitTask.AddTask(new UnitTaskMove(targetPosition, navigationUnit));
        }
    }
}