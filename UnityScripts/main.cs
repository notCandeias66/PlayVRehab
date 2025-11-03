using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

//public class main : MonoBehaviour
//{
//    //
//    public static main Instance;
//    //

//    // Handler for SkeletalTracking thread.
//    public GameObject m_tracker;
//    private SkeletalTrackingProvider m_skeletalTrackingProvider;
//    public BackgroundData m_lastFrameData = new BackgroundData();

//    //
//    private TrackerHandler m_trackerHandler;

//    // Scene detection
//    private ConnAuth connAuth;
//    private bool isInQRScene = false;
//    //

//    //
//    private bool isInitializing = false;
//    //

//    //
//    void Awake()
//    {
//        // Singleton pattern with DontDestroyOnLoad
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        ////
//        //InitializeKinect();
//        ////
//    }
//    //

//    void Start()
//    {
//        //
//        DetectSceneType();

//        //tracker ids needed for when there are two trackers
//        //const int TRACKER_ID = 0;
//        //m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID);
//        //
//    }

//    //
//    void OnEnable()
//    {
//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    void OnDisable()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        //DetectSceneType(); // Re-detect components after scene load

//        StartCoroutine(DelayedSceneDetection());
//    }
//    //

//    // Add delay to ensure all scene objects are properly initialized
//    private IEnumerator DelayedSceneDetection()
//    {
//        yield return new WaitForSeconds(0.5f);
//        DetectSceneType();
//    }

//    //
//    /*private void InitializeKinect()
//    {
//        //tracker ids needed for when there are two trackers
//        const int TRACKER_ID = 0;
//        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID);
//    }*/
//    //

//    ////
//    private void InitializeKinect(bool enableBodyTracking)
//    {
//        /*const int TRACKER_ID = 0;

//        if (m_skeletalTrackingProvider != null)
//        {
//            m_skeletalTrackingProvider.Dispose();
//            m_skeletalTrackingProvider = null;
//        }

//        // Give some time to cleanup
//        System.GC.Collect();
//        System.GC.WaitForPendingFinalizers();
//        Thread.Sleep(500); // small delay to ensure cleanup

//        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID, enableBodyTracking);

//        m_skeletalTrackingProvider.InitializeColorTexture(1920, 1080);*/

//        //
//        if (isInitializing) return;
//        isInitializing = true;

//        StartCoroutine(InitializeKinectAsync(enableBodyTracking));
//        //
//    }
//    ////

//    //
//    private IEnumerator InitializeKinectAsync(bool enableBodyTracking)
//    {
//        const int TRACKER_ID = 0;

//        Debug.Log($"Initializing Kinect - Body Tracking: {enableBodyTracking}");

//        // Properly dispose existing provider
//        if (m_skeletalTrackingProvider != null)
//        {
//            Debug.Log("Disposing existing provider...");
//            m_skeletalTrackingProvider.Dispose();
//            m_skeletalTrackingProvider = null;

//            // Wait for disposal to complete
//            yield return new WaitForSeconds(1.0f);

//            System.GC.Collect();
//            System.GC.WaitForPendingFinalizers();

//            // Additional wait for cleanup
//            yield return new WaitForSeconds(0.5f);
//        }

//        try
//        {
//            Debug.Log("Creating new SkeletalTrackingProvider...");
//            m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID, enableBodyTracking);

//            // Wait a frame before initializing color texture
//            //yield return null;
//            Thread.Sleep(1);

//            m_skeletalTrackingProvider.InitializeColorTexture(1920, 1080);

//            Debug.Log("Kinect initialization complete");
//        }
//        catch (System.Exception ex)
//        {
//            Debug.LogError($"Failed to initialize Kinect: {ex.Message}");
//        }
//        finally
//        {
//            isInitializing = false;
//        }
//    }
//    //

//    //
//    private void DetectSceneType()
//    {
//        if (isInitializing)
//        {
//            Debug.Log("Already initializing, skipping scene detection");
//            return;
//        }

//        connAuth = FindObjectOfType<ConnAuth>();
//        bool wasInQRScene = isInQRScene;
//        isInQRScene = connAuth != null;

//        m_trackerHandler = null;

//        if (m_tracker != null)
//        {
//            m_trackerHandler = m_tracker.GetComponent<TrackerHandler>();
//        }
//        else
//        {
//            TrackerHandler foundHandler = FindObjectOfType<TrackerHandler>();
//            if (foundHandler != null)
//            {
//                m_tracker = foundHandler.gameObject;
//                m_trackerHandler = foundHandler;
//            }
//        }

//        Debug.Log($"Scene detected - QR Scene: {isInQRScene}, Motion Scene: {m_trackerHandler != null}");

//        // Only reinitialize if scene type changed or if provider doesn't exist
//        if (wasInQRScene != isInQRScene || m_skeletalTrackingProvider == null)
//        {
//            Debug.Log("Scene type changed or provider missing - reinitializing Kinect");
//            InitializeKinect(!isInQRScene); // Body tracking enabled for motion scenes
//        }
//        else
//        {
//            Debug.Log("Scene type unchanged - keeping existing provider");
//            if (m_skeletalTrackingProvider != null)
//            {
//                m_skeletalTrackingProvider.InitializeColorTexture(1920, 1080);
//            }
//        }

//        /*// Find ConnAuth in current scene
//        connAuth = FindObjectOfType<ConnAuth>();

//        bool wasInQRScene = isInQRScene;

//        isInQRScene = connAuth != null;

//        ////
//        m_trackerHandler = null;
//        ////

//        // Find TrackerHandler in current scene
//        if (m_tracker != null)
//        {
//            m_trackerHandler = m_tracker.GetComponent<TrackerHandler>();
//        }
//        else
//        {
//            // Try to find it if not assigned
//            TrackerHandler foundHandler = FindObjectOfType<TrackerHandler>();
//            if (foundHandler != null)
//            {
//                m_tracker = foundHandler.gameObject;
//                m_trackerHandler = foundHandler;
//            }
//        }

//        Debug.Log($"Scene detected - QR Scene: {isInQRScene}, Motion Scene: {m_trackerHandler != null}");

//        ////
//        //InitializeKinect(!isInQRScene);

//        //if (m_skeletalTrackingProvider != null)
//        //{
//        //m_skeletalTrackingProvider.InitializeColorTexture(1920, 1080); // <-- ensure it's created in main thread
//        //}
//        ////

//        // Only reinitialize if scene type changed or if provider doesn't exist
//        if (wasInQRScene != isInQRScene || m_skeletalTrackingProvider == null)
//        {
//            Debug.Log("Scene type changed or provider missing - reinitializing Kinect");
//            InitializeKinect(!isInQRScene); // Body tracking enabled for motion scenes
//        }
//        else
//        {
//            Debug.Log("Scene type unchanged - keeping existing provider");
//            // Still ensure color texture is initialized
//            if (m_skeletalTrackingProvider != null)
//            {
//                m_skeletalTrackingProvider.InitializeColorTexture(1920, 1080);
//            }
//        }*/
//    }
//    //

//    void Update()
//    {
//        /*if (m_skeletalTrackingProvider == null)
//        {
//            Debug.LogWarning("SkeletalTrackingProvider is null");
//            if (isInQRScene && connAuth != null)
//            {
//                connAuth.UpdateConnectionStatus(false);
//            }
//            return;
//        }

//        if (!m_skeletalTrackingProvider.IsRunning)
//        {
//            Debug.LogWarning("SkeletalTrackingProvider is not running");
//            if (isInQRScene && connAuth != null)
//            {
//                connAuth.UpdateConnectionStatus(false);
//            }
//            return;
//        }

//        // Always try to get frame data (contains both motion and color info)
//        if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
//        {
//            // Handle motion tracking for motion scenes
//            if (!isInQRScene && m_trackerHandler != null)
//            {
//                if (m_lastFrameData.NumOfBodies > 0)
//                {
//                    Debug.Log($"Motion scene: Updating tracker with {m_lastFrameData.NumOfBodies} bodies");
//                    m_trackerHandler.updateTracker(m_lastFrameData);
//                }
//            }
//            else if (!isInQRScene)
//            {
//                Debug.LogWarning("Motion scene but TrackerHandler is null");
//            }
//        }

//        // Handle color data for QR scenes
//        if (isInQRScene && connAuth != null)
//        {
//            Texture2D colorTexture = m_skeletalTrackingProvider.GetColorTexture();
//            connAuth.UpdateConnectionStatus(true);

//            if (colorTexture != null)
//            {
//                connAuth.ProcessColorFrame(colorTexture);
//            }
//        }*/

//        if (isInitializing) return;

//        if (m_skeletalTrackingProvider == null)
//        {
//            Debug.LogWarning("SkeletalTrackingProvider is null");
//            if (isInQRScene && connAuth != null)
//            {
//                connAuth.UpdateConnectionStatus(false);
//            }
//            return;
//        }

//        if (!m_skeletalTrackingProvider.IsRunning)
//        {
//            Debug.LogWarning("SkeletalTrackingProvider is not running");
//            if (isInQRScene && connAuth != null)
//            {
//                connAuth.UpdateConnectionStatus(false);
//            }
//            return;
//        }

//        // Always try to get frame data
//        if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
//        {
//            // Handle motion tracking for motion scenes
//            if (!isInQRScene && m_trackerHandler != null)
//            {
//                if (m_lastFrameData.NumOfBodies > 0)
//                {
//                    m_trackerHandler.updateTracker(m_lastFrameData);
//                }
//            }
//        }

//        // Handle color data for QR scenes
//        if (isInQRScene && connAuth != null)
//        {
//            Texture2D colorTexture = m_skeletalTrackingProvider.GetColorTexture();
//            connAuth.UpdateConnectionStatus(true);

//            if (colorTexture != null)
//            {
//                connAuth.ProcessColorFrame(colorTexture);
//            }
//        }
//    }

//    //
//    // Called when scene changes
//    /*void OnLevelWasLoaded(int level)
//    {
//        DetectSceneType();
//    }*/
//    //

//    //
//    public bool IsKinectConnected()
//    {
//        return m_skeletalTrackingProvider != null && m_skeletalTrackingProvider.IsRunning;
//    }
//    //

//    //
//    public void ReconnectKinect()
//    {
//        /*Debug.Log("Attempting to reconnect Kinect...");

//        if (m_skeletalTrackingProvider != null)
//        {
//            m_skeletalTrackingProvider.Dispose();
//            m_skeletalTrackingProvider = null;
//        }

//        System.GC.Collect();
//        System.GC.WaitForPendingFinalizers();*/

//        //InitializeKinect();

//        ////
//        InitializeKinect(!isInQRScene);
//        ////
//    }
//    //

//    void OnApplicationQuit()
//    {
//        if (m_skeletalTrackingProvider != null)
//        {
//            Debug.Log("Application quitting - disposing SkeletalTrackingProvider");
//            m_skeletalTrackingProvider.Dispose();
//            m_skeletalTrackingProvider = null;
//        }
//    }
//}

public class main : MonoBehaviour
{
    public static main Instance;

    public GameObject m_tracker;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();

    private TrackerHandler m_trackerHandler;
    private ConnAuth connAuth;
    private bool isInQRScene = false;
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Kinect once at startup with both capabilities
        InitializeKinectOnce();
    }

    void Start()
    {
        StartCoroutine(WaitForInitializationAndDetectScene());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fast scene switching - just change processing mode
        DetectSceneType();
    }

    private void InitializeKinectOnce()
    {
        const int TRACKER_ID = 0;

        Debug.Log("Initializing Kinect with full capabilities...");

        // Initialize with both body tracking and color capabilities
        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID, true); // Always enable body tracking
        m_skeletalTrackingProvider.InitializeColorTexture(1920, 1080);

        isInitialized = true;
    }

    private IEnumerator WaitForInitializationAndDetectScene()
    {
        // Wait for Kinect to be ready
        while (!isInitialized || !IsKinectConnected())
        {
            yield return new WaitForSeconds(0.1f);
        }

        DetectSceneType();
    }

    private void DetectSceneType()
    {
        if (!isInitialized)
        {
            Debug.Log("Kinect not initialized yet, skipping scene detection");
            return;
        }

        connAuth = FindObjectOfType<ConnAuth>();
        bool wasInQRScene = isInQRScene;
        isInQRScene = connAuth != null;

        m_trackerHandler = null;

        if (m_tracker != null)
        {
            m_trackerHandler = m_tracker.GetComponent<TrackerHandler>();
        }
        else
        {
            TrackerHandler foundHandler = FindObjectOfType<TrackerHandler>();
            if (foundHandler != null)
            {
                m_tracker = foundHandler.gameObject;
                m_trackerHandler = foundHandler;
            }
        }

        Debug.Log($"Scene detected - QR Scene: {isInQRScene}, Motion Scene: {m_trackerHandler != null}");

        // FAST SWITCHING: Just change the processing mode
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.SetProcessingMode(!isInQRScene); // Body tracking for motion scenes
            Debug.Log($"Switched processing mode - Body tracking: {!isInQRScene}");
        }

        // If switching from QR to motion scene, clear any accumulated body data
        if (wasInQRScene && !isInQRScene)
        {
            m_lastFrameData = new BackgroundData();
        }
    }

    void Update()
    {
        if (!isInitialized || m_skeletalTrackingProvider == null)
        {
            if (isInQRScene && connAuth != null)
            {
                connAuth.UpdateConnectionStatus(false);
            }
            return;
        }

        if (!m_skeletalTrackingProvider.IsRunning)
        {
            //Debug.LogWarning("SkeletalTrackingProvider is not running");
            if (isInQRScene && connAuth != null)
            {
                connAuth.UpdateConnectionStatus(false);
            }
            return;
        }

        // Always try to get frame data
        if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
        {
            // Handle motion tracking for motion scenes
            if (!isInQRScene && m_trackerHandler != null)
            {
                if (m_lastFrameData.NumOfBodies > 0)
                {
                    m_trackerHandler.updateTracker(m_lastFrameData);
                }
            }
        }

        // Handle color data for QR scenes
        if (isInQRScene && connAuth != null)
        {
            Texture2D colorTexture = m_skeletalTrackingProvider.GetColorTexture();
            connAuth.UpdateConnectionStatus(true);

            if (colorTexture != null)
            {
                connAuth.ProcessColorFrame(colorTexture);
            }
        }
    }

    public bool IsKinectConnected()
    {
        return m_skeletalTrackingProvider != null && m_skeletalTrackingProvider.IsRunning;
    }

    public void ReconnectKinect()
    {
        Debug.Log("Attempting to reconnect Kinect...");

        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
            m_skeletalTrackingProvider = null;
        }

        isInitialized = false;
        InitializeKinectOnce();
    }

    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            Debug.Log("Application quitting - disposing SkeletalTrackingProvider");
            m_skeletalTrackingProvider.Dispose();
            m_skeletalTrackingProvider = null;
        }
    }
}
