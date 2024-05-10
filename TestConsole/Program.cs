namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var password = "helloworld123";
            var key = "12345678901234567890123456789012";
            var iv = "1234567890123456";
            var encrypted = Wesky.Net.OpenTools.Security.AesCipher.AesEncrypt(key, password, iv);
            var decrypted = Wesky.Net.OpenTools.Security.AesCipher.AESDecrypt(key, encrypted, iv);
        }
    }
}
