using GörselProgramlamaFinal.Entities;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Layout;
using System.Net.Mail;
using System.Net;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Runtime.Remoting.Contexts;

// iş katmanı
namespace GörselProgramlamaFinal
{
    internal class FonksiyonK
    {
        // veri katmanına erişim
        private VeriErisimK VeriErisimK = new VeriErisimK();

        // Üye ekleme fonksiyonu
        public void UyeEkle(Uye uye)
        {
            // Veri Erişim Katmanı'ndan yeni bir nesne oluşturuluyor
            VeriErisimK VeriErisimK = new VeriErisimK();
            if (VeriErisimK.UyeVarMi(uye.ID))
            {
                throw new Exception("Bu ID'ye sahip bir üye zaten mevcut. Lütfen farklı bir ID kullanın.");
            }
    
            VeriErisimK.UyeEkle(uye); 
        }


        public bool UyeVarMi(int id)
        {
            return VeriErisimK.UyeVarMi(id);
            // Veritabanında belirtilen ID'ye sahip üye olup olmadığını kontrol eder.
        }

        public void UyeSil(int uyeID)
        {
            
            VeriErisimK.UyeSil(uyeID);
            // Veritabanında belirtilen ID'ye sahip üyeyi siler.
        }

        public List<Uye> UyeListele()
        {
            return VeriErisimK.UyeListele();
        }

        public void UyeGuncelle(Uye uye)
        {
            VeriErisimK.UyeGuncelle(uye);
            // Veritabanındaki tüm üyeleri listeleyerek geri döndürür.
        }

        public Uye UyeIDileGetir(int uyeID)
        {
            // Veritabanındaki üyelerden verilen ID'ye sahip olanı getirir.
            return VeriErisimK.UyeIDileGetir(uyeID); 
        }

        public List<string> UyeEmailListele()
        {
            // Veritabanındaki tüm üyelerin e-posta adreslerini liste olarak döndürür.
            return VeriErisimK.UyeEmailListele(); 
        }

        public Uye UyeBilgileriniGetir(int id)
        {
            // Veritabanındaki üyeden verilen ID'ye sahip olanın temel bilgilerini getirir.
            return VeriErisimK.UyeBilgileriniGetir(id); 
        }
        public Uye UyeTumBilgileriniGetir(int id)
        {
            // Veritabanındaki üyeden verilen ID'ye sahip olanın tüm bilgilerini getirir.
            return VeriErisimK.UyeTumBilgileriniGetir(id); 
        }

        public void AidatOde(int id, decimal odemeMiktari)
        {
            // Üyenin aidat ödemesini gerçekleştirir.
            Uye uye = UyeBilgileriniGetir(id);

            if (uye == null)
            {
                throw new Exception("Kullanıcı bulunamadı.");
            }

            if (odemeMiktari > uye.GüncelBorc)
            {
                throw new Exception("Ödeme miktarı güncel borçtan fazla olamaz.");
            }

            VeriErisimK.AidatOde(id, odemeMiktari); 
        }

        public void MakbuzOlustur(Uye uye, decimal odemeMiktari)
        {
            // Verilen üye ve ödeme miktarına göre bir aidat ödeme makbuzu oluşturur ve PDF formatında kaydeder.

            string pdfPath = $"Makbuz_{uye.AdSoyad}_MakbuzNO_{uye.ID}.pdf";
            // Makbuz dosyasının adı, üyenin ad-soyadı ve ID'si ile oluşturulur.

            using (FileStream stream = new FileStream(pdfPath, FileMode.Create))
            {
                // PDF dosyasını oluşturmak için FileStream kullanılır.

                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);
                // PDF yazıcı ve belge nesneleri oluşturuluyor.
                // Makbuz başlığı ekleniyor.
                document.Add(new Paragraph("Aidat Ödeme Makbuzu")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(18));

                // Üyenin adı-soyadı, ID'si, ödenen tutar ve kalan borç gibi bilgileri ekliyoruz.
                document.Add(new Paragraph($"Ad Soyad: {uye.AdSoyad}"));
                document.Add(new Paragraph($"ID: {uye.ID}"));
                document.Add(new Paragraph($"Ödenen Tutar: {odemeMiktari:C}")); 
                document.Add(new Paragraph($"Kalan Güncel Borç: {(uye.GüncelBorc - odemeMiktari):C}"));
                document.Add(new Paragraph($"Tarih: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));

               
                document.Close();
            }

           
            MessageBox.Show($"Makbuz oluşturuldu: {pdfPath}", "Makbuz Oluşturma", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public List<Uye> BorclariListele()
        {
            // Üye borçlarını listeleyen fonksiyon.
            return VeriErisimK.BorclariListele();
        }

        public void EtkinlikEpostasiGonder(string etkinlikAdi, string etkinlikYeri, DateTime etkinlikSaati)
        {
            // Etkinlik duyurusunu tüm üyelere e-posta olarak gönderir.
            List<string> emailListesi = VeriErisimK.UyeEmailListele();
            // Üye e-posta adreslerini alır

            string mesaj = $"{etkinlikAdi} etkinliğimize herkes davetlidir. Etkinliğimiz {etkinlikSaati} tarihinde  olacaktır. Hepinizi bekliyoruz.<br>Etkinlik Yeri: {etkinlikYeri}.<br>Bu mesaj Kümbetli Köyü Derneği tarafından gönderilmektedir.<br>Toplu mesajdır lütfen cevap vermeyiniz.";
            // Etkinlik mesajı hazırlanır.


            // Her bir üye için e-posta gönderimi yapılır.
            foreach (var email in emailListesi)
            {
                MailGonder(email, mesaj);  
            }
        }

        private void MailGonder(string aliciEmail, string mesaj)
        {
            // E-posta gönderme işlemi için kullanılan fonksiyon.

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                // SMTP istemcisi ayarları yapılır.
                Port = 587,
                Credentials = new NetworkCredential("eraydernekproje@gmail.com", "wxzevmxwgjtmjmiu"),
                EnableSsl = true
            };

            MailMessage mailMessage = new MailMessage
            {
                // E-posta içeriği oluşturulur.
                From = new MailAddress("eraydernekproje@gmail.com"),
                Subject = "Etkinlik Davetiyesi",
                Body = mesaj,
                IsBodyHtml = true
            };

            mailMessage.To.Add(aliciEmail); 

            try
            {
                // E-posta gönderimi yapılır.
                smtpClient.Send(mailMessage);  
                MessageBox.Show("E-posta başarıyla gönderildi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"E-posta gönderme hatası: {ex.Message}");
            }
        }

        public void DuyuruGonder(string aliciEmail, string mesaj)
        {
            // Duyuru e-postası gönderimi için kullanılan fonksiyon.

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("eraydernekproje@gmail.com", "wxzevmxwgjtmjmiu"),
                EnableSsl = true
            };

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress("eraydernekproje@gmail.com"),
                Subject = "Kümbetli Köyü Derneği",
                Body = mesaj,
                IsBodyHtml = true
            };

            mailMessage.To.Add(aliciEmail);

            try
            {
                smtpClient.Send(mailMessage); 
                MessageBox.Show("E-posta başarıyla gönderildi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"E-posta gönderme hatası: {ex.Message}");
            }
        }
        public void AidatGuncelle(int uyeID, decimal aidat)
        {
            // Belirtilen üyenin aidat bilgisini güncelleyen fonksiyon.
            VeriErisimK.AidatGuncelle(uyeID, aidat);
            // Veri erişim katmanındaki ilgili fonksiyonla aidat güncellenir.
        }


        public void AidatGuncellemeTumu(decimal aidat)
        {
            // Tüm üyelerin aidat bilgisini güncelleyen fonksiyon.
            List<Uye> uyeler = VeriErisimK.UyeListele();
            foreach (var uye in uyeler)
            {
                AidatGuncelle(uye.ID, aidat);
            }
        }


    }
}
