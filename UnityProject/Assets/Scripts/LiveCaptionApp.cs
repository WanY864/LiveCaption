using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Android;

public sealed class LiveCaptionBootstrap : MonoBehaviour
{
    private static bool created;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateApp()
    {
        if (created) return;
        created = true;

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(242, 243, 245, 255);
        camera.orthographic = true;

        var root = new GameObject("LiveCaptionApp");
        DontDestroyOnLoad(root);
        root.AddComponent<StickyNotesBoard>();
    }
}

public sealed class LiveCaptionApp : MonoBehaviour
{
    private Text captionText;
    private Text statusText;
    private Button startButton;
    private Image captionFill;
    private PermissionCallbacks permissionCallbacks;

    private bool listening;
    private bool largeText = true;
    private int fontSize = 62;
    private float captionOpacity = 0.35f;

    private readonly Color pageBackground = new Color32(242, 243, 245, 255);
    private readonly Color borderGrey = new Color32(145, 150, 158, 230);
    private readonly Color buttonGrey = new Color32(225, 228, 232, 255);
    private readonly Color buttonPressed = new Color32(205, 210, 216, 255);
    private readonly Color black = new Color32(24, 27, 31, 255);
    private readonly Color secondary = new Color32(100, 106, 114, 255);

    private void Start()
    {
        BuildInterface();
        SetStatus("READY");
        ApplyCaptionStyle();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && listening)
            StopListening("PAUSED · RETURN TO APP TO RESUME");
    }

    private void OnDestroy() { StopNativeBridge(); }

    public void OnCaption(string value)
    {
        if (captionText != null && !string.IsNullOrWhiteSpace(value))
            captionText.text = value.Trim();
    }

    public void OnSpeechStatus(string value)
    {
        SetStatus(string.IsNullOrWhiteSpace(value) ? "LISTENING · ENGLISH" : value.ToUpperInvariant());
    }

    public void OnSpeechError(string value)
    {
        listening = false;
        SetButtonLabel("START LISTENING");
        SetStatus(string.IsNullOrWhiteSpace(value) ? "RECOGNITION ERROR" : value.ToUpperInvariant());
    }

    private void BuildInterface()
    {
        var canvasObject = new GameObject("LiveCaptionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var background = MakeRect("Background", canvasObject.transform, new Vector2(0, 0), new Vector2(1, 1));
        background.gameObject.AddComponent<Image>().color = pageBackground;

        statusText = MakeText("Status", canvasObject.transform, "READY", 27, secondary, TextAnchor.MiddleLeft);
        Anchor(statusText.rectTransform, new Vector2(0.07f, 0.89f), new Vector2(0.65f, 0.95f));

        var brand = MakeText("Brand", canvasObject.transform, "LIVE CAPTION", 23, secondary, TextAnchor.MiddleRight);
        Anchor(brand.rectTransform, new Vector2(0.60f, 0.89f), new Vector2(0.93f, 0.95f));

        // Grey outer frame + adjustable translucent white inner surface.
        var frame = MakeRect("CaptionBorder", canvasObject.transform, new Vector2(0.06f, 0.53f), new Vector2(0.94f, 0.76f));
        frame.gameObject.AddComponent<Image>().color = borderGrey;

        var inner = MakeRect("CaptionPanel", frame, new Vector2(0.006f, 0.025f), new Vector2(0.994f, 0.975f));
        captionFill = inner.gameObject.AddComponent<Image>();
        captionFill.color = new Color(1f, 1f, 1f, captionOpacity);
        inner.gameObject.AddComponent<RectMask2D>();

        captionText = MakeText("CurrentCaption", inner, "Your English captions will appear here…", fontSize, black, TextAnchor.MiddleLeft);
        captionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        captionText.verticalOverflow = VerticalWrapMode.Truncate;
        captionText.alignByGeometry = true;
        Anchor(captionText.rectTransform, new Vector2(0.055f, 0.12f), new Vector2(0.945f, 0.88f));
        ApplyCaptionStyle();

        var sizeLabel = MakeText("SizeLabel", canvasObject.transform, "TEXT SIZE", 23, secondary, TextAnchor.MiddleLeft);
        Anchor(sizeLabel.rectTransform, new Vector2(0.07f, 0.43f), new Vector2(0.35f, 0.49f));
        MakeButton(canvasObject.transform, "A−", new Vector2(0.40f, 0.425f), new Vector2(0.55f, 0.49f), () => SetFontSize(50));
        MakeButton(canvasObject.transform, "A", new Vector2(0.57f, 0.425f), new Vector2(0.72f, 0.49f), () => SetFontSize(62));
        MakeButton(canvasObject.transform, "A+", new Vector2(0.74f, 0.425f), new Vector2(0.93f, 0.49f), () => SetFontSize(74));

        var opacityLabel = MakeText("OpacityLabel", canvasObject.transform, "BACKGROUND OPACITY", 23, secondary, TextAnchor.MiddleLeft);
        Anchor(opacityLabel.rectTransform, new Vector2(0.07f, 0.33f), new Vector2(0.50f, 0.39f));
        MakeButton(canvasObject.transform, "25%", new Vector2(0.52f, 0.325f), new Vector2(0.64f, 0.39f), () => SetOpacity(0.25f));
        MakeButton(canvasObject.transform, "50%", new Vector2(0.66f, 0.325f), new Vector2(0.79f, 0.39f), () => SetOpacity(0.50f));
        MakeButton(canvasObject.transform, "75%", new Vector2(0.81f, 0.325f), new Vector2(0.93f, 0.39f), () => SetOpacity(0.75f));

        startButton = MakeButton(canvasObject.transform, "START LISTENING", new Vector2(0.07f, 0.17f), new Vector2(0.64f, 0.25f), ToggleListening, true);
        MakeButton(canvasObject.transform, "CLEAR", new Vector2(0.67f, 0.17f), new Vector2(0.93f, 0.25f), ClearCaption);

        var footer = MakeText("Footer", canvasObject.transform, "ENGLISH ONLY · KEEP THIS SCREEN OPEN", 19, secondary, TextAnchor.MiddleCenter);
        Anchor(footer.rectTransform, new Vector2(0.06f, 0.075f), new Vector2(0.94f, 0.12f));
    }

    private static RectTransform MakeRect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text MakeText(string name, Transform parent, string value, int size, Color color, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.supportRichText = false;
        text.raycastTarget = false;
        return text;
    }

    private Button MakeButton(Transform parent, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action, bool primary = false)
    {
        var rect = MakeRect(label.Replace(" ", "") + "Button", parent, min, max);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = primary ? new Color32(208, 232, 224, 255) : buttonGrey;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        var colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color32(215, 220, 225, 255);
        colors.pressedColor = buttonPressed;
        colors.selectedColor = image.color;
        button.colors = colors;

        var outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = borderGrey;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var text = MakeText("Label", rect, label, 25, black, TextAnchor.MiddleCenter);
        Anchor(text.rectTransform, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.96f));
        return button;
    }

    private void ToggleListening()
    {
        if (listening) StopListening("PAUSED");
        else StartListening();
    }

    private void StartListening()
    {
#if UNITY_EDITOR
        listening = true;
        SetButtonLabel("PAUSE");
        SetStatus("EDITOR DEMO · ENGLISH");
        StopAllCoroutines();
        StartCoroutine(EditorDemo());
#elif UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            permissionCallbacks = new PermissionCallbacks();
            permissionCallbacks.PermissionGranted += _ => StartNativeListening();
            permissionCallbacks.PermissionDenied += _ => OnSpeechError("Microphone permission denied");
            Permission.RequestUserPermission(Permission.Microphone, permissionCallbacks);
        }
        else StartNativeListening();
#else
        SetStatus("ANDROID DEVICE REQUIRED");
#endif
    }

    private void StartNativeListening()
    {
        try
        {
            using (var bridge = new AndroidJavaClass("com.wany864.livecaption.LiveCaptionSpeechBridge"))
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                bridge.CallStatic("start", player.GetStatic<AndroidJavaObject>("currentActivity"), "LiveCaptionApp");
            }
            listening = true;
            SetButtonLabel("PAUSE");
            SetStatus("STARTING · ENGLISH");
        }
        catch (System.Exception e)
        {
            OnSpeechError("START FAILED: " + e.Message);
        }
    }

    private System.Collections.IEnumerator EditorDemo()
    {
        string[] samples =
        {
            "Today we're exploring human computer interaction.",
            "Live captions should be clear and easy to follow.",
            "This is an English only speech recognition demo."
        };
        var index = 0;
        while (listening)
        {
            OnCaption(samples[index++ % samples.Length]);
            yield return new WaitForSeconds(3f);
        }
    }

    private void StopListening(string message)
    {
        listening = false;
        StopAllCoroutines();
        StopNativeBridge();
        SetButtonLabel("START LISTENING");
        SetStatus(message);
    }

    private void StopNativeBridge()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var bridge = new AndroidJavaClass("com.wany864.livecaption.LiveCaptionSpeechBridge"))
                bridge.CallStatic("stop");
        }
        catch { }
#endif
    }

    private void ClearCaption()
    {
        captionText.text = "";
        SetStatus(listening ? "LISTENING · ENGLISH" : "CLEARED");
    }

    private void SetFontSize(int value)
    {
        fontSize = value;
        ApplyCaptionStyle();
    }

    private void SetOpacity(float value)
    {
        captionOpacity = Mathf.Clamp01(value);
        ApplyCaptionStyle();
    }

    private void ApplyCaptionStyle()
    {
        if (captionText != null)
        {
            captionText.fontSize = fontSize;
            captionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            captionText.verticalOverflow = VerticalWrapMode.Truncate;
            // Fixed-height text box sized for two lines at the selected font size.
            var rect = captionText.rectTransform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fontSize * 2.55f);
        }

        if (captionFill != null)
            captionFill.color = new Color(1f, 1f, 1f, captionOpacity);
    }

    private void SetButtonLabel(string value)
    {
        var label = startButton != null ? startButton.GetComponentInChildren<Text>() : null;
        if (label != null) label.text = value;
    }

    private void SetStatus(string value)
    {
        if (statusText != null) statusText.text = value;
    }
}
