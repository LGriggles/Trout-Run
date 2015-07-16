using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour
{
    float _yTop;
    float _leeway = 0.3f; // little bit of leeway blud, so can be slightly lower than platform and collide if like moving fast and whatnot
    Collider2D _myCollider;



    void Start()
    {
        _myCollider = GetComponent<EdgeCollider2D>();
        _yTop = _myCollider.bounds.max.y;
        _yTop -= _leeway; // little bit of leeway blud
    }



    void OnTriggerStay2D(Collider2D other)
    {
        // If has mover, check to see if ignoring platforms
        Mover mover = other.GetComponent<Mover>();
        if (mover != null)
        {
            if (mover.IsIgnoringPlatforms())
            {
                Physics2D.IgnoreCollision(_myCollider, other, true);
                return;
            }
        }

        // Else, just go on if higher than platform
        float otherY = other.bounds.min.y;
        if (otherY > _yTop)
        {
            // can collide
            Physics2D.IgnoreCollision(_myCollider, other, false);
        }
        else
        {
            // can't collide
            Physics2D.IgnoreCollision(_myCollider, other, true);
        }
    }

}
