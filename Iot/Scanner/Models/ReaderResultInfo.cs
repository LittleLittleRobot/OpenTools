namespace Wesky.Net.OpenTools.Iot.Scanner.Models
{
    /// <summary>
    /// Represents the result information from a scanner.
    /// 表示扫描器的结果信息。
    /// </summary>
    public class ReaderResultInfo
    {
        /// <summary>
        /// Indicates whether the scan was successful.
        /// 指示扫描是否成功。
        /// </summary>
        public bool IsSucceed { get; set; } = false;

        /// <summary>
        /// The error message if the scan failed.
        /// 如果扫描失败，错误信息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The result of the scan.
        /// 扫描结果。
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The time taken for the scan in milliseconds.
        /// 扫描所耗费的时间（毫秒）。
        /// </summary>
        public long ElapsedMilliseconds { get; set; } = 0;

        /// <summary>
        /// The number identifying the scanner.
        /// 扫描器编号。
        /// </summary>
        public ushort ReaderNo { get; set; } = 0;

        /// <summary>
        /// The brand of the scanner.
        /// 扫描器品牌。
        /// </summary>
        public string Brand { get; set; } = string.Empty;
    }

}
