/// <summary>
///********************************************
/// Author ： Wesky
/// CreateTime ： 2024/7/16 11:49:47
/// Description ： EMA算法
///********************************************
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wesky.Net.OpenTools.Algorithms
{
    /// <summary>
    /// EMA Algorithm
    /// </summary>
    public class EMA
    {
        public static List<double> Ema(IEnumerable<double> input, int period)
        {
            var inputArray = input as double[] ?? input.ToArray(); // 确保转换一次，避免多次迭代
            var returnValues = new List<double>(inputArray.Length); // 预先设定容量，减少动态扩容的开销

            double multiplier = 2.0 / (period + 1);
            double initialSMA = inputArray.Take(period).Average(); // 只计算一次初始的 SMA

            returnValues.Add(initialSMA);

            double lastEMA = initialSMA; // 使用变量来存储上一个 EMA 值

            for (int i = period; i < inputArray.Length; i++)
            {
                double newEMA = (inputArray[i] - lastEMA) * multiplier + lastEMA;
                returnValues.Add(newEMA);
                lastEMA = newEMA; // 更新上一个 EMA 值
            }
            return returnValues;
        }
    }
}
