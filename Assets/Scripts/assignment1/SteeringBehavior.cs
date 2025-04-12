using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    // you can use this label to show debug information,
    // like the distance to the (next) target
    public TextMeshProUGUI label, label2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
        // Assignment 1: If a single target was set, move to that target
        //                If a path was set, follow that path ("tightly")

        // you can use kinematic.SetDesiredSpeed(...) and kinematic.SetDesiredRotationalVelocity(...)
        //    to "request" acceleration/decceleration to a target speed/rotational velocity


    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
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
        int pointsTraveled = 0;
        while (pointsTraveled < path.Count)
        {
            Vector3 currentTarget = path[pointsTraveled];
            this.SetTarget(currentTarget);
            yield return null;
            yield return new WaitUntil(() => kinematic.GetdistanceToTarget() < 0.75f);
            pointsTraveled++;
        }
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }
}
