using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalAnimator : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(0, Time.fixedDeltaTime * 90, 0);
    }
}
