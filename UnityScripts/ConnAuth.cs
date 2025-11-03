using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Drawing;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class ConnAuth : MonoBehaviour
{
    //public static ConnAuth Instance;

    //
    //private Device kinect;
    //private Texture2D kinectColorTexture;
    //private Color32[] pixels;
    //private byte[] rawColorData;
    //

    [SerializeField] private UnityEngine.UI.RawImage authCamera;
    [SerializeField] private UnityEngine.UI.RawImage authStatus;
    [SerializeField] private TextMeshProUGUI authText;

    [SerializeField] private Texture2D yesAuth;
    [SerializeField] private Texture2D noAuth;

    // Kinect Status
    [SerializeField] private UnityEngine.UI.RawImage connImage;
    [SerializeField] private TextMeshProUGUI connText;

    [SerializeField] private Texture2D yesConn;   // Connected texture for RawImage
    [SerializeField] private Texture2D noConn; // Disconnected texture for RawImage

    private bool isAuthenticated = false;

    //
    private bool isKinectConnected = false;
    //

    private float scanCooldown = 1.5f; // Cooldown to avoid excessive scans
    private float lastScanTime = 0f;

    [SerializeField] private GameObject reconnectButton;
    [SerializeField] private GameObject changeUserButton;

    [SerializeField] private Button continueButton;

    //public bool IsConnected;

    //
    //private void Awake()
    //{
        //if (Instance != null)
        //{
            //Destroy(gameObject); // Prevent duplicates
            //return;
        //}

        //Instance = this;
        //DontDestroyOnLoad(gameObject); // <== Prevent destruction on scene load

        //InitKinect(); // Initialize Kinect only once
    //}
    //

    void Start()
    {
        //InitKinect();
        reconnectButton.gameObject.SetActive(false);
        changeUserButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false); // Hide continue button initially

        //
        // Initial UI update
        UpdateKinectStatus();
        //
    }

    //
    // Called by main.cs to update connection status
    public void UpdateConnectionStatus(bool connected)
    {
        if (isKinectConnected != connected)
        {
            isKinectConnected = connected;

            UpdateKinectStatus();
        }
    }
    //
    //Called by main.cs when color frame is available
    public void ProcessColorFrame(Texture2D colorTexture)
    {
        if (colorTexture == null || isAuthenticated) return;

        authCamera.texture = colorTexture;

        // Only scan QR codes if not authenticated and cooldown has passed
        if (Time.time - lastScanTime > scanCooldown)
        {
            lastScanTime = Time.time;
            ScanQRCode(colorTexture);
        }
    }
    //

    //

    //
    /*private void InitKinect()
    {
        try
        {
            kinect = Device.Open();

            kinect.StartCameras(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R1080p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS30
            });

            int width = kinect.GetCalibration().ColorCameraCalibration.ResolutionWidth;
            int height = kinect.GetCalibration().ColorCameraCalibration.ResolutionHeight;

            kinectColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            pixels = new Color32[width * height];
            rawColorData = new byte[width * height * 4]; // BGRA format

            UpdateKinectStatus(true);  // Device is connected and working
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Kinect: " + e.Message);
            kinect = null;
            UpdateKinectStatus(false);  // Device is not connected
        }
    }*/
    //

    //
    /*void Update()
    {
        if (kinect == null)
        {
            Debug.Log("No Kinect connection.");
            UpdateKinectStatus(false);
        }
        else
        {
            try
            {
                using (var capture = kinect.GetCapture())
                {
                    if (capture != null)
                    {
                        // Handle capture and processing here.
                        // For example, grab the color image and process it
                        var colorImg = capture.Color;
                        System.Buffer.BlockCopy(colorImg.Memory.ToArray(), 0, rawColorData, 0, rawColorData.Length);
                        // Further processing and displaying camera feed
                        ProcessColorImage(rawColorData);
                    }
                }
            }
            catch (System.Exception)
            {
                // Catch exceptions when capture fails (device is disconnected or unavailable)
                Debug.LogWarning("Kinect capture failed or Kinect is disconnected.");
                UpdateKinectStatus(false);
            }
        }
    }*/
    //

    //
    /*private void ProcessColorImage(byte[] rawColorData)
    {
        // Convert BGRA to RGBA for Unity
        for (int i = 0; i < pixels.Length; i++)
        {
            int index = i * 4;
            pixels[i] = new Color32(rawColorData[index + 2], rawColorData[index + 1], rawColorData[index], rawColorData[index + 3]);
        }

        kinectColorTexture.SetPixels32(pixels);
        kinectColorTexture.Apply();
        authCamera.texture = kinectColorTexture;

        // Only scan QR codes if not authenticated
        if (Time.time - lastScanTime > scanCooldown)
        {
            lastScanTime = Time.time;
            ScanQRCode(kinectColorTexture);
        }
    }*/
    //

    //
    //private void UpdateKinectStatus(bool isConnected)
    private void UpdateKinectStatus()
    //
    {
        //
        // Camera and authentication independent
        //if (isConnected)
        if (isKinectConnected)
        //
        {
            connImage.texture = yesConn;
            connText.text = "Kinect Connected";
            connText.color = Color.green;
            reconnectButton.gameObject.SetActive(false);
        }
        else
        {
            connImage.texture = noConn;
            connText.text = "Kinect Not Connected";
            connText.color = Color.red;
            reconnectButton.gameObject.SetActive(true);
        }

        if (isAuthenticated)
        {
            authCamera.gameObject.SetActive(false);  // Show the "Authenticated" texture
            authStatus.gameObject.SetActive(true);
            authStatus.texture = yesAuth;
            authText.text = "User Authenticated";
            authText.rectTransform.localPosition = new Vector3(286, -140, 0);
            authText.color = Color.green;
            changeUserButton.gameObject.SetActive(true);
            //continueButton.gameObject.SetActive(true);
        }
        else
        {
            //
            //if (isConnected)
            if (isKinectConnected)
            //
            {
                authStatus.gameObject.SetActive(false);
                authCamera.gameObject.SetActive(true);
                
                //
                //authCamera.texture = kinectColorTexture;
                //
                authText.rectTransform.localPosition = new Vector3(286, -179, 0);
                authText.text = "Scanning...";
                authText.color = Color.yellow;
                changeUserButton.gameObject.SetActive(false);
                //continueButton.gameObject.SetActive(false);
            }
            else
            {
                authCamera.gameObject.SetActive(false);
                authStatus.gameObject.SetActive(true);
                authStatus.texture = noAuth;
                authText.text = "User Not Authenticated";
                authText.rectTransform.localPosition = new Vector3(286, -140, 0);
                authText.color = Color.red;
                changeUserButton.gameObject.SetActive(false);
                //continueButton.gameObject.SetActive(false);
            }
        }

        //
        //if (isConnected && isAuthenticated)
        if (isKinectConnected && isAuthenticated)
        //
        {
            continueButton.gameObject.SetActive(true);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    private void ScanQRCode(Texture2D texture)
    {
        if (texture == null || isAuthenticated) return;

        try
        {
            Texture2D grayscaleTexture = ConvertToGrayscale(texture);
            var pixels = grayscaleTexture.GetPixels32();
            var width = grayscaleTexture.width;
            var height = grayscaleTexture.height;

            var barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = { PossibleFormats = new[] { BarcodeFormat.QR_CODE } }
            };

            var result = barcodeReader.Decode(pixels, width, height);

            if (result != null)
            {
                Debug.Log("QR Code detected: " + result.Text);
                OnQRCodeDetected(result.Text);
            }
            //
            /*else
            {
                Debug.LogWarning("No QR code detected in the frame.");
            }*/
            //
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error scanning QR code: " + ex.Message);
        }
    }

    private void OnQRCodeDetected(string qrText)
    {
        if (isAuthenticated) return;

        // Validate QR code format (id;name;password_hash;qrcode_number)
        string pattern = @"^(\d+);([a-zA-Z0-9_ ]+);(\$2b\$12\$.*);(\d+)$";
        Match match = Regex.Match(qrText, pattern);

        if (!match.Success)
        {
            Debug.LogWarning("Invalid QR code format.");
            return;
        }

        // Extract data from QR code
        int id = int.Parse(match.Groups[1].Value);
        string name = match.Groups[2].Value;
        string passwordHash = match.Groups[3].Value;
        int qrCodeNumber = int.Parse(match.Groups[4].Value);

        Debug.Log($"Extracted ID: {id}, Name: {name}, PasswordHash: {passwordHash}, QRCode: {qrCodeNumber}");

        // Get the singleton instance of SQLiteManager
        SQLiteManager sqliteManager = SQLiteManager.GetInstance();

        // Try logging in the user
        bool loginSuccess = sqliteManager.LoginUser(id, name, passwordHash, qrCodeNumber);

        if (loginSuccess)
        {
            Debug.Log("User authenticated successfully!");
            isAuthenticated = true;
            
            //
            //UpdateKinectStatus(true);
            UpdateKinectStatus();
            //

            // Store in UserSession
            UserSession.UserID = id;
            UserSession.UserName = name;
            UserSession.PasswordHash = passwordHash;
            UserSession.QRCodeNumber = qrCodeNumber;
        }
        else
        {
            Debug.LogWarning("Authentication failed.");
        }
    }

    private Texture2D ConvertToGrayscale(Texture2D texture)
    {
        Texture2D grayscale = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            byte gray = (byte)(0.299f * pixels[i].r + 0.587f * pixels[i].g + 0.114f * pixels[i].b);
            pixels[i] = new Color32(gray, gray, gray, pixels[i].a);
        }

        grayscale.SetPixels32(pixels);
        grayscale.Apply();
        return grayscale;
    }

    //
    /*private void StopKinect()
    {
        if (kinect != null)
        {
            kinect.StopCameras();
            kinect.Dispose();
            kinect = null;
            UpdateKinectStatus(false);
            
            GC.Collect();  // Force garbage collection to remove lingering Kinect references
            GC.WaitForPendingFinalizers();
        }
    }*/
    //

    //
    /*private void OnDestroy()
    {
        StopKinect();
    }*/
    //

    public void ReconnectKinect()
    {
        Debug.Log("Attempting to reconnect Kinect...");

        if (main.Instance != null)
        {
            main.Instance.ReconnectKinect();
        }

        //
        //StopKinect(); // Ensure previous instance is fully cleaned up

        //InitKinect();
        //

        /*Debug.Log("Reconnecting Kinect...");
        if (KinectManager.Instance != null)
        {
            KinectManager.Instance.StopKinect();
            // Re-initialize
            // (You can add a public Init method in KinectManager if needed)
            KinectManager.Instance.InitKinect();
        }*/
    }

    public void ChangeUser()
    {
        // Clear the stored user session
        UserSession.ClearSession();

        // Reset authentication state
        isAuthenticated = false;

        //
        /*if (kinect != null)
        //if (KinectManager.Instance != null && IsConnected)
        {
            // Only reset authentication when Kinect is connected
            UpdateKinectStatus(true);  // Still connected but not authenticated
        }
        else
        {
            UpdateKinectStatus(false); // Disconnected, and not authenticated
        }*/
        //

        //
        // Update UI
        UpdateKinectStatus();
        //
    }

}
