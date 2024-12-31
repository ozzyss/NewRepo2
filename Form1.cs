using Microsoft.EntityFrameworkCore; 
using OkulBilgiApp; 
using System.Windows.Forms; 
using System;

namespace OkulBilgiApp
{
    public partial class Form1 : Form
    {
        Ogrenci? ogr; // seçili olan öðrenciyi tutmak için deðiþken.

        public Form1()
        {
            InitializeComponent(); // Form bileþenlerini baþlatýr.
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (var ctx = new OkulDbContext()) 
                {
                    var siniflar = ctx.TblSiniflar.ToList(); // Veri tabanýndaki tüm sýnýflar alýnýyor.

                    cbxSýnýfSeç.DataSource = siniflar; // Sýnýf bilgileri ComboBox'a atanýyor.
                    cbxSýnýfSeç.DisplayMember = "SinifAd"; // ComboBox'ta sýnýf adý görünecek.
                    cbxSýnýfSeç.ValueMember = "SinifId"; // Sýnýf ID'si deðeri olarak kullanýlacak.
                }
            }
            catch (Exception ex)
            {
                // Eðer bir hata olursa, kullanýcýya mesaj gösterilir.
                MessageBox.Show($"Bir hata oluþtu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            // Kullanýcý giriþlerini al.
            var ogrenciAd = txtAd.Text.Trim(); // Öðrenci adý.
            var ogrenciSoyad = txtSoyad.Text.Trim(); // Öðrenci soyadý.
            var ogrenciNumara = txtNumara.Text.Trim(); // Öðrenci numarasý.
            var selectedSinifId = (int)cbxSýnýfSeç.SelectedValue; // Seçilen sýnýf ID'si.

            // Eðer herhangi bir alan boþ býrakýlmýþsa, kullanýcý uyarýlýr.
            if (string.IsNullOrWhiteSpace(ogrenciAd) ||
                string.IsNullOrWhiteSpace(ogrenciSoyad) ||
                string.IsNullOrWhiteSpace(ogrenciNumara) ||
                string.IsNullOrWhiteSpace(selectedSinifId.ToString()))
            {
                MessageBox.Show("Lütfen tüm alanlarý doldurun.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Ýþlem sonlandýrýlýr.
            }

            try
            {
                using (var ctx = new OkulDbContext()) // Veri tabaný baðlantýsý.
                {
                    // Seçilen sýnýfý veri tabanýndan al.
                    var sinif = ctx.TblSiniflar
                        .Include(s => s.Ogrenciler) // Sýnýfa ait öðrencileri de yükle.
                        .FirstOrDefault(s => s.SinifId == selectedSinifId);

                    if (sinif == null)
                    {
                        // Eðer sýnýf bulunamazsa hata göster.
                        MessageBox.Show("Geçersiz sýnýf seçimi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Sýnýfýn kontenjanýný kontrol et.
                    int kontenjan;
                    bool kontenjanGeçerli = int.TryParse(sinif.Kontenjan, out kontenjan); // Kontenjan sayý mý?
                    if (!kontenjanGeçerli)
                    {
                        MessageBox.Show("Sýnýfýn kontenjan deðeri geçersiz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Eðer sýnýfta yer yoksa, kullanýcý uyarýlýr.
                    int mevcutOgrenciSayisi = sinif.Ogrenciler.Count();
                    if (mevcutOgrenciSayisi >= kontenjan)
                    {
                        MessageBox.Show("Seçilen sýnýfýn kontenjaný dolmuþ.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Yeni öðrenci oluþtur.
                    var ogr = new Ogrenci
                    {
                        Ad = ogrenciAd,
                        Soyad = ogrenciSoyad,
                        Numara = ogrenciNumara,
                        SinifId = selectedSinifId // Sýnýf ID'sini baðla.
                    };

                    // Öðrenci veri tabanýna eklenir.
                    ctx.Ogrenciler.Add(ogr);
                    int sonuc = ctx.SaveChanges(); // Deðiþiklikler kaydedilir.

                    if (sonuc > 0)
                    {
                        // Baþarýlý iþlem sonrasý alanlarý temizle.
                        MessageBox.Show("Öðrenci baþarýyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtAd.Clear();
                        txtSoyad.Clear();
                        txtNumara.Clear();
                        cbxSýnýfSeç.SelectedIndex = -1; // ComboBox seçim kaldýrýlýr.
                        txtAd.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Öðrenci eklenirken bir hata oluþtu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
         
                MessageBox.Show($"Bir hata oluþtu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBul_Click(object sender, EventArgs e)
        {
            var ogrenciNumara = txtNumara.Text.Trim(); // Öðrenci numarasýný al.

            if (string.IsNullOrWhiteSpace(ogrenciNumara))
            {
                // Eðer öðrenci numarasý girilmemiþse uyar.
                MessageBox.Show("Lütfen bir öðrenci numarasý giriniz.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var ctx = new OkulDbContext())
                {
                    // Öðrenciyi numarasýna göre bul ve sýnýf bilgisini yükle.
                    var ogrenci = ctx.Ogrenciler
                                     .Include(o => o.Sinif)
                                     .FirstOrDefault(o => o.Numara == ogrenciNumara);

                    if (ogrenci != null)
                    {
                        // Eðer öðrenci bulunduysa, bilgileri doldur.
                        txtAd.Text = ogrenci.Ad;
                        txtSoyad.Text = ogrenci.Soyad;

                        if (ogrenci.Sinif != null)
                        {
                            cbxSýnýfSeç.SelectedValue = ogrenci.SinifId; // Sýnýf seçimini güncelle.
                        }
                        else
                        {
                            MessageBox.Show("Öðrencinin sýnýfý bulunmamaktadýr.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        ogr = ogrenci; // Seçili öðrenci deðiþkenini güncelle.
                    }
                    else
                    {
                        MessageBox.Show("Öðrenci bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj göster.
                MessageBox.Show($"Bir hata oluþtu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (ogr == null)
            {
                // Eðer bir öðrenci seçilmemiþse uyar.
                MessageBox.Show("Lütfen önce bir öðrenci seçiniz.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var ctx = new OkulDbContext())
                {
                    // Öðrenci bilgilerini güncelle.
                    ogr.Ad = txtAd.Text.Trim();
                    ogr.Soyad = txtSoyad.Text.Trim();
                    ogr.Numara = txtNumara.Text.Trim();

                    int selectedSinifId = (int)cbxSýnýfSeç.SelectedValue;
                    ogr.SinifId = selectedSinifId; // Sýnýf ID'sini güncelle.

                    ctx.Entry(ogr).State = EntityState.Modified; // Öðrenci güncellenecek olarak iþaretlenir.
                    var sonuc = ctx.SaveChanges(); // Deðiþiklikler kaydedilir.

                    MessageBox.Show(sonuc > 0 ? "Güncelleme baþarýlý!" : "Güncelleme baþarýsýz!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                cbxSýnýfSeç.SelectedValue = ogr.SinifId; // ComboBox seçim güncellenir.
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj göster.
                MessageBox.Show($"Bir hata oluþtu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDersSeç_Click(object sender, EventArgs e)
        {
            if (ogr == null)
            {
                // Eðer bir öðrenci seçilmemiþse uyar.
                MessageBox.Show("Lütfen önce bir öðrenci seçiniz.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Yeni bir Form2 oluþtur ve öðrenci bilgilerini aktar.
            Form2 frm2 = new Form2(ogr.OgrenciId);
            frm2.SetOgrenci(ogr.Ad, ogr.Soyad, ogr.Numara);
            frm2.LoadDersler(); // Ders bilgilerini yükle.
            frm2.Show(); 
        }
    }
}
