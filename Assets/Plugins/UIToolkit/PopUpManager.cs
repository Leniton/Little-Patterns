using AddressableAsyncInstances;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PopUpManager : MonoBehaviour
{
    private static PopUpManager instance;
    private UIDocument document;

    private static Queue<Action> pendingPopup = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Setup()
    {
        //Debug.Log("setup");
        if (instance != null) return;

        CheckListEvent checkList = new CheckListEvent(2);
        PanelSettings settings = null;
        VisualTreeAsset visualTree = null;

        //load assets needed
        new AAAsset<PanelSettings>("UIToolkit/Settings").QueueAction((asset) =>
        {
            settings = asset;
            checkList.MarkProgress();
        });
        new AAAsset<VisualTreeAsset>("UIToolkit/VisualTree").QueueAction((asset) =>
        {
            visualTree = asset;
            checkList.MarkProgress();
        });

        //when completed create singleton
        checkList.onComplete += () =>
        {
            //Debug.Log($"{settings} | {visualTree}");

            //Create instance
            GameObject gameObject = new GameObject("Popup Manager");
            instance = gameObject.AddComponent<PopUpManager>();
            instance.document = gameObject.AddComponent<UIDocument>();
            DontDestroyOnLoad(gameObject);

            instance.document.panelSettings = settings;
            instance.document.visualTreeAsset = visualTree;
            instance.Activate();
        };
    }

    private void Activate()
    {
        int queuedCount = pendingPopup.Count;
        for (int i = 0; i < queuedCount; i++) pendingPopup.Dequeue().Invoke();
    }

    public static void ShowPopUp(string title, Vector2? position = null, Action<Popup> onOpen = null, float fontScale = 1)
    {
        if (instance == null)
        {
            pendingPopup.Enqueue(() => ShowPopUp(title, position, onOpen, fontScale));
            return;
        }

        Vector2 pos = position ?? Input.mousePosition;
        pos.x = Math.Min(pos.x, Screen.currentResolution.width);
        pos.y = Math.Min(pos.y, Screen.currentResolution.height);
        Popup popup = new(title, instance.document.rootVisualElement, pos, fontScale);
        onOpen?.Invoke(popup);
    }

    public static void DescriptionPopUp(string title, string description, Vector2? position = null, float fontScale = 1)
    {
        Label desc = new Label(description);
        desc.style.color = ColorExtensions.GrayShade(.85f);
        desc.style.whiteSpace = WhiteSpace.Normal;
        ShowPopUp(title, position, (popup) => popup.AddContent(desc), fontScale);
    }

    public static void ClosePopup(Popup popup)
    {
        if (instance == null)
        {
            pendingPopup.Enqueue(() => ClosePopup(popup));
            return;
        }
        if(!instance.document.rootVisualElement.Contains(popup)) return;
        instance.document.rootVisualElement.Remove(popup);
    }

    public static void ClosePopUps()
    {
        if (instance == null)
        {
            pendingPopup.Enqueue(ClosePopUps);
            return;
        }
        instance.document.rootVisualElement.Clear();
    }
}

public class Popup : Image, IHover
{
    private VisualElement content;

    private readonly float deadZone = 20;
    private Vector2 screenSize = Vector2.zero;

    private bool set = false;
    private static Queue<Action> pendingContent = new();

    public Action onEnter { get; set; }
    public Action onExit { get; set; }

    public Popup(string title, VisualElement parent, Vector2? position = null, float fontScale = 1)
    {
        name = $"Popup({title})";

        Resolution resolution = Screen.currentResolution;
        screenSize = new Vector2(resolution.width, resolution.height);
        screenSize -= Vector2.one * deadZone;

        Vector2 pos = position ?? Vector2.zero;
        pos.y = Mathf.Abs(pos.y - Screen.height);

        //basic window
        Label Title = new Label(title);
        Title.SetMargin(6);
        ScrollView view = new ScrollView();
        content = view.contentContainer;

        //adding visual elements
        Add(Title);
        Add(view);
        parent.Add(this);

        style.backgroundColor = Color.gray;
        focusable = true;
        transform.position = pos;
        style.position = Position.Absolute;
        style.minWidth = 500;

        Title.style.fontSize = 36 * fontScale;
        Title.style.whiteSpace = WhiteSpace.Normal;

        //content area
        content.style.fontSize = 22 * fontScale;
        view.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        view.style.backgroundColor = ColorExtensions.GrayShade(.4f);
        view.style.maxHeight = 700;
        view.SetMargin(10);
        view.style.borderLeftWidth = 10;
        view.EnableMouseDrag();
        view.IndicateMoreContent(.45f);

        RegisterCallback<GeometryChangedEvent>((evt) =>
        {
            KeepOnScreen();
            if (set) return;
            content.style.width = resolvedStyle.width;
            set = true;
            int queuedCount = pendingContent.Count;
            for (int i = 0; i < queuedCount; i++) pendingContent.Dequeue().Invoke();
        });

        RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        RegisterCallback<PointerLeaveEvent>(OnPointerExit);
    }

    public void OnPointerEnter(PointerEnterEvent eventData) => onEnter?.Invoke();

    public void OnPointerExit(PointerLeaveEvent eventData) => onExit?.Invoke();

    private void KeepOnScreen()
    {
        Vector3 position = transform.position;
        Vector3 size = localBound.size;

        Vector3 corner = position;
        corner.x += size.x;
        corner.y -= size.y;

        Vector3 difference = Vector3.zero;
        difference.x = Mathf.Max(-position.x + deadZone, 0) + Mathf.Min(screenSize.x - corner.x, 0);
        difference.y = Mathf.Min(screenSize.y - (position.y + size.y), 0) - Mathf.Min(position.y - deadZone, 0);

        transform.position += difference;
    }

    public void AddContent(VisualElement element)
    {
        if (!set)
        {
            pendingContent.Enqueue(() => AddContent(element));
            return;
        }
        content.Add(element);
    }

    public void ClearContents() => content.Clear();
}