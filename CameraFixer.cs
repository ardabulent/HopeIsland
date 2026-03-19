using UnityEngine;
using UnityEngine.UI;

public class CameraFixer : MonoBehaviour
{
    public RawImage background; // Ekrana basýlan RawImage
    public AspectRatioFitter fit; // RawImage'e eklediðin AspectRatioFitter
    public bool isFrontFacing = true; // Ön kamera mý?

    [Header("Manual Fixes")]
    public bool flipY = false; // Görüntü tepetaklak ise bunu iþaretle
    public bool flipX = false; // Ayna görüntüsü ters ise bunu iþaretle

    private WebCamTexture backCam;
    private bool camAvailable;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("Kamera bulunamadý!");
            camAvailable = false;
            return;
        }

        // Ön kamerayý bul
        string camName = "";
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == isFrontFacing)
            {
                camName = devices[i].name;
                break;
            }
        }

        // Bulamazsa ilkini al
        if (string.IsNullOrEmpty(camName)) camName = devices[0].name;

        backCam = new WebCamTexture(camName, Screen.width, Screen.height);

        if (background == null) background = GetComponent<RawImage>();

        background.texture = backCam;
        background.material.mainTexture = backCam;
        backCam.Play();
        camAvailable = true;
    }

    void Update()
    {
        if (!camAvailable) return;

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;

        if (flipY) scaleY *= -1f;

        float scaleX = isFrontFacing ? -1f : 1f;
        if (flipX) scaleX *= -1f;

        int orient = -backCam.videoRotationAngle;

        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        background.rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    // --- ÝÞTE EKSÝK OLAN KISIM BURASI ---
    // Diðer scriptin bu kameraya ulaþmasý için bu fonksiyon þart:
    public WebCamTexture GetCamTexture()
    {
        return backCam;
    }
}