using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isJoin", isJoin);
        animator.SetBool("isTouch", JoystickManager.isTouch);
        if (animationNum != 0)
        {
            string a = "animation" + animationNum;
            animator.SetTrigger(a);
            animationNum = 0;
        }
    }
}
