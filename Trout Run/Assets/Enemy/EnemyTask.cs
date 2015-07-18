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
    enum MoveState { IDLE, WALKING, CLIMBING, JUMPING } //jumping refers to falling through plats as well
    MoveState _moveState = MoveState.IDLE;

    EnemyPath _path;
    Vector2 _target;

    int _curNode;
    Vector2 _nodeStart;
    Vector2 _nodeEnd;
    float _moveDir; //!< 1 for up / right and -1 for down / left
    float _platDelay = 0; //!< Delay to stop chaining platforms too fast
    



    public ETWalkTo(Enemy enemy, EnemyPath path, Vector2 target)
    {
        _myEnemy = enemy;
        _path = path;
        _target = target;
        _curNode = 0;

        //_path.DebugPrint();
        _path.NormalizeConnections(new Vector2(1, 1));
        MoveToNode(0);
    }


    bool ReachedNodeEndX()
    {
        if (_moveDir > 0)
        {
            if (_myEnemy.Position.x >= _nodeEnd.x) return true;
        }
        else
        {
            if (_myEnemy.Position.x <= _nodeEnd.x) return true;
        }
        return false;
    }

    bool ReachedNodeEndY()
    {
        if (_moveDir > 0)
        {
            if (_myEnemy.Position.y >= _nodeEnd.y) return true;
        }
        else
        {
            if (_myEnemy.Position.y <= _nodeEnd.y) return true;
        }
        return false;
    }



    public override State DoTask()
    {
        PathNode curPathNode = _path[_curNode];

        switch (_moveState)
        {
            // IDLE - Work out what state should be based on current level node
            case MoveState.IDLE:
                if (curPathNode.levelNode.LevelType == LevelNode.Type.LADDER)
                {
                    _moveState = MoveState.CLIMBING;
                }
                else
                {
                    _moveState = MoveState.WALKING;
                }

                break;


            // WALKING - Walk left or right to destination
            case MoveState.WALKING:
                if (ReachedNodeEndX())
                {
                    // If platform, may need to jump if end point higher or lower than current pos
                    if (curPathNode.connection == LevelNode.Direction.UP || curPathNode.connection == LevelNode.Direction.DOWN)
                    {
                        // Above or below? = JUMP or DROP
                        _moveState = MoveState.JUMPING;
                        _platDelay = 0.2f;
                        if (curPathNode.connection == LevelNode.Direction.UP) _myEnemy.Jump();
                        else _myEnemy.DropThroughPlatform();
                    }
                    else
                    {
                        _moveState = MoveState.IDLE;
                        if (_curNode == _path.Size - 1) return State.COMPLETE;
                        else MoveToNode(_curNode + 1);
                    }
                }
                else
                {
                    _myEnemy.Walk(_moveDir);
                }

                break;


            // CLIMBING - Walk left or right to destination
            case MoveState.CLIMBING:
                if (ReachedNodeEndY())
                {
                    _moveState = MoveState.IDLE;
                    if (_curNode == _path.Size - 1) return State.COMPLETE;
                    else MoveToNode(_curNode + 1);
                    
                }
                else
                {
                    _myEnemy.UseLadder(_moveDir, _path[_curNode].levelNode);
                }

                break;


            // JUMPING - Jump up or fall through platform
            case MoveState.JUMPING:
                if (_platDelay > 0)
                {
                    _platDelay -= Time.deltaTime;
                }
                else
                {
                    if (_myEnemy.OnGround)
                    {
                        _moveState = MoveState.IDLE;
                        if (_curNode == _path.Size - 1) return State.COMPLETE;
                        else MoveToNode(_curNode + 1);
                    }
                }

                break;
        }

        return State.ONGOING;
    }




    void MoveToNode(int node)
    {
        _curNode = node;
        bool isLastNode = (_curNode == _path.Size - 1);
        

        // Calculate start and end position
        _nodeStart = _path[_curNode].entryPoint;    
        if (isLastNode) _nodeEnd = _target;
        else _nodeEnd = _path[_curNode+1].entryPoint;

        // Calculate direction
        switch (_path[_curNode].levelNode.LevelType)
        {
            case LevelNode.Type.FLOOR:
            case LevelNode.Type.PLATFORM:
                if (_nodeEnd.x > _nodeStart.x) _moveDir = 1;
                else _moveDir = -1;
                
                break;

            case LevelNode.Type.LADDER:
                if (_nodeEnd.y > _nodeStart.y) _moveDir = 1;
                else _moveDir = -1;
                
                break;

        }

       //Debug.Log("Moving to node " + node + "(" + _path[node].levelNode.LevelType.ToString() + "): Start = " + _nodeStart.ToString() + ", End = " + _nodeEnd.ToString());

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










//! Charge towards player like a loon with no thought for one's own safety
public class ETChargeAtPlayer : EnemyTask
{
    Transform _playerTrans;     //!< The player's transform
    float _initSqrDist;         //!< Initial squared distance - if player moves and ends up charging totally wrong direction, will reset charge if fursther than this
    float _sqrOvershoot;        //!< If passes player, how far should it go before turning round and charging again? Squared for performance reasons
    Vector2 _dir;               //!< Direction of charge

    enum ChargeState { START_CHARGE, CHARGING, OVERSHOT }
    ChargeState _chargeState;

    public ETChargeAtPlayer(Enemy enemy, float overshoot)
    {
        _myEnemy = enemy;
        _playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        _sqrOvershoot = overshoot * overshoot;
        _chargeState = ChargeState.START_CHARGE;
    }


    public override State DoTask()
    {
        Vector2 enemyToPlayer = (Vector2)(_playerTrans.position) - _myEnemy.Position;
        float sqrDist = enemyToPlayer.sqrMagnitude;

        switch (_chargeState)
        {
            case ChargeState.START_CHARGE:
                // begin charge
                _dir = enemyToPlayer.normalized;
                _initSqrDist = sqrDist;
                _chargeState = ChargeState.CHARGING;

                break;

            case ChargeState.CHARGING:
                // check if overshot
                if (sqrDist < _sqrOvershoot) _chargeState = ChargeState.OVERSHOT;
                else if (sqrDist > _initSqrDist) _chargeState = ChargeState.START_CHARGE;

                break;


            case ChargeState.OVERSHOT:
                // check dist is greater than overshoot
                if (sqrDist > _sqrOvershoot) _chargeState = ChargeState.START_CHARGE;

                break;
        }



        // Move based on ability
        if(_myEnemy.HasAbilities(EnemyProps.WALKS))
        {
            _myEnemy.Walk(Mathf.Sign(_dir.x));
        }
        else if (_myEnemy.HasAbilities(EnemyProps.FLYS))
        {
            _myEnemy.Fly(_dir);
        }
        else
        {
            return State.FAILED;
        }

        return State.ONGOING;
       
    }
}
