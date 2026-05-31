from fastapi import FastAPI
from pydantic import BaseModel
import random

app = FastAPI(title="AI & Synthetic Data Microservice")

# Gelen isteklerin veri modelini tanımlıyoruz
class DataRequest(BaseModel):
    wallet_count: int
    transaction_count: int

@app.get("/")
def read_root():
    return {"message": "AI Mikroservisi Aktif. Emir'in şantiyesi çalışıyor!"}

@app.post("/api/ai/generate-synthetic-data")
def generate_data(request: DataRequest):
    # Emir buraya kendi makine öğrenmesi ve sentetik veri algoritmasını yazacak.
    # Şimdilik API'nin C# ile konuşabildiğini kanıtlamak için sahte (dummy) veri dönüyoruz.
    
    dummy_wallets = [f"Sentetik_Cuzdan_{i}" for i in range(request.wallet_count)]
    
    return {
        "status": "success",
        "message": f"{request.wallet_count} cüzdan ve {request.transaction_count} işlem için sentetik veri modeli üretildi.",
        "generated_wallets": dummy_wallets
    }