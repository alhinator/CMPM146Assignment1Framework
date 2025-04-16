using UnityEngine;

public class LerpTargetIndicator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SteeringBehavior sb;
    
    void Update()
    {
        transform.position = sb.lerpTarget;

    }

}
