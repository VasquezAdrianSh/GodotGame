using Godot;

public partial class UnitInteractionArea : Area3D {
    Unit unit;

    public override void _Ready() {
        base._Ready();
        unit = this.TryFindParentNodeOfType<Unit>();
        BodyEntered += OnInteractionAreaEnteredHandler;
        BodyExited += OnInteractionAreaExitedHandler;
    }

    void OnInteractionAreaEnteredHandler(Node3D body) {
        if (CanInteract(body)) return;
        if (unit.Player.Name != "Environment") {
            unit.Player.InteractionPanel.Message.Text = "Talk to " + body.Name;
        } else {
            unit.Player.InteractionPanel.Message.Text = "Interact with " + body.Name;
        }
        unit.Player.InteractionPanel.Visible = true;
    }

    void OnInteractionAreaExitedHandler(Node3D body) {
        if (CanInteract(body)) return;
        unit.Player.InteractionPanel.Visible = false;
    }

    bool CanInteract(Node3D body) {
        return !unit.Player.IsFirstPlayer() || body is not Unit enteringUnit || enteringUnit.Player.IsFirstPlayer() || unit.Player.InteractionPanel is null;
    }
}