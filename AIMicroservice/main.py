from fastapi import FastAPI
from pydantic import BaseModel
import random
import math
import time

# Uygulamanın ana omurgasını oluşturan FastAPI nesnesinin başlatılması.
app = FastAPI(title="AI & Synthetic Data Microservice")

# C# backend servisinden gelecek HTTP POST isteklerinin gövde yapısını belirleyen model.
# Veri tiplerinin doğruluğu FastAPI tarafından otomatik olarak kontrol edilir.
class DataRequest(BaseModel):
    wallet_count: int       # Üretilecek eşsiz cüzdan sayısı
    transaction_count: int # Bu cüzdanlar arasında döndürülecek toplam transfer simülasyonu sayısı

@app.get("/")
def read_root():
    # Mikroservisin ayakta ve erişilebilir olduğunu doğrulamak için kullanılan kök uç nokta.
    return {"message": "AI Mikroservisi Aktif"}

@app.post("/api/ai/generate-synthetic-data")
def generate_data(request: DataRequest):
    """
    gerçekçi sentetik veriyi üretecek ana fonksiyon, dışardan alınan parametreler ile çalışır
    
    """
    
    # cüzdan adreslerinin üretilmesi
    # Başına '0x' eklenerek ve 40 karaktere tamamlanarak gerçekçi adreslere benzetme
    wallets = [f"0x{random.getrandbits(160):040x}" for i in range(request.wallet_count)]
    
    # pareto dağılımı ile serveti eşitsiz ağırlıklarla dağıtır
    # gerçek blokzincir ağlarında 80/20 kuralı geçerlidir; işlemlerin çok büyük bir kısmı zengin balinalar tarafından gerçekleştirilir
    # alpha = 1.5 "eğri" seçilerek güç yasasına uygun bir ağırlık listesi oluşturulur.
    alpha = 1.5
    wallet_weights = [1.0 / (math.pow(i + 1, alpha)) for i in range(request.wallet_count)]
    
    generated_transactions = []
    current_timestamp = int(time.time()) # İşlemlerin zaman damgası için başlangıç noktası (Unix Epoch)
    
    # normal dağılım ile işlemlerin çoğunun küçük miktarlarda, çok azının ise fazla miktarlarda yapılması simüle edilir
    mu = 2.0   
    sigma = 1.2
    
    for _ in range(request.transaction_count):
        # yukarıda oluşturulan Pareto ağırlık listesine göre gönderici ve alıcı endeksleri seçilir.
        # bu sayede aktif cüzdanların işlem yapma olasılığı pasiflere göre matematiksel olarak yüksek olur, normalizasyon
        sender_idx = random.choices(range(request.wallet_count), weights=wallet_weights, k=1)[0]
        receiver_idx = random.choices(range(request.wallet_count), weights=wallet_weights, k=1)[0]
        
        # gönderici ve alıcı aynı çıkarsa, alıcı cüzdan farklı bir adres olana kadar yeniden seçilir.
        while sender_idx == receiver_idx and request.wallet_count > 1:
            receiver_idx = random.choices(range(request.wallet_count), weights=wallet_weights, k=1)[0]
            
        # normal dağılımdan rastgele miktar üretilir ve virgülden sonra 6 basamağa yuvarlanır.
        amount = round(random.lognormvariate(mu, sigma), 6)
        
        # her bir işlem için 256-bitlik özel bir işlem özeti "hash" simüle edilir.
        tx_id = f"0x{random.getrandbits(256):064x}"
        
        # tüm işlemlerin aynı saniyede gerçekleşmesi veri analitiği açısından hatalıdır.
        # gerçekçi zaman akışı için 1 ile 60 saniye arasında eklemeler
        current_timestamp += random.randint(1, 60)
        
        # C# servisinin veri taslağına göre yapılar düzenlenir
        generated_transactions.append({
            "tx_id": tx_id,
            "sender_address": wallets[sender_idx],
            "receiver_address": wallets[receiver_idx],
            "amount": amount,
            "timestamp": current_timestamp
        })

    # c# dan belirlenen JSON şablon yapısına sadık kalınarak
    # üretilen tüm anlamlı ve sentetik veriler üst anahtarlar altında geri döndürülür.
    return {
        "status": "success",
        "message": f"{request.wallet_count} cüzdan ve {request.transaction_count} işlem için gerçekçi sentetik veri modeli üretildi.",
        "generated_wallets": wallets,
        "generated_transactions": generated_transactions
    }