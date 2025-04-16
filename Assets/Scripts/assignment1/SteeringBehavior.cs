using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public Vector3 lerpTarget;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    // you can use this label to show debug information,
    // like the distance to the (next) target
    public TextMeshProUGUI label, label2;

    public bool lookingForFinalPoint;
    private int pointsTraveled;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        lookingForFinalPoint = true;
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
        // Assignment 1: If a single target was set, move to that target
        //                If a path was set, follow that path ("tightly")

        // you can use kinematic.SetDesiredSpeed(...) and kinematic.SetDesiredRotationalVelocity(...)
        //    to "request" acceleration/decceleration to a target speed/rotational velocity

        SetLerpTarget();

        kinematic.SetDesiredRotationalVelocity(kinematic.DetermineDesiredRotationalVelocity());
        kinematic.SetDesiredSpeed(kinematic.DetermineDesiredSpeed(lookingForFinalPoint));
    }
    private void SetLerpTarget() //aleghart's code
    {
        if (!lookingForFinalPoint)
        {
            //Debug.Log("lerp can be valid");
            lerpTarget = Vector3.Lerp(target, GetNextTarget(), 0.5f);
        }
        else
        {
            lerpTarget = target;
        }
        //Debug.Log("lerpTg:" + lerpTarget);
    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        if(this.path == null)
        {
            lookingForFinalPoint = true;

        }
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
        if (path != null && path.Count > 0)
        {
            StartCoroutine(FollowPath());
        }
    }

    private IEnumerator FollowPath() //bdelinel's code
    {
        lookingForFinalPoint = false;

        pointsTraveled = 0;
        while (pointsTraveled < path.Count)
        {
            Vector3 currentTarget = path[pointsTraveled];
            this.SetTarget(currentTarget);
            yield return null;
            //aleghart edited for part 2
            if (pointsTraveled == path.Count - 1) //we're looking for the final point
            {
                Debug.Log("On final point of path.");
                lookingForFinalPoint = true;
                yield return new WaitUntil(() => kinematic.GetDistanceToTarget() < 0.75f);

            }
            else
            {
                yield return new WaitUntil(() => kinematic.GetDistanceToTarget() < 5f);
                //changed to 5 on other points for larger tolerance
            }
            pointsTraveled++;
        }
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }

    public Vector3 GetNextTarget() //aleghart's code
    {
        if (path is null || lookingForFinalPoint)
        {
            return Vector3.negativeInfinity;
        }
        else
        {
            return path[pointsTraveled + 1];
        }
    }
}
