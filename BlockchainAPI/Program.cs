var builder = WebApplication.CreateBuilder(args);

// 1. CORS Ayarlarını Servislere Ekle (Herkese açık hale getiriyoruz)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Herhangi bir adresten (file:// dahil) gelen isteğe izin ver
              .AllowAnyMethod()   // GET, POST gibi tüm işlemlere izin ver
              .AllowAnyHeader();  // Tüm veri başlıklarına izin ver
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// 2. CORS Politikasını Uygulamaya Tanıt (Burası çok önemli, MapControllers'dan önce olmalı)
app.UseCors("AllowAll");

app.MapControllers();

app.Run();