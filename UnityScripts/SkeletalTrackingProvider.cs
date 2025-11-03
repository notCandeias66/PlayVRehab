using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
//using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

//public class SkeletalTrackingProvider : BackgroundDataProvider
//{
//    bool readFirstFrame = false;
//    TimeSpan initialTimestamp;

//    //Color data properties
//    public Texture2D ColorTexture { get; private set; }
//    private Color32[] colorPixels;
//    private byte[] rawColorData;
//    private readonly object colorLock = new object();
//    private bool hasNewColorData = false;

//    private bool enableBodyTracking = true;

//    private volatile bool isDisposing = false;
//    //

//    //
//    //public SkeletalTrackingProvider(int id) : base(id)
//    //{
//    //Debug.Log("in the skeleton provider constructor");
//    //}
//    //

//    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter { get; set; } = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

//    public SkeletalTrackingProvider(int id, bool enableBodyTracking) : base(id)
//    {
//        this.enableBodyTracking = enableBodyTracking;
//        Debug.Log("SkeletalTrackingProvider created. Body Tracking: " + enableBodyTracking);
//    }

//    public Stream RawDataLoggingFile = null;

//    ////
//    // Called on main thread, create texture here
//    public void InitializeColorTexture(int width, int height)
//    {
//        lock (colorLock)
//        {
//            if (ColorTexture == null)
//            {
//                ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
//                colorPixels = new Color32[width * height];
//                rawColorData = new byte[width * height * 4];
//                Debug.Log($"Initialized ColorTexture {width}x{height}");
//            }
//        }
//    }
//    ////

//    //
//    // Method to get color texture thread-safely
//    public Texture2D GetColorTexture()
//    {
//        lock (colorLock)
//        {
//            if (hasNewColorData && ColorTexture != null && colorPixels != null)
//            {
//                ColorTexture.SetPixels32(colorPixels);
//                ColorTexture.Apply();
//                hasNewColorData = false;
//            }
//            return ColorTexture;
//        }
//    }
//    //

//    protected override void RunBackgroundThreadAsync(int id, CancellationToken token)
//    {
//        try
//        {
//            Debug.Log("Starting body tracker background thread.");

//            // Buffer allocations.
//            BackgroundData currentFrameData = new BackgroundData();

//            // Open device.
//            using (Device device = Device.Open(id))
//            {
//                device.StartCameras(new DeviceConfiguration()
//                {
//                    CameraFPS = FPS.FPS30,
//                    //ColorResolution = ColorResolution.Off,
//                    ColorResolution = ColorResolution.R1080p,

//                    //
//                    ColorFormat = ImageFormat.ColorBGRA32,
//                    //

//                    DepthMode = DepthMode.NFOV_Unbinned,
//                    WiredSyncMode = WiredSyncMode.Standalone,

//                    //
//                    SynchronizedImagesOnly = true,
//                    //
//                });

//                Debug.Log("Open K4A device successful. id " + id + "sn:" + device.SerialNum);

//                var deviceCalibration = device.GetCalibration();

//                //
//                //Initialize color textureA
//                /*int colorWidth = deviceCalibration.ColorCameraCalibration.ResolutionWidth;
//                int colorHeight = deviceCalibration.ColorCameraCalibration.ResolutionHeight;

//                lock (colorLock)
//                {
//                    ColorTexture = new Texture2D(colorWidth, colorHeight, TextureFormat.RGBA32, false);
//                    colorPixels = new Color32[colorWidth * colorHeight];
//                    rawColorData = new byte[colorWidth * colorHeight * 4];
//                }*/
//                //

//                if (enableBodyTracking)
//                {
//                    // Create body tracker
//                    using (Tracker tracker = Tracker.Create(deviceCalibration, new TrackerConfiguration()
//                    {
//                        ProcessingMode = TrackerProcessingMode.Gpu,
//                        SensorOrientation = SensorOrientation.Default
//                    }))
//                    {
//                        Debug.Log("Body tracker created.");

//                        while (!token.IsCancellationRequested)
//                        {
//                            using (Capture sensorCapture = device.GetCapture())
//                            {
//                                ProcessColorData(sensorCapture);

//                                tracker.EnqueueCapture(sensorCapture);
//                            }

//                            using (Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
//                            {
//                                if (frame == null)
//                                {
//                                    // Tracker timed out, but keep running
//                                    //IsRunning = true;
//                                }
//                                else
//                                {
//                                    IsRunning = true;

//                                    // Process body frame
//                                    currentFrameData.NumOfBodies = frame.NumberOfBodies;

//                                    for (uint i = 0; i < currentFrameData.NumOfBodies; i++)
//                                    {
//                                        currentFrameData.Bodies[i].CopyFromBodyTrackingSdk(frame.GetBody(i), deviceCalibration);
//                                    }

//                                    // Process depth image and timestamp
//                                    Capture bodyFrameCapture = frame.Capture;
//                                    Image depthImage = bodyFrameCapture.Depth;

//                                    if (!readFirstFrame)
//                                    {
//                                        readFirstFrame = true;
//                                        initialTimestamp = depthImage.DeviceTimestamp;
//                                    }
//                                    currentFrameData.TimestampInMs = (float)(depthImage.DeviceTimestamp - initialTimestamp).TotalMilliseconds;
//                                    currentFrameData.DepthImageWidth = depthImage.WidthPixels;
//                                    currentFrameData.DepthImageHeight = depthImage.HeightPixels;

//                                    var depthFrame = MemoryMarshal.Cast<byte, ushort>(depthImage.Memory.Span);

//                                    int byteCounter = 0;
//                                    currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 3;

//                                    for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
//                                    {
//                                        byte b = (byte)(depthFrame[it] / (ConfigLoader.Instance.Configs.SkeletalTracking.MaximumDisplayedDepthInMillimeters) * 255);
//                                        currentFrameData.DepthImage[byteCounter++] = b;
//                                        currentFrameData.DepthImage[byteCounter++] = b;
//                                        currentFrameData.DepthImage[byteCounter++] = b;
//                                    }

//                                    if (RawDataLoggingFile != null && RawDataLoggingFile.CanWrite)
//                                    {
//                                        binaryFormatter.Serialize(RawDataLoggingFile, currentFrameData);
//                                    }

//                                    SetCurrentFrameData(ref currentFrameData);
//                                }
//                            }
//                        }

//                        Debug.Log("Disposing of tracker now.");
//                        tracker.Dispose();
//                    }
//                }
//                else
//                {
//                    // Body tracking disabled — only process color data
//                    while (!token.IsCancellationRequested)
//                    {
//                        using (Capture sensorCapture = device.GetCapture())
//                        {
//                            ProcessColorData(sensorCapture);
//                        }

//                        IsRunning = true;

//                        Thread.Sleep(10); // small delay to reduce CPU usage
//                    }
//                }


//                ////
//                /*using (Tracker tracker = Tracker.Create(deviceCalibration, new TrackerConfiguration() { ProcessingMode = TrackerProcessingMode.Gpu, SensorOrientation = SensorOrientation.Default }))
//                {
//                    Debug.Log("Body tracker created.");
//                    while (!token.IsCancellationRequested)
//                    {
//                        using (Capture sensorCapture = device.GetCapture())
//                        {
//                            //
//                            // Process color data if available
//                            ProcessColorData(sensorCapture);
//                            //

//                            // Queue latest frame from the sensor.
//                            tracker.EnqueueCapture(sensorCapture);
//                        }

//                        // Try getting latest tracker frame.
//                        using (Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
//                        {
//                            if (frame == null)
//                            {
//                                //
//                                //UnityEngine.Debug.Log("Pop result from tracker timeout!");
//                                //

//                                //
//                                // Still running even without body tracking frame
//                                IsRunning = true;
//                                //
//                            }
//                            else
//                            {
//                                IsRunning = true;
//                                // Get number of bodies in the current frame.
//                                currentFrameData.NumOfBodies = frame.NumberOfBodies;

//                                // Copy bodies.
//                                for (uint i = 0; i < currentFrameData.NumOfBodies; i++)
//                                {
//                                    currentFrameData.Bodies[i].CopyFromBodyTrackingSdk(frame.GetBody(i), deviceCalibration);
//                                }

//                                // Store depth image.
//                                Capture bodyFrameCapture = frame.Capture;
//                                Image depthImage = bodyFrameCapture.Depth;
//                                if (!readFirstFrame)
//                                {
//                                    readFirstFrame = true;
//                                    initialTimestamp = depthImage.DeviceTimestamp;
//                                }
//                                currentFrameData.TimestampInMs = (float)(depthImage.DeviceTimestamp - initialTimestamp).TotalMilliseconds;
//                                currentFrameData.DepthImageWidth = depthImage.WidthPixels;
//                                currentFrameData.DepthImageHeight = depthImage.HeightPixels;

//                                // Read image data from the SDK.
//                                var depthFrame = MemoryMarshal.Cast<byte, ushort>(depthImage.Memory.Span);

//                                // Repack data and store image data.
//                                int byteCounter = 0;
//                                currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 3;

//                                for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
//                                {
//                                    byte b = (byte)(depthFrame[it] / (ConfigLoader.Instance.Configs.SkeletalTracking.MaximumDisplayedDepthInMillimeters) * 255);
//                                    currentFrameData.DepthImage[byteCounter++] = b;
//                                    currentFrameData.DepthImage[byteCounter++] = b;
//                                    currentFrameData.DepthImage[byteCounter++] = b;
//                                }

//                                if (RawDataLoggingFile != null && RawDataLoggingFile.CanWrite)
//                                {
//                                    binaryFormatter.Serialize(RawDataLoggingFile, currentFrameData);
//                                }

//                                // Update data variable that is being read in the UI thread.
//                                SetCurrentFrameData(ref currentFrameData);
//                            }

//                        }
//                    }
//                    Debug.Log("dispose of tracker now!!!!!");
//                    tracker.Dispose();
//                }*/
//                /////

//                device.Dispose();
//            }

//            if (RawDataLoggingFile != null)
//            {
//                RawDataLoggingFile.Close();
//            }
//        }
//        catch (Exception e)
//        {
//            //IsRunning = false;

//            Debug.Log($"catching exception for background thread {e.Message}");
//            token.ThrowIfCancellationRequested();
//        }
//    }

//    //
//    private void ProcessColorData(Capture capture)
//    {
//        if (capture.Color == null || ColorTexture == null || colorPixels == null) return;

//        if (capture.Color != null)
//        {
//            var colorImage = capture.Color;
//            var colorData = colorImage.Memory.ToArray();

//            lock (colorLock)
//            {
//                if (rawColorData != null && colorData.Length <= rawColorData.Length)
//                {
//                    Buffer.BlockCopy(colorData, 0, rawColorData, 0, colorData.Length);

//                    // Convert BGRA to RGBA for Unity
//                    for (int i = 0; i < colorPixels.Length; i++)
//                    {
//                        int index = i * 4;
//                        if (index + 3 < rawColorData.Length)
//                        {
//                            colorPixels[i] = new Color32(
//                                rawColorData[index + 2], // R (was B)
//                                rawColorData[index + 1], // G
//                                rawColorData[index],     // B (was R)
//                                rawColorData[index + 3]  // A
//                            );
//                        }
//                    }

//                    hasNewColorData = true;
//                }
//            }
//        }
//    }
//    //
//}


public class SkeletalTrackingProvider : BackgroundDataProvider
{
    bool readFirstFrame = false;
    TimeSpan initialTimestamp;

    // Color data properties
    public Texture2D ColorTexture { get; private set; }
    private Color32[] colorPixels;
    private byte[] rawColorData;
    private readonly object colorLock = new object();
    private bool hasNewColorData = false;

    // Processing mode control
    private volatile bool processBodyTracking = true;
    private readonly object modeLock = new object();

    private volatile bool isDisposing = false;
    private Device kinectDevice = null;
    private Tracker bodyTracker = null;

    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter { get; set; } =
        new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

    public Stream RawDataLoggingFile = null;

    public SkeletalTrackingProvider(int id, bool enableBodyTracking) : base(id)
    {
        this.processBodyTracking = enableBodyTracking;
        Debug.Log("SkeletalTrackingProvider created with body tracking capability");
    }

    // FAST MODE SWITCHING - no device restart needed!
    public void SetProcessingMode(bool enableBodyTracking)
    {
        lock (modeLock)
        {
            bool wasProcessing = processBodyTracking;
            processBodyTracking = enableBodyTracking;

            Debug.Log($"Processing mode changed: Body tracking {wasProcessing} -> {enableBodyTracking}");

            // Clear any old frame data when switching modes
            if (wasProcessing != enableBodyTracking)
            {
                readFirstFrame = false;
            }
        }
    }

    // Called on main thread, create texture here
    public void InitializeColorTexture(int width, int height)
    {
        lock (colorLock)
        {
            if (ColorTexture == null)
            {
                ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                colorPixels = new Color32[width * height];
                rawColorData = new byte[width * height * 4];
                Debug.Log($"Initialized ColorTexture {width}x{height}");
            }
        }
    }

    // Method to get color texture thread-safely
    public Texture2D GetColorTexture()
    {
        lock (colorLock)
        {
            if (hasNewColorData && ColorTexture != null && colorPixels != null)
            {
                ColorTexture.SetPixels32(colorPixels);
                ColorTexture.Apply();
                hasNewColorData = false;
            }
            return ColorTexture;
        }
    }

    protected override void RunBackgroundThreadAsync(int id, CancellationToken token)
    {
        try
        {
            Debug.Log("Starting Kinect background thread with dynamic processing");

            BackgroundData currentFrameData = new BackgroundData();

            // Open device once
            kinectDevice = OpenDevice(id);
            if (kinectDevice == null)
            {
                Debug.LogError("Failed to open Kinect device");
                return;
            }

            var deviceCalibration = kinectDevice.GetCalibration();

            // Create body tracker once (we'll use it when needed)
            bodyTracker = Tracker.Create(deviceCalibration, new TrackerConfiguration()
            {
                ProcessingMode = TrackerProcessingMode.Gpu,
                SensorOrientation = SensorOrientation.Default
            });

            Debug.Log("Body tracker created and ready");

            // Main processing loop with dynamic mode switching
            while (!token.IsCancellationRequested && !isDisposing)
            {
                try
                {
                    bool shouldProcessBodies;

                    using (Capture sensorCapture = kinectDevice.GetCapture())
                    {
                        // Always process color data
                        ProcessColorData(sensorCapture);

                        // Check current processing mode
                        //bool shouldProcessBodies;
                        lock (modeLock)
                        {
                            shouldProcessBodies = processBodyTracking;
                        }

                        if (shouldProcessBodies)
                        {
                            // Enqueue for body tracking
                            bodyTracker.EnqueueCapture(sensorCapture);
                        }
                    }

                    // Process body tracking if enabled
                    //bool shouldProcessBodies;
                    lock (modeLock)
                    {
                        shouldProcessBodies = processBodyTracking;
                    }

                    if (shouldProcessBodies)
                    {
                        // Try to get body tracking result
                        using (Frame frame = bodyTracker.PopResult(TimeSpan.FromMilliseconds(16), throwOnTimeout: false))
                        {
                            if (frame != null)
                            {
                                ProcessBodyFrame(frame, currentFrameData, deviceCalibration);
                            }
                        }
                    }
                    else
                    {
                        // Clear body tracking queue when not needed
                        try
                        {
                            while (bodyTracker.PopResult(TimeSpan.Zero, throwOnTimeout: false) != null)
                            {
                                // Drain the queue
                            }
                        }
                        catch (TimeoutException)
                        {
                            // Expected when queue is empty
                        }
                    }

                    IsRunning = true;
                }
                catch (TimeoutException)
                {
                    // Normal timeout, continue
                    continue;
                }
                catch (Exception ex)
                {
                    //Debug.LogWarning($"Frame processing error: {ex.Message}");
                    IsRunning = false;
                    Thread.Sleep(10);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Background thread exception: {e.Message}");
            IsRunning = false;
        }
        finally
        {
            CleanupResources();
        }
    }

    private Device OpenDevice(int id)
    {
        try
        {
            Debug.Log($"Opening Kinect device {id}");

            var device = Device.Open(id);

            device.StartCameras(new DeviceConfiguration()
            {
                CameraFPS = FPS.FPS30,
                ColorResolution = ColorResolution.R1080p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned,
                WiredSyncMode = WiredSyncMode.Standalone,
                SynchronizedImagesOnly = true,
            });

            Debug.Log($"Kinect device opened successfully - ID: {id}, SN: {device.SerialNum}");
            return device;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open device: {ex.Message}");
            return null;
        }
    }

    private void ProcessBodyFrame(Frame frame, BackgroundData currentFrameData, Calibration deviceCalibration)
    {
        currentFrameData.NumOfBodies = frame.NumberOfBodies;

        for (uint i = 0; i < currentFrameData.NumOfBodies; i++)
        {
            currentFrameData.Bodies[i].CopyFromBodyTrackingSdk(frame.GetBody(i), deviceCalibration);
        }

        // Process depth image and timestamp
        Capture bodyFrameCapture = frame.Capture;
        Image depthImage = bodyFrameCapture.Depth;

        if (!readFirstFrame)
        {
            readFirstFrame = true;
            initialTimestamp = depthImage.DeviceTimestamp;
        }

        currentFrameData.TimestampInMs = (float)(depthImage.DeviceTimestamp - initialTimestamp).TotalMilliseconds;
        currentFrameData.DepthImageWidth = depthImage.WidthPixels;
        currentFrameData.DepthImageHeight = depthImage.HeightPixels;

        var depthFrame = MemoryMarshal.Cast<byte, ushort>(depthImage.Memory.Span);

        int byteCounter = 0;
        currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 3;

        for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
        {
            byte b = (byte)(depthFrame[it] / (ConfigLoader.Instance.Configs.SkeletalTracking.MaximumDisplayedDepthInMillimeters) * 255);
            currentFrameData.DepthImage[byteCounter++] = b;
            currentFrameData.DepthImage[byteCounter++] = b;
            currentFrameData.DepthImage[byteCounter++] = b;
        }

        if (RawDataLoggingFile != null && RawDataLoggingFile.CanWrite)
        {
            binaryFormatter.Serialize(RawDataLoggingFile, currentFrameData);
        }

        SetCurrentFrameData(ref currentFrameData);
    }

    private void ProcessColorData(Capture capture)
    {
        if (capture.Color == null || ColorTexture == null || colorPixels == null || isDisposing)
            return;

        var colorImage = capture.Color;
        var colorData = colorImage.Memory.ToArray();

        lock (colorLock)
        {
            if (rawColorData != null && colorData.Length <= rawColorData.Length)
            {
                Buffer.BlockCopy(colorData, 0, rawColorData, 0, colorData.Length);

                // Convert BGRA to RGBA for Unity
                for (int i = 0; i < colorPixels.Length; i++)
                {
                    int index = i * 4;
                    if (index + 3 < rawColorData.Length)
                    {
                        colorPixels[i] = new Color32(
                            rawColorData[index + 2], // R (was B)
                            rawColorData[index + 1], // G
                            rawColorData[index],     // B (was R)
                            rawColorData[index + 3]  // A
                        );
                    }
                }

                hasNewColorData = true;
            }
        }
    }

    private void CleanupResources()
    {
        Debug.Log("Cleaning up Kinect resources");

        if (bodyTracker != null)
        {
            try
            {
                bodyTracker.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error disposing tracker: {ex.Message}");
            }
            bodyTracker = null;
        }

        if (kinectDevice != null)
        {
            try
            {
                kinectDevice.StopCameras();
                kinectDevice.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error disposing device: {ex.Message}");
            }
            kinectDevice = null;
        }

        if (RawDataLoggingFile != null)
        {
            RawDataLoggingFile.Close();
            RawDataLoggingFile = null;
        }

        IsRunning = false;
        Debug.Log("Resource cleanup complete");
    }

    public override void Dispose()
    {
        isDisposing = true;
        base.Dispose();
        CleanupResources();
    }
}