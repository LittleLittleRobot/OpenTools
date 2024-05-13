using System;
using System.IO;

namespace Wesky.Net.OpenTools.Converter
{
    public class ImageConvert
    {
        public static string ConvertImageToBase64(string imagePath)
        {
            // 确保文件存在
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("指定的图片路径不存在");
            }

            // 读取文件的字节
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            // 转换为 Base64
            string base64String = Convert.ToBase64String(imageBytes);

            // 返回完整的 data URI，这里假定图片类型是 PNG
            return $"data:image/jpg;base64,{base64String}";
        }
    }
}
