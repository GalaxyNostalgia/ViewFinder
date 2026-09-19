using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Polaroid : MonoBehaviour
{
    public Camera playerCamera;
    public CustomFrustumLocalSpace customFrustumLocalSpace;
    bool isFilmSpawned;
    

    void Update()
    {
        
        if (!isFilmSpawned)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                customFrustumLocalSpace.Cut(true);
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                customFrustumLocalSpace.capturePoint.position = playerCamera.transform.position;
                customFrustumLocalSpace.capturePoint.rotation = playerCamera.transform.rotation;
                customFrustumLocalSpace.Cut(false);
            }
        }
    }
    
    
    
}