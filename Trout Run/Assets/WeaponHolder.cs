using UnityEngine;
using System.Collections;

public class WeaponHolder : MonoBehaviour 
{
    public Weapon weapon { set { _weapon = value; } get { return _weapon; }}
    protected Weapon _weapon;
    protected Vector2 _weaponOffset = new Vector2();

    protected float _facingDirection = 1; // should be 1 or -1 - mainly for animations, this is which direction you're "facing" (left or right)
    public float facingDirection { get { return _facingDirection; } }

    protected Mover _mover;
    public Mover mover { get { return _mover; } }  // accessing like this is probably faster and easier than using GetComponent

    public enum Team
    {
        PLAYER,
        ENEMY,
    };

    public void PickupWeapon(Weapon weapon)
    {
        if (_weapon != null) return; // currently refuse if has weapon, later though we could make him drop current if already has one
        weapon.Pickup(this, _weaponOffset);
        weapon.DebugPrint();
        weapon.SetDirection(new Vector2(_facingDirection, 0));
    }

    public virtual void DropWeapon()
    {
        _weapon.DropMe();
        _weapon = null;
    }

    public Team myTeam;
    //Identifies the team that the holder is on
	// Use this for initialization

	// Update is called once per frame
	void Update ()
    {
	
	}
}
