using UnityEngine;
using System.Collections;

//! Base class for all enemy tasks
public abstract class EnemyTask
{
    public enum State { ONGOING, COMPLETE, FAILED }
    protected State _taskState;

    protected Enemy _myEnemy;

    public abstract State DoTask();

}



//! Walk to a location
public class ETWalkTo : EnemyTask
{
    EnemyPath _path;
    Vector2 _target;

    int _curNode;
    float _nodeTarget; //!< Target pos moving to from this node - note float rather than vector as moves either left-right (on floor) ot up-down (on ladder), as dictated by _moveDir
    Vector2 _moveDir;
    



    public ETWalkTo(Enemy enemy, EnemyPath path, Vector2 target)
    {
        _myEnemy = enemy;
        _path = path;
        _target = target;
        _curNode = 0;

        //_path.DebugPrint();
        MoveToNode(0);
    }


    public override State DoTask()
    {
        bool nodeEnd = false;

        if (_moveDir.y == 0)
        {
            _myEnemy.Walk(_moveDir.x);
            nodeEnd = ((_moveDir.x > 0 && _myEnemy.Position.x >= _nodeTarget) || (_moveDir.x < 0 && _myEnemy.Position.x <= _nodeTarget));
        }
        else
        {
            _myEnemy.UseLadder(_moveDir.y, _path[_curNode]);
            nodeEnd = ((_moveDir.y > 0 && _myEnemy.Position.y >= _nodeTarget) || (_moveDir.y < 0 && _myEnemy.Position.y <= _nodeTarget));
        }



        if (nodeEnd)
        {
            if (_curNode == _path.Size - 1) return State.COMPLETE;
            else MoveToNode(_curNode + 1);
        }


        return State.ONGOING;
    }




    void MoveToNode(int node)
    {
        _curNode = node;
        bool isLastNode = (_curNode == _path.Size - 1);

        // Calculate general direction to move in
        if (isLastNode)
        {
            _moveDir = _target - _myEnemy.Position;
        }
        else
        {
            _moveDir = _path[_curNode + 1].Centre - _myEnemy.Position;
        }



        // Change move dir to be in range of -1 to 1 on axis desired and calculate target
        if (_path[_curNode].LevelType == LevelNode.Type.FLOOR)
        {
            _moveDir.x = Mathf.Sign(_moveDir.x);
            _moveDir.y = 0;

            if (isLastNode) _nodeTarget = _target.x;
            else
            {
                if (_path[_curNode + 1].LevelType == LevelNode.Type.LADDER) _nodeTarget = _path[_curNode + 1].Centre.x;
                else if (_moveDir.x > 0) _nodeTarget = _path[_curNode + 1].Left;
                else _nodeTarget = _path[_curNode + 1].Right;
            }
        }
        else if (_path[_curNode].LevelType == LevelNode.Type.LADDER)
        {
            _moveDir.x = 0;
            _moveDir.y = Mathf.Sign(_moveDir.y);


            if (isLastNode) _nodeTarget = _target.y;
            else
            {
                if (_moveDir.y > 0) _nodeTarget = _path[_curNode].Top + 0.5f;//half size of enemy
                else _nodeTarget = _path[_curNode].Bottom + 0.5f;//half size of enemy
            }
        }
    }
}



//! Fly to a location
public class ETFlyTo : EnemyTask
{
    Vector2 _target;

    public ETFlyTo(Enemy enemy, Vector2 target)
    {
        _myEnemy = enemy;
        _target = target;
    }


    public override State DoTask()
    {
        Vector2 dir = (_target - _myEnemy.Position).normalized;
        _myEnemy.Fly(dir);

        if (Vector2.Distance(_target, _myEnemy.Position) < 0.1f)
        {
            return State.COMPLETE;
        }

        return State.ONGOING;
    }
}
