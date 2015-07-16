using UnityEngine;
using System.Collections;
using System;

// Misnamed - LevelNode would make more sense?




public class LevelNode : MonoBehaviour
{
    public enum Type { FLOOR, LADDER, PLATFORM }
    public enum Direction { UP, DOWN, LEFT, RIGHT }

    [Serializable]
    public struct Neighbour
    {
        public Direction connectionDir; // note this is conceptually the direction you would press to move from one node to the other - e.g. from floor to ladder going up would be UP. From floor to other floor moving right would be RIGHT.
        public LevelNode node;
    }

    [SerializeField] Type _type;
    [SerializeField] Neighbour[] _neighbours;
    

    //Renderer _renderer;


    public Neighbour[] GetNeighbours() { return _neighbours; }
    public Type LevelType { get { return _type; } }

    public float Left { get { return _bounds.min.x; } }
    public float Right { get { return _bounds.max.x; } }
    public float Top { get { return _bounds.max.y; } }
    public float Bottom { get { return _bounds.min.y; } }
    public Vector2 Centre { get { return _bounds.center; } }

    Vector2 _minEntry, _maxEntry;
    public Vector2 MinEntry { get { return _minEntry; } }
    public Vector2 MaxEntry { get { return _maxEntry; } }
    //public Vector2 CentreEntry { get { return _minEntry + ((_maxEntry - _minEntry) * 0.5f); } } // for platforms only - if you fall through or jump up it takes entry / exit as being centre

    Bounds _bounds;


    void Awake()
    {
        // Calculate Bounds
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        Vector3 min = colliders[0].bounds.min;
        Vector3 max = colliders[0].bounds.max;

        for (int i = 1; i < colliders.Length; ++i)
        {
            if (colliders[i].isTrigger) continue;
            min.x = Mathf.Min(min.x, colliders[i].bounds.min.x);
            min.y = Mathf.Min(min.y, colliders[i].bounds.min.y);
            max.x = Mathf.Max(max.x, colliders[i].bounds.max.x);
            max.y = Mathf.Max(max.y, colliders[i].bounds.max.y);
        }

        _bounds = new Bounds();
        _bounds.min = min;
        _bounds.max = max;


        // Calculate entry points
        switch (_type)
        {
            case Type.FLOOR:
            case Type.PLATFORM:
                _minEntry = new Vector2(Left, GetEntryY(Left + 0.1f));
                _maxEntry = new Vector2(Right, GetEntryY(Right - 0.1f));
                break;


            case Type.LADDER:
                _minEntry = new Vector2(Centre.x, Bottom);
                _maxEntry = new Vector2(Centre.x, Top);
                break;
        }
    }


    float GetEntryY(float entryX)
    {
        int layerMask = LayerMask.GetMask(LayerMask.LayerToName(this.gameObject.layer));
        RaycastHit2D[] hits;
                
        hits = Physics2D.RaycastAll(new Vector2(entryX, Top + 0.1f), Vector2.down, Mathf.Infinity, layerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.GetComponentInParent<LevelNode>() == this)
            {
                return hit.point.y;
            }
        }
        return Top;
    }



    public Neighbour GetNeighbour(LevelNode neighbourNode)
    {
        for (int i = 0; i < _neighbours.Length; ++i)
        {
            if (_neighbours[i].node == neighbourNode) return _neighbours[i];
        }
        return new Neighbour();
    }






    //! Get mask of abilities required to traverse this node
    public uint TraversalAbilities
    {
        get
        {
            switch (_type)
            {
                case Type.FLOOR: return EnemyProps.WALKS;
                case Type.LADDER: return EnemyProps.LADDERS;
                case Type.PLATFORM: return EnemyProps.JUMPS;  
            }

            return 0; // should never reach here!
        }
    }


    






}
