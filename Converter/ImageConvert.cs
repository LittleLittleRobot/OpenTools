using System;
using System.IO;

namespace Wesky.Net.OpenTools.Converter
{
    public class ImageConvert
    {
        /// <summary>
        /// 将图片文件转换为 Base64 编码的字符串。
        /// Converts an image file to a Base64-encoded string.
        /// </summary>
        /// <param name="imagePath">图片文件的路径。Path to the image file.</param>
        /// <returns>返回 Base64 编码的图片字符串。Returns a Base64-encoded image string.</returns>
        public static string ConvertImageToBase64(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("指定的图片路径不存在。Specified image path does not exist.");
            }
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string mimeType = GetMimeType(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            return $"data:{mimeType};base64,{base64String}";
        }

        /// <summary>
        /// 根据文件扩展名获取 MIME 类型。
        /// Gets the MIME type based on file extension.
        /// </summary>
        /// <param name="filePath">文件的路径。Path of the file.</param>
        /// <returns>返回文件的 MIME 类型。Returns the MIME type of the file.</returns>
        private static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".tif":
                case ".tiff":
                    return "image/tiff";
                case ".ico":
                    return "image/x-icon";
                default:
                    throw new NotSupportedException("不支持的文件类型。Unsupported file type.");
            }
        }
    }
}
