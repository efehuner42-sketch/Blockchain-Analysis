# BMB2006 Veri Yapıları - Proje 4: Blokzincir İşlem Ağlarının Analizi

## Projenin Amacı

Bu proje, blokzincir sistemlerinde bulunan işlem verilerinin sadeleştirilmiş bir model üzerinden incelenmesini amaçlamaktadır.

Temel hedef; işlem ağını yönlü graf (Directed Graph) veri yapısı ile temsil etmek, Merkle Ağacı kullanarak işlem bütünlüğünü doğrulamak ve BFS/DFS algoritmaları yardımıyla belirli bir cüzdanın fon akışını takip etmektir.

## Kullanılan Teknolojiler

- **Backend (Çekirdek Veri Yapıları ve API):** C#, ASP.NET Core
- **Frontend (Arayüz ve Görselleştirme):** HTML5, CSS3, JavaScript
- **Yapay Zekâ ve Sentetik Veri Servisi:** Asenkron Mikroservis
- **DevOps ve Dağıtım:** Docker, Docker Compose
- **Versiyon Kontrol:** Git, GitHub

## Kurulum ve Çalıştırma Adımları

Projenin yerel ortamda (local environment) sorunsuz çalıştırılabilmesi için sistemde **Docker** ve **Docker Compose** kurulu olmalıdır.

### 1. Projeyi Klonlama

Terminal üzerinden proje deposu bilgisayara kopyalanmalı ve oluşturulan klasöre giriş yapılmalıdır:

```bash
git clone https://github.com/efehuner42-sketch/Blockchain-Analysis.git
cd Blockchain-Analysis
```

### 2. Mikroservisleri Başlatma

Ana dizinde aşağıdaki komut çalıştırılarak C# Backend ve Python AI servisleri, Docker'ın bridge ağı üzerinde eş zamanlı olarak ayağa kaldırılır:

```bash
docker-compose up --build
```

### 3. Erişim Noktaları (Endpoints)

Konteynerler başarıyla başlatıldıktan sonra aşağıdaki adreslere erişilebilir:

- **C# Backend API (Swagger UI):** http://localhost:5008/swagger
- **Python AI Servisi (FastAPI Dokümantasyonu):** http://localhost:5009/docs
- **Frontend Arayüzü:** Cytoscape.js tabanlı ağ görselleştirmesini görüntülemek için proje klasöründeki `index.html` dosyası herhangi bir modern web tarayıcısında açılmalıdır.

## Ekip Üyeleri ve Görev Dağılımı

- **Efe Hüner:** DevOps, Versiyon Kontrol (GitHub) ve Sistem Entegrasyonu (Docker & API)
- **Füsun Gün:** Çekirdek Veri Yapıları Geliştiricisi (Yönlü Graf, Merkle Ağacı, Hash Table)
- **Murat Aybey Nurçin:** Çekirdek Algoritma Geliştiricisi (BFS/DFS, Bakiye Hesaplama ve Big-O Analizleri)
- **Mehmet Berke Terzi:** Frontend ve Görselleştirme Sorumlusu (Node-Link Diyagramı Arayüzü)
- **Emir Berat Zorlu:** AI Mikroservisi, Sentetik Veri Üretimi ve Dokümantasyon Sorumlusu

## Geliştirme Süreci ve Kurallar

- Bu proje GitHub üzerinden yürütülmektedir. Ekip büyüklüğü göz önünde bulundurularak **main** dalına doğrudan kod gönderilmesi yasaktır.
- Her yeni geliştirme için ayrı bir dal (branch) oluşturulmalı ve değişiklikler **Pull Request (PR)** mekanizması kullanılarak ana dala entegre edilmelidir.
- Tüm kod değişiklikleri, ilgili görev veya geliştirme kapsamında anlamlı commit mesajları ile kayıt altına alınmalıdır.
