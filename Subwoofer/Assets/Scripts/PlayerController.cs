using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Use of rigid body allows the physics engine to apply
    private Rigidbody rb;
    private int speed;

    //The start function runs once at the beginning of the game
    void Start ()
    {
        //Obtain a reference to the rigid body
        rb = GetComponent<Rigidbody>();

        speed = 100;
    }
	
	//The update function runs each frame
	void FixedUpdate ()
    {
        //Obtain the movements
        //float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

		if (moveVertical > 0)
			rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
		
        //Apply the movements
        Vector2 movement = new Vector2(0.0f, moveVertical);

        rb.AddForce(movement * speed);
    }
}
