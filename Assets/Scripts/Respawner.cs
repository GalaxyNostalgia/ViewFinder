using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Respawner : MonoBehaviour
{

    public Transform respawnPoint;

    Vector3 startPosition;
    Quaternion startRotation;
    bool hasStartPose;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;

        var player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            startPosition = player.transform.position;
            startRotation = player.transform.rotation;
            hasStartPose = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        if (respawnPoint != null)
            player.Teleport(respawnPoint.position, respawnPoint.rotation);
        else if (hasStartPose)
            player.Teleport(startPosition, startRotation);
    }
}
