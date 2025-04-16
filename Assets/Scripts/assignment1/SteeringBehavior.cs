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

    public float angleToTarget;
    public float distanceToTarget;

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
        UpdateAngleAndDistanceToTarget();

        SetLerpTarget();

        kinematic.SetDesiredRotationalVelocity(DetermineDesiredRotationalVelocity());
        kinematic.SetDesiredSpeed(DetermineDesiredSpeed(lookingForFinalPoint));
    }
    private void UpdateAngleAndDistanceToTarget()
    {
        Vector3 directionToTarget = target - this.transform.position;
        distanceToTarget = Vector3.Distance(this.transform.position, target);
        label2.text = "Distance to target: " + distanceToTarget;


        angleToTarget = Vector3.SignedAngle(this.transform.forward, directionToTarget, Vector3.up);
        label.text = "angle to target: " + angleToTarget;


    }
    public float DetermineDesiredSpeed(bool lastTarget) //aleghart's code
    {
        float absAngle = Mathf.Abs(angleToTarget);
        float desired;
        if (distanceToTarget > 20) //outside of arrival tolerance
        {

            desired = Mathf.Lerp(kinematic.max_speed / 3, kinematic.max_speed, distanceToTarget / 20);
            if (absAngle > 45)
            {
                float angleMultiplier = Mathf.Lerp(0.8f, 0.3f, absAngle / 180);
                desired *= angleMultiplier;
            }

        }
        else if (distanceToTarget < 20 && !lastTarget)
        {
            if (CheckNextTurn())
            {
                desired = kinematic.max_speed / 4;
            }
            else
            {
                desired = kinematic.max_speed;
            }
        }
        else
        {
            desired = Mathf.Lerp(0, kinematic.max_speed / 1.5f, distanceToTarget / 10);
            if (Mathf.Abs(angleToTarget) > 45)
            {
                float angleMultiplier = Mathf.Lerp(0.8f, 0.3f, absAngle / 180);
                desired *= angleMultiplier;
            }
            if (distanceToTarget < 1)
            {
                desired = 0;
            }
        }
        return desired;
    }

    public float DetermineDesiredRotationalVelocity() //aleghart's code
    {
        Vector3 directionToTarget = target - this.transform.position;
        float absAngle = Mathf.Abs(angleToTarget);
        float desired;

        float percentOfTurn = absAngle / 180;

        desired = Mathf.Lerp(kinematic.max_rotational_velocity * 0.4f, kinematic.max_rotational_velocity, percentOfTurn);

        if (absAngle < 10)
        {
            desired = kinematic.max_rotational_velocity * 0.05f;
        }
        if (absAngle < 2 || distanceToTarget < 0.75f)
        {
            desired = 0;
        }

        desired *= Mathf.Sign(angleToTarget);
        return desired;
    }


    private void SetLerpTarget() //aleghart's code
    {
        if (!lookingForFinalPoint)
        {
            //Debug.Log("lerp can be valid");
            float distMult = GetDistanceToTarget() / 20;
            lerpTarget = Vector3.Lerp(GetNextTarget(), target, distMult);
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
                yield return new WaitUntil(() => GetDistanceToTarget() < 0.75f);

            }
            else
            {
                yield return new WaitUntil(() => GetDistanceToTarget() < 5f);
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
        if (path is null || pointsTraveled == path.Count)
        {
            return Vector3.negativeInfinity;
        }
        else
        {
            return path[pointsTraveled + 1];
        }
    }

    public float GetDistanceToTarget() //bdelinel's code
    {
        return distanceToTarget;
    }

    private bool CheckNextTurn()
    {
        Vector3 next = GetNextTarget();
        if (next != Vector3.negativeInfinity)
        {
            Vector3 directionToTarget = next - this.transform.position;
            float ang = Vector3.SignedAngle(this.transform.forward, directionToTarget, Vector3.up);
            return Mathf.Abs(ang) > 60f;
        }
        else { return false; }
    }




    /*public float DetermineDesiredSpeedOld() //aleghart's code, from single-target follow
    {
        float absAngle = Mathf.Abs(angleToTarget);
        steeringBehavior.label2.text = "Distance to target: " + distanceToTarget;
        float desired = 0;
        bool high, a, b, c, d, e, f;
        high = absAngle >= 60; //high turn angle
        a = distanceToTarget > 20 && absAngle < 60; //far and generally ahead
        b = distanceToTarget <= 20 && distanceToTarget >= 10 && absAngle < 60; // midrange and generally ahead;
        c = distanceToTarget <= 12; //close;
        d = distanceToTarget <= 6 && distanceToTarget > 0.75f && speed > max_speed * 0.15f;//quite close and high speed
        e = distanceToTarget <= 6 && distanceToTarget > 0.75f && speed < max_speed * 0.15f; //quite close and low speed
        f = distanceToTarget <= 0.75f; // within bounds
        if (d || f)
        {
            desired = 0;
        }
        else if (high)
        {
            desired = max_speed * 0.5f;
        }
        else if (a)
        {
            desired = max_speed;
        }
        else if (b)
        {
            desired = max_speed * 0.6f;
        }
        else if (c)
        {
            desired = max_speed * 0.25f;
        }
        else if (e)
        {
            desired = max_speed * 0.15f;
        }
        else
        {
            desired = 0;
        }
        return desired;


    }*/
}
