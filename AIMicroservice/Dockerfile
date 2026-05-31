# Temel Python imajını kullan (Hafif bir sürüm)
FROM python:3.11-slim

# Konteyner içinde çalışma dizinini ayarla
WORKDIR /app

# Önce kütüphane listesini kopyala ve yükle (Önbelleği verimli kullanmak için)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Şimdi Emir'in tüm kodlarını kopyala
COPY . .

# FastAPI sunucusunu 8000 portundan dışarı aç
EXPOSE 8000

# Uvicorn ile sunucuyu başlat
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]