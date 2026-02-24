using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCapture : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    int photoWidth = 1920;
    [SerializeField]
    int photoHeight = 1080;
    [SerializeField]
    string saveFolder = "AircraftPhotos";
    [SerializeField]
    KeyCode captureKey = KeyCode.P;

    [Header("UI")]
    [SerializeField]
    Text photoCountText = null;
    [SerializeField]
    RawImage photoPreview = null;
    [SerializeField]
    Image flashPanel = null;
    [SerializeField]
    float previewDuration = 3f;
    [SerializeField]
    float flashDuration = 0.15f;

    [Header("Audio")]
    [SerializeField]
    AudioClip shutterSound = null;

    int photoCount;
    float previewTimer;
    float flashTimer;
    AudioSource audioSource;
    string savePath;

    private void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveFolder);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (photoPreview != null) photoPreview.gameObject.SetActive(false);
        if (flashPanel != null) flashPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            CapturePhoto();
        }

        // Preview countdown
        if (previewTimer > 0f)
        {
            previewTimer -= Time.deltaTime;
            if (previewTimer <= 0f && photoPreview != null)
            {
                photoPreview.gameObject.SetActive(false);
            }
        }

        // Flash countdown
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashPanel != null)
            {
                Color c = flashPanel.color;
                c.a = Mathf.Clamp01(flashTimer / flashDuration);
                flashPanel.color = c;
                if (flashTimer <= 0f)
                {
                    flashPanel.gameObject.SetActive(false);
                }
            }
        }

        if (photoCountText != null)
        {
            photoCountText.text = "PHOTOS: " + photoCount;
        }
    }

    void CapturePhoto()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Render to texture
        RenderTexture rt = new RenderTexture(photoWidth, photoHeight, 24);
        RenderTexture previousTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();

        // Read pixels
        RenderTexture.active = rt;
        Texture2D photo = new Texture2D(photoWidth, photoHeight, TextureFormat.RGB24, false);
        photo.ReadPixels(new Rect(0, 0, photoWidth, photoHeight), 0, 0);
        photo.Apply();

        // Restore camera
        cam.targetTexture = previousTarget;
        RenderTexture.active = null;
        rt.Release();

        // Save PNG
        byte[] pngData = photo.EncodeToPNG();
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = "Flight_" + timestamp + "_" + photoCount.ToString("D3") + ".png";
        string filePath = Path.Combine(savePath, filename);
        File.WriteAllBytes(filePath, pngData);

        photoCount++;
        Debug.Log("Photo saved: " + filePath);

        // Show preview thumbnail
        if (photoPreview != null)
        {
            photoPreview.texture = photo;
            photoPreview.gameObject.SetActive(true);
            previewTimer = previewDuration;
        }

        // Flash effect
        if (flashPanel != null)
        {
            flashPanel.gameObject.SetActive(true);
            Color c = flashPanel.color;
            c.a = 1f;
            flashPanel.color = c;
            flashTimer = flashDuration;
        }

        // Shutter sound
        if (shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }
    }
}