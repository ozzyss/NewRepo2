using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace OkulBilgiApp
{
    public partial class Form2 : Form
    {
        // Öğrenci kimliği için özel alan
        private int ogrenciId;

        // Form2'nin yapıcı metodu, öğrenci kimliğini parametre olarak alır
        public Form2(int ogrId)
        {
            InitializeComponent(); // Formun bileşenlerini başlatır
            ogrenciId = ogrId; // Öğrenci kimliğini atar
        }

        // Öğrenci bilgilerini kullanıcıya göstermek için bir etiket ayarlar
        public void SetOgrenci(string ogrenciAd, string ogrenciSoyad, string ogrenciNumara)
        {
            lbl.Text = $"Ad: {ogrenciAd} Soyad: {ogrenciSoyad} Numara: {ogrenciNumara}"; // Öğrenci bilgilerini etikete yazdırır
        }

        // Ders bilgilerini yükleyip DataGridView'de görüntüler
        public void LoadDersler()
        {
            using (var ctx = new OkulDbContext()) // Veritabanı bağlantısı başlatılır
            {
                var dersler = ctx.TblDersler.ToList(); // Veritabanındaki tüm dersler alınır
                dataGridView1.DataSource = dersler; // DataGridView'e dersler atanır

                // Eğer "DersleriSec" adında bir sütun yoksa yeni bir sütun ekler
                if (!dataGridView1.Columns.Contains("DersleriSec"))
                {
                    DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn
                    {
                        Name = "DersleriSec",    // Sütun adı
                        HeaderText = "Seç",       // Başlık metni
                        FalseValue = false,       // Varsayılan değer: seçilmemiş
                        TrueValue = true          // Seçildiğinde değer: true
                    };
                    dataGridView1.Columns.Insert(4, checkBoxColumn); // Sütunu tabloya ekler
                }
            }
        }

        // Seçilen dersleri kaydetmek için kullanılan butonun tıklama olayı
        private void btnDersKaydet_Click(object sender, EventArgs e)
        {
            var seciliDersler = new List<int>(); // Seçilen derslerin ID'lerini tutacak liste

            // DataGridView'deki her satırı kontrol eder
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var isSelected = Convert.ToBoolean(row.Cells["DersleriSec"].Value); // "Seç" sütununun değeri kontrol edilir
                if (isSelected) // Eğer seçilmişse
                {
                    var dersId = Convert.ToInt32(row.Cells["DersId"].Value); // Ders ID'si alınır
                    seciliDersler.Add(dersId); // Listeye eklenir
                }
            }

            // Eğer hiçbir ders seçilmemişse kullanıcıya uyarı verir
            if (seciliDersler.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir ders seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Seçilen dersleri veritabanına kaydeder
            using (var ctx = new OkulDbContext()) // Veritabanı bağlantısı başlatılır
            {
                foreach (var dersId in seciliDersler) // Seçilen her ders için döngü
                {
                    var ogrenciDers = new OgrenciDers
                    {
                        OgrenciId = ogrenciId, // Öğrenci kimliği atanır
                        DersId = dersId         // Ders kimliği atanır
                    };
                    ctx.TblOgrenciDers.Add(ogrenciDers); // Yeni kayıt eklenir
                    MessageBox.Show(ctx.SaveChanges() > 0 ? "Dersler Başarıyla Kaydedildi" : "Ders Kaydı Başarısız"); // Kayıt işleminin sonucu gösterilir
                }
            }
        }
    }
}
