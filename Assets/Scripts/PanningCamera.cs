using UnityEngine;

public class PanningCamera : MonoBehaviour
{
    [Tooltip("How far it swings to each side, in degrees.")]
    public float sweepAngle = 20f;

    [Tooltip("Seconds for one full right-left-right sweep.")]
    public float period = 24f;

    [Tooltip("Where in the sweep it starts, 0-1. Handy if you want it off-centre.")]
    [Range(0f, 1f)]
    public float startOffset;

    Vector3 baseEuler;

    void Start()
    {
        baseEuler = transform.eulerAngles;
    }

    void Update()
    {
        if (period <= 0f)
            return;

        float phase = (Time.time / period + startOffset) * Mathf.PI * 2f;
        float yaw = Mathf.Sin(phase) * sweepAngle;

        transform.eulerAngles = new Vector3(baseEuler.x, baseEuler.y + yaw, baseEuler.z);
    }
}
