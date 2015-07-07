using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class EnemyPathfinder 
{
   public static EnemyPath FindPath(Enemy enemy, Vector2 target)
   {
       // Find start and end nodes
       LevelNode startNode = null;
       LevelNode endNode = null;     
       RaycastHit2D hit;
       
       hit = Physics2D.Raycast(enemy.Position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Platforms", "Ladders"));
       if (hit.collider != null) startNode = hit.collider.GetComponent<LevelNode>();
       
       hit = Physics2D.Raycast(target, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Platforms", "Ladders"));
       if (hit.collider != null) endNode = hit.collider.GetComponent<LevelNode>();
       
       if (startNode == null || endNode == null) return null;
      




       // Begin A* Pathfinding
       List<LevelNode> path = new List<LevelNode>(); // the return path
       List<PathNode> openList = new List<PathNode>();
       List<PathNode> closedList = new List<PathNode>();
       PathNode currentNode;

       // Set current node to start node
       currentNode = new PathNode(startNode);

       
       while (currentNode.node != endNode)
       {
           // Look at all nodes that are reachable from this node
           foreach (LevelNode levelNode in currentNode.node.GetNeighbours())
           {
               // Check traversability and if has all abilities required then examine path node
               uint trav = levelNode.TraversalAbilities;
               if ((enemy.Abilities & trav) == trav)
               {
                   PathNode neighbour = closedList.Find(x => x.node == levelNode); // if on the closed list, set neighbour to that pathnode
                   if (neighbour == null) neighbour = openList.Find(x => x.node == levelNode); // if not on open list, look for it on open list

                   // If neighbour is STILL null then it's not on open or closed list. Create and score it, then stick it on open list
                   if (neighbour == null)
                   {
                       neighbour = new PathNode(levelNode);
                       neighbour.parent = currentNode; // neighbour's parent is current node

                       // Let's calculate G!
                       Vector2 parentPos = new Vector2();
                       Vector2 thisPos = new Vector2();

                       switch (neighbour.parent.node.LevelType)
                       {
                           case LevelNode.Type.FLOOR:
                               float posY = neighbour.parent.node.Centre.y;

                               if (neighbour.node.Left > neighbour.parent.node.Left)
                               {
                                   if (neighbour.parent.node == startNode) parentPos = new Vector2(enemy.Position.x, posY);
                                   else parentPos = new Vector2(neighbour.parent.node.Left, posY);

                                   thisPos = new Vector2(neighbour.node.Left, posY);
                               }
                               else if (neighbour.node.Right < neighbour.parent.node.Right)
                               {
                                   if (neighbour.parent.node == startNode) parentPos = new Vector2(enemy.Position.x, posY);
                                   else parentPos = new Vector2(neighbour.parent.node.Right, posY);

                                   thisPos = new Vector2(neighbour.node.Right, posY);
                               }
                               break;

                           case LevelNode.Type.LADDER:
                               float posX = neighbour.parent.node.Centre.x;

                               if (neighbour.node.Bottom > neighbour.parent.node.Bottom)
                               {
                                   if (neighbour.parent.node == startNode) parentPos = new Vector2(posX, enemy.Position.y);
                                   else parentPos = new Vector2(posX, neighbour.parent.node.Bottom);

                                   thisPos = new Vector2(posX, neighbour.node.Bottom);
                               }
                               else if (neighbour.node.Top < neighbour.parent.node.Top)
                               {
                                   if (neighbour.parent.node == startNode) parentPos = new Vector2(posX, enemy.Position.y);
                                   else parentPos = new Vector2(posX, neighbour.parent.node.Top);

                                   thisPos = new Vector2(posX, neighbour.node.Top);
                               }
                               break;
                       }


                       float G = (Mathf.Abs(thisPos.x) - Mathf.Abs(parentPos.x)) + (Mathf.Abs(thisPos.y) - Mathf.Abs(parentPos.y));
                       neighbour.G = neighbour.parent.G + G; // G is node's parent G plus movement cost

                       // H = Heuristic Score. Estimated cost to get to goal
                       float H = (Mathf.Abs(target.x) - Mathf.Abs(thisPos.x)) + (Mathf.Abs(target.y) - Mathf.Abs(thisPos.y));
                       neighbour.H = H;

                       // Add neighbour to the open list
                       openList.Add(neighbour);

                   }
                   else
                   {
                       if (neighbour.parent != null)
                       {

                           // Neighbour already on a list. Just update G score if this would be a better parent
                           // Let's calculate G!
                           Vector2 parentPos = new Vector2();
                           Vector2 thisPos = new Vector2();

                           switch (neighbour.parent.node.LevelType)
                           {
                               case LevelNode.Type.FLOOR:
                                   float posY = neighbour.parent.node.Centre.y;

                                   if (neighbour.node.Left > neighbour.parent.node.Left)
                                   {
                                       if (neighbour.parent.node == startNode) parentPos = new Vector2(enemy.Position.x, posY);
                                       else parentPos = new Vector2(neighbour.parent.node.Left, posY);

                                       thisPos = new Vector2(neighbour.node.Left, posY);
                                   }
                                   else if (neighbour.node.Right < neighbour.parent.node.Right)
                                   {
                                       if (neighbour.parent.node == startNode) parentPos = new Vector2(enemy.Position.x, posY);
                                       else parentPos = new Vector2(neighbour.parent.node.Right, posY);

                                       thisPos = new Vector2(neighbour.node.Right, posY);
                                   }
                                   break;

                               case LevelNode.Type.LADDER:
                                   float posX = neighbour.parent.node.Centre.x;

                                   if (neighbour.node.Bottom > neighbour.parent.node.Bottom)
                                   {
                                       if (neighbour.parent.node == startNode) parentPos = new Vector2(posX, enemy.Position.y);
                                       else parentPos = new Vector2(posX, neighbour.parent.node.Bottom);

                                       thisPos = new Vector2(posX, neighbour.node.Bottom);
                                   }
                                   else if (neighbour.node.Top < neighbour.parent.node.Top)
                                   {
                                       if (neighbour.parent.node == startNode) parentPos = new Vector2(posX, enemy.Position.y);
                                       else parentPos = new Vector2(posX, neighbour.parent.node.Top);

                                       thisPos = new Vector2(posX, neighbour.node.Top);
                                   }
                                   break;
                           }


                           float G = (Mathf.Abs(thisPos.x) - Mathf.Abs(parentPos.x)) + (Mathf.Abs(thisPos.y) - Mathf.Abs(parentPos.y));
                           G = neighbour.parent.G + G; // G is node's parent G plus movement cost
                           if (G < neighbour.G)
                           {
                               neighbour.G = G;
                               neighbour.parent = currentNode;
                           }
                       }
                   }

               }

           }

           // Sort open list by final score. Will put best node at the end
           openList.Sort(delegate(PathNode x, PathNode y)
           {
               if (x.F < y.F) return 1;
               if (x.F > y.F) return -1;
               return 0;
           });



           // Change current node
           closedList.Add(currentNode); // Add current node to closed list
           if (openList.Count == 0)
           {
               return null; // Ran out of open nodes so it must be impossible to get to the target node
           }
           currentNode = openList[openList.Count - 1]; // Set last element (lowest score) to current node
           openList.RemoveAt(openList.Count - 1); // Remove current node from open list
       }

       // Reconstruct the path
       while (currentNode.node != startNode)
       {
           path.Add(currentNode.node);
           currentNode = currentNode.parent;
       }
       path.Add(currentNode.node); // Make sure we add start node as breaks out of loop before start is added!
       path.Reverse();

       return new EnemyPath(path);
   }
}


class PathNode
{
    public PathNode(LevelNode levelNode) { node = levelNode; }

    public LevelNode node;
    public PathNode parent;
    public float G; //!< Geographical Score
    public float H; //!< Heuristic Score
    public float F { get { return G + H; } } //!< Final score

}


public class EnemyPath
{
    List<LevelNode> _nodes;

    public EnemyPath(List<LevelNode> nodes)
    {
        _nodes = nodes;
    }


    public int Size { get { return _nodes.Count; } }
    public LevelNode this[int key] { get { return _nodes[key]; } }


    
    public void DebugPrint()
    {
        Debug.Log("PATH:");
        foreach (LevelNode node in _nodes)
        {
            Debug.Log(node.gameObject.name);
        }
    }
}
