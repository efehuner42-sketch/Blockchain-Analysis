# BMB2006 Veri Yapıları - Proje 4: Blokzincir İşlem Ağlarının Analizi

## 📌 Projenin Amacı
* Bu proje, blokzincir sistemlerinde bulunan işlem verilerinin sadeleştirilmiş bir model üzerinden incelenmesini amaçlamaktadır.  
* Temel hedefimiz; işlem ağını yönlü graf (Directed Graph) yapısı ile temsil etmek, Merkle Ağacı ile işlem bütünlüğünü doğrulamak ve BFS/DFS algoritmaları kullanarak belirli bir cüzdanın fon akışını takip etmektir.

## 💻 Kullanılan Teknolojiler
* **Backend (Çekirdek Veri Yapıları & API):** C#, ASP.NET Core
* **Frontend (Arayüz & Görselleştirme):** HTML5, CSS3, JavaScript
* **Yapay Zeka & Sentetik Veri Servisi:** Asenkron Mikroservis
* **DevOps & Dağıtım:** Docker, docker-compose
* **Versiyon Kontrol:** Git, GitHub

## 👥 Ekip Üyeleri ve Görev Dağılımı
* **Efe Hüner:** DevOps, Versiyon Kontrol (GitHub) ve Mimari Entegrasyon (Docker & API)
* **Füsun Gün:** Core Veri Yapıları Geliştiricisi (Yönlü Graf, Merkle Ağacı, Hash Table)
* **Murat Aybey Nurçin:** Core Algoritma Geliştiricisi (BFS/DFS, Bakiye Hesaplama, Big-O Analizleri)
* **Mehmet Berke Terzi:** Frontend ve Görselleştirme Sorumlusu (Node-link diyagramı arayüzü)
* **Emir Berat Zorlu:** AI Mikroservisi, Sentetik Veri Üretimi ve Dökümantasyon Sorumlusu

## 🚀 Geliştirme Süreci ve Kurallar
* Bu proje GitHub üzerinden yürütülmekte olup, ekip sayısının büyüklüğü göz önüne alınarak **master/main** dalına doğrudan kod gönderilmesi yasaktır.
*  Her yeni geliştirme için ayrı bir dal (branch) açılmalı ve değişiklikler **Pull Request (PR)** mekanizması ile ana dala entegre edilmelidir. 
