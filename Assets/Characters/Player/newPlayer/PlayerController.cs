using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]

public class PlayerController : MonoBehaviour {
    CharacterController controller;
    Animator animator;
    public Transform cube;

    public float speed = 5;
    public float rotateSpeed = 3;



    // Use this for initialization
    void Start () {
        controller = GetComponent<CharacterController> ();
        animator = GetComponent<Animator> ();
	
	}
	
	// Update is called once per frame
	void Update () {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (x != 0)
        {
            transform.Rotate(0f, x * rotateSpeed, 0f);
        }

        //Движение вперед
        if (z != 0)
        {
            animator.SetBool("Running", true);
            Vector3 dir = transform.TransformDirection(new Vector3(x * speed * Time.deltaTime, 0f, z * speed * Time.deltaTime));
            controller.Move(dir);
        } else
        {
            animator.SetBool("Running", false);
        }

    }

    void OnAnimatorIK()
    {
        animator.SetLookAtWeight(1f);
        animator.SetLookAtPosition(cube.position);
    }


}
