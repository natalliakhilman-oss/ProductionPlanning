using System.Reflection;
using System.Runtime.InteropServices;

namespace ProductionPlanning.Models
{
    public static class AppInfo
    {
        // Db Settings
        public const int db_port_default = 3306;
        public const string db_name_default = "Genezis";
        public const string db_user_default = "root";
        public const string db_password_default = "Basis_777";
        public const int db_timeout_default = 60;
        public const string encryptionKey = "Bazis_software";
        public const string server_urls_default = "http://*:17794";
        // User Settings
        public const int MainAdminId = 1;
        public const int MainUserId = 2;
        public static string GetAppPath()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return path;
        }

        public static string FraemworkInfo()
        {
            return RuntimeInformation.FrameworkDescription;
        }

        public static string DateRelease()
        {
            return File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToShortDateString();
        }

        public static string VersionApp()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
