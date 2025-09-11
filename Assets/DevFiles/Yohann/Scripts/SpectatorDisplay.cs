using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
// SPECTATOR DISPLAY MANAGER
// This script manages a multi-display spectator system for VR in Unity. 
// It enables a second display for spectator viewing, sets up a dedicated camera and UI canvas for the spectator view, 
// and provides runtime controls for window mode (windowed, maximized, fullscreen, borderless, etc.) on Windows. 
// The script also handles window styling (title bar, resize, minimize/maximize/close buttons), 
// updates the spectator display with the correct render texture, and shows overlay information about the current spectator camera. 
// It uses Windows API calls to control window properties when running on Windows standalone builds.
public class MultiDisplaySpectatorManager : MonoBehaviour
{
    [Header("Multi-Display Settings")]
    [SerializeField] private bool useSecondDisplay = true;
    [SerializeField] private int spectatorDisplayIndex = 1;
    [SerializeField] private Vector2Int spectatorDisplayResolution = new(1920, 1080);

    [Header("Window Style Settings")]
    [SerializeField] private WindowMode spectatorWindowMode = WindowMode.Windowed;
    [SerializeField] private Vector2Int windowedSize = new(1280, 720);
    [SerializeField] private Vector2Int windowedPosition = new(100, 100);
    [SerializeField] private bool showTitleBar = true;
    [SerializeField] private bool allowWindowResize = true;
    [SerializeField] private bool showMinimizeButton = true;
    [SerializeField] private bool showMaximizeButton = true;
    [SerializeField] private bool showCloseButton = true;
    [SerializeField] private string spectatorWindowTitle = "VR Spectator View";

    [Header("Runtime Window Controls")]
    [SerializeField] private KeyCode toggleFullscreenKey = KeyCode.F11;
    [SerializeField] private KeyCode toggleWindowModeKey = KeyCode.F10;

    [Header("References - REQUIRED")]
    [SerializeField] private VRSpectatorCameraManager spectatorCameraManager;
    [SerializeField] private Transform vrPlayerHead;

    // Internal components
    private UnityEngine.Camera displayCamera;
    private Canvas displayCanvas;
    private RawImage spectatorRawImage;
    private List<IntPtr> windowHandles = new List<IntPtr>();
    // Windows API imports for proper window styling
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowText(IntPtr hWnd, string text);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    // Window constants
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    // Window styles
    private const uint WS_OVERLAPPED = 0x00000000;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_MINIMIZE = 0x20000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_DISABLED = 0x08000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WS_MAXIMIZE = 0x01000000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_DLGFRAME = 0x00400000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;
    private const uint WS_SYSMENU = 0x00080000;
    private const uint WS_THICKFRAME = 0x00040000;
    private const uint WS_GROUP = 0x00020000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_MINIMIZEBOX = 0x00020000;
    private const uint WS_MAXIMIZEBOX = 0x00010000;

    // Extended window styles
    private const uint WS_EX_DLGMODALFRAME = 0x00000001;
    private const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_ACCEPTFILES = 0x00000010;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint WS_EX_MDICHILD = 0x00000040;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;
    private const uint WS_EX_WINDOWEDGE = 0x00000100;
    private const uint WS_EX_CLIENTEDGE = 0x00000200;
    private const uint WS_EX_CONTEXTHELP = 0x00000400;
    private const uint WS_EX_RIGHT = 0x00001000;
    private const uint WS_EX_LEFT = 0x00000000;
    private const uint WS_EX_RTLREADING = 0x00002000;
    private const uint WS_EX_LTRREADING = 0x00000000;
    private const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
    private const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;
    private const uint WS_EX_CONTROLPARENT = 0x00010000;
    private const uint WS_EX_STATICEDGE = 0x00020000;
    private const uint WS_EX_APPWINDOW = 0x00040000;

    // Combined styles
    private const uint WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
    private const uint WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);

    // Window show states
    private const int SW_HIDE = 0;
    private const int SW_SHOWNORMAL = 1;
    private const int SW_NORMAL = 1;
    private const int SW_SHOWMINIMIZED = 2;
    private const int SW_SHOWMAXIMIZED = 3;
    private const int SW_MAXIMIZE = 3;
    private const int SW_SHOWNOACTIVATE = 4;
    private const int SW_SHOW = 5;
    private const int SW_MINIMIZE = 6;
    private const int SW_SHOWMINNOACTIVE = 7;
    private const int SW_SHOWNA = 8;
    private const int SW_RESTORE = 9;

    // SetWindowPos flags
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOREDRAW = 0x0008;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_HIDEWINDOW = 0x0080;

    // Monitor constants
    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }
#endif

    public enum WindowMode
    {
        Windowed,
        Maximized,
        Fullscreen,
        FullscreenWindowed,
        BorderlessWindowed
    }

    private IntPtr windowHandle;
    private WindowMode currentWindowMode;
    private bool isInitialized = false;
    private uint originalStyle = 0;
    private uint originalExStyle = 0;
    private RECT originalRect;

    void Start()
    {
        SetupMultiDisplaySpectator();
    }

    void Update()
    {
        HandleWindowControls();
    }

    void HandleWindowControls()
    {
        if (Input.GetKeyDown(toggleFullscreenKey))
        {
            ToggleFullscreen();
        }

        if (Input.GetKeyDown(toggleWindowModeKey))
        {
            CycleWindowMode();
        }
    }

    void SetupMultiDisplaySpectator()
    {
        if (spectatorCameraManager == null)
        {
            Debug.LogError("VRSpectatorCameraManager is required! Please assign it in the inspector.");
            return;
        }

        if (!useSecondDisplay)
        {
            Debug.Log("Second display disabled in settings");
            return;
        }

        if (Display.displays.Length <= spectatorDisplayIndex)
        {
            Debug.LogWarning($"Display {spectatorDisplayIndex + 1} not available. Total displays: {Display.displays.Length}");
            return;
        }

        var targetDisplay = Display.displays[spectatorDisplayIndex];
        targetDisplay.Activate(
                       spectatorDisplayResolution.x,
                       spectatorDisplayResolution.y,
                       new RefreshRate { numerator = 60, denominator = 1 }
);

        Debug.Log($"Activated Display {spectatorDisplayIndex + 1} at {spectatorDisplayResolution.x}x{spectatorDisplayResolution.y}");

        CreateSpectatorDisplaySystem();

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        StartCoroutine(SetupWindowControls());
#endif
    }

    void CreateSpectatorDisplaySystem()
    {
        // Create display camera
        GameObject displayCamObj = new GameObject("Second Display Camera");
        displayCamera = displayCamObj.AddComponent<UnityEngine.Camera>();

        displayCamera.targetDisplay = spectatorDisplayIndex;
        displayCamera.depth = 10;
        displayCamera.clearFlags = CameraClearFlags.SolidColor;
        displayCamera.backgroundColor = Color.black;
        displayCamera.cullingMask = 0;

        // Create UI Canvas
        GameObject canvasObj = new GameObject("Spectator Display Canvas");
        displayCanvas = canvasObj.AddComponent<Canvas>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

        displayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        displayCanvas.targetDisplay = spectatorDisplayIndex;
        displayCanvas.sortingOrder = 0;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = spectatorDisplayResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateSpectatorDisplayImage();
        CreateControlOverlay();
    }

    void CreateSpectatorDisplayImage()
    {
        GameObject imageObj = new GameObject("Spectator View Display");
        imageObj.transform.SetParent(displayCanvas.transform, false);

        spectatorRawImage = imageObj.AddComponent<RawImage>();
        RectTransform imageRect = spectatorRawImage.GetComponent<RectTransform>();

        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        RenderTexture spectatorRenderTexture = spectatorCameraManager.GetSpectatorRenderTexture();

        if (spectatorRenderTexture != null)
        {
            spectatorRawImage.texture = spectatorRenderTexture;
            Debug.Log($"Assigned spectator render texture: {spectatorRenderTexture.name}");
        }
        else
        {
            Debug.LogError("Could not get spectator render texture!");
            CreateTestTexture();
        }
    }

    void CreateTestTexture()
    {
        Texture2D testTexture = new Texture2D(256, 256);
        Color[] colors = new Color[256 * 256];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.Lerp(Color.red, Color.blue, (float)i / colors.Length);
        }

        testTexture.SetPixels(colors);
        testTexture.Apply();

        spectatorRawImage.texture = testTexture;
        Debug.LogWarning("Using test texture - spectator render texture not available");
    }

    void CreateControlOverlay()
    {
        GameObject infoPanel = new GameObject("Info Panel");
        infoPanel.transform.SetParent(displayCanvas.transform, false);

        Image panelBg = infoPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.7f);

        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(0, 80);

        CreateInfoText(infoPanel, spectatorWindowTitle, new Vector2(0, -20), 32, Color.white);
        CreateInfoText(infoPanel, "F10: Window Mode | F11: Fullscreen | Space: Cycle Cameras",
                      new Vector2(0, -50), 16, Color.gray);

        GameObject cameraInfoObj = CreateInfoText(infoPanel, "Camera: Loading...", new Vector2(0, -70), 14, Color.yellow);
        SpectatorInfoUpdater infoUpdater = cameraInfoObj.AddComponent<SpectatorInfoUpdater>();
        infoUpdater.Initialize(cameraInfoObj.GetComponent<Text>(), spectatorCameraManager);
    }

    GameObject CreateInfoText(GameObject parent, string text, Vector2 position, int fontSize, Color color)
    {
        GameObject textObj = new GameObject($"Text_{text.Substring(0, Mathf.Min(10, text.Length))}");
        textObj.transform.SetParent(parent.transform, false);

        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1);
        textRect.anchorMax = new Vector2(0.5f, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(800, 30);

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);

        return textObj;
    }

    // Add a list of window handles

    System.Collections.IEnumerator SetupWindowControls()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    yield return new WaitForSeconds(1f);

    windowHandles.Clear();

    // Always try to get the main Unity window
    IntPtr mainHandle = GetActiveWindow();
    if (mainHandle == IntPtr.Zero) mainHandle = GetForegroundWindow();
    if (mainHandle == IntPtr.Zero) mainHandle = FindWindow(null, Application.productName);

    if (mainHandle != IntPtr.Zero)
    {
        windowHandles.Add(mainHandle);
    }

    // Also try to find the spectator display window by title
    IntPtr spectatorHandle = FindWindow(null, spectatorWindowTitle);
    if (spectatorHandle != IntPtr.Zero && spectatorHandle != mainHandle)
    {
        windowHandles.Add(spectatorHandle);
    }

    if (windowHandles.Count > 0)
    {
        foreach (var handle in windowHandles)
        {
            // Store original properties just for the first window
            if (handle == windowHandles[0])
            {
                originalStyle = (uint)GetWindowLong(handle, GWL_STYLE);
                originalExStyle = (uint)GetWindowLong(handle, GWL_EXSTYLE);
                GetWindowRect(handle, out originalRect);
            }

            // Set title text for spectator window
            SetWindowText(handle, spectatorWindowTitle);

            // Apply style and mode
            ApplyWindowStyle(handle);
            ApplyWindowMode(spectatorWindowMode, handle);
        }

        currentWindowMode = spectatorWindowMode;
        isInitialized = true;
        Debug.Log($"Window controls initialized. Found {windowHandles.Count} windows.");
    }
    else
    {
        Debug.LogWarning("Could not get any window handles for window controls");
    }
#else
        yield return null;
#endif
    }

#if UNITY_STANDALONE_WIN
    // Updated ApplyWindowStyle to take a handle
    void ApplyWindowStyle(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        uint style = WS_VISIBLE;
        uint exStyle = WS_EX_LEFT | WS_EX_LTRREADING | WS_EX_RIGHTSCROLLBAR;

        if (showTitleBar)
        {
            style |= WS_CAPTION | WS_SYSMENU;

            if (showMinimizeButton) style |= WS_MINIMIZEBOX;
            if (showMaximizeButton) style |= WS_MAXIMIZEBOX;

            style |= allowWindowResize ? WS_THICKFRAME : WS_BORDER;
        }
        else
        {
            style |= WS_POPUP;
        }

        SetWindowLong(handle, GWL_STYLE, (int)style);
        SetWindowLong(handle, GWL_EXSTYLE, (int)exStyle);

        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

        Debug.Log($"Applied window style to handle {handle}");
    }

    // Updated ApplyWindowMode to take a handle
    void ApplyWindowMode(WindowMode mode, IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        switch (mode)
        {
            case WindowMode.Windowed:
                SetWindowedMode(handle);
                break;
            case WindowMode.Maximized:
                Screen.fullScreen = false;
                ShowWindow(handle, SW_MAXIMIZE);
                break;
            case WindowMode.Fullscreen:
                Screen.fullScreen = true;
                break;
            case WindowMode.FullscreenWindowed:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case WindowMode.BorderlessWindowed:
                SetBorderlessWindowedMode(handle);
                break;
        }

        Debug.Log($"Applied window mode {mode} to handle {handle}");
    }

    void SetWindowedMode(IntPtr handle)
    {
        Screen.fullScreen = false;
        ApplyWindowStyle(handle);

        // Position + size logic same as before, just use `handle` instead of windowHandle
        SetWindowPos(handle, IntPtr.Zero, windowedPosition.x, windowedPosition.y,
            windowedSize.x, windowedSize.y, SWP_SHOWWINDOW);

        ShowWindow(handle, SW_RESTORE);
    }

    void SetBorderlessWindowedMode(IntPtr handle)
    {
        Screen.fullScreen = false;
        uint style = WS_VISIBLE | WS_POPUP;
        SetWindowLong(handle, GWL_STYLE, (int)style);

        IntPtr monitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);
        MONITORINFO monitorInfo = new MONITORINFO();
        monitorInfo.cbSize = (uint)Marshal.SizeOf(monitorInfo);

        if (GetMonitorInfo(monitor, ref monitorInfo))
        {
            SetWindowPos(handle, IntPtr.Zero,
                monitorInfo.rcMonitor.Left, monitorInfo.rcMonitor.Top,
                monitorInfo.rcMonitor.Width, monitorInfo.rcMonitor.Height,
                SWP_SHOWWINDOW | SWP_FRAMECHANGED);
        }
    }
#endif


    // Public methods
    public void SetWindowMode(WindowMode mode)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    if (isInitialized)
    {
        // Apply to all window handles
        foreach (var handle in windowHandles)
        {
            ApplyWindowMode(mode, handle);
        }
        currentWindowMode = mode;
    }
    else
    {
        spectatorWindowMode = mode;
    }
#endif
    }

    public void ToggleFullscreen()
    {
        if (currentWindowMode == WindowMode.Fullscreen)
        {
            SetWindowMode(WindowMode.Windowed);
        }
        else
        {
            SetWindowMode(WindowMode.Fullscreen);
        }
    }

    public void CycleWindowMode()
    {
        switch (currentWindowMode)
        {
            case WindowMode.Windowed:
                SetWindowMode(WindowMode.Maximized);
                break;
            case WindowMode.Maximized:
                SetWindowMode(WindowMode.BorderlessWindowed);
                break;
            case WindowMode.BorderlessWindowed:
                SetWindowMode(WindowMode.Fullscreen);
                break;
            case WindowMode.Fullscreen:
                SetWindowMode(WindowMode.Windowed);
                break;
        }
    }

    public void SetWindowTitle(string title)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (windowHandle != IntPtr.Zero)
        {
            SetWindowText(windowHandle, title);
        }
#endif
        spectatorWindowTitle = title;
    }

    public void SetShowTitleBar(bool show)
    {
        showTitleBar = show;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    if (isInitialized)
    {
        // Apply to all window handles
        foreach (var handle in windowHandles)
        {
            ApplyWindowStyle(handle);
        }
    }
#endif
    }

    public void SetWindowButtons(bool minimize, bool maximize, bool close)
    {
        showMinimizeButton = minimize;
        showMaximizeButton = maximize;
        showCloseButton = close;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    if (isInitialized)
    {
        // Apply to all window handles
        foreach (var handle in windowHandles)
        {
            ApplyWindowStyle(handle);
        }
    }
#endif
    }

    public void RefreshSpectatorDisplay()
    {
        if (spectatorRawImage != null && spectatorCameraManager != null)
        {
            RenderTexture newTexture = spectatorCameraManager.GetSpectatorRenderTexture();
            if (newTexture != null)
            {
                spectatorRawImage.texture = newTexture;
                Debug.Log("Refreshed spectator display with new render texture");
            }
        }
    }

    [ContextMenu("Check Display Status")]
    public void CheckDisplayStatus()
    {
        Debug.Log("=== Display Status ===");
        for (int i = 0; i < Display.displays.Length; i++)
        {
            var display = Display.displays[i];
            Debug.Log($"Display {i + 1}: {display.systemWidth}x{display.systemHeight} - Active: {display.active}");
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (windowHandle != IntPtr.Zero)
        {
            Debug.Log($"Window Handle: {windowHandle}");
            Debug.Log($"Current Window Mode: {currentWindowMode}");
            Debug.Log($"Window Title: {spectatorWindowTitle}");
        }
#endif
    }
}

public class SpectatorInfoUpdater : MonoBehaviour
{
    private Text infoText;
    private VRSpectatorCameraManager spectatorManager;

    public void Initialize(Text text, VRSpectatorCameraManager manager)
    {
        infoText = text;
        spectatorManager = manager;
    }

    void Update()
    {
        if (infoText != null && spectatorManager != null)
        {
            string info = $"Current Camera: {spectatorManager.GetCurrentCameraName()} ";
            info += $"({spectatorManager.GetCurrentCameraIndex() + 1}/{spectatorManager.GetTotalCameraCount()})";

            infoText.text = info;
        }
    }
}