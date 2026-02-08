using System;
using System.Management;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json; // Cài qua NuGet: Install-Package Newtonsoft.Json
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(MyAutoCADTool.LicenseSystem))]

namespace MyAutoCADTool
{
    public class LicenseSystem
    {
        // THAY THẾ BẰNG URL WEB APP CỦA BẠN
        private const string apiUrl = "https://script.google.com";

        // 1. LẤY MÃ Ổ CỨNG (HARDWARE ID)
        public string GetHDDSerial()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    if (wmi_HD["SerialNumber"] != null)
                        return wmi_HD["SerialNumber"].ToString().Trim();
                }
            }
            catch { return "UNKNOWN_ID"; }
            return "UNKNOWN_ID";
        }

        // 2. GỌI API GOOGLE CHECK ONLINE
        public async Task<bool> VerifyLicenseOnline(string key)
        {
            try
            {
                string hwid = GetHDDSerial();
                using (HttpClient client = new HttpClient())
                {
                    // Tăng timeout nếu mạng chậm
                    client.Timeout = TimeSpan.FromSeconds(15);
                    string requestUrl = $"{apiUrl}?hwid={hwid}&key={key}";
                    
                    var response = await client.GetStringAsync(requestUrl);
                    dynamic result = JsonConvert.DeserializeObject(response);

                    if (result.status == "success")
                    {
                        // Lưu license vào máy để dùng offline (đơn giản hóa)
                        string expiry = result.expiry;
                        SaveLicenseLocal(hwid, expiry);
                        return true;
                    }
                }
            }
            // catch (Exception ex)
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Lỗi kết nối: " + ex.Message);
            }
            return false;
        }

        // 3. LƯU & KIỂM TRA LICENSE OFFLINE (Lưu vào thư mục AppData)
        private void SaveLicenseLocal(string hwid, string expiry)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\my_tool_lic.dat";
            // Trong thực tế nên mã hóa chuỗi này trước khi ghi file
            string content = $"{hwid}|{expiry}";
            System.IO.File.WriteAllText(path, content);
        }

        private bool IsLicenseValid()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\my_tool_lic.dat";
                if (!System.IO.File.Exists(path)) return false;

                string[] data = System.IO.File.ReadAllText(path).Split('|');
                string localHwid = data[0];
                DateTime expiryDate = DateTime.Parse(data[1]);

                return (localHwid == GetHDDSerial() && expiryDate > DateTime.Now);
            }
            catch { return false; }
        }

        // 4. LỆNH KÍCH HOẠT (GÕ LỆNH: ACTIVATE_TOOL)
        [CommandMethod("ACTIVATE_TOOL")]
        public void ActivateCommand()
        {
            string hwid = GetHDDSerial();
            
            using (Form form = new Form())
            {
                form.Text = "Kích hoạt Bản quyền";
                form.Size = new System.Drawing.Size(400, 250);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.TopMost = true;

                Label lblHwid = new Label() { Left = 20, Top = 20, Width = 350, Text = "Hardware ID (Gửi mã này cho người bán):" };
                TextBox txtHwid = new TextBox() { Left = 20, Top = 45, Width = 340, Text = hwid, ReadOnly = true };
                Label lblKey = new Label() { Left = 20, Top = 80, Width = 350, Text = "Nhập Activation Key:" };
                TextBox txtKey = new TextBox() { Left = 20, Top = 105, Width = 340 };
                Button btnBtn = new Button() { Text = "Kích hoạt", Left = 260, Top = 150, Width = 100, Height = 30 };

                btnBtn.Click += async (s, e) =>
                {
                    btnBtn.Enabled = false;
                    btnBtn.Text = "Checking...";
                    bool ok = await VerifyLicenseOnline(txtKey.Text);
                    if (ok) {
                        MessageBox.Show("Thành công! Hãy khởi động lại lệnh Tool.");
                        form.Close();
                    } else {
                        MessageBox.Show("Key sai hoặc hết hạn!");
                        btnBtn.Enabled = true;
                        btnBtn.Text = "Kích hoạt";
                    }
                };

                form.Controls.AddRange(new Control[] { lblHwid, txtHwid, lblKey, txtKey, btnBtn });
                Application.ShowModalDialog(form);
            }
        }

        // 5. LỆNH CHẠY TOOL LISP (GÕ LỆNH: MYTOOL)
        [CommandMethod("MYTOOL")]
        public void RunMyLispTool()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (IsLicenseValid())
            {
                // Gọi hàm C:TENHAM trong file Lisp đã load
                // doc.SendStringToExecute("(C:TEN_HAM_LISP_CUA_BAN) ", true, false, false);

                // BƯỚC 2: ĐỌC NỘI DUNG LISP TỪ TRONG DLL
                // Lưu ý: Tên resource thường là "Tên_Project.Tên_File.lsp"
                string lispContent = ReadLispFromResource("MyApp.my_logic.lsp");    
                if (!string.IsNullOrEmpty(lispContent))
                {
                    // BƯỚC 3: ĐẨY NỘI DUNG VÀO AUTOCAD ĐỂ CHẠY
                    // Thêm (C:TEN_LENH) vào cuối để nó tự thực thi sau khi load
                    // Gửi nội dung lisp vào AutoCAD + lệnh thực thi hàm C:HELLO_CONG
                    doc.SendStringToExecute(lispContent + "\n(C:HELLO_CONG)\n", true, false, false);
                    doc.Editor.WriteMessage("\n[Success] Tool đã được kích hoạt và thực thi.");
                }

            }
            else
            {
                Application.ShowAlertDialog("Bạn chưa có bản quyền! Hãy dùng lệnh ACTIVATE_TOOL.");
            }
        }

        // Hàm phụ trợ để đọc file đã nhúng (Embedded Resource)
        private string ReadLispFromResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch { return null; }
        }

    }
}
