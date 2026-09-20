using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFrustumLocalSpace : MonoBehaviour
{
    [Tooltip("Camera that defines the shape of the photo. Only its FOV and aspect are used.")]
    public Camera finder;

    [Tooltip("How far into the world the photo reaches, in metres. This is deliberately NOT " +
             "the camera far clip plane - at 300 a single shot swallows the whole level.")]
    public float captureDistance = 25f;

    [Tooltip("Thickness of the slabs that detect what needs cutting. Negative pushes them " +
             "outwards, which keeps the frustum volume from re-catching the outside pieces.")]
    public float customOffset = -0.1f;

    public Transform capturePoint;
    public PlayerController controller;

    public bool IsCutting { get; private set; }

    const int SideCount = 4;
    const int FrustumSide = 4;

    class Side
    {
        public MeshFilter filter;
        public MeshCollider collider;
        public Plane plane;
        public Vector3 contactPoint;
        public readonly List<GameObject> detected = new List<GameObject>();
    }

    readonly Side[] sides = new Side[SideCount];
    readonly List<GameObject> objectsInFrustum = new List<GameObject>();

    MeshFilter frustumFilter;
    MeshCollider frustumCollider;

    Vector3 leftUp, rightUp, leftDown, rightDown, cameraPos, forwardVector;
    GameObject ending;
    PolaroidFilm activeFilm;

    float captureFieldOfView = -1f;

    void Start()
    {
        for (int i = 0; i < SideCount; i++)
        {
            sides[i] = new Side();
            CreateTrigger("CutPlane" + i, i, out sides[i].filter, out sides[i].collider);
        }

        CreateTrigger("FrustumVolume", FrustumSide, out frustumFilter, out frustumCollider);
    }

    void CreateTrigger(string name, int side, out MeshFilter filter, out MeshCollider collider)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        go.transform.localScale = Vector3.one;

        collider = go.GetComponent<MeshCollider>();
        collider.convex = true;
        collider.isTrigger = true;
        collider.enabled = false;

        filter = go.GetComponent<MeshFilter>();
        go.GetComponent<MeshRenderer>().enabled = false;

        var checker = go.AddComponent<CollisionChecker>();
        checker.frustumLocalSpace = this;
        checker.side = side;
    }

    public void Cut(bool isTakingPicture)
    {
        IsCutting = true;
        controller.ChangePlayerState(false);

        if (isTakingPicture || captureFieldOfView < 0f)
            captureFieldOfView = finder.fieldOfView;

        float height = 2f * captureDistance * Mathf.Tan(captureFieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * finder.aspect;

        leftUp    = capturePoint.TransformPoint(new Vector3(-width / 2,  height / 2, captureDistance));
        rightUp   = capturePoint.TransformPoint(new Vector3( width / 2,  height / 2, captureDistance));
        leftDown  = capturePoint.TransformPoint(new Vector3(-width / 2, -height / 2, captureDistance));
        rightDown = capturePoint.TransformPoint(new Vector3( width / 2, -height / 2, captureDistance));

        cameraPos = capturePoint.position;
        forwardVector = capturePoint.forward;

        var corners = new[]
        {
            (a: leftUp,    b: leftDown),
            (a: rightDown, b: rightUp),
            (a: rightUp,   b: leftUp),
            (a: leftDown,  b: rightDown),
        };

        for (int i = 0; i < SideCount; i++)
        {
            var side = sides[i];
            var (a, b) = corners[i];

            side.detected.Clear();
            side.plane = new Plane(cameraPos, a, b);
            side.contactPoint = (a + b + cameraPos) / 3f;

            Vector3 offset = side.plane.normal * customOffset;
            Vector3 mid = (a + b) * 0.5f;

            side.filter.mesh = CreateBoxMesh(cameraPos, a, mid, b,
                                             b + offset, mid + offset, a + offset, cameraPos + offset);
            side.collider.sharedMesh = side.filter.mesh;
            side.collider.enabled = true;
        }

        objectsInFrustum.Clear();
        ending = null;

        StartCoroutine(RunCut(isTakingPicture));
    }

    IEnumerator RunCut(bool isTakingPicture)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        foreach (var side in sides)
            side.collider.enabled = false;

        var allObjects = new List<GameObject>();
        var intactObjects = new List<GameObject>();
        var chunks = new Dictionary<GameObject, List<GameObject>>();

        foreach (var side in sides)
            CutAlong(side, isTakingPicture, allObjects, intactObjects, chunks);

        frustumFilter.mesh = CreateFrustumMesh();
        frustumCollider.sharedMesh = frustumFilter.mesh;
        frustumCollider.enabled = true;

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        frustumCollider.enabled = false;

        if (ending != null)
            objectsInFrustum.Add(ending);

        if (isTakingPicture)
        {
            activeFilm = new PolaroidFilm(objectsInFrustum, capturePoint);

            foreach (var obj in intactObjects)
                obj.SetActive(true);

            foreach (var obj in allObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
        }
        else
        {
            foreach (var obj in objectsInFrustum)
            {
                if (obj != null)
                    Destroy(obj);
            }

            if (activeFilm != null)
            {
                activeFilm.ActivateFilm();
                activeFilm = null;
            }

            captureFieldOfView = -1f;
        }

        yield return new WaitForSeconds(0.5f);

        IsCutting = false;
        controller.ChangePlayerState(true);
    }

    void CutAlong(Side side, bool isTakingPicture, List<GameObject> allObjects,
                  List<GameObject> intactObjects, Dictionary<GameObject, List<GameObject>> chunks)
    {
        foreach (var obj in side.detected)
        {
            if (obj == null)
                continue;

            if (!allObjects.Contains(obj))
                allObjects.Add(obj);

            bool firstTouch = !chunks.TryGetValue(obj, out var pieces);

            if (firstTouch)
            {
                pieces = new List<GameObject> { obj };
                chunks[obj] = pieces;

                if (isTakingPicture)
                {
                    var intact = Instantiate(obj, obj.transform.position, obj.transform.rotation,
                                             obj.transform.parent);
                    intact.name = obj.name;
                    intact.SetActive(false);
                    intactObjects.Add(intact);
                }
            }

            int count = pieces.Count;
            for (int i = 0; i < count; i++)
            {
                var newPiece = Cutter.Cut(pieces[i], side.contactPoint, side.plane.normal);
                if (newPiece == null)
                    continue;

                pieces.Add(newPiece);
                allObjects.Add(newPiece);
            }
        }
    }

    public void AddObjectToCut(GameObject toCut, int side)
    {
        var list = side == FrustumSide ? objectsInFrustum : sides[side].detected;

        if (!list.Contains(toCut))
            list.Add(toCut);
    }

    public void AddEndingObject(GameObject end) => ending = end;

    Mesh CreateFrustumMesh()
    {
        Vector3 leftOffset = sides[0].plane.normal * -customOffset;
        Vector3 rightOffset = sides[1].plane.normal * -customOffset;

        var vertices = new[]
        {
            cameraPos + forwardVector * -customOffset,
            rightDown + rightOffset,
            rightUp + rightOffset,
            leftUp + leftOffset,
            leftDown + leftOffset,
        };

        var triangles = new[]
        {
            0, 2, 1,
            4, 1, 2,
            4, 2, 3,
            0, 4, 3,
            0, 1, 4,
            0, 3, 2,
        };

        return new Mesh { vertices = vertices, triangles = triangles };
    }

    Mesh CreateBoxMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,
                       Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8)
    {
        var vertices = new[] { v1, v2, v3, v4, v5, v6, v7, v8 };

        var triangles = new[]
        {
            0, 1, 2,  0, 2, 3,
            7, 5, 6,  7, 4, 5,
            0, 1, 6,  0, 6, 7,
            1, 2, 5,  1, 5, 6,
            2, 3, 4,  2, 4, 5,
            3, 0, 7,  3, 7, 4,
        };

        return new Mesh { vertices = vertices, triangles = triangles };
    }

    void OnDrawGizmos()
    {
        if (finder == null || capturePoint == null)
            return;

        float height = 2f * captureDistance * Mathf.Tan(finder.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * finder.aspect;

        var lu = capturePoint.TransformPoint(new Vector3(-width / 2,  height / 2, captureDistance));
        var ru = capturePoint.TransformPoint(new Vector3( width / 2,  height / 2, captureDistance));
        var ld = capturePoint.TransformPoint(new Vector3(-width / 2, -height / 2, captureDistance));
        var rd = capturePoint.TransformPoint(new Vector3( width / 2, -height / 2, captureDistance));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(capturePoint.position, lu);
        Gizmos.DrawLine(capturePoint.position, ru);
        Gizmos.DrawLine(capturePoint.position, ld);
        Gizmos.DrawLine(capturePoint.position, rd);
        Gizmos.DrawLine(lu, ru);
        Gizmos.DrawLine(ld, rd);
        Gizmos.DrawLine(lu, ld);
        Gizmos.DrawLine(ru, rd);
    }
}

public class PolaroidFilm
{
    readonly List<GameObject> placeHolders = new List<GameObject>();

    public PolaroidFilm(List<GameObject> captured, Transform parentToFollow)
    {
        foreach (var obj in captured)
        {
            if (obj == null)
                continue;

            var placeholder = Object.Instantiate(obj, obj.transform.position, obj.transform.rotation);
            placeholder.transform.SetParent(parentToFollow);
            placeholder.SetActive(false);
            placeHolders.Add(placeholder);
        }
    }

    public void ActivateFilm()
    {
        foreach (var placeholder in placeHolders)
        {
            placeholder.transform.SetParent(null);
            placeholder.SetActive(true);
        }
    }
}
