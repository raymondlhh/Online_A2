using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [Tooltip("Does this enemy attack with a gun?")]
    public bool attacksWithGun = false;
    [Tooltip("Does this enemy attack with a sword? Only one attack type should be selected.")]
    public bool attacksWithSword = false;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Attack()
    {
        if (attacksWithGun)
        {
            animator.SetBool("IsGunAttacking", true);
        }
        else if (attacksWithSword)
        {
            animator.SetBool("IsSwordAttacking", true);
        }
    }

    // This could be called from an animation event to signal the end of an attack
    public void EndAttack()
    {
        animator.SetBool("IsGunAttacking", false);
        animator.SetBool("IsSwordAttacking", false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
