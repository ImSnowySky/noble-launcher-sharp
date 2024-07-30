using Force.Crc32;
using NobleLauncher.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace NobleLauncher.Models
{
    static class HashCalculator
    {
        private static readonly int MAX_BUFFER_SIZE = 262144; //256 kb;
        public static string CalcCRC32Hash(IUpdateable patch, Action<long> OnBlockRead) {
            uint hash = 0u;
            var buffer = new byte[MAX_BUFFER_SIZE];

            if (!File.Exists(patch.FullPath)) {
                return "";
            }

            using (var f = File.OpenRead(patch.FullPath)) {
                int count = 0;
                while ((count = f.Read(buffer, 0, buffer.Length)) != 0) {
                    hash = Crc32Algorithm.Append(hash, buffer, 0, count);
                    OnBlockRead(count);
                }
            }
            System.Console.WriteLine(patch.LocalPath);
            return hash.ToString("X8");
        }
    }
}
