using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Polaroid : MonoBehaviour
{
    public Camera playerCamera;

    [FormerlySerializedAs("customFrustumLocalSpace")]
    public CustomFrustumLocalSpace frustum;

    public PhotoHandHUD hud;

    [Tooltip("Width of the photo preview texture. Height follows the camera aspect.")]
    public int previewWidth = 320;

    [Header("Aiming")]
    [Tooltip("Field of view while the right mouse button is held.")]
    public float aimFieldOfView = 30f;

    [Tooltip("How quickly the zoom eases in and out.")]
    public float aimSpeed = 12f;

    [Header("Audio")]
    [Tooltip("Shutter click, played the moment the photo is taken. Drop a clip from Assets/Audio here.")]
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

    // Scenes built before the shutter existed have no AudioSource on the Polaroid,
    // so make one rather than relying on RequireComponent to have added it.
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
