using UnityEngine;
using System.Collections;

public class ReflectiveSurface : MonoBehaviour {
    
    float _tempX, _tempY;
    bool _hitFlag; // Flag used so that the bullet won't "hit" multiple times
    public bool negative; // If negative is set to true, it negatives any velocities put into it
    
    
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        
    }
    
    public ParticleSystem.Particle Hit (ParticleSystem.Particle partycool, Weapon weaponUsed) {
        _tempX = partycool.velocity.x;
        _tempY = partycool.velocity.y;
        if (weaponUsed.possibleDirections != Weapon.WeaponDir.EIGHT)
        {
        if (_hitFlag == false){
            _hitFlag = true;
            partycool.lifetime += 2.0f;
            if (negative == false) partycool.velocity = new Vector2 (_tempY, _tempX);
            else partycool.velocity = new Vector2 (-_tempY, -_tempX);
            partycool.position = new Vector2 (partycool.position.x + (partycool.velocity.x/64), partycool.position.y + (partycool.velocity.y/64));
            StartCoroutine(RefractoryPeriod());
        }
        }
        else partycool.lifetime = 0;
        return partycool;
    }
    
    private IEnumerator RefractoryPeriod(){
        yield return new WaitForSeconds (0.08f);
        _hitFlag = false;
    }
}
