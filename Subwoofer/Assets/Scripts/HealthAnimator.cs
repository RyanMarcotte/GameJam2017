﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthAnimator : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(0, 0, Time.fixedDeltaTime * -90);
    }
}
