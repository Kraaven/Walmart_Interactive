using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    // Start is called before the first frame update

    public Animator animator;

    public InputActionReference Grab;
    public InputActionReference Activate;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Grip", Grab.action.ReadValue<float>());
        animator.SetFloat("Trigger", Activate.action.ReadValue<float>());

    }
}
