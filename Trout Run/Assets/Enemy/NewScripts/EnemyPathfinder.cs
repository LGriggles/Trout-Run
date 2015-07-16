using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class EnemyPathfinder 
{
   public static EnemyPath FindPath(Enemy enemy, Vector2 target)
   {
       int fuckOffAndDie = 0; // because Unity really fucking hates endless loops and it's just not worth spending 15 minutes looking at a frozen computer



       // Find start and end nodes
       LevelNode startLevelNode = null;
       LevelNode endLevelNode = null;     
       RaycastHit2D hit;
       int mask = LayerMask.GetMask("SolidScenery", "Platform", "Ladder");


       hit = Physics2D.Raycast(enemy.Position, Vector2.down, Mathf.Infinity, mask);
       if (hit.collider != null) startLevelNode = hit.collider.GetComponentInParent<LevelNode>();

       hit = Physics2D.Raycast(target, Vector2.down, Mathf.Infinity, mask);
       if (hit.collider != null) endLevelNode = hit.collider.GetComponentInParent<LevelNode>();
       
       

       if (startLevelNode == null || endLevelNode == null) return null;


       // Begin A* Pathfinding
       List<PathNode> path = new List<PathNode>(); // the return path
       List<PathNode> openList = new List<PathNode>();
       List<PathNode> closedList = new List<PathNode>();
       PathNode curPathNode;
       LevelNode curLevelNode;

       // Set current node to start node
       curPathNode = new PathNode(startLevelNode);
       curPathNode.entryPoint = enemy.Position;
       

       
       while (curPathNode.levelNode != endLevelNode)
       {
           curLevelNode = curPathNode.levelNode;




           // Look at all nodes that are reachable from this node
           foreach (LevelNode.Neighbour neighbour in curLevelNode.GetNeighbours())
           {
               LevelNode nextLevelNode = neighbour.node;

               // Check traversability and if has all abilities required then examine path node
               uint trav = nextLevelNode.TraversalAbilities;
               if ((enemy.Abilities & trav) == trav)
               {
                   Vector2 exitPoint = new Vector2(); // the entry point of neighbour if we went to it

                   // FROM FLOOR
                   // LEFT OR RIGHT:
                   // Right(max to min) or Left(min to max)

                   // UP OR DOWN:
                   // IF TO LADDER: (min max = other.centre.x > or < than this.centre.x) to Up(min) or Down(max)
                   // IF TO PLATFORM: min/max to min/max with the shortest distance


                   // FROM LADDER
                   // LEFT OR RIGHT:
                   // (min max = other.centre.y > or < than this.centre.y) to Right(min) or Left(max)

                   // UP OR DOWN
                   // IF TO LADDER: Up(max to min) or Down(min to max)
                   // IF TO PLATFORM: ?? Does this make any sense? To me it infurs you got to the top of a platform then jumped. I guess it would be min/max of ladder to nearest min/max of platform (to ladder.x) 


                   // FROM PLATFORM
                   // LEFT OR RIGHT:
                   // Right(max to min) or Left(min to max)

                   // UP OR DOWN:
                   // IF TO LADDER: (min max = other.centre.x > or < than this.centre.x) to Up(min) or Down(max)
                   // ELSE: min/max to min/max with the shortest distance





                   LevelNode.Direction dir = neighbour.connectionDir;
                   if (dir == LevelNode.Direction.LEFT || dir == LevelNode.Direction.RIGHT)
                   {
                       if (nextLevelNode.LevelType == LevelNode.Type.LADDER) { Debug.LogWarning("Pathfinder Error - Shouldn't move horizontally onto from ladder"); }

                       if (dir == LevelNode.Direction.RIGHT) exitPoint = nextLevelNode.MinEntry;
                       else exitPoint = nextLevelNode.MaxEntry;
                   }
                   else
                   {
                       if (nextLevelNode.LevelType != LevelNode.Type.LADDER)
                       {
                           if (curLevelNode.LevelType == LevelNode.Type.LADDER) { Debug.LogWarning("Pathfinder Error - Shouldn't move vertically from ladder to platform / floor"); }
                           else
                           {
                               // min/max to min/max with the shortest distance
                               float distToMin = Mathf.Abs(curPathNode.entryPoint.x - nextLevelNode.MinEntry.x);
                               float distToMax = Mathf.Abs(curPathNode.entryPoint.x - nextLevelNode.MaxEntry.x);

                               if (distToMin < distToMax) exitPoint = nextLevelNode.MinEntry;
                               else exitPoint = nextLevelNode.MaxEntry;
                           }
                       }
                       else
                       {
                           // If you are going onto a ladder, exit point it that ladders entry
                           if (dir == LevelNode.Direction.UP) exitPoint = nextLevelNode.MinEntry;
                           else exitPoint = nextLevelNode.MaxEntry;
                       }
                   }


                   // Do the A*
                   float travDist = Mathf.Abs(exitPoint.x - curPathNode.entryPoint.x) + Mathf.Abs(exitPoint.y - curPathNode.entryPoint.y); // traversal distance to calculate G score
                   PathNode nextPathNode = closedList.Find(x => x.levelNode == nextLevelNode); // if on the closed list, set neighbour to that pathnode
                   if (nextPathNode == null) nextPathNode = openList.Find(x => x.levelNode == nextLevelNode); // if not on open list, look for it on open list

                   // If neighbour is STILL null then it's not on open or closed list. Create and score it, then stick it on open list
                   if (nextPathNode == null)
                   {
                       nextPathNode = new PathNode(nextLevelNode);
                       nextPathNode.parentPathNode = curPathNode; // neighbour's parent is current node

                       // Let's calculate G and H!
                       nextPathNode.G = curPathNode.G + travDist;
                       nextPathNode.H = Mathf.Abs(target.x - exitPoint.x) - Mathf.Abs(target.y - exitPoint.y);
                       nextPathNode.entryPoint = exitPoint;
                       nextPathNode.connection = neighbour.connectionDir;

                       // Add neighbour to the open list
                       openList.Add(nextPathNode);

                   }
                   else
                   {
                       
                       if (nextPathNode.parentPathNode != null)
                       {
                           float G = curPathNode.G + travDist;
                           if (G < nextPathNode.G)
                           {
                               nextPathNode.G = G;
                               nextPathNode.parentPathNode = curPathNode;
                               nextPathNode.entryPoint = exitPoint;
                               nextPathNode.connection = neighbour.connectionDir;
                           }
                       }
                   }


               }//end if travsable           
           }// end look at all neighbours

           // Sort open list by final score. Will put best node at the end
           openList.Sort(delegate(PathNode x, PathNode y)
           {
               if (x.F < y.F) return 1;
               if (x.F > y.F) return -1;
               return 0;
           });



           // Change current node
           closedList.Add(curPathNode); // Add current node to closed list
           if (openList.Count == 0)
           {
               return null; // Ran out of open nodes so it must be impossible to get to the target node
           }
           curPathNode = openList[openList.Count - 1]; // Set last element (lowest score) to current node
           openList.RemoveAt(openList.Count - 1); // Remove current node from open list
       
       }//end while current node != end node



       // Reconstruct the path
       while (curPathNode.parentPathNode != null && fuckOffAndDie != 100)
       {
           path.Add(curPathNode);
           curPathNode = curPathNode.parentPathNode;
           fuckOffAndDie++;


       }
       path.Add(curPathNode); // add the start node!

       path.Reverse();

       // Correct connections (actual connection stored in neighbour)
       for (int i = 0; i < path.Count - 1; ++i)
       {
           path[i].connection = path[i + 1].connection;
       }


       return new EnemyPath(path);
   }
}


public class PathNode
{
    public PathNode(LevelNode node) { levelNode = node; }

    public LevelNode levelNode;
    public PathNode parentPathNode;
    public Vector2 entryPoint;
    public LevelNode.Direction connection;
    public float G; //!< Geographical Score
    public float H; //!< Heuristic Score
    public float F { get { return G + H; } } //!< Final score
}


public class EnemyPath
{
    List<PathNode> _nodes;

    public EnemyPath(List<PathNode> nodes)
    {
        _nodes = nodes;
    }


    public int Size { get { return _nodes.Count; } }
    public PathNode this[int key] { get { return _nodes[key]; } }


    
    public void DebugPrint()
    {
        Debug.Log("PATH:");
        foreach (PathNode node in _nodes)
        {
            Debug.Log(node.levelNode.gameObject.name + ": Entry = " + node.entryPoint.ToString());
        }
    }



    // Adjusts node entry points to be more accurate
    public void NormalizeConnections(Vector2 enemySize)
    {
        Vector2 enemyHalfSize = enemySize * 0.5f;

        for (int i = 0; i < _nodes.Count; ++i)
        {
            // If not last node
            if (i != _nodes.Count - 1)
            {
                PathNode thisNode = _nodes[i];
                PathNode nextNode = _nodes[i + 1];

                if (nextNode.levelNode.LevelType == LevelNode.Type.PLATFORM)
                {
                    if (thisNode.connection == LevelNode.Direction.UP || thisNode.connection == LevelNode.Direction.DOWN)
                    {
                        //Debug.Log(nextNode.levelNode.name + " centre.x = " + nextNode.levelNode.Centre.x);
                        nextNode.entryPoint.x = nextNode.levelNode.Centre.x;
                    }
                }

                if (thisNode.levelNode.LevelType == LevelNode.Type.LADDER)
                {
                    if (nextNode.entryPoint.y > thisNode.entryPoint.y) // moooovin on up
                    {

                    }
                    else
                    {
                        nextNode.entryPoint.y = 4;
                    }

                }

            }

        }
    }
}
