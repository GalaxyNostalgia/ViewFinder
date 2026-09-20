using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

public static class ViewfinderLevelBuilder
{
    const string Level01Path = "Assets/Scenes/Level01.unity";
    const string Level02Path = "Assets/Scenes/Level02.unity";

    const string MaterialFolder = "Assets/Level/Materials/Generated";
    const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
    const string PanelSettingsPath = "Assets/UI/GameHUD_PanelSettings.asset";
    const string PhotoHudUxmlPath = "Assets/UI/PhotoHandHUD.uxml";
    const string LevelEndUxmlPath = "Assets/UI/LevelEnd.uxml";

    static int CuttableLayer => LayerMask.NameToLayer("Cuttable");

    [MenuItem("Tools/Viewfinder/Build Level 01 Scene")]
    public static void BuildLevel01()
    {
        if (!BeginScene(out var scene))
            return;

        var stone = GetMaterial("Gen_Stone", new Color(0.62f, 0.62f, 0.60f));
        var accent = GetMaterial("Gen_Accent", new Color(0.85f, 0.45f, 0.25f));
        var goalMat = GetMaterial("Gen_Goal", new Color(0.30f, 0.75f, 0.55f));

        Cuttable("StartPlatform", new Vector3(0f, 0f, 0f), new Vector3(10f, 1f, 10f), stone);
        Cuttable("GoalPlatform", new Vector3(0f, 0f, 26f), new Vector3(10f, 1f, 10f), stone);
        Cuttable("BridgeSlab", new Vector3(16f, 0f, 13f), new Vector3(4f, 1f, 16f), accent);
        Cuttable("BackWall", new Vector3(22f, 4f, 13f), new Vector3(1f, 10f, 20f), stone);
        Cuttable("PillarLeft", new Vector3(-6f, 3f, 13f), new Vector3(1.5f, 7f, 1.5f), stone);
        Cuttable("PillarRight", new Vector3(6f, 3f, 13f), new Vector3(1.5f, 7f, 1.5f), stone);

        BuildGoal(new Vector3(0f, 1.6f, 28f), goalMat);
        BuildRespawnVolume(new Vector3(0f, -25f, 13f), 200f);
        BuildPlayer(new Vector3(0f, 1.2f, -3f), captureDistance: 25f);

        SaveScene(scene, Level01Path);
    }

    [MenuItem("Tools/Viewfinder/Build Level 02 Scene")]
    public static void BuildLevel02()
    {
        if (!BeginScene(out var scene))
            return;

        var stone = GetMaterial("Gen_Stone", new Color(0.62f, 0.62f, 0.60f));
        var accent = GetMaterial("Gen_Accent", new Color(0.85f, 0.45f, 0.25f));
        var barrier = GetMaterial("Gen_Barrier", new Color(0.45f, 0.17f, 0.20f));
        var scenery = GetMaterial("Gen_Scenery", new Color(0.24f, 0.25f, 0.30f));
        var goalMat = GetMaterial("Gen_Goal", new Color(0.30f, 0.75f, 0.55f));

        Cuttable("A_Spawn", new Vector3(0f, 0f, 2f), new Vector3(16f, 1f, 16f), stone);

        Cuttable("B_Alcove", new Vector3(-16.5f, 0f, 2f), new Vector3(17f, 1f, 8f), stone);

        Scenery("A_ObeliskL", new Vector3(-9f, 5f, -5f), new Vector3(1.6f, 11f, 1.6f), scenery);
        Scenery("A_ObeliskR", new Vector3(9f, 5f, -5f), new Vector3(1.6f, 11f, 1.6f), scenery);

        Cuttable("B_Plank", new Vector3(-20f, 0f, 19f), new Vector3(4f, 1f, 22f), accent);
        Cuttable("B_Landing", new Vector3(0f, 0f, 34.5f), new Vector3(16f, 1f, 17f), stone);

        Scenery("B_SpireW", new Vector3(-24f, 2f, 26f), new Vector3(3f, 24f, 3f), scenery);
        Scenery("B_SpireE", new Vector3(20f, -1f, 20f), new Vector3(3f, 20f, 3f), scenery);

        Cuttable("C_Gate", new Vector3(0f, 5f, 44f), new Vector3(18f, 10f, 2.5f), barrier);
        Cuttable("C_Court", new Vector3(0f, 0f, 54f), new Vector3(18f, 1f, 22f), stone);

        Scenery("C_FrameL", new Vector3(-10f, 6f, 44f), new Vector3(2f, 13f, 3f), scenery);
        Scenery("C_FrameR", new Vector3(10f, 6f, 44f), new Vector3(2f, 13f, 3f), scenery);

        Cuttable("D_Block", new Vector3(6f, 1.5f, 58f), new Vector3(5f, 4f, 5f), accent);
        Cuttable("D_Ledge", new Vector3(0f, 4.1f, 74f), new Vector3(18f, 1f, 16f), stone);

        Scenery("D_Buttress", new Vector3(0f, 1f, 66.5f), new Vector3(18f, 1.5f, 1.5f), scenery);

        Cuttable("E_Arm", new Vector3(-14f, 4.1f, 70f), new Vector3(10f, 1f, 10f), stone);
        Cuttable("E_Span", new Vector3(-14f, 4.1f, 96f), new Vector3(4f, 1f, 24f), accent);

        Cuttable("E_Barrier", new Vector3(0f, 8f, 92f), new Vector3(16f, 9f, 1.5f), barrier);

        Cuttable("E_Portal", new Vector3(0f, 4.1f, 110f), new Vector3(18f, 1f, 16f), stone);

        Scenery("E_SpireW", new Vector3(-22f, 0f, 96f), new Vector3(3.5f, 30f, 3.5f), scenery);
        Scenery("E_SpireE", new Vector3(22f, -2f, 90f), new Vector3(3.5f, 26f, 3.5f), scenery);
        Scenery("E_Backdrop", new Vector3(0f, 8f, 124f), new Vector3(30f, 18f, 2f), scenery);

        BuildGoal(new Vector3(0f, 6.2f, 116f), goalMat);
        BuildRespawnVolume(new Vector3(0f, -30f, 55f), 260f);
        BuildPlayer(new Vector3(0f, 1.2f, -3f), captureDistance: 30f);

        SaveScene(scene, Level02Path);
    }

    [MenuItem("Tools/Viewfinder/Set Up Build Scene List")]
    public static void SetUpBuildSettings()
    {
        var wanted = new[]
        {
            "Assets/Scenes/MainMenu.unity",
            Level01Path,
            Level02Path,
            "Assets/Scenes/Game.unity",
        };

        var list = new List<EditorBuildSettingsScene>();

        foreach (var path in wanted)
        {
            if (File.Exists(path))
                list.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("Build scene list: " + string.Join(", ", list.ConvertAll(s => s.path)));
    }

    static bool BeginScene(out UnityEngine.SceneManagement.Scene scene)
    {
        scene = default;

        if (CuttableLayer < 0)
        {
            EditorUtility.DisplayDialog("Missing layer",
                "There is no layer called 'Cuttable'. Add it in Project Settings > Tags and Layers first.",
                "OK");
            return false;
        }

        scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        BuildLighting();
        BuildEventSystem();

        return true;
    }

    static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Built {path}. Run Tools > Viewfinder > Set Up Build Scene List next.");
    }

    static void BuildLighting()
    {
        var go = new GameObject("Directional Light");
        go.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        light.shadows = LightShadows.Soft;
    }

    static void BuildEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    static GameObject Cuttable(string name, Vector3 position, Vector3 scale, Material material)
    {
        var go = MakeBox(name, position, scale, material);
        go.layer = CuttableLayer;

        var body = go.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        return go;
    }

    static GameObject Scenery(string name, Vector3 position, Vector3 scale, Material material)
    {
        return MakeBox(name, position, scale, material);
    }

    static GameObject MakeBox(string name, Vector3 position, Vector3 scale, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;

        if (material != null)
            go.GetComponent<MeshRenderer>().sharedMaterial = material;

        return go;
    }

    static void BuildGoal(Vector3 position, Material material)
    {
        var goal = MakeBox("LevelExit", position, new Vector3(2.5f, 2.5f, 0.4f), material);
        goal.GetComponent<BoxCollider>().isTrigger = true;

        var document = goal.AddComponent<UIDocument>();
        document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LevelEndUxmlPath);

        var end = goal.AddComponent<LevelEnd>();
        end.ui = document;
        end.mainMenuSceneName = "MainMenu";
    }

    static void BuildRespawnVolume(Vector3 position, float size)
    {
        var volume = MakeBox("RespawnVolume", position, new Vector3(size, 1f, size), null);
        volume.GetComponent<BoxCollider>().isTrigger = true;
        volume.AddComponent<Respawner>();
    }

    static void BuildPlayer(Vector3 spawn, float captureDistance)
    {
        var player = new GameObject("Player");
        player.transform.position = spawn;

        var controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        var holder = new GameObject("Camera Holder");
        holder.transform.SetParent(player.transform);
        holder.transform.localPosition = new Vector3(0f, 1.6f, 0f);

        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        cameraObject.transform.SetParent(holder.transform);
        cameraObject.transform.localPosition = Vector3.zero;

        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 70f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 400f;
        cameraObject.AddComponent<AudioListener>();

        var capturePoint = new GameObject("CapturePoint");
        capturePoint.transform.SetParent(holder.transform);
        capturePoint.transform.localPosition = Vector3.zero;

        var playerController = player.AddComponent<PlayerController>();
        playerController.cameraHolder = holder.transform;
        playerController.speed = 6f;
        playerController.mouseSensitivity = 0.1f;
        AssignInputActions(playerController);

        var frustum = player.AddComponent<CustomFrustumLocalSpace>();
        frustum.finder = camera;
        frustum.capturePoint = capturePoint.transform;
        frustum.controller = playerController;
        frustum.captureDistance = captureDistance;
        frustum.customOffset = -0.1f;

        player.AddComponent<LevelRestart>();

        var hud = BuildHud();

        var polaroidObject = new GameObject("Polaroid");
        polaroidObject.transform.SetParent(holder.transform);
        polaroidObject.transform.localPosition = Vector3.zero;

        var polaroid = polaroidObject.AddComponent<Polaroid>();
        polaroid.playerCamera = camera;
        polaroid.frustum = frustum;
        polaroid.hud = hud;
    }

    static PhotoHandHUD BuildHud()
    {
        var go = new GameObject("HUD");

        var document = go.AddComponent<UIDocument>();
        document.panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PhotoHudUxmlPath);

        return go.AddComponent<PhotoHandHUD>();
    }

    static void AssignInputActions(PlayerController controller)
    {
        var serialized = new SerializedObject(controller);

        SetActionReference(serialized, "moveAction", "Move");
        SetActionReference(serialized, "lookAction", "Look");
        SetActionReference(serialized, "jumpAction", "Jump");

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetActionReference(SerializedObject serialized, string field, string actionName)
    {
        var property = serialized.FindProperty(field);
        if (property == null)
            return;

        var reference = FindActionReference(actionName);
        if (reference == null)
        {
            Debug.LogWarning($"Could not find an InputActionReference named '{actionName}'.");
            return;
        }

        property.objectReferenceValue = reference;
    }

    static InputActionReference FindActionReference(string actionName)
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(InputActionsPath))
        {
            if (asset is InputActionReference reference &&
                reference.action != null &&
                reference.action.name == actionName &&
                reference.action.actionMap != null &&
                reference.action.actionMap.name == "Player")
            {
                return reference;
            }
        }

        return null;
    }

    static Material GetMaterial(string name, Color color)
    {
        string path = $"{MaterialFolder}/{name}.mat";

        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader);
        material.color = color;

        Directory.CreateDirectory(MaterialFolder);
        AssetDatabase.CreateAsset(material, path);

        return material;
    }
}
