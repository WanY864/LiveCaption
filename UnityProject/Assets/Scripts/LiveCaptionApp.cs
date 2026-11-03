using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class LiveCaptionBootstrap : MonoBehaviour {
    private static bool created;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateApp() {
        if(created) return;
        created=true;
        var root=new GameObject("LiveCaptionApp");
        DontDestroyOnLoad(root);
        root.AddComponent<LiveCaptionApp>();
    }
}

public sealed class LiveCaptionApp : MonoBehaviour {
    private Text captionText,statusText;
    private Button startButton;
    private bool listening,twoLines,largeText=true;
    private readonly Color ink=new Color32(8,11,18,255), panel=new Color32(21,27,39,255),
        accent=new Color32(113,224,193,255), muted=new Color32(146,155,170,255), paper=new Color32(244,247,251,255);

    private void Start(){BuildInterface();SetStatus("READY");}
    private void OnApplicationPause(bool paused){if(paused&&listening)StopListening("PAUSED · RETURN TO APP TO RESUME");}
    private void OnDestroy(){StopNativeBridge();}
    public void OnCaption(string value){if(captionText!=null&&!string.IsNullOrWhiteSpace(value))captionText.text=value.Trim();}
    public void OnSpeechStatus(string value){SetStatus(string.IsNullOrWhiteSpace(value)?"LISTENING · ENGLISH":value.ToUpperInvariant());}
    public void OnSpeechError(string value){listening=false;SetButtonLabel("START LISTENING");SetStatus(value.ToUpperInvariant());}

    private void BuildInterface(){
        var c=new GameObject("LiveCaptionCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        c.transform.SetParent(transform,false);c.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=c.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
        if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        var bg=Rect("Background",c.transform,new Vector2(0,0),new Vector2(1,1));bg.gameObject.AddComponent<Image>().color=ink;
        statusText=Txt("Status",c.transform,"READY",30,muted,TextAnchor.MiddleLeft);Anchor(statusText.rectTransform,new Vector2(.08f,.87f),new Vector2(.92f,.94f));
        var brand=Txt("Brand",c.transform,"LIVE CAPTION",24,muted,TextAnchor.MiddleRight);Anchor(brand.rectTransform,new Vector2(.55f,.87f),new Vector2(.92f,.94f));
        var card=Rect("CaptionPanel",c.transform,new Vector2(.07f,.38f),new Vector2(.93f,.66f));card.gameObject.AddComponent<Image>().color=panel;
        captionText=Txt("CurrentCaption",card,"Your English captions will appear here…",68,muted,TextAnchor.MiddleLeft);
        captionText.horizontalOverflow=HorizontalWrapMode.Wrap;captionText.verticalOverflow=VerticalWrapMode.Truncate;
        Anchor(captionText.rectTransform,new Vector2(.06f,.08f),new Vector2(.94f,.92f));
        var layout=Txt("LayoutLabel",c.transform,"CAPTION LAYOUT",24,muted,TextAnchor.MiddleLeft);Anchor(layout.rectTransform,new Vector2(.08f,.29f),new Vector2(.5f,.34f));
        Btn(c.transform,"1 LINE",new Vector2(.52f,.285f),new Vector2(.70f,.345f),()=>SetLines(false));
        Btn(c.transform,"2 LINES",new Vector2(.72f,.285f),new Vector2(.92f,.345f),()=>SetLines(true));
        var size=Txt("SizeLabel",c.transform,"TEXT SIZE",24,muted,TextAnchor.MiddleLeft);Anchor(size.rectTransform,new Vector2(.08f,.22f),new Vector2(.5f,.27f));
        Btn(c.transform,"A",new Vector2(.68f,.215f),new Vector2(.79f,.275f),()=>SetSize(false));
        Btn(c.transform,"A+",new Vector2(.81f,.215f),new Vector2(.92f,.275f),()=>SetSize(true));
        startButton=Btn(c.transform,"START LISTENING",new Vector2(.08f,.105f),new Vector2(.66f,.18f),Toggle,true);
        Btn(c.transform,"CLEAR",new Vector2(.69f,.105f),new Vector2(.92f,.18f),Clear);
        var foot=Txt("Footer",c.transform,"ENGLISH ONLY · KEEP THIS SCREEN OPEN",20,muted,TextAnchor.MiddleCenter);Anchor(foot.rectTransform,new Vector2(.06f,.045f),new Vector2(.94f,.085f));
    }
    private static RectTransform Rect(string n,Transform p,Vector2 a,Vector2 b){var g=new GameObject(n,typeof(RectTransform));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=a;r.anchorMax=b;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;return r;}
    private static void Anchor(RectTransform r,Vector2 a,Vector2 b){r.anchorMin=a;r.anchorMax=b;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private static Text Txt(string n,Transform p,string v,int s,Color c,TextAnchor a){var g=new GameObject(n,typeof(RectTransform),typeof(Text));g.transform.SetParent(p,false);var t=g.GetComponent<Text>();t.text=v;t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=s;t.color=c;t.alignment=a;t.raycastTarget=false;return t;}
    private Button Btn(Transform p,string label,Vector2 a,Vector2 b,UnityEngine.Events.UnityAction action,bool primary=false){var r=Rect(label,p,a,b);var im=r.gameObject.AddComponent<Image>();im.color=primary?accent:new Color32(34,43,58,255);var bt=r.gameObject.AddComponent<Button>();bt.targetGraphic=im;bt.onClick.AddListener(action);var t=Txt("Label",r,label,27,primary?ink:paper,TextAnchor.MiddleCenter);Anchor(t.rectTransform,new Vector2(.04f,.04f),new Vector2(.96f,.96f));return bt;}
    private void Toggle(){if(listening)StopListening("PAUSED");else StartListening();}
    private void StartListening(){
#if UNITY_EDITOR
        listening=true;SetButtonLabel("PAUSE");SetStatus("EDITOR DEMO · ENGLISH");StartCoroutine(Demo());
#elif UNITY_ANDROID && !UNITY_EDITOR
        try{using(var b=new AndroidJavaClass("com.wany864.livecaption.LiveCaptionSpeechBridge"))using(var p=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))b.CallStatic("start",p.GetStatic<AndroidJavaObject>("currentActivity"),"LiveCaptionApp");listening=true;SetButtonLabel("PAUSE");SetStatus("STARTING · ENGLISH");}catch(System.Exception e){SetStatus("START FAILED: "+e.Message);}
#else
        SetStatus("ANDROID DEVICE REQUIRED");
#endif
    }
    private System.Collections.IEnumerator Demo(){string[] s={"Today we're exploring human computer interaction.","Live captions should be clear and easy to follow.","This is an English only speech recognition demo."};int i=0;while(listening){OnCaption(s[i++%s.Length]);yield return new WaitForSeconds(3f);}}
    private void StopListening(string message){listening=false;StopAllCoroutines();StopNativeBridge();SetButtonLabel("START LISTENING");SetStatus(message);}
    private void StopNativeBridge(){
#if UNITY_ANDROID && !UNITY_EDITOR
        try{using(var b=new AndroidJavaClass("com.wany864.livecaption.LiveCaptionSpeechBridge"))b.CallStatic("stop");}catch{}
#endif
    }
    private void Clear(){captionText.text="";SetStatus(listening?"LISTENING · ENGLISH":"CLEARED");}
    private void SetLines(bool value){twoLines=value;captionText.verticalOverflow=VerticalWrapMode.Truncate;}
    private void SetSize(bool value){largeText=value;captionText.fontSize=value?68:52;}
    private void SetButtonLabel(string value){var t=startButton!=null?startButton.GetComponentInChildren<Text>():null;if(t!=null)t.text=value;}
    private void SetStatus(string value){if(statusText!=null)statusText.text=value;}
}
