using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingBehaviour : MonoBehaviour
{
    private const float distancePerSecond = 6.0f;
    private const float frameInterval = 0.05f;
    private float distancePerFrame = distancePerSecond * frameInterval;
    private float persistLength = 1.0f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public void AnimateSinglePingObject(Vector3 start, Vector3 end, float maxDistance)
    {
        
        StartCoroutine("AnimatePingMechanic", new PingInfo(start, end, maxDistance));
    }

    private IEnumerator AnimatePingMechanic(PingInfo pingInfo)
    {
        Vector3 start = pingInfo.Start;
        Vector3 end = pingInfo.End;
        float maxDistance = pingInfo.MaxDistance;
        float targetDistance = Vector3.Distance(start, end);
        transform.position = start;
        for (int i = 0; i < targetDistance/distancePerFrame; i++)
        {
            yield return new WaitForSeconds(frameInterval);
            transform.position = Vector3.MoveTowards(transform.position, end, distancePerFrame);
        }
        if (targetDistance < maxDistance - distancePerFrame)
        {
            transform.position = end;
            yield return new WaitForSeconds(persistLength);
        }
        Destroy(this.gameObject);
    }
    
    private class PingInfo
    {
        public Vector3 Start { get; private set; }
        public Vector3 End { get; private set; }
        public float MaxDistance { get; private set; }

        public PingInfo(Vector3 start, Vector3 end, float maxDistance) {
            Start = start;
            End = end;
            MaxDistance = maxDistance;
        }
    }
}
