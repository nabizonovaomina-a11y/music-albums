# Musiqa albomlari (15-variant)

Nabijonova Omina, ATT 02-24R

Mavzu: **Musiqa albomlari**. Album (nomi, ijrochi, yil, treklar soni, davomiylik) va Janr (spravochnik). Filtr: janr, ijrochi, yil. Saralash: yil, davomiylik. Fayl: muqova. Maxsus qoida: davomiylik (daqiqa) treklar sonidan kam bo'lmasligi kerak.

## Texnologiyalar
ASP.NET Core Web API (.NET 10), EF Core + SQLite, FluentValidation, JWT, Angular 22.

## Serverni ishga tushirish
```
cd MusicAlbums.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "kamida-32-belgili-maxfiy-kalit-yozing-bu-yerga"
dotnet ef database update
dotnet run
```
Scalar hujjati: http://localhost:5161/scalar/v1

## Klientni ishga tushirish
```
cd client
npm install
ng serve
```
Manzil: http://localhost:4200

## Test foydalanuvchi
Login: `test`  Parol: `123456`
(Yo'q bo'lsa, `/api/auth/register` orqali yarating.)

## So'rovlar
Barcha so'rovlar `MusicAlbums.Api/MusicAlbums.Api.http` faylida (kirish va muqova yuklash ham bor).

## Skrinshotlar
1. Albomlar ro'yxati
2. Bitta albom sahifasi
3. Xatolar ko'rsatilgan forma
4. Yuklangan muqova