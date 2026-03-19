using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SentisEmotionRunner : MonoBehaviour
{
    public CameraFixer cameraFixer;
    public ModelAsset modelAsset;
    public TextMeshProUGUI statusText;
    public Image aiEyeImage;

    private Worker worker;
    private Model runtimeModel;
    private WebCamTexture webcamTexture;
    private bool faceDetected = false;

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        yield return new WaitUntil(() => cameraFixer != null && cameraFixer.GetCamTexture() != null);
        webcamTexture = cameraFixer.GetCamTexture();
        yield return new WaitUntil(() => webcamTexture.width > 100);

        faceDetected = false;
        if (statusText != null) statusText.text = "Get Ready & Smile! 😊";
        yield return new WaitForSeconds(2f);

        for (int i = 3; i > 0; i--)
        {
            if (statusText != null) statusText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (statusText != null) statusText.text = "Analyzing...";
        yield return new WaitForEndOfFrame();

        ExecuteAnalysis();

        if (!faceDetected)
        {
            if (statusText != null) statusText.text = "I Can't See You! 🧐";
            if (aiEyeImage != null) aiEyeImage.color = Color.gray;
            yield return new WaitForSeconds(2.5f);
            StartCoroutine(IntroSequence());
        }
        else
        {
            if (webcamTexture != null && webcamTexture.isPlaying) webcamTexture.Stop();
            Invoke("StartGame", 2f);
        }
    }

    void ExecuteAnalysis()
    {
        if (webcamTexture == null) return;

        // 1. HATA VEREN KISMI TAMAMEN SİLDİK. 
        // Unity 6 Sentis için en sade ve hatasız dönüşüm budur:
        // Bu komut görüntüyü otomatik olarak 224x224 boyutuna getirir.
        Tensor<float> inputTensor = TextureConverter.ToTensor(webcamTexture, 224, 224, 3);

        // 2. MODELİ ÇALIŞTIR
        worker.Schedule(inputTensor);

        var outputTensor = worker.PeekOutput() as Tensor<float>;

        // GPU'dan veriyi oku (Readback)
        using var cpuCopy = outputTensor.ReadbackAndClone();

        // Hafızayı boşalt
        inputTensor.Dispose();

        // 3. GÜVEN KONTROLÜ (Yüz var mı?)
        float totalConfidence = 0;
        for (int i = 0; i < 8; i++) totalConfidence += Mathf.Abs(cpuCopy[i]);

        if (totalConfidence < 0.01f)
        {
            faceDetected = false;
            return;
        }

        faceDetected = true;

        // 4. EN YÜKSEK SKORU BUL (Argmax)
        int maxIndex = 0;
        float maxLogit = cpuCopy[0];
        for (int i = 1; i < 8; i++)
        {
            if (cpuCopy[i] > maxLogit)
            {
                maxLogit = cpuCopy[i];
                maxIndex = i;
            }
        }

        // 5. OYUN ZORLUĞUNU GÜNCELLE (Pozitif -> Zor | Negatif -> Kolay)
        if (maxIndex == 1 || maxIndex == 3) // Happy(1) veya Surprise(3)
        {
            GameControl.Difficulty = "Zor";
            statusText.text = "MODE: HARD (Awesome!)";
            if (aiEyeImage != null) aiEyeImage.color = Color.green;
        }
        else if (maxIndex == 0) // Neutral(0)
        {
            GameControl.Difficulty = "Normal";
            statusText.text = "MODE: NORMAL";
            if (aiEyeImage != null) aiEyeImage.color = Color.blue;
        }
        else // Negatif Duygular (Sad, Fear, Anger vb.)
        {
            GameControl.Difficulty = "Kolay";
            statusText.text = "MODE: EASY (Take a Rest)";
            if (aiEyeImage != null) aiEyeImage.color = Color.red;
        }

        // Veriyi Kaydet
        if (ParentDataManager.Instance != null)
        {
            int moodValue = (maxIndex == 1 || maxIndex == 3) ? 100 : (maxIndex == 0 ? 50 : 25);
            ParentDataManager.Instance.SaveDailyMood(moodValue);
        }
    }

    void StartGame() { SceneManager.LoadScene("MainGame"); }

    void OnDestroy()
    {
        worker?.Dispose();
        if (webcamTexture != null && webcamTexture.isPlaying) webcamTexture.Stop();
    }
}