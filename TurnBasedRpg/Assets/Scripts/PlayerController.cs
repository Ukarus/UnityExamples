using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float movementSpeed = 10f;
    public Rigidbody2D rb;
    private Animator animator;
    Vector2 movement;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        //rb = GetComponent<Rigidbody2D>();
        Debug.Log(rb);
    }

    // Update is called once per frame
    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.magnitude);
    }

    void FixedUpdate()
    {
        // handle movement here
        rb.MovePosition(rb.position + (movementSpeed * Time.fixedDeltaTime * movement.normalized));
    }
}
