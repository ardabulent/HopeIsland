import mindspore as ms
import mindspore.dataset as ds
import mindspore.dataset.vision as vision
import mindspore.nn as nn
from mindspore.train import Model, LossMonitor, TimeMonitor
import numpy as np
import os
from mindcv.models import create_model

# 1. Cihaz Ayarı (CPU üzerinde en stabil çalışma)
ms.set_context(device_target="CPU")

# 2. Veri Yükleyici
def get_dataset(path):
    # Klasörün varlığını kontrol et
    if not os.path.exists(path):
        raise FileNotFoundError(f"Hata: {path} dizini bulunamadı!")
        
    dataset = ds.ImageFolderDataset(path, decode=True)
    
    # Görsel Ön İşleme (MobileNetV2 standartları)
    transform_ops = [
        vision.Resize(224),
        vision.Rescale(1.0 / 255.0, 0.0),
        vision.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
        vision.HWC2CHW()
    ]
    
    dataset = dataset.map(operations=transform_ops, input_columns=["image"])
    return dataset.batch(32)

# --- DÜZENLEMEN GEREKEN YER ---
# Veri setinin bilgisayarındaki tam yolunu buraya yaz (Örn: C:/Projeler/Dataset/train)
dataset_path = r"C:\UmutAdasi_AI\AffectNetCustom\train" 

try:
    train_ds = get_dataset(dataset_path)
    print("Veri seti başarıyla yüklendi.")

    # 3. Model Oluşturma (Transfer Learning)
    # 8 duygu sınıfı için önceden eğitilmiş MobileNetV2
    net = create_model(model_name='mobilenet_v2_100', pretrained=True, num_classes=8)

    # 4. Eğitim Ayarları
    opt = nn.Adam(net.trainable_params(), learning_rate=0.001)
    loss = nn.CrossEntropyLoss()
    model = Model(net, loss_fn=loss, optimizer=opt, metrics={'acc'})

    print("Eğitim başlıyor... Bu işlem bilgisayar hızına göre zaman alabilir.")
    # Sunum için hızlıca 1 tur (epoch) dönüyoruz
    model.train(15, train_ds, callbacks=[LossMonitor(10), TimeMonitor()])

    # 5. ONNX Çıktısı (Uygulama entegrasyonu için)
    dummy_input = ms.Tensor(np.ones([1, 3, 224, 224]).astype(np.float32))
    onnx_filename = "umut_adasi_final.onnx"
    ms.export(net, dummy_input, file_name=onnx_filename, file_format="ONNX")
    
    print("-" * 30)
    print(f"BAŞARILI! Model dosyası oluşturuldu: {os.path.abspath(onnx_filename)}")
    print("-" * 30)

except Exception as e:
    print(f"Bir hata oluştu: {e}")