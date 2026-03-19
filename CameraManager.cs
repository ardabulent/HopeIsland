using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using TMPro;

public class CameraManager : MonoBehaviour
{
    [Header(" -- ARDA'NIN MODELİ -- ")]
    public string MODEL_FILE_NAME = "emotion_model.ms"; // Unity'deki gerçek dosya adı

    [Header(" -- EKRANLAR -- ")]
    public RawImage cameraDisplay;
    public RawImage debugFaceView;
    public TextMeshProUGUI infoText;

    private WebCamTexture camTexture;
    private bool isCameraRunning = false;

    // MindSpore Java Nesneleri
    private AndroidJavaObject modelExecutor;
    private bool isModelReady = false;
    private string modelPath = "";
    
    // Yeni AI Bileşenleri
    private EmotionAnalyzer emotionAnalyzer;
    
    // Texture2D Flip için
    private Texture2D flippedCameraTexture;
    private int lastFrameCount = -1;

    // 🔥 ANDROID LOG FONKSİYONU (Release build'de de çalışır) 🔥
    void AndroidLog(string tag, string message)
    {
        // Hem Unity Debug.Log hem de Android log'a yaz
        Debug.Log($"[{tag}] {message}");
        
        // Android'de native log'a da yaz (Release build'de de çalışır)
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                AndroidJavaClass logClass = new AndroidJavaClass("android.util.Log");
                logClass.CallStatic<int>("d", tag, message);
            }
            catch (System.Exception e)
            {
                // Log yazma hatası - sessizce devam et
            }
        }
    }

    void Start()
    {
        // 🔥 DİKEY YÖNLENDİRME (PORTRAIT) 🔥
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(StartSystem());
    }

    IEnumerator StartSystem()
    {
        // 1. KAMERA İZNİ VE AÇILIŞI
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

        WebCamDevice[] devices = WebCamTexture.devices;
        string frontCamName = "";
        foreach (var d in devices) if (d.isFrontFacing) frontCamName = d.name;

        if (string.IsNullOrEmpty(frontCamName))
        {
            Debug.LogError("❌ Ön kamera bulunamadı!");
            yield break;
        }

        // Dikey yönlendirme için kamera çözünürlüğü (Portrait: genişlik x yükseklik)
        // Android'de ön kamera genellikle yatay gelir, bu yüzden 480x640 kullanıyoruz
        camTexture = new WebCamTexture(frontCamName, 480, 640, 30);
        if (cameraDisplay != null)
        {
            cameraDisplay.texture = camTexture;
        }

        camTexture.Play();

        // 🔥 KAMERA HAZIR OLANA KADAR BEKLE (İlk açılışta siyah ekran sorunu için) 🔥
        yield return new WaitUntil(() => camTexture.width > 100 && camTexture.height > 100);

        // 🔥 KAMERA DÜZELTMESİ - Texture2D Flip ile (Çözüm 2: Önerilen) 🔥
        AndroidLog("CameraManager", $"✅ Kamera hazır! Çözünürlük: {camTexture.width}x{camTexture.height}");
        
        // Texture2D flip ile kamera görüntüsünü düzelt (önerilen yöntem)
        if (cameraDisplay != null)
        {
            StartCoroutine(UpdateCameraDisplayWithFlip());
            AndroidLog("CameraManager", "✅ Kamera görüntüsü Texture2D flip ile düzeltiliyor");
        }
        
        if (debugFaceView != null)
        {
            // Debug görüntü için de aynı düzeltme - rotation kaldırıldı (flip Texture2D'de yapılıyor)
            debugFaceView.rectTransform.localEulerAngles = Vector3.zero;
            debugFaceView.rectTransform.localScale = Vector3.one;
        }

        isCameraRunning = true;

        // 2. ARDA'NIN MODELİNİ YÜKLEME (Android Native)
        if (Application.platform == RuntimePlatform.Android)
        {
            yield return StartCoroutine(LoadMindSporeNative());
        }
        else
        {
            // Unity Editor'da mock modu için bilgi ver
            AndroidLog("CameraManager", "🧪 [UNITY EDITOR] Android platformu değil, mock modu kullanılacak");
            AndroidLog("CameraManager", "🧪 [UNITY EDITOR] Gerçek test için Android build alın!");
            // Mock modu için kısa bir bekleme
            yield return new WaitForSeconds(0.5f);
            isModelReady = true; // Mock modu için true yap
        }
        
        // 2.5. Emotion Analyzer'ı başlat
        emotionAnalyzer = new EmotionAnalyzer();

        // 3. ANALİZ DÖNGÜSÜ
        StartCoroutine(AnalyzeRoutine());
    }

    // 🔥 YENİ: Java Wrapper Kullanarak Model Yükleme 🔥
    IEnumerator LoadMindSporeNative()
    {
        AndroidLog("CameraManager", "🧠 ========== MODEL YÜKLEME BAŞLATILIYOR ==========");
        AndroidLog("CameraManager", "Model dosyası: " + MODEL_FILE_NAME);

        // Android'de streamingAssetsPath farklı çalışır, Java tarafından yükleyelim
        try
        {
            // Java wrapper sınıfımızı kullan
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            AndroidLog("CameraManager", "✅ Unity context alındı");

            // MindSporeModelLoader sınıfımızı çağır
            AndroidJavaObject modelLoader = new AndroidJavaObject("com.TheraTech.HopeIsland.ai.MindSporeModelLoader", context);
            AndroidLog("CameraManager", "✅ MindSporeModelLoader oluşturuldu");

            // Modeli yükle
            AndroidLog("CameraManager", "🔵 Model yükleniyor...");
            bool success = modelLoader.Call<bool>("loadModel", MODEL_FILE_NAME);
            AndroidLog("CameraManager", "🔵 Model yükleme sonucu: " + success);

            if (success)
            {
                // Model loader'ı sakla (sonra inference için kullanacağız)
                modelExecutor = modelLoader;

                // Model dosyasının yolunu al
                modelPath = modelLoader.Call<string>("getModelPath");

                // Model hazır mı kontrol et
                bool modelReady = modelLoader.Call<bool>("isModelReady");
                AndroidLog("CameraManager", "🔵 Model hazır mı: " + modelReady);

                isModelReady = modelReady;
                
                if (isModelReady)
                {
                    AndroidLog("CameraManager", "✅ ========== ARDA'NIN MODELİ BAŞARIYLA YÜKLENDİ! ==========");
                    AndroidLog("CameraManager", "Model yolu: " + modelPath);
                }
                else
                {
                    AndroidLog("CameraManager", "❌ Model yüklendi ama hazır değil!");
                }
            }
            else
            {
                AndroidLog("CameraManager", "❌ ========== MODEL YÜKLENEMEDİ! ==========");
                AndroidLog("CameraManager", "Model dosyası Assets/StreamingAssets/ klasöründe olmalı: " + MODEL_FILE_NAME);
                AndroidLog("CameraManager", "Dosya adını kontrol et: " + MODEL_FILE_NAME);
                isModelReady = false;
            }
        }
        catch (System.Exception e)
        {
            AndroidLog("CameraManager", "❌ ========== MODEL YÜKLEME HATASI ==========");
            AndroidLog("CameraManager", "Hata: " + e.Message);
            AndroidLog("CameraManager", "Stack Trace: " + e.StackTrace);
            isModelReady = false;
        }

        yield return null;
    }

    IEnumerator AnalyzeRoutine()
    {
        // 🔥 TEST: Önce kamera testi yap (MindSpore Lite olmadan)
        yield return StartCoroutine(TestCameraWithoutMindSpore());
        
        // 🔥 TEST: Sonra model yükleme testi yap (kamera olmadan)
        yield return StartCoroutine(TestModelLoadingWithoutCamera());
        
        // Normal analiz döngüsü
        while (true)
        {
            yield return new WaitForSeconds(1.0f); // Her saniye analiz et

            // Unity Editor'da kamera olmasa bile mock modu çalışsın
            bool canAnalyze = false;
            if (Application.platform == RuntimePlatform.Android)
            {
                canAnalyze = isModelReady && camTexture != null && camTexture.width > 100;
            }
            else
            {
                // Unity Editor'da mock modu - kamera olmasa bile çalış
                canAnalyze = isModelReady;
            }

            if (canAnalyze)
            {
                // Görüntüyü Arda'nın formatına çevirip Java'ya yolla
                RunInferenceOnMindSpore();
            }
        }
    }
    
    /// <summary>
    /// TEST 1: MindSpore Lite olmadan kamera testi
    /// Eğer bu çalışıyorsa, sorun MindSpore Lite değil
    /// </summary>
    IEnumerator TestCameraWithoutMindSpore()
    {
        AndroidLog("CameraManager", "🧪 TEST 1: Kamera testi (MindSpore Lite olmadan) başlatılıyor...");
        
        // Kamera zaten açıldı, sadece kontrol et
        yield return new WaitForSeconds(2.0f); // Kamera açılması için bekle
        
        if (camTexture != null && camTexture.isPlaying)
        {
            if (camTexture.width > 100 && camTexture.height > 100)
            {
                AndroidLog("CameraManager", "✅ TEST 1 BAŞARILI: Kamera çalışıyor! Çözünürlük: " + camTexture.width + "x" + camTexture.height);
                AndroidLog("CameraManager", "✅ Sonuç: Sorun MindSpore Lite ile ilgili DEĞİL");
                
                if (infoText != null)
                {
                    infoText.text = "✅ Kamera çalışıyor!\nÇözünürlük: " + camTexture.width + "x" + camTexture.height;
                }
            }
            else
            {
                AndroidLog("CameraManager", "❌ TEST 1 BAŞARISIZ: Kamera açıldı ama görüntü gelmiyor!");
                AndroidLog("CameraManager", "⚠️ Sorun: Kamera entegrasyonu veya izinler");
            }
        }
        else
        {
            AndroidLog("CameraManager", "❌ TEST 1 BAŞARISIZ: Kamera açılamadı!");
            AndroidLog("CameraManager", "⚠️ Sorun: Kamera izinleri veya cihaz desteği");
        }
        
        yield return new WaitForSeconds(1.0f);
    }
    
    /// <summary>
    /// TEST 2: MindSpore Lite model yükleme testi (kamera olmadan)
    /// Eğer bu çalışıyorsa, sorun kamera entegrasyonu
    /// </summary>
    IEnumerator TestModelLoadingWithoutCamera()
    {
        AndroidLog("CameraManager", "🧪 TEST 2: Model yükleme testi (kamera olmadan) başlatılıyor...");
        
        if (Application.platform == RuntimePlatform.Android)
        {
            // Model yükleme testi
            yield return StartCoroutine(LoadMindSporeNative());
            
            if (isModelReady)
            {
                AndroidLog("CameraManager", "✅ TEST 2 BAŞARILI: Model yüklendi!");
                AndroidLog("CameraManager", "✅ Sonuç: Sorun kamera entegrasyonu ile ilgili");
                
                if (infoText != null)
                {
                    infoText.text = "✅ Model yüklendi!\n✅ Kamera çalışıyor\n🔄 Analiz başlatılıyor...";
                }
            }
            else
            {
                AndroidLog("CameraManager", "❌ TEST 2 BAŞARISIZ: Model yüklenemedi!");
                AndroidLog("CameraManager", "⚠️ Sorun: Model dosyası veya MindSpore Lite entegrasyonu");
                
                if (infoText != null)
                {
                    infoText.text = "❌ Model yüklenemedi!\nModel dosyasını kontrol edin: " + MODEL_FILE_NAME;
                }
            }
        }
        else
        {
            AndroidLog("CameraManager", "⚠️ TEST 2 ATLANDI: Android platformu değil (Unity Editor)");
        }
        
        yield return new WaitForSeconds(1.0f);
    }

    // 🔥 YENİ: Düzeltilmiş Inference Fonksiyonu 🔥
    void RunInferenceOnMindSpore()
    {
        // Unity Editor'da mock modu için farklı kontrol
        if (Application.platform != RuntimePlatform.Android)
        {
            // Mock modu - rastgele test verisi oluştur
            RunMockInference();
            return;
        }
        
        // Android'de gerçek kamera kontrolü
        if (camTexture == null || !camTexture.isPlaying || camTexture.width < 100)
        {
            Debug.LogWarning("⚠️ Kamera hazır değil!");
            return;
        }

        // 1. GÖRÜNTÜYÜ HAZIRLA (48x48 Grayscale)
        // 🔥 DÜZELTME: GetPixels() kullan (GetPixel() Android'de çalışmayabilir) 🔥
        int inputSize = 48;
        Texture2D scaledTex = new Texture2D(inputSize, inputSize, TextureFormat.RGB24, false);
        float[] inputData = new float[inputSize * inputSize];

        try
        {
            // WebCamTexture'den tüm pixel'leri al
            Color[] pixels = camTexture.GetPixels();
            
            if (pixels == null || pixels.Length == 0)
            {
                AndroidLog("CameraManager", "⚠️ GetPixels() NULL veya boş döndü!");
                return;
            }

            AndroidLog("CameraManager", $"✅ GetPixels() başarılı! Pixel sayısı: {pixels.Length}, Texture: {camTexture.width}x{camTexture.height}");

            // Basitçe orta kısmı alıp küçültüyoruz (Crop & Resize)
            float xStep = (float)camTexture.width / inputSize;
            float yStep = (float)camTexture.height / inputSize;

            for (int y = 0; y < inputSize; y++)
            {
                for (int x = 0; x < inputSize; x++)
                {
                    int px = Mathf.FloorToInt(x * xStep);
                    int py = Mathf.FloorToInt(y * yStep);
                    
                    // Pixel index'ini hesapla (WebCamTexture'de y koordinatı ters olabilir)
                    int pixelIndex = py * camTexture.width + px;
                    
                    // Güvenlik kontrolü
                    if (pixelIndex >= 0 && pixelIndex < pixels.Length)
                    {
                        Color c = pixels[pixelIndex];

                        // Grayscale Formülü
                        float gray = (c.r * 0.3f + c.g * 0.59f + c.b * 0.11f);
                        inputData[y * inputSize + x] = gray;
                        scaledTex.SetPixel(x, y, new Color(gray, gray, gray));
                    }
                    else
                    {
                        // Hata durumunda siyah pixel
                        inputData[y * inputSize + x] = 0.0f;
                        scaledTex.SetPixel(x, y, Color.black);
                    }
                }
            }
            
            scaledTex.Apply();
            
            // 🔥 DEBUG GÖRÜNTÜYÜ HER ZAMAN GÖSTER (Model yüklenmese bile) 🔥
            if (debugFaceView != null) 
            {
                debugFaceView.texture = scaledTex;
                // Debug görüntüyü de döndür ve flip yap (kamera ile aynı)
                debugFaceView.rectTransform.localEulerAngles = new Vector3(0, 0, 90);
                debugFaceView.rectTransform.localScale = new Vector3(-1, 1, 1);
                AndroidLog("CameraManager", $"✅ Debug görüntü güncellendi: {scaledTex.width}x{scaledTex.height} (Model hazır: {isModelReady})");
            }
            
            // Model yüklenmese bile görüntü işlendi, bu iyi bir işaret
            if (!isModelReady)
            {
                AndroidLog("CameraManager", "⚠️ Model hazır değil ama görüntü işlendi - inference atlanıyor");
                if (infoText != null)
                {
                    infoText.text = "Model yükleniyor...\nGörüntü hazır!";
                }
                return; // Inference yapmadan çık
            }
        }
        catch (System.Exception e)
        {
            AndroidLog("CameraManager", $"❌ Görüntü işleme hatası: {e.Message}");
            AndroidLog("CameraManager", $"Stack Trace: {e.StackTrace}");
            return;
        }

        // 2. GERÇEK MINDSPORE LITE INFERENCE YAP
        try
        {
            if (modelExecutor == null)
            {
                Debug.LogWarning("⚠️ Model executor NULL!");
                if (infoText != null)
                {
                    infoText.text = "Model yükleniyor...";
                }
                return;
            }

            if (!isModelReady)
            {
                Debug.LogWarning("⚠️ Model hazır değil! isModelReady: false");
                if (infoText != null)
                {
                    infoText.text = "Model yükleniyor...";
                }
                return;
            }

            // Gerçek MindSpore Lite inference çalıştır
            AndroidLog("CameraManager", $"🧠 ========== INFERENCE ÇALIŞTIRILIYOR ==========");
            AndroidLog("CameraManager", $"Input data length: {inputData.Length}");
            
            float[] results = modelExecutor.Call<float[]>("runInference", inputData, inputSize);

            // Eğer inference başarısız olursa
            if (results == null)
            {
                AndroidLog("CameraManager", "⚠️ Inference sonuç NULL döndü!");
                if (infoText != null)
                {
                    infoText.text = "Analiz ediliyor...";
                }
                return;
            }

            AndroidLog("CameraManager", $"✅ Inference başarılı! Sonuç uzunluğu: {results.Length}");

            if (results != null && results.Length >= 7)
            {
                // Yeni EmotionAnalyzer kullanarak duygu analizi yap
                string detectedEmotion = "Nötr";
                if (emotionAnalyzer != null)
                {
                    detectedEmotion = emotionAnalyzer.ProcessOutput(results);
                    
                    // Oyun adaptasyon ayarlarını al
                    AdaptationSettings settings = emotionAnalyzer.GetAdaptationSettings(detectedEmotion);
                    
                    // GameManager'a duygu seviyesini bildir
                    if (GameManager.Instance != null)
                    {
                        if (detectedEmotion == "Mutlu")
                        {
                            GameManager.Instance.currentMoodLevel = 2; // Enerjik (Zor Mod)
                        }
                        else if (detectedEmotion == "Üzgün")
                        {
                            GameManager.Instance.currentMoodLevel = 1; // Yorgun (Sakin Mod)
                        }
                        else
                        {
                            GameManager.Instance.currentMoodLevel = 0; // Normal
                        }
                    }
                    
                    AndroidLog("CameraManager", $"🧠 ========== DUYGU TESPİT EDİLDİ ==========");
                    AndroidLog("CameraManager", $"Duygu: {detectedEmotion}");
                    AndroidLog("CameraManager", $"Oyun Temposu: {settings.gamePace}");
                    AndroidLog("CameraManager", $"Mesaj: {settings.message}");

                    // UI'da göster
                    if (infoText != null)
                    {
                        infoText.text = $"Duygu: {detectedEmotion}\n{settings.message}";
                    }
                }
                else
                {
                    // Fallback: Eski yöntem
                    int maxIndex = 0;
                    float maxValue = results[0];
                    for (int i = 1; i < results.Length; i++)
                    {
                        if (results[i] > maxValue)
                        {
                            maxValue = results[i];
                            maxIndex = i;
                        }
                    }

                    string[] emotions = { "Mutlu 😊", "Üzgün 😢", "Kızgın 😠", "Korkmuş 😨", "Şaşkın 😲", "İğrenmiş 🤢", "Nötr 😐" };
                    detectedEmotion = emotions[maxIndex];

                    AndroidLog("CameraManager", $"🧠 Duygu: {detectedEmotion}");
                    AndroidLog("CameraManager", $"Güven: {maxValue * 100:F1}%");

                    if (infoText != null)
                    {
                        infoText.text = $"Duygu: {detectedEmotion}\nGüven: {(maxValue * 100):F1}%";
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Model sonuç uzunluğu yetersiz! Beklenen: 7, Gelen: {(results != null ? results.Length : 0)}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ========== AI ÇALIŞTIRMA HATASI ==========");
            Debug.LogError("Hata: " + e.Message);
            Debug.LogError("Stack Trace: " + e.StackTrace);
        }
    }
    
    /// <summary>
    /// Unity Editor'da mock inference (test için)
    /// </summary>
    void RunMockInference()
    {
        AndroidLog("CameraManager", "🧪 [MOCK MODE] Unity Editor'da mock inference çalıştırılıyor...");
        
        // Mock test verisi oluştur (48x48 = 2304 float)
        float[] mockInputData = new float[48 * 48];
        for (int i = 0; i < mockInputData.Length; i++)
        {
            mockInputData[i] = Random.Range(0f, 1f); // Rastgele gri tonlama değerleri
        }
        
        // Mock sonuçlar oluştur
        float[] mockResults = new float[7];
        int randomEmotion = Random.Range(0, 7);
        
        for (int i = 0; i < 7; i++)
        {
            if (i == randomEmotion)
            {
                mockResults[i] = Random.Range(0.6f, 0.95f);
            }
            else
            {
                mockResults[i] = Random.Range(0.0f, 0.3f);
            }
        }
        
        // Normalize et
        float sum = 0f;
        for (int i = 0; i < 7; i++) sum += mockResults[i];
        for (int i = 0; i < 7; i++) mockResults[i] /= sum;
        
        // EmotionAnalyzer ile işle
        if (emotionAnalyzer != null)
        {
            string detectedEmotion = emotionAnalyzer.ProcessOutput(mockResults);
            AdaptationSettings settings = emotionAnalyzer.GetAdaptationSettings(detectedEmotion);
            
            if (GameManager.Instance != null)
            {
                if (detectedEmotion == "Mutlu")
                {
                    GameManager.Instance.currentMoodLevel = 2;
                }
                else if (detectedEmotion == "Üzgün")
                {
                    GameManager.Instance.currentMoodLevel = 1;
                }
                else
                {
                    GameManager.Instance.currentMoodLevel = 0;
                }
            }
            
            AndroidLog("CameraManager", $"🧪 [MOCK MODE] Tespit edilen duygu: {detectedEmotion}");
            AndroidLog("CameraManager", $"🧪 [MOCK MODE] Oyun Temposu: {settings.gamePace}");
            
            if (infoText != null)
            {
                infoText.text = $"🧪 [TEST MODU]\nDuygu: {detectedEmotion}\n{settings.message}";
            }
        }
    }
    
    /// <summary>
    /// Çözüm 2: Texture2D Flip ile kamera görüntüsünü güncelle (Önerilen)
    /// Kontrol edilebilir, performanslı, tüm cihazlarda çalışır
    /// </summary>
    IEnumerator UpdateCameraDisplayWithFlip()
    {
        // Kamera hazır olana kadar bekle
        yield return new WaitUntil(() => camTexture != null && camTexture.isPlaying && camTexture.width > 100);
        
        AndroidLog("CameraManager", "🔄 Texture2D Flip ile kamera görüntüsü güncelleniyor...");
        
        // İlk texture'ı oluştur - Hem yatay hem dikey flip (upside-down sorunu için)
        flippedCameraTexture = FlipTexture2D(camTexture, flipHorizontal: true, flipVertical: true);
        if (flippedCameraTexture != null && cameraDisplay != null)
        {
            cameraDisplay.texture = flippedCameraTexture;
            // Rotation: 180 derece döndür (upside-down düzeltmesi için)
            cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, 180);
            cameraDisplay.rectTransform.localScale = Vector3.one; // Scale'i sıfırla, flip Texture2D'de yapıldı
            AndroidLog("CameraManager", "✅ Kamera görüntüsü düzeltildi: 180° rotation + flip");
        }
        
        // Periyodik güncelleme (performans için her 3 frame'de bir)
        while (camTexture != null && camTexture.isPlaying && cameraDisplay != null)
        {
            // Sadece frame değiştiğinde güncelle (performans optimizasyonu)
            if (Time.frameCount % 3 == 0)
            {
                Texture2D newFlippedTexture = FlipTexture2D(camTexture, flipHorizontal: true, flipVertical: true);
                
                if (newFlippedTexture != null)
                {
                    // Eski texture'ı temizle
                    if (flippedCameraTexture != null && flippedCameraTexture != camTexture)
                    {
                        Destroy(flippedCameraTexture);
                    }
                    
                    // Yeni texture'ı ata
                    flippedCameraTexture = newFlippedTexture;
                    cameraDisplay.texture = flippedCameraTexture;
                }
            }
            
            yield return new WaitForEndOfFrame();
        }
    }
    
    /// <summary>
    /// Texture2D'yi flip eder (yatay ve/veya dikey)
    /// </summary>
    /// <param name="source">Kaynak Texture2D veya WebCamTexture</param>
    /// <param name="flipHorizontal">Yatay flip (mirror)</param>
    /// <param name="flipVertical">Dikey flip</param>
    /// <returns>Flipped Texture2D</returns>
    Texture2D FlipTexture2D(Texture source, bool flipHorizontal = true, bool flipVertical = false)
    {
        if (source == null)
        {
            return null;
        }
        
        try
        {
            int width = source.width;
            int height = source.height;
            
            // WebCamTexture'den pixel'leri al
            Texture2D sourceTex;
            if (source is WebCamTexture)
            {
                sourceTex = new Texture2D(width, height);
                sourceTex.SetPixels((source as WebCamTexture).GetPixels());
                sourceTex.Apply();
            }
            else if (source is Texture2D)
            {
                sourceTex = source as Texture2D;
            }
            else
            {
                return null;
            }
            
            // Yeni Texture2D oluştur
            Texture2D flippedTex = new Texture2D(width, height, sourceTex.format, false);
            
            // Pixel'leri flip ederek kopyala
            Color[] pixels = sourceTex.GetPixels();
            Color[] flippedPixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sourceX = flipHorizontal ? (width - 1 - x) : x;
                    int sourceY = flipVertical ? (height - 1 - y) : y;
                    
                    int sourceIndex = sourceY * width + sourceX;
                    int destIndex = y * width + x;
                    
                    if (sourceIndex >= 0 && sourceIndex < pixels.Length)
                    {
                        flippedPixels[destIndex] = pixels[sourceIndex];
                    }
                }
            }
            
            flippedTex.SetPixels(flippedPixels);
            flippedTex.Apply();
            
            // Geçici texture'ı temizle (eğer WebCamTexture'den oluşturulduysa)
            if (source is WebCamTexture && sourceTex != source)
            {
                Destroy(sourceTex);
            }
            
            return flippedTex;
        }
        catch (System.Exception e)
        {
            AndroidLog("CameraManager", $"❌ Texture2D flip hatası: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Temizlik - Texture'ları temizle
    /// </summary>
    void OnDestroy()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
            Destroy(camTexture);
        }
        
        // Flipped texture'ı temizle
        if (flippedCameraTexture != null)
        {
            Destroy(flippedCameraTexture);
        }
    }

}
