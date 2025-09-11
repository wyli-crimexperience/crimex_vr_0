using UnityEngine;

public class DynamicFootSolverMod : MonoBehaviour
{
    [SerializeField] IKFootSolver[] feet;
    [SerializeField] Transform body;

    [SerializeField] float baseStepDistance = 2f;
    [SerializeField] float baseStepLength = 2f;

    [SerializeField] float distanceScalingFactor = 0.1f;
    [SerializeField] float lengthScalingFactor = 0.2f;
    [SerializeField] float minStep = 1f;
    [SerializeField] float maxStep = 5f;

    private Vector3 lastBodyPosition;
    private float currentSpeed;

    void Start()
    {
        if (body == null) body = transform;
        lastBodyPosition = body.position;
    }

    void Update()
    {
        currentSpeed = (body.position - lastBodyPosition).magnitude / Time.deltaTime;
        lastBodyPosition = body.position;

        float stepDistance = Mathf.Clamp(baseStepDistance + currentSpeed * distanceScalingFactor, minStep, maxStep);
        float stepLength = Mathf.Clamp(baseStepLength + currentSpeed * lengthScalingFactor, minStep, maxStep);

        foreach (var foot in feet)
        {
            if (foot != null)
            {
                foot.SetStepSettings(stepDistance, stepLength);
            }
        }
    }
}
