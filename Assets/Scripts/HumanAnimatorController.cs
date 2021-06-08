using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class HumanAnimatorController : MonoBehaviour
{
    public static bool isJoin = false;
    public static int animationNum = 0;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [PunRPC]
    public void PlayAnimation(string animation)
    {
        animator.SetTrigger(animation);
    }
    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isJoin", isJoin);
        animator.SetBool("isTouch", JoystickManager.isTouch);
    }
}
