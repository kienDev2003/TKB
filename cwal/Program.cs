using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using xNet;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Data.SqlClient;
using System.Configuration;

namespace cwal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            int mode = Mode();
            CheckMode(mode);
        }

        public static void Cwal_Data_Theo_MSV(int msv)
        {
            String MaMH = "";
            String TenMH = "";
            String NhomMH = "";
            String SoTinChi = "";
            String MaLop = "";
            String NgayHoc = "";
            String TietBatDau = "";
            String SoTiet = "";
            String Phong = "";
            String Tuan = "";
            String Tuan1 = "";
            String startDate = "";
            String endDate = "";



            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36";
                webClient.Headers["Accept-Language"] = "vi";
                webClient.Headers["Accept-Encoding"] = "gzip,deflate,br";
                String html = GetHtmlData(msv);
                if (html != "0")
                {
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(html);

                    var ttsv = document.DocumentNode.Descendants("div")
                        .Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ctl00_ContentPlaceHolder1_ctl00_pnlTKB")).FirstOrDefault();
                    if (ttsv != null)
                    {

                        var data = ttsv.Descendants("table");
                        foreach (var table in data)
                        {
                            String ttsvText = table.InnerText;
                            Console.WriteLine(ttsvText);
                        }
                    }

                    var element = document.DocumentNode.Descendants("div")
                        .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("grid-roll2")).FirstOrDefault();

                    if (element != null)
                    {
                        List<string> tableContents = new List<string>();

                        var data = element.Descendants("table");
                        foreach (var table in data)
                        {

                            var data2 = table.Descendants("td").ToList();
                            foreach (var tt in data2)
                            {

                                MaMH = data2.ElementAt(0).InnerText.ToString();
                                TenMH = data2.ElementAt(1).InnerText.ToString();
                                NhomMH = data2.ElementAt(2).InnerText.ToString();
                                SoTinChi = data2.ElementAt(3).InnerText.ToString();
                                MaLop = data2.ElementAt(4).InnerText.ToString();
                                NgayHoc = data2.ElementAt(8).InnerText.ToString();
                                TietBatDau = data2.ElementAt(9).InnerText.ToString();
                                SoTiet = data2.ElementAt(10).InnerText.ToString();
                                Phong = data2.ElementAt(11).InnerText.ToString();
                                Tuan = data2.ElementAt(13).InnerText.ToString();

                                var NgayHocTong = data2.ElementAt(13).SelectSingleNode("div");
                                Tuan1 = NgayHocTong.GetAttributeValue("onmouseover", "").ToString();
                                //Cắt chuỗi lấy dữ liệu ngày học tổng
                                int startIndex = Tuan1.IndexOf("('");
                                int endIndex = Tuan1.LastIndexOf("')");
                                if (startIndex >= 0 && endIndex >= 0)
                                {
                                    string desiredText = Tuan1.Substring(startIndex + 2, endIndex - startIndex - 2);
                                    string originalText = desiredText;
                                    string[] parts = originalText.Split(new string[] { "--" }, StringSplitOptions.None);

                                    if (parts.Length >= 2)
                                    {
                                        startDate = parts[0];
                                        endDate = parts[1];
                                    }

                                }

                                String query = $"INSERT INTO tbl_ThoiKhoaBieu (MaMH, TenMH, NhomMH, STC, MaLop, NgayHoc, TietBatDau, SoTiet, Phong, TuanHoc, NgayBatDauHoc, NgayKetThucHoc) VALUES " +
                                               $"('{MaMH}', '{TenMH}', '{NhomMH}', '{SoTinChi}', '{MaLop}', '{NgayHoc}', '{TietBatDau}', '{SoTiet}', '{Phong}', '{Tuan}', '{startDate}', '{endDate}')";

                                query = ConvertToUtf8(query, Encoding.UTF8);

                                bool check = Command(query);
                                if (check)
                                {
                                    Console.WriteLine("Thêm vào database thành công");
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Thêm vào database thất bại");
                                }

                            }
                            /*Console.WriteLine($"Mã Môn Học: {MaMH}_Tên Môn Học: {TenMH}\n" +
                                              $"Nhóm Học: {NhomMH}_Số Tín Chỉ: {SoTinChi}\nMã Lớp: {MaLop}_Ngày Học: Thứ {NgayHoc}\n" +
                                              $"Tiết Bắt Đầu: {TietBatDau}_Số Tiết: {SoTiet}\nPhòng: {Phong}_Ngày Bắt Đầu: {startDate}_Ngày Kết Thúc: {endDate}\n\n\n");*/
                        }
                        /*foreach (var dataEnd in tableContents)
                        {

                            Console.WriteLine(dataEnd);
                        }*/
                        Rerun();
                    }
                }
                else
                {
                    Console.WriteLine("\n\n\tKhông lấy được data.Hãy kiểm tra lại mã sinh viên");
                    Console.WriteLine("\tBạn Có Muốn Thử Lại Không_1.Có--2.Không");
                    Rerun();
                }
            }


        }

        static void CheckMode(int checkMode)
        {
            if (checkMode == 1)
            {
                XemThoiKhoaBieu();
            }
            else if (checkMode == 2)
            {
                int delete = DeleteData();
                if (delete == 0)
                {
                    Console.WriteLine("Lỗi kết nối đến database. Vui lòng thử lại sau");
                }
                else if (delete == 2)
                {
                    Rerun();
                }
                else if (delete == 1)
                {
                    InsertData();
                }
            }
            else
            {
                Rerun();
            }
        }

        static bool Command(String query)
        {
            String strConn = "Data Source=DESKTOP-R61I86K\\SQLEXPRESS;Initial Catalog=TKB;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                int check = cmd.ExecuteNonQuery();
                if (check > 0)
                {
                    return true;
                }
                return false;
            }
        }

        static SqlDataReader Reader()
        {
            SqlDataReader reader = null;
            String StrConn = "Data Source=DESKTOP-R61I86K\\SQLEXPRESS;Initial Catalog=TKB;Integrated Security=True";
            SqlConnection conn = new SqlConnection(StrConn);


            conn.Open();
            String query = "SELECT * FROM tbl_ThoiKhoaBieu";
            SqlCommand cmd = new SqlCommand(query, conn);
            reader = cmd.ExecuteReader();
            return reader;

        }

        /*public static String GetHtmlData(int msv)
        {
            // Khởi tạo trình duyệt Chrome
            IWebDriver driver = new ChromeDriver();

            // Truy cập trang web cần thực hiện tương tác
            driver.Navigate().GoToUrl($"https://daotao.vnua.edu.vn/default.aspx?page=thoikhoabieu&sta=1&id={msv}");

            if (CheckAlert(driver))
            {
                // Nếu có, xử lý cửa sổ thông báo
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
            }
            // Tìm và click vào phần tử ctl00$ContentPlaceHolder1$ctl00$rad_ThuTiet
            IWebElement element = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_rad_ThuTiet"));
            element.Click();

            if (CheckAlert(driver))
            {
                // Nếu có, xử lý cửa sổ thông báo
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
            }

            // Lấy nội dung HTML sau khi tương tác
            string html = driver.PageSource;

            // Làm việc với chuỗi HTML theo nhu cầu của bạn

            // Đóng trình duyệt
            driver.Quit();
            return html;
        }*/

        static void InsertData()
        {
            int msv = NhapMaSinhVien();
            Cwal_Data_Theo_MSV(msv);
            Console.WriteLine("\n");
        }

        static int DeleteData()
        {
            SqlDataReader data = Reader();
            if (data.Read())
            {
                Console.WriteLine("Đã có dữ liệu. Bạn có thật sự muốn thiết lập lại dữ liệu ?\n" +
                                  "Nhập: yes (Đồng ý)__no(Hủy bỏ thiết lập dữ liệu)");
                String checkData = Console.ReadLine();
                if (checkData == "yes")
                {
                    String query = "DELETE FROM tbl_ThoiKhoaBieu";
                    bool check = Command(query);
                    if (!check)
                    {
                        return 0;
                    }
                    return 1;
                }
                return 2;
            }
            return 1;

        }
        static int Mode()
        {
            Console.WriteLine("Chọn chế độ !\n 1_Xem Thời Khóa Biêu\n 2_Thiết Lập Lại Cơ Sở Dữ Liệu");
            int mode = int.Parse(Console.ReadLine());
            if (mode == 1)
            {
                return 1;
            }
            else if (mode == 2)
            {
                return 2;
            }
            else
            {
                Console.WriteLine("Vui lòng chọn đúng số theo Chế Độ !\n");
                return 0;
            }
        }

        public static string GetHtmlData(int msv)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            IWebDriver driver = new ChromeDriver(chromeOptions);

            try
            {
                // Khởi tạo trình duyệt Chrome trong chế độ headless (ẩn)


                // Truy cập trang web cần thực hiện tương tác
                driver.Navigate().GoToUrl($"https://daotao.vnua.edu.vn/default.aspx?page=thoikhoabieu&sta=1&id={msv}");

                // Thực hiện các tương tác trên trang web theo nhu cầu của bạn
                if (CheckAlert(driver))
                {
                    IAlert alert = driver.SwitchTo().Alert();
                    alert.Accept();
                }


                IWebElement element = driver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_rad_ThuTiet"));
                element.Click();
                if (CheckAlert(driver))
                {
                    IAlert alert = driver.SwitchTo().Alert();
                    alert.Accept();
                }

                // Lấy nội dung HTML sau khi tương tác
                string html = driver.PageSource;

                // Đóng trình duyệt ảo
                driver.Quit();

                return html;
            }
            catch (NoSuchElementException e)
            {
                // Xử lý exception, ví dụ: đóng trình duyệt và thực hiện lại
                driver.Quit();
                return "0";
            }
        }

        static string ConvertToUtf8(string input, Encoding sourceEncoding)
        {
            byte[] sourceBytes = sourceEncoding.GetBytes(input);
            string utf8String = Encoding.UTF8.GetString(sourceBytes);
            return utf8String;
        }

        static bool CheckAlert(IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }

        public static void Rerun()
        {

            Console.WriteLine("\n\nBạn Có Muốn Tiếp Tục Không?_1.Có--2.Không");
            int rerun;
            String input = Console.ReadLine();
            int.TryParse(input, out rerun);
            if (rerun == 1)
            {
                Console.Clear();
                int mode = Mode();
                CheckMode(mode);
            }
        }

        public static int NhapMaSinhVien()
        {
            Console.WriteLine("\n\t\tHãy Nhập Mã Sinh Viên");
            int msv;
            String input = Console.ReadLine();
            int.TryParse(input, out msv);
            return msv;

        }

        static void XemThoiKhoaBieu()
        {
            string connectionString = "Data Source=DESKTOP-R61I86K\\SQLEXPRESS;Initial Catalog=TKB;Integrated Security=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                DateTime currentDate = DateTime.Now;
                String currentDayOfWeek = TranslateToVietnamese(currentDate.DayOfWeek);

                string query = "SELECT * FROM tbl_ThoiKhoaBieu " +
                               "WHERE NgayHoc = @currentDayOfWeek " +
                               "AND " +
                               "CONVERT(datetime, NgayBatDauHoc, 103) <= @currentDate " +
                               "AND " +
                               "CONVERT(datetime, NgayKetThucHoc, 103) >= @currentDate";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@currentDayOfWeek", currentDayOfWeek);
                    command.Parameters.AddWithValue("@currentDate", currentDate);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        if(reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string maMonHoc = reader["MaMH"].ToString();
                                string tenMonHoc = reader["TenMH"].ToString();
                                string soTinChi = reader["STC"].ToString();
                                string ngayHoc = reader["NgayHoc"].ToString();
                                string tietBatDau = reader["TietBatDau"].ToString();
                                string soTiet = reader["SoTiet"].ToString();
                                string phong = reader["Phong"].ToString();
                                string tuanHoc = reader["TuanHoc"].ToString();


                                Console.WriteLine($"\n Mã môn học: {maMonHoc}\n Tên Môn Học: {tenMonHoc}\n Số Tín Chỉ: {soTinChi}\n " +
                                                  $"Ngày học: Thứ {ngayHoc}\n Tiết Bắt Đầu: {tietBatDau}\n Số Tiết Học: {soTiet}\n " +
                                                  $"Phòng học: {phong}\n Tuần Học: {tuanHoc}\n");
                            }
                            Rerun();
                        }
                        else
                        {
                            Console.WriteLine("Hôm nay không có môn học nào !");
                            Rerun();
                        }
                    }
                }
            }

        }

        static string TranslateToVietnamese(DayOfWeek dayOfWeekInEnglish)
        {
            // Ánh xạ các thứ tiếng Anh sang tiếng Việt
            switch (dayOfWeekInEnglish)
            {
                case DayOfWeek.Monday:
                    return "Hai";
                case DayOfWeek.Tuesday:
                    return "Ba";
                case DayOfWeek.Wednesday:
                    return "Tư";
                case DayOfWeek.Thursday:
                    return "Năm";
                case DayOfWeek.Friday:
                    return "Sáu";
                case DayOfWeek.Saturday:
                    return "Bảy";
                case DayOfWeek.Sunday:
                    return "CN";
                default:
                    return string.Empty;
            }
        }
    }
}
