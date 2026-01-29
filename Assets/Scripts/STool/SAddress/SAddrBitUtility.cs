using System;
using System.Collections.Generic;

namespace STool.SAddress
{
    public static class SAddrBitUtility
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool MatchByMask(ulong a, ulong b, ulong mask)
        {
            return (a & mask) == (b & mask);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ulong MergeAddr(ulong a, ulong b)
        {
            return a | b;
        }
        
        public static ulong MakeMask(int x, int w)
        {
            if (x is < 0 or > 63) throw new ArgumentOutOfRangeException(nameof(x));
            if (w is <= 0 or > 64) throw new ArgumentOutOfRangeException(nameof(w));
            if (x + w > 64) throw new ArgumentException("x + w must be <= 64");

            // 生成 w 个 1（未移位）
            var baseMask = w == 64 ? ulong.MaxValue : (1UL << w) - 1UL;

            // 左移到目标偏移
            return baseMask << x;
        }

        private static readonly HashSet<ulong> AddrBucket = new();
        public static void AllocateNewAddr(ICollection<ISAddressable> addresses, int x, int w)
        {
            var startAddr = MakeMask(x, 1);
            var endAddr = MakeMask(x, w);
            var curAddr = startAddr;
            
            AddrBucket.Clear();
            // foreach (var iAddr in addresses) AddrBucket.Add(iAddr.Address);
            
            //尝试分配Addr
            foreach (var iAddr in addresses)
            {
                // if (iAddr.Address != 0) continue;
                while (AddrBucket.Contains(curAddr)) curAddr += startAddr;
                iAddr.Address = curAddr;
                AddrBucket.Add(iAddr.Address);
            }
        }
    }
}
