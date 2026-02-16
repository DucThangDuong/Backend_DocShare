using Domain.Entities;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace API.Extensions
{
    public static class StringHelpers
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration config)
        {
            _configuration = config;
        }
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
            const string KeyChars = "áàạảãâấầậẩẫăắằặẳẵÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴéèẹẻẽêếềệểễÉÈẸẺẼÊẾỀỆỂỄóòọỏõôốồộổỗơớờợởỡÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠúùụủũưứừựửữÚÙỤỦŨƯỨỪỰỬỮíìịỉĩÍÌỊỈĨđĐýỳỵỷỹÝỲỴỶỸ ";
            const string ReplChars = "aaaaaaaaaaaaaaaaaAAAAAAAAAAAAAAAAAeeeeeeeeeeeEEEEEEEEEEEoooooooooooooooooOOOOOOOOOOOOOOOOOuuuuuuuuuuuUUUUUUUUUUUiiiiiIIIIIdDyyyyyYYYYY_";
            StringBuilder sb = new StringBuilder(fileName.Length);
            foreach (char c in fileName)
            {
                if (c == '^') continue;

                int index = KeyChars.IndexOf(c);
                if (index != -1)
                {
                    sb.Append(ReplChars[index]);
                }
                else
                {
                    sb.Append(c);
                }
            }
            string result = sb.ToString();
            return Regex.Replace(result, @"[^a-zA-Z0-9_\-\.]", "");
        }
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = ConvertToUnSign(str);
            str = Regex.Replace(str, @"\s+", "-");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
        private static string ConvertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static string Create_s3ObjectKey_file(string filename, int userId)
        {
            string rawFileName = Path.GetFileNameWithoutExtension(filename);
            string safeFileName = StringHelpers.SanitizeFileName(rawFileName);
            string extension = Path.GetExtension(filename).ToLower();
            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            return $"{userId}/{dateFolder}/{safeFileName}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
        }
        public static string Create_s3ObjectKey_avatar(string ext, int userId)
        {
            return $"{userId}_{ext}";
        }
        public static string GetFinalAvatarUrl(string Avatar)
        {
            string minioBaseUrl = _configuration["Storage:ServiceUrl"] ?? "";
            string storage_avatar = _configuration["Storage:Avatar_storage"] ?? "";
            if (Avatar.StartsWith("https://")) return Avatar;
            return $"{minioBaseUrl}/{storage_avatar}/{Avatar}";
        }
    }
}
