using UnityEngine;
using System.Collections;

// Misnamed - LevelNode would make more sense?
public class LevelNode : MonoBehaviour
{
    public enum Type { FLOOR, LADDER, PLATFORM }

    [SerializeField] LevelNode[] _neighbours;
    [SerializeField] Type _type;

    Renderer _renderer;


    public LevelNode[] GetNeighbours() { return _neighbours; }
    public Type LevelType { get { return _type; } }

    public float Left { get { return _renderer.bounds.min.x; } }
    public float Right { get { return _renderer.bounds.max.x; } }
    public float Top { get { return _renderer.bounds.max.y; } }
    public float Bottom { get { return _renderer.bounds.min.y; } }
    public Vector2 Centre { get { return _renderer.bounds.center; } }
    

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
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
            }

            return 0; // should never reach here!
        }
    }


    
    
    //! Calc distance from one node to another. If start node is platform, this is horiz distance, if ladder it's vert.
    // It goes from "start" of first node to "start" of end node, so if end node is on the right and start is a platform
    // it is distance from leftmost point of start node to leftmost point of end node.
    public static float TravelDist(LevelNode startNode, LevelNode endNode)
    {
        switch(startNode.LevelType)
        {
            case Type.FLOOR:
                if (endNode.Left > startNode.Left)
                {
                    return endNode.Left - startNode.Left;
                }
                else if (endNode.Right < startNode.Right)
                {
                    return startNode.Right - endNode.Right;
                }
                
                return 0;

            case Type.LADDER:
                if (endNode.Bottom > startNode.Bottom)
                {
                    return endNode.Bottom - startNode.Bottom;
                }
                else if (endNode.Top < startNode.Top)
                {
                    return startNode.Top - endNode.Top;
                }
                
                return 0;
        }

        return 0;
    }


    public static float TravelDist(LevelNode startNode, LevelNode endNode, Vector2 startPoint)
    {
        switch (startNode.LevelType)
        {
            case Type.FLOOR:
                if (endNode.Left > startPoint.x)
                {
                    return endNode.Left - startPoint.x;
                }
                else if (endNode.Right < startPoint.x)
                {
                    return startPoint.x - endNode.Right;
                }

                return 0;

            case Type.LADDER:
                if (endNode.Bottom > startPoint.y)
                {
                    return endNode.Bottom - startPoint.y;
                }
                else if (endNode.Top < startPoint.y)
                {
                    return startPoint.y - endNode.Top;
                }

                return 0;
        }

        return 0;
    }









}
