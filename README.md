# Kayıp Şövalye — Yapay Zeka Destekli Oyunum

Unity 6 ile geliştirilmiş, 2D piksel-art bir aksiyon-platform oyunu. Oyuncu; devriye gezen düşmanların, düşen platformların ve ateş tuzaklarının arasından geçerek haritanın sonundaki şövalyeyi kurtarmaya çalışır. İsim, projenin bir bölümünün yapay zekâ desteğiyle geliştirilmesinden gelir.

> Bu proje bir öğrenme ve geliştirme çalışmasıdır. Oyun mekanikleri, arayüz ve "vuruş hissi" (game feel) efektleri tamamen C# ile kodlanmıştır; arayüzün büyük kısmı koddan üretilir.

---

## İçindekiler

- [Öne Çıkanlar](#öne-çıkanlar)
- [Özellikler](#özellikler)
- [Kontroller](#kontroller)
- [Oynanış](#oynanış)
- [Nasıl Çalıştırılır](#nasıl-çalıştırılır)
- [Teknik Detaylar](#teknik-detaylar)
- [Proje Yapısı](#proje-yapısı)

---

## Öne Çıkanlar

- 🎮 Çift zıplama, saldırı ve görünmezlik yeteneğiyle akıcı karakter kontrolü
- 🗡️ Vuruş hissi efektleri: ekran sarsıntısı, zaman donması, yanıp sönme ve hasar sayıları
- 👾 Üç farklı düşman tipi; ortak temel sınıflar üzerine kurulu yapay zekâ
- 🧪 Can, zehir ve güç iksirleriyle risk-ödül dengesi
- 🖥️ Tamamen koddan üretilen modern arayüz: HUD, duraklatma menüsü, sekmeli rehber ve oyun sonu ekranları
- 🏠 Animasyonlu ana menü ve en iyi süre kaydı

## Özellikler

### Oynanış
- **Üç farklı düşman tipi** — her biri kendi can, hasar ve hız değerleriyle devriye gezer, oyuncuyu görünce takip eder ve yaklaşınca saldırır.
- **Akıcı karakter kontrolü** — çift zıplama ve kare hızından bağımsız, tutarlı hareket.
- **Görünmezlik yeteneği** — kısa süreliğine düşmanlara görünmez olma (sınırlı kullanım hakkı).
- **İksir sistemi** — can yenileyen, canı azaltan (zehir) ve saldırı gücünü kalıcı artıran iksirler.
- **Çevre tehlikeleri** — basınca düşen platformlar ve düzenli hasar veren ateş alanları.

### Vuruş Hissi (Game Feel)
- İsabet anında **kırmızı yanıp sönme**, kısa **zaman donması** (hitstop), **kamera sarsıntısı** ve yukarı süzülen **hasar sayıları**.
- Bu efektler sahneye elle bileşen eklenmeden, çalışma zamanında kendini kurar.

### Arayüz
- **Oyun içi HUD** — can, görünmezlik ve güç göstergeleri; süre sayacı; düşük canda kırmızı kenar uyarısı; iksir alımında anlık bildirimler ve kurtarma ilerlemesi.
- **Duraklatma menüsü** — ESC ile devam / rehber / ana menü; pencere odağı kaybolunca otomatik duraklatma.
- **Sekmeli oyun rehberi** — Görev, Kontroller, İksirler, Düşmanlar ve Tehlikeler sekmeleri; değerler doğrudan oyun kodundan okunur.
- **Oyun sonu ve kazanma ekranları** — süre, öldürülen düşman ve en iyi derece özetiyle.
- **Animasyonlu ana menü** — hareketli bir tanıtım sahnesi, yumuşak sahne geçişleri ve en iyi süre kaydı.

## Kontroller

| Tuş | İşlev |
|-----|-------|
| **A / D** | Sola / sağa hareket |
| **Space** | Zıpla (havadayken bir kez daha zıplanabilir) |
| **E** | Saldır |
| **H** | Görünmez ol (sınırlı hak) |
| **F** | Basılı tut — şövalyeyi kurtar ve oyunu kazan |
| **I** | Oyun rehberini aç/kapat |
| **ESC** | Duraklat |

## Oynanış

Amaç, haritanın sonundaki **şövalyeye** ulaşıp **F** tuşunu basılı tutarak onu kurtarmaktır. Yol boyunca:

- **Düşmanları** ya saldırarak alt edersin ya da **görünmezlik** yeteneğiyle yanlarından sıyrılırsın.
- **Can iksirlerini** toplayıp hayatta kalır, **zehir iksirlerinden** uzak durursun.
- **Güç iksiri** saldırını kalıcı artırır — en dayanıklı rakibe ulaşmadan önce toplanması önerilir.
- **Düşen platformlara** ve **ateş alanlarına** dikkat edersin; haritadan aşağı düşmek oyunu bitirir.

Oyunu ne kadar hızlı bitirdiğin kaydedilir; **en iyi süreni** geçmeye çalışabilirsin.

## Nasıl Çalıştırılır

1. **Unity 6 (6000.0.41f1)** sürümünü [Unity Hub](https://unity.com/download) üzerinden kur.
2. Bu depoyu klonla:
   ```bash
   git clone https://github.com/BekirUtku/Yapay-Zeka-Destekli-Oyunum.git
   ```
3. Unity Hub'da **Add → Add project from disk** ile `Project_1` klasörünü seç.
4. Projeyi aç; Unity gerekli paketleri otomatik indirecektir.
5. `Assets/Scenes/Menu.unity` sahnesini açıp **Play** tuşuna bas.

> İlk açılış, Unity kütüphanesini (`Library/`) yeniden oluşturacağı için birkaç dakika sürebilir.

## Teknik Detaylar

- **Motor:** Unity 6 (6000.0.41f1)
- **Render:** Universal Render Pipeline (URP) 2D
- **Dil:** C#
- **Kullanılan paketler:** Cinemachine (kamera takibi), TextMesh Pro (arayüz metinleri), Unity 2D Feature set
- **Yazı tipi:** Exo 2 (Regular / SemiBold / ExtraBold)

Öne çıkan kod yaklaşımları:

- Vuruş hissi efektleri (`HitFlash`, `HitStop`, `ScreenShake`, `DamagePopup`) sahneye elle bileşen eklemeden, çalışma zamanında kendilerini kurar.
- Düşman mantığı ortak temel sınıflar (`EnemyAIBase`, `EnemyCombatBase`, `EnemyHealthBase`) üzerine kuruludur; yeni düşman eklemek kolaydır.
- Arayüz büyük ölçüde koddan üretilir; tema, tipografi, buton efektleri ve yerleşim tek bir `UITheme` üzerinden yönetilir.
- Oyun durumu (`GameSession`) süre, öldürülen düşman ve en iyi dereceyi takip eder; en iyi süre `PlayerPrefs` ile yerelde saklanır.
- Sahneler arası yumuşak geçişler `SceneFader` ile yönetilir.

## Proje Yapısı

```
Project_1/
├── Assets/
│   ├── Code/              # Oyun mantığı (oyuncu, düşmanlar, iksirler, efektler)
│   │   └── UI/            # Arayüz sistemi (HUD, menüler, rehber, tema, geçişler)
│   ├── Scenes/
│   │   ├── Menu.unity         # Ana menü
│   │   └── SampleScene.unity  # Oynanış bölümü
│   ├── Resources/Fonts/   # Exo 2 yazı tipi
│   ├── Hero_Knight/       # Oyuncu karakteri sprite ve animasyonları
│   ├── Enemy/, Fight/, Warrior/   # Düşman varlıkları
│   └── ...                # Tile'lar, efektler, arka planlar
├── Packages/              # Paket bağımlılıkları
└── ProjectSettings/       # Proje ayarları
```

---

<p align="center">Unity 6 ile geliştirilmiştir.</p>
