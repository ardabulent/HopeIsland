using UnityEngine;
using UnityEngine.SceneManagement;
using HmsPlugin;
using HuaweiMobileServices.Id;
using HuaweiMobileServices.Utils;

public class LoginManager : MonoBehaviour
{
    public string nextSceneName = "IntroScene"; // Intro veya CameraSystem sahnesi
    
    private bool isHuaweiAvailable = false;
    private AndroidJavaClass hmsHelperClass;

    void Start()
    {
        AndroidLog("LoginManager", "========== LoginManager Start() Başladı ==========");
        
        // Huawei servislerini kontrol et (bilgi amaçlı)
        CheckHuaweiAvailability();
        
        // Huawei Kit'in cevaplarını her zaman dinle (HMS Core kuruluysa çalışır)
        try
        {
            AndroidLog("LoginManager", "HMSAccountKitManager.Instance kontrol ediliyor...");
            
            if (HMSAccountKitManager.Instance != null)
            {
                AndroidLog("LoginManager", "SUCCESS: HMSAccountKitManager.Instance mevcut!");
                HMSAccountKitManager.Instance.OnSignInSuccess = OnLoginSuccess;
                HMSAccountKitManager.Instance.OnSignInFailed = OnLoginFailed;
                AndroidLog("LoginManager", "SUCCESS: Huawei giriş dinleyicileri ayarlandı");
                AndroidLog("LoginManager", "SUCCESS: OnSignInSuccess: " + (HMSAccountKitManager.Instance.OnSignInSuccess != null ? "SET" : "NULL"));
                AndroidLog("LoginManager", "SUCCESS: OnSignInFailed: " + (HMSAccountKitManager.Instance.OnSignInFailed != null ? "SET" : "NULL"));
            }
            else
            {
                AndroidLog("LoginManager", "WARNING: HMSAccountKitManager.Instance NULL - Misafir girişi kullanılabilir");
            }
        }
        catch (System.Exception e)
        {
            AndroidLog("LoginManager", "ERROR: HMSAccountKitManager başlatılamadı: " + e.Message);
            AndroidLog("LoginManager", "ERROR: Stack Trace: " + e.StackTrace);
            AndroidLog("LoginManager", "INFO: Misafir girişi kullanılabilir");
        }
        
        AndroidLog("LoginManager", "========== LoginManager Start() Bitti ==========");
    }

    // 🔥 YENİ: Huawei Servisleri Kontrolü ve Başlatma 🔥
    void CheckHuaweiAvailability()
    {
        // Sadece Android platformunda kontrol et
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("ℹ️ Android platformu değil - Huawei servisleri kullanılamaz");
            isHuaweiAvailable = false;
            return;
        }
        
        try
        {
            Debug.Log("🔵 Huawei servisleri kontrol ediliyor...");
            
            // Java helper sınıfını kullan
            if (hmsHelperClass == null)
            {
                hmsHelperClass = new AndroidJavaClass("com.TheraTech.HopeIsland.utils.HMSCoreHelper");
            }
            
            // Sadece HMS Core kontrolü (emülatör kontrolü yok)
            bool isAvailable = hmsHelperClass.CallStatic<bool>("isHuaweiServicesAvailable");
            
            if (isAvailable)
            {
                Debug.Log("✅ Huawei servisleri kullanılabilir!");
                isHuaweiAvailable = true;
                
                // HMSAccountKitManager kontrolü
                try
                {
                    if (HMSAccountKitManager.Instance != null)
                    {
                        Debug.Log("✅ HMSAccountKitManager hazır!");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ HMSAccountKitManager bulunamadı!");
                        isHuaweiAvailable = false;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("⚠️ HMSAccountKitManager kontrolü başarısız: " + e.Message);
                    isHuaweiAvailable = false;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ HMS Core hazır değil - Huawei servisleri kullanılamıyor");
                isHuaweiAvailable = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("⚠️ Huawei servisleri kontrolü başarısız: " + e.Message);
            Debug.LogWarning("ℹ️ Uygulama misafir girişi ile devam edecek");
            isHuaweiAvailable = false;
        }
    }

    // Android Log'a yazma fonksiyonu (Release build'de de çalışır)
    void AndroidLog(string tag, string message)
    {
        try
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass logClass = new AndroidJavaClass("android.util.Log");
                logClass.CallStatic<int>("d", tag, message);
            }
        }
        catch { }
        // Unity Debug.Log da ekle
        Debug.Log($"[{tag}] {message}");
    }

    // --- 1. HUAWEI BUTONU İÇİN ---
    public void HuaweiLoginBtn_Click()
    {
        // İLK TEST: Buton çalışıyor mu?
        AndroidLog("LoginManager", "========== HUAWEI BUTONUNA BASILDI ==========");
        
        // Toast mesajı göster (Android'de)
        try
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", context, "Huawei butonuna basildi!", 1); // 1 = Toast.LENGTH_SHORT
                toast.Call("show");
                AndroidLog("LoginManager", "SUCCESS: Toast mesaji gosterildi");
            }
        }
        catch (System.Exception e)
        {
            AndroidLog("LoginManager", "WARNING: Toast gosterilemedi: " + e.Message);
        }
        
        // Her zaman Huawei girişini dene (HMS Core kuruluysa çalışır)
        try
        {
            // HMSAccountKitManager kontrolü
            if (HMSAccountKitManager.Instance == null)
            {
                AndroidLog("LoginManager", "ERROR: HMSAccountKitManager.Instance NULL!");
                AndroidLog("LoginManager", "WARNING: Misafir girişine yönlendiriliyor...");
                GuestLoginBtn_Click();
                return;
            }
            
            AndroidLog("LoginManager", "SUCCESS: HMSAccountKitManager.Instance mevcut");
            
            // Event'lerin set edilip edilmediğini kontrol et
            if (HMSAccountKitManager.Instance.OnSignInSuccess == null)
            {
                AndroidLog("LoginManager", "WARNING: OnSignInSuccess event'i set edilmemiş! Tekrar set ediliyor...");
                HMSAccountKitManager.Instance.OnSignInSuccess = OnLoginSuccess;
            }
            
            if (HMSAccountKitManager.Instance.OnSignInFailed == null)
            {
                AndroidLog("LoginManager", "WARNING: OnSignInFailed event'i set edilmemiş! Tekrar set ediliyor...");
                HMSAccountKitManager.Instance.OnSignInFailed = OnLoginFailed;
            }
            
            AndroidLog("LoginManager", "SUCCESS: Event'ler kontrol edildi ve hazır");
            
            // Huawei girişini başlat
            AndroidLog("LoginManager", "SignIn() çağrılıyor...");
            HMSAccountKitManager.Instance.SignIn();
            AndroidLog("LoginManager", "SignIn() çağrıldı, callback bekleniyor...");
        }
        catch (System.Exception e)
        {
            AndroidLog("LoginManager", "ERROR: Huawei giriş başlatılamadı: " + e.Message);
            AndroidLog("LoginManager", "ERROR: Stack Trace: " + e.StackTrace);
            AndroidLog("LoginManager", "WARNING: Misafir girişine yönlendiriliyor...");
            GuestLoginBtn_Click();
        }
    }

    // --- 2. MİSAFİR BUTONU İÇİN (YENİ) ---
    public void GuestLoginBtn_Click()
    {
        Debug.Log("🟢 Misafir Girişi Yapılıyor...");

        // Oyuna "Bu kişi Misafir" diye not düşelim (İlerde lazım olur)
        PlayerPrefs.SetInt("IsGuest", 1);
        PlayerPrefs.Save();

        // Direkt diğer sahneye uçur (Sorgu sual yok)
        SceneManager.LoadScene(nextSceneName);
    }

    // Huawei Başarılı Olursa
    public void OnLoginSuccess(AuthAccount account)
    {
        AndroidLog("LoginManager", "========== HUAWEI GİRİŞİ BAŞARILI! ==========");
        if (account != null)
        {
            AndroidLog("LoginManager", "SUCCESS: Kullanıcı Adı: " + account.DisplayName);
            AndroidLog("LoginManager", "SUCCESS: Email: " + account.Email);
            AndroidLog("LoginManager", "SUCCESS: UID: " + account.Uid);
        }
        else
        {
            AndroidLog("LoginManager", "WARNING: Account objesi null!");
        }
        
        PlayerPrefs.SetInt("IsGuest", 0); // Bu gerçek kullanıcı
        PlayerPrefs.Save();
        AndroidLog("LoginManager", "SUCCESS: Sahne değiştiriliyor: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnLoginFailed(HMSException error)
    {
        AndroidLog("LoginManager", "========== HUAWEI GİRİŞ HATASI ==========");
        
        // Hata bilgilerini logla
        if (error != null)
        {
            AndroidLog("LoginManager", "ERROR: Hata Mesajı: " + error.Message);
            AndroidLog("LoginManager", "ERROR: Hata Kodu: " + error.ErrorCode);
            AndroidLog("LoginManager", "ERROR: WrappedCauseMessage: " + error.WrappedCauseMessage);
            AndroidLog("LoginManager", "ERROR: WrappedExceptionMessage: " + error.WrappedExceptionMessage);
            
            // HMS Core yoksa özel mesaj
            if (error.ErrorCode == 1 || // SERVICE_MISSING
                error.ErrorCode == 9)   // SERVICE_INVALID
            {
                AndroidLog("LoginManager", "ERROR: HMS Core kurulu değil veya güncel değil!");
            }
        }
        else
        {
            AndroidLog("LoginManager", "ERROR: Hata objesi NULL - Bilinmeyen hata");
        }
        
        // Kullanıcıyı üzmemek için otomatik misafir girişine yönlendir
        AndroidLog("LoginManager", "WARNING: Huawei girişi başarısız - Misafir girişine yönlendiriliyor...");
        GuestLoginBtn_Click();
    }
}

