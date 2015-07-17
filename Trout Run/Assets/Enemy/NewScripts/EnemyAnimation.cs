using UnityEngine;
using System.Collections;

public class EnemyAnimation
{
    private Animator _animator;
    private int _baseLayer;






    public EnemyAnimation(Animator animator)
    {
        if (animator == null) Debug.Log("Enemy has been created without an animator on it blud");
        _animator = animator;
        _baseLayer = _animator.GetLayerIndex("Base Layer");
    }







    public void SetAnim(AnimationHashIDs.Anim anim)
    {
        int hashId = AnimationHashIDs.GetHash(anim);

        if (_animator.HasState(_baseLayer, hashId))
        {
            _animator.Play(hashId);
            //Debug.Log("PLAYING " + anim.ToString());
        }
    }
	
}
















public static class AnimationHashIDs
{
    public enum Anim { IDLE, WALK, HIT, SHOOT, NUM_ANIMS }


    private static int[] _ids = new int[(int)Anim.NUM_ANIMS];
    private static bool _genHash = false;

    public static int GetHash(Anim anim)
    {
        if(anim == Anim.NUM_ANIMS) return -1;
        if (!_genHash)
        {
            GenerateHashIDs();
            _genHash = true;
        }
        return _ids[(int)anim];
    }


    private static void GenerateHashIDs()
    {
        _ids[(int)Anim.IDLE]     = Animator.StringToHash("Idle");
        _ids[(int)Anim.WALK]     = Animator.StringToHash("Walk");
        _ids[(int)Anim.HIT]      = Animator.StringToHash("Hit");
        _ids[(int)Anim.SHOOT]    = Animator.StringToHash("Shoot");
    }




}
