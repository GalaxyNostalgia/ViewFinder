using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public CustomFrustumLocalSpace frustumLocalSpace;
    public int side;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Cuttable"))
            frustumLocalSpace.AddObjectToCut(other.gameObject, side);
        else if (other.gameObject.name.Contains("Ending"))
            frustumLocalSpace.AddEndingObject(other.gameObject);
    }
}
