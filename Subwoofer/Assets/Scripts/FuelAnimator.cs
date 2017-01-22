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
        if (increase == true)
        {
            transform.localScale += new Vector3(0.01f, 0.01f, 0);
            if (transform.localScale.x > 2.0f)
            {
                increase = false;
            }
        }
        else
        {
            transform.localScale -= new Vector3(0.01f, 0.01f, 0);
            if (transform.localScale.x < 0.5f)
            {
                increase = true;
            }
        }
    }
}
