using Microsoft.EntityFrameworkCore; 
using OkulBilgiApp; 
using System.Windows.Forms; 
using System;

namespace OkulBilgiApp
{
    public partial class Form1 : Form
    {
        Ogrenci? ogr; // se�ili olan ��renciyi tutmak i�in de�i�ken.

        public Form1()
        {
            InitializeComponent(); // Form bile�enlerini ba�lat�r.
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (var ctx = new OkulDbContext()) 
                {
                    var siniflar = ctx.TblSiniflar.ToList(); // Veri taban�ndaki t�m s�n�flar al�n�yor.

                    cbxS�n�fSe�.DataSource = siniflar; // S�n�f bilgileri ComboBox'a atan�yor.
                    cbxS�n�fSe�.DisplayMember = "SinifAd"; // ComboBox'ta s�n�f ad� g�r�necek.
                    cbxS�n�fSe�.ValueMember = "SinifId"; // S�n�f ID'si de�eri olarak kullan�lacak.
                }
            }
            catch (Exception ex)
            {
                // E�er bir hata olursa, kullan�c�ya mesaj g�sterilir.
                MessageBox.Show($"Bir hata olu�tu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            // Kullan�c� giri�lerini al.
            var ogrenciAd = txtAd.Text.Trim(); // ��renci ad�.
            var ogrenciSoyad = txtSoyad.Text.Trim(); // ��renci soyad�.
            var ogrenciNumara = txtNumara.Text.Trim(); // ��renci numaras�.
            var selectedSinifId = (int)cbxS�n�fSe�.SelectedValue; // Se�ilen s�n�f ID'si.

            // E�er herhangi bir alan bo� b�rak�lm��sa, kullan�c� uyar�l�r.
            if (string.IsNullOrWhiteSpace(ogrenciAd) ||
                string.IsNullOrWhiteSpace(ogrenciSoyad) ||
                string.IsNullOrWhiteSpace(ogrenciNumara) ||
                string.IsNullOrWhiteSpace(selectedSinifId.ToString()))
            {
                MessageBox.Show("L�tfen t�m alanlar� doldurun.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // ��lem sonland�r�l�r.
            }

            try
            {
                using (var ctx = new OkulDbContext()) // Veri taban� ba�lant�s�.
                {
                    // Se�ilen s�n�f� veri taban�ndan al.
                    var sinif = ctx.TblSiniflar
                        .Include(s => s.Ogrenciler) // S�n�fa ait ��rencileri de y�kle.
                        .FirstOrDefault(s => s.SinifId == selectedSinifId);

                    if (sinif == null)
                    {
                        // E�er s�n�f bulunamazsa hata g�ster.
                        MessageBox.Show("Ge�ersiz s�n�f se�imi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // S�n�f�n kontenjan�n� kontrol et.
                    int kontenjan;
                    bool kontenjanGe�erli = int.TryParse(sinif.Kontenjan, out kontenjan); // Kontenjan say� m�?
                    if (!kontenjanGe�erli)
                    {
                        MessageBox.Show("S�n�f�n kontenjan de�eri ge�ersiz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // E�er s�n�fta yer yoksa, kullan�c� uyar�l�r.
                    int mevcutOgrenciSayisi = sinif.Ogrenciler.Count();
                    if (mevcutOgrenciSayisi >= kontenjan)
                    {
                        MessageBox.Show("Se�ilen s�n�f�n kontenjan� dolmu�.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Yeni ��renci olu�tur.
                    var ogr = new Ogrenci
                    {
                        Ad = ogrenciAd,
                        Soyad = ogrenciSoyad,
                        Numara = ogrenciNumara,
                        SinifId = selectedSinifId // S�n�f ID'sini ba�la.
                    };

                    // ��renci veri taban�na eklenir.
                    ctx.Ogrenciler.Add(ogr);
                    int sonuc = ctx.SaveChanges(); // De�i�iklikler kaydedilir.

                    if (sonuc > 0)
                    {
                        // Ba�ar�l� i�lem sonras� alanlar� temizle.
                        MessageBox.Show("��renci ba�ar�yla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtAd.Clear();
                        txtSoyad.Clear();
                        txtNumara.Clear();
                        cbxS�n�fSe�.SelectedIndex = -1; // ComboBox se�im kald�r�l�r.
                        txtAd.Focus();
                    }
                    else
                    {
                        MessageBox.Show("��renci eklenirken bir hata olu�tu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
         
                MessageBox.Show($"Bir hata olu�tu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBul_Click(object sender, EventArgs e)
        {
            var ogrenciNumara = txtNumara.Text.Trim(); // ��renci numaras�n� al.

            if (string.IsNullOrWhiteSpace(ogrenciNumara))
            {
                // E�er ��renci numaras� girilmemi�se uyar.
                MessageBox.Show("L�tfen bir ��renci numaras� giriniz.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var ctx = new OkulDbContext())
                {
                    // ��renciyi numaras�na g�re bul ve s�n�f bilgisini y�kle.
                    var ogrenci = ctx.Ogrenciler
                                     .Include(o => o.Sinif)
                                     .FirstOrDefault(o => o.Numara == ogrenciNumara);

                    if (ogrenci != null)
                    {
                        // E�er ��renci bulunduysa, bilgileri doldur.
                        txtAd.Text = ogrenci.Ad;
                        txtSoyad.Text = ogrenci.Soyad;

                        if (ogrenci.Sinif != null)
                        {
                            cbxS�n�fSe�.SelectedValue = ogrenci.SinifId; // S�n�f se�imini g�ncelle.
                        }
                        else
                        {
                            MessageBox.Show("��rencinin s�n�f� bulunmamaktad�r.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        ogr = ogrenci; // Se�ili ��renci de�i�kenini g�ncelle.
                    }
                    else
                    {
                        MessageBox.Show("��renci bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj g�ster.
                MessageBox.Show($"Bir hata olu�tu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (ogr == null)
            {
                // E�er bir ��renci se�ilmemi�se uyar.
                MessageBox.Show("L�tfen �nce bir ��renci se�iniz.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var ctx = new OkulDbContext())
                {
                    // ��renci bilgilerini g�ncelle.
                    ogr.Ad = txtAd.Text.Trim();
                    ogr.Soyad = txtSoyad.Text.Trim();
                    ogr.Numara = txtNumara.Text.Trim();

                    int selectedSinifId = (int)cbxS�n�fSe�.SelectedValue;
                    ogr.SinifId = selectedSinifId; // S�n�f ID'sini g�ncelle.

                    ctx.Entry(ogr).State = EntityState.Modified; // ��renci g�ncellenecek olarak i�aretlenir.
                    var sonuc = ctx.SaveChanges(); // De�i�iklikler kaydedilir.

                    MessageBox.Show(sonuc > 0 ? "G�ncelleme ba�ar�l�!" : "G�ncelleme ba�ar�s�z!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                cbxS�n�fSe�.SelectedValue = ogr.SinifId; // ComboBox se�im g�ncellenir.
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj g�ster.
                MessageBox.Show($"Bir hata olu�tu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDersSe�_Click(object sender, EventArgs e)
        {
            if (ogr == null)
            {
                // E�er bir ��renci se�ilmemi�se uyar.
                MessageBox.Show("L�tfen �nce bir ��renci se�iniz.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Yeni bir Form2 olu�tur ve ��renci bilgilerini aktar.
            Form2 frm2 = new Form2(ogr.OgrenciId);
            frm2.SetOgrenci(ogr.Ad, ogr.Soyad, ogr.Numara);
            frm2.LoadDersler(); // Ders bilgilerini y�kle.
            frm2.Show(); 
        }
    }
}
