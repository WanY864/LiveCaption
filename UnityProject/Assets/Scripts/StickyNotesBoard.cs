using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Android;

[Serializable] public class StickyNoteData { public int id,colorIndex; public string text; public float x,y; }
[Serializable] public class BoardStickerData { public int id,colorIndex; public string symbol; public float x,y; }
[Serializable] public class StickyBoardData {
    public List<StickyNoteData> notes=new List<StickyNoteData>();
    public List<BoardStickerData> stickers=new List<BoardStickerData>();
    public int nextNoteId=1,nextStickerId=1;
}
public sealed class StickyDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public RectTransform target; public Canvas rootCanvas; public Action finished;
    public void OnBeginDrag(PointerEventData e) {}
    public void OnDrag(PointerEventData e) {
        if(target==null)return;
        float s=rootCanvas!=null?Mathf.Max(.01f,rootCanvas.scaleFactor):1f;
        target.anchoredPosition+=e.delta/s;
    }
    public void OnEndDrag(PointerEventData e){finished?.Invoke();}
}

public sealed class StickyNotesBoard : MonoBehaviour {
    private const string FileName="livecaption_sticky_board.json";
    private readonly Color ink=new Color32(25,28,32,255), muted=new Color32(100,106,114,255);
    private readonly Color[] papers={
        new Color32(255,245,190,255),new Color32(255,218,225,255),
        new Color32(211,235,255,255),new Color32(218,244,220,255),
        new Color32(238,224,255,255)};
    private readonly Color[] stickerColors={
        new Color32(238,157,52,255),new Color32(221,83,111,255),
        new Color32(65,135,196,255),new Color32(83,157,104,255)};
    private Canvas canvas;
    private RectTransform board,stickerLayer,notesPage,captionsPage;
    private Text emptyLabel,captionText,captionStatus;
    private Button startButton;
    private Image captionFill;
    private PermissionCallbacks permissionCallbacks;
    private StickyBoardData data=new StickyBoardData();
    private bool listening;
    private int captionSize=62;
    private float captionOpacity=.35f;
    private string SavePath=>Path.Combine(Application.persistentDataPath,FileName);

    private void Start(){BuildUI();Load();RefreshEmpty();SetStatus("READY");}
    private void OnApplicationPause(bool p){if(p){Save();if(listening)StopListening("PAUSED");}}
    private void OnApplicationQuit(){Save();}
    private void OnDestroy(){Save();StopBridge();}

    // Android SpeechRecognizer callbacks remain available as an optional feature.
    public void OnCaption(string v){if(captionText!=null&&!string.IsNullOrWhiteSpace(v))captionText.text=v.Trim();}
    public void OnSpeechStatus(string v){SetStatus(string.IsNullOrWhiteSpace(v)?"LISTENING · ENGLISH":v.ToUpperInvariant());}
    public void OnSpeechError(string v){listening=false;SetStartLabel("START LISTENING");SetStatus(v);}

    private void BuildUI(){
        var g=new GameObject("LiveCaptionCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        g.transform.SetParent(transform,false);canvas=g.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=g.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
        if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        notesPage=Rect("NotesPage",g.transform,Vector2.zero,Vector2.one);
        notesPage.gameObject.AddComponent<Image>().color=new Color32(242,243,245,255);BuildNotes(notesPage);
        captionsPage=Rect("CaptionsPage",g.transform,Vector2.zero,Vector2.one);
        captionsPage.gameObject.AddComponent<Image>().color=new Color32(242,243,245,255);BuildCaptions(captionsPage);
        captionsPage.gameObject.SetActive(false);
    }

    private void BuildNotes(RectTransform root){
        var title=Txt("Title",root,"MY STICKY BOARD",32,ink,TextAnchor.MiddleLeft);
        Anchor(title.rectTransform,new Vector2(.06f,.91f),new Vector2(.58f,.97f));
        Button(root,"CAPTIONS",new Vector2(.70f,.91f),new Vector2(.94f,.97f),ShowCaptions);
        Button(root,"+ NOTE",new Vector2(.06f,.835f),new Vector2(.34f,.895f),AddNote,true);
        Button(root,"★",new Vector2(.39f,.835f),new Vector2(.51f,.895f),()=>AddSticker("★",0));
        Button(root,"♥",new Vector2(.53f,.835f),new Vector2(.65f,.895f),()=>AddSticker("♥",1));
        Button(root,"✦",new Vector2(.67f,.835f),new Vector2(.79f,.895f),()=>AddSticker("✦",2));
        Button(root,"✓",new Vector2(.81f,.835f),new Vector2(.94f,.895f),()=>AddSticker("✓",3));
        var hint=Txt("Hint",root,"Tap + NOTE to write · Drag note headers and stickers to move",19,muted,TextAnchor.MiddleCenter);
        Anchor(hint.rectTransform,new Vector2(.05f,.79f),new Vector2(.95f,.825f));
        var frame=Rect("BoardFrame",root,new Vector2(.045f,.075f),new Vector2(.955f,.78f));
        frame.gameObject.AddComponent<Image>().color=new Color32(228,231,234,255);
        board=Rect("Board",frame,new Vector2(.012f,.012f),new Vector2(.988f,.988f));
        board.gameObject.AddComponent<Image>().color=new Color32(249,249,247,255);
        emptyLabel=Txt("EmptyBoard",board,"Your notes will appear here.",25,muted,TextAnchor.MiddleCenter);
        Anchor(emptyLabel.rectTransform,new Vector2(.12f,.42f),new Vector2(.88f,.58f));
        stickerLayer=Rect("Stickers",board,Vector2.zero,Vector2.one);
    }

    private void BuildCaptions(RectTransform root){
        Button(root,"← NOTES",new Vector2(.06f,.91f),new Vector2(.34f,.97f),ShowNotes);
        var brand=Txt("CaptionBrand",root,"LIVE CAPTION",24,muted,TextAnchor.MiddleRight);
        Anchor(brand.rectTransform,new Vector2(.58f,.91f),new Vector2(.94f,.97f));
        captionStatus=Txt("CaptionStatus",root,"READY",25,muted,TextAnchor.MiddleLeft);
        Anchor(captionStatus.rectTransform,new Vector2(.07f,.84f),new Vector2(.93f,.89f));
        var frame=Rect("CaptionBorder",root,new Vector2(.06f,.53f),new Vector2(.94f,.76f));
        frame.gameObject.AddComponent<Image>().color=new Color32(145,150,158,255);
        var inner=Rect("CaptionPanel",frame,new Vector2(.006f,.025f),new Vector2(.994f,.975f));
        captionFill=inner.gameObject.AddComponent<Image>();captionFill.color=new Color(1,1,1,captionOpacity);
        inner.gameObject.AddComponent<RectMask2D>();
        captionText=Txt("CurrentCaption",inner,"Your English captions will appear here…",captionSize,ink,TextAnchor.MiddleLeft);
        captionText.horizontalOverflow=HorizontalWrapMode.Wrap;captionText.verticalOverflow=VerticalWrapMode.Truncate;
        Anchor(captionText.rectTransform,new Vector2(.055f,.12f),new Vector2(.945f,.88f));
        var size=Txt("Size",root,"TEXT SIZE",22,muted,TextAnchor.MiddleLeft);
        Anchor(size.rectTransform,new Vector2(.07f,.43f),new Vector2(.35f,.49f));
        Button(root,"A−",new Vector2(.40f,.425f),new Vector2(.55f,.49f),()=>SetCaptionSize(50));
        Button(root,"A",new Vector2(.57f,.425f),new Vector2(.72f,.49f),()=>SetCaptionSize(62));
        Button(root,"A+",new Vector2(.74f,.425f),new Vector2(.93f,.49f),()=>SetCaptionSize(74));
        var op=Txt("Opacity",root,"BACKGROUND OPACITY",20,muted,TextAnchor.MiddleLeft);
        Anchor(op.rectTransform,new Vector2(.07f,.33f),new Vector2(.50f,.39f));
        Button(root,"25%",new Vector2(.52f,.325f),new Vector2(.64f,.39f),()=>SetCaptionOpacity(.25f));
        Button(root,"50%",new Vector2(.66f,.325f),new Vector2(.79f,.39f),()=>SetCaptionOpacity(.5f));
        Button(root,"75%",new Vector2(.81f,.325f),new Vector2(.93f,.39f),()=>SetCaptionOpacity(.75f));
        startButton=Button(root,"START LISTENING",new Vector2(.07f,.17f),new Vector2(.64f,.25f),ToggleListening,true);
        Button(root,"CLEAR",new Vector2(.67f,.17f),new Vector2(.93f,.25f),ClearCaption);
    }

    private void ShowCaptions(){Save();notesPage.gameObject.SetActive(false);captionsPage.gameObject.SetActive(true);}
    private void ShowNotes(){captionsPage.gameObject.SetActive(false);notesPage.gameObject.SetActive(true);Save();}

    private void AddNote(){
        int i=data.notes.Count;
        var n=new StickyNoteData{id=data.nextNoteId++,text="",colorIndex=i%papers.Length,
            x=(i%2==0?-230f:230f),y=440f-((i/2)%3)*390f};
        data.notes.Add(n);MakeNote(n);RefreshEmpty();Save();
    }
    private void MakeNote(StickyNoteData n){
        var card=Rect("Note_"+n.id,board,Vector2.zero,Vector2.zero);
        card.sizeDelta=new Vector2(430,350);card.anchoredPosition=new Vector2(n.x,n.y);
        var bg=card.gameObject.AddComponent<Image>();bg.color=papers[Mathf.Abs(n.colorIndex)%papers.Length];
        var head=Rect("DragHeader",card,new Vector2(0,.82f),new Vector2(1,1));
        head.gameObject.AddComponent<Image>().color=new Color(1,1,1,.32f);
        var drag=head.gameObject.AddComponent<StickyDragHandle>();drag.target=card;drag.rootCanvas=canvas;drag.finished=Save;
        var label=Txt("DragLabel",head,"NOTE "+n.id+" · DRAG HERE",18,ink,TextAnchor.MiddleLeft);
        Anchor(label.rectTransform,new Vector2(.04f,.05f),new Vector2(.56f,.95f));
        Button(head,"COLOR",new Vector2(.59f,.12f),new Vector2(.82f,.88f),()=>CycleColor(n,bg));
        Button(head,"×",new Vector2(.84f,.12f),new Vector2(.97f,.88f),()=>DeleteNote(n,card));
        var box=Rect("InputBackground",card,new Vector2(.045f,.055f),new Vector2(.955f,.79f));
        box.gameObject.AddComponent<Image>().color=new Color(1,1,1,.28f);
        var input=MakeInput(box,n.text);
        input.onValueChanged.AddListener(v=>{n.text=v;Save();});
        input.onEndEdit.AddListener(v=>{n.text=v;Save();});
    }

    private InputField MakeInput(Transform parent,string value){
        var go=new GameObject("NoteInput",typeof(RectTransform),typeof(Image),typeof(InputField));
        go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
        r.offsetMin=new Vector2(12,10);r.offsetMax=new Vector2(-12,-10);
        var image=go.GetComponent<Image>();image.color=new Color(1,1,1,0);
        var text=Txt("Text",go.transform,"",25,ink,TextAnchor.UpperLeft);
        text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;
        Anchor(text.rectTransform,Vector2.zero,Vector2.one);
        var placeholder=Txt("Placeholder",go.transform,"Write your note…",25,new Color32(90,95,100,150),TextAnchor.UpperLeft);
        Anchor(placeholder.rectTransform,Vector2.zero,Vector2.one);
        var field=go.GetComponent<InputField>();field.textComponent=text;field.placeholder=placeholder;
        field.text=value??"";field.lineType=InputField.LineType.MultiLineNewline;field.characterLimit=1200;
        field.targetGraphic=image;field.shouldHideMobileInput=false;return field;
    }

    private void CycleColor(StickyNoteData n,Image bg){
        n.colorIndex=(n.colorIndex+1)%papers.Length;bg.color=papers[n.colorIndex];Save();
    }
    private void DeleteNote(StickyNoteData n,RectTransform card){
        data.notes.Remove(n);Destroy(card.gameObject);RefreshEmpty();Save();
    }
    private void AddSticker(string symbol,int color){
        int i=data.stickers.Count;
        var s=new BoardStickerData{id=data.nextStickerId++,symbol=symbol,colorIndex=color,
            x=-300f+(i%5)*145f,y=180f-((i/5)%4)*145f};
        data.stickers.Add(s);MakeSticker(s);stickerLayer.SetAsLastSibling();Save();
    }
    private void MakeSticker(BoardStickerData s){
        var r=Rect("Sticker_"+s.id,stickerLayer,Vector2.zero,Vector2.zero);
        r.sizeDelta=new Vector2(105,105);r.anchoredPosition=new Vector2(s.x,s.y);
        var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize=76;t.alignment=TextAnchor.MiddleCenter;t.text=s.symbol;
        t.color=stickerColors[Mathf.Abs(s.colorIndex)%stickerColors.Length];t.raycastTarget=true;
        var drag=r.gameObject.AddComponent<StickyDragHandle>();drag.target=r;drag.rootCanvas=canvas;drag.finished=Save;
    }
    private void RefreshEmpty(){if(emptyLabel!=null)emptyLabel.gameObject.SetActive(data.notes.Count==0&&data.stickers.Count==0);}
    private void Load(){
        try{
            if(!File.Exists(SavePath))return;
            var loaded=JsonUtility.FromJson<StickyBoardData>(File.ReadAllText(SavePath));
            if(loaded==null)return;
            if(loaded.notes==null)loaded.notes=new List<StickyNoteData>();
            if(loaded.stickers==null)loaded.stickers=new List<BoardStickerData>();
            data=loaded;
            foreach(var n in data.notes)MakeNote(n);
            foreach(var s in data.stickers)MakeSticker(s);
            stickerLayer.SetAsLastSibling();
        }catch(Exception e){Debug.LogWarning("LiveCaption notes could not be loaded: "+e.Message);}
    }
    private void Save(){
        try{File.WriteAllText(SavePath,JsonUtility.ToJson(data,true));}
        catch(Exception e){Debug.LogWarning("LiveCaption notes could not be saved: "+e.Message);}
    }

    private RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max){
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;
        r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;return r;
    }
    private static void Anchor(RectTransform r,Vector2 min,Vector2 max){
        r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
    }
    private Text Txt(string name,Transform parent,string value,int size,Color color,TextAnchor align){
        var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
        var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text=value;t.fontSize=size;t.color=color;t.alignment=align;t.supportRichText=false;
        t.raycastTarget=false;return t;
    }
    private Button Button(Transform parent,string label,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action,bool primary=false){
        var r=Rect(label.Replace(" ","")+"_Button",parent,min,max);
        var image=r.gameObject.AddComponent<Image>();image.color=primary?new Color32(208,232,224,255):new Color32(225,228,232,255);
        var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);
        var colors=b.colors;colors.normalColor=image.color;colors.highlightedColor=new Color32(215,220,225,255);
        colors.pressedColor=new Color32(205,210,216,255);b.colors=colors;
        var outline=r.gameObject.AddComponent<Outline>();outline.effectColor=new Color32(145,150,158,255);
        outline.effectDistance=new Vector2(1.5f,-1.5f);
        var t=Txt("Label",r,label,25,ink,TextAnchor.MiddleCenter);
        Anchor(t.rectTransform,new Vector2(.03f,.04f),new Vector2(.97f,.96f));return b;
    }

    private void ToggleListening(){if(listening)StopListening("PAUSED");else StartListening();}
    private void StartListening(){
#if UNITY_EDITOR
        listening=true;SetStartLabel("PAUSE");SetStatus("EDITOR DEMO · ENGLISH");StartCoroutine(EditorDemo());
#elif UNITY_ANDROID && !UNITY_EDITOR
        if(!Permission.HasUserAuthorizedPermission(Permission.Microphone)){
            permissionCallbacks=new PermissionCallbacks();
            permissionCallbacks.PermissionGranted+=_=>StartNativeListening();
            permissionCallbacks.PermissionDenied+=_=>OnSpeechError("Microphone permission denied");
            Permission.RequestUserPermission(Permission.Microphone,permissionCallbacks);
        }else StartNativeListening();
#else
        SetStatus("ANDROID DEVICE REQUIRED");
#endif
    }
    private void StartNativeListening(){
        try{
            using(var bridge=new AndroidJavaClass("com.wany864.livecaption.LiveCaptionSpeechBridge"))
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                bridge.CallStatic("start",player.GetStatic<AndroidJavaObject>("currentActivity"),"LiveCaptionApp");
            listening=true;SetStartLabel("PAUSE");SetStatus("STARTING · ENGLISH");
        }catch(Exception e){OnSpeechError("START FAILED: "+e.Message);}
    }
    private System.Collections.IEnumerator EditorDemo(){
        string[] samples={"Today we're exploring human computer interaction.","Live captions should be clear and easy to follow.","This is an English only speech recognition demo."};
        int i=0;while(listening){OnCaption(samples[i++%samples.Length]);yield return new WaitForSeconds(3f);}
    }
    private void StopListening(string message){
        listening=false;StopAllCoroutines();StopBridge();SetStartLabel("START LISTENING");SetStatus(message);
    }
    private void StopBridge(){
#if UNITY_ANDROID && !UNITY_EDITOR
        try{using(var bridge=new AndroidJavaClass("com.wany864.livecaption.LiveCaptionSpeechBridge"))bridge.CallStatic("stop");}catch{}
#endif
    }
    private void ClearCaption(){if(captionText!=null)captionText.text="";SetStatus(listening?"LISTENING · ENGLISH":"CLEARED");}
    private void SetCaptionSize(int size){captionSize=size;if(captionText!=null)captionText.fontSize=size;}
    private void SetCaptionOpacity(float value){captionOpacity=Mathf.Clamp01(value);if(captionFill!=null)captionFill.color=new Color(1,1,1,captionOpacity);}
    private void SetStartLabel(string value){var t=startButton!=null?startButton.GetComponentInChildren<Text>():null;if(t!=null)t.text=value;}
    private void SetStatus(string value){if(captionStatus!=null)captionStatus.text=value??"";}
}
