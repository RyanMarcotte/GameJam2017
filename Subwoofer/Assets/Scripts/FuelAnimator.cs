using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelAnimator : MonoBehaviour
{
    private bool increase = true;

    void FixedUpdate()
    {
        //Rotation
        transform.Rotate(0, 0, Time.fixedDeltaTime * 90);

        //Pulse
        transform.localScale += (new Vector3(0.005f, 0.005f, 0) * (increase == true ? 1 : -1));

        //Control pulse radius
        if (transform.localScale.x > 1.3f || transform.localScale.x < 0.7f)
        {
            increase = !increase;
        }
    }
}