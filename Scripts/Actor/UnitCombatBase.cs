
using Godot;
public partial class UnitCombatBase : Node3D {
    protected Unit unit = null;
    Unit target = null;
    bool isAlreadyAttacking = false;
    bool isAttackManuallyStopped = false;
    bool isInCombat = false;
    RayCast3D combatRayCast;

    public bool IsInCombat { get => isInCombat; }
    public Unit Target { get => target; }

    public delegate void OnCombatEvent();
    public event OnCombatEvent OnCombatEndEvent;
    public event OnCombatEvent OnAttackReachedEvent;
    public event OnCombatEvent OnAttackFailedEvent;

    public override void _Ready() {
        base._Ready();
        unit = this.TryFindParentNodeOfType<NavigationUnit>();
        combatRayCast = unit.GetNode<RayCast3D>(StaticNodePaths.CombatRayCast);
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (target is null || target.UnitAttributes.CanBeKilled) EndCombat();
    }

    public void StartCombat(Unit target) {
        unit.Player.DebugLog("[StartCombat] " + unit.Name + " -> " + target.Name);
        this.target = target;
        isInCombat = true;
    }

    public void EndCombat() {
        if (!isInCombat) return;
        unit.Player.DebugLog("[EndCombat] " + unit.Name + " has finished combat");
        target = null;
        isInCombat = false;
        OnCombatEndEvent?.Invoke();
    }

    public void TryStartAttack() {
        if (isAlreadyAttacking || !isInCombat) return;
        isAlreadyAttacking = true;
        if (isAttackManuallyStopped) {
            isAttackManuallyStopped = false;
            return;
        }
        CastAttack();
        AttributesExport attributes = unit.GetAttributes();
        if (attributes.AttackCastDuration > 0) TimerUtils.CreateSimpleTimer((s, e) => OnAttackCastEnd(), attributes.AttackCastDuration);
        else OnAttackCastEnd();
    }

    public void StopAttack() {
        // We only stop ongoing attack state
        if (!isAlreadyAttacking) return;
        unit.Player.DebugLog("[StopAttack] Stop attack state");
        isAttackManuallyStopped = true;
        isAlreadyAttacking = false;
    }

    void CastAttack() {
        combatRayCast.CollideWithBodies = true;
        combatRayCast.TargetPosition = target.GlobalPosition + Vector3.Up.Magnitude(0.5f) - unit.GlobalPosition;
        combatRayCast.ForceRaycastUpdate();
        unit.Player.DebugLog("[CastAttack] " + unit.Name + " is attacking to " + combatRayCast.TargetPosition);

        if (combatRayCast.IsColliding()) {
            GodotObject collisionTarget = combatRayCast.GetCollider();
            if (collisionTarget is NavigationUnit targetUnit) {
                unit.Player.DebugLog("[CastAttack] " + unit.Name + " -> " + targetUnit.Name);
                OnTargetAttackReached(targetUnit);
            } else {
                OnAttackFailedEvent?.Invoke();
            }
        } else {
            OnAttackFailedEvent?.Invoke();
        }
        combatRayCast.CollideWithBodies = false;
    }


    void OnTargetAttackReached(NavigationUnit targetUnit) {
        OnAttackReachedEvent?.Invoke();
        AttributesExport unitAttributes = unit.GetAttributes();
        AttributesExport targetUnitAttributes = unit.GetAttributes();
        var finalDamage = targetUnit.UnitAttributes.ApplyDamage(unitAttributes.BaseDamage);
        unit.Player.DebugLog("[TryStartAttack] " + unit.Name + " dealt " + finalDamage + " of damage. " +
             " \n Target hitpoints: " + targetUnitAttributes.HitPoints);
    }

    void OnAttackCastEnd() {
        AttributesExport attributes = unit.GetAttributes();
        TimerUtils.CreateSimpleTimer((s, e) => OnAttackCooldownEnd(), attributes.AttackSpeed);
        unit.Player.DebugLog("[OnAttackCastEnd]");
    }

    void OnAttackCooldownEnd() {
        isAlreadyAttacking = false;
        unit.Player.DebugLog("[OnAttackCooldownEnd]");

        if (isAttackManuallyStopped) isAttackManuallyStopped = false;
        else CallDeferred("TryStartAttack");
    }
}