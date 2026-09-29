using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Polaroid : MonoBehaviour
{
    public Camera playerCamera;

    [FormerlySerializedAs("customFrustumLocalSpace")]
    public CustomFrustumLocalSpace frustum;

    public PhotoHandHUD hud;

    public int previewWidth = 320;

    [Header("Aiming")]
    public float aimFieldOfView = 30f;

    public float aimSpeed = 12f;

    [Header("Audio")]
    public AudioClip shutterClip;

    [Range(0f, 1f)]
    public float shutterVolume = 1f;

    AudioSource shutterSource;
    Camera previewCamera;
    RenderTexture previewTexture;
    float defaultFieldOfView;
    bool isAiming;
    bool isHoldingPhoto;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (hud == null)
            hud = FindAnyObjectByType<PhotoHandHUD>();
    }

    void Start()
    {
        defaultFieldOfView = playerCamera.fieldOfView;
        CreatePreviewCamera();
        CreateShutterSource();

        if (hud != null)
            hud.HidePhoto();
    }

    void CreatePreviewCamera()
    {
        int height = Mathf.Max(1, Mathf.RoundToInt(previewWidth / playerCamera.aspect));
        previewTexture = new RenderTexture(previewWidth, height, 16);

        var go = new GameObject("PhotoPreviewCamera");
        go.transform.SetParent(frustum.capturePoint, false);

        previewCamera = go.AddComponent<Camera>();
        previewCamera.CopyFrom(playerCamera);
        previewCamera.targetTexture = previewTexture;

        previewCamera.farClipPlane = frustum.captureDistance;
    }

    
    void CreateShutterSource()
    {
        shutterSource = GetComponent<AudioSource>();

        if (shutterSource == null)
            shutterSource = gameObject.AddComponent<AudioSource>();

        shutterSource.playOnAwake = false;
        shutterSource.loop = false;
        shutterSource.spatialBlend = 0f;
    }

    void Update()
    {
        UpdateAim();

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (frustum.IsCutting)
            return;

        if (!isHoldingPhoto && !isAiming)
            return;

        frustum.capturePoint.SetPositionAndRotation(playerCamera.transform.position,
                                                    playerCamera.transform.rotation);

        if (!isHoldingPhoto)
            TakePhoto();
        else
            PlacePhoto();
    }

    void UpdateAim()
    {
        isAiming = Mouse.current.rightButton.isPressed;

        float target = isAiming ? aimFieldOfView : defaultFieldOfView;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, target,
                                              1f - Mathf.Exp(-aimSpeed * Time.deltaTime));

        if (previewCamera != null)
            previewCamera.fieldOfView = playerCamera.fieldOfView;
    }

    void TakePhoto()
    {
        if (shutterClip != null)
            shutterSource.PlayOneShot(shutterClip, shutterVolume);

        previewCamera.enabled = false;

        if (hud != null)
            hud.ShowPhoto(previewTexture);

        frustum.Cut(true);
        isHoldingPhoto = true;
    }

    void PlacePhoto()
    {
        if (hud != null)
            hud.HidePhoto();

        frustum.Cut(false);
        previewCamera.enabled = true;
        isHoldingPhoto = false;
    }

    void OnDestroy()
    {
        if (previewTexture != null)
            previewTexture.Release();
    }
}
