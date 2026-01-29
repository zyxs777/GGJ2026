using System.Text;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using STool.EditorExtension.SEditorExtension.Attributes;
using UnityEditor;
using UnityEngine;

namespace STool.EditorExtension.Binary
{
    public class BinaryDrawer : OdinAttributeDrawer<BinaryAttribute, ulong>
    {
        private ValueResolver<int> _startResolver;
        private ValueResolver<int> _lengthResolver;

        protected override void Initialize()
        {
            _startResolver = ValueResolver.Get<int>(
                Property, Attribute.StartBit);

            _lengthResolver = ValueResolver.Get<int>(
                Property, Attribute.Length);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var start = _startResolver.GetValue();
            var len = _lengthResolver.GetValue();

            var value = ValueEntry.SmartValue;

            var total = Mathf.Clamp(Attribute.TotalBits, 1, 64);

            // 生成完整 64-bit 二进制字符串（MSB->LSB）
            var full = ToBinaryString(value, total); // length == total

            // full 是 [MSB ... LSB]，而 start 是从 LSB 数起
            // 要截取从 LSB start 开始的 len 位，对应 full 的区间：
            var msbIndex = total - 1 - start; // 该段起点在 full 中的“右侧”位置
            var left = msbIndex - (len - 1); // 左边界
            var slice = full.Substring(left, len);

            var displayLabel = label?.text ?? "Binary";

            if (!Attribute.Editable)
            {
                SirenixEditorFields.TextField(displayLabel, slice);
                return;
            }

            // 可编辑：只允许 0/1，长度固定
            EditorGUI.BeginChangeCheck();
            var newSlice = SirenixEditorFields.TextField(displayLabel, slice);
            if (EditorGUI.EndChangeCheck())
            {
                newSlice = Sanitize01(newSlice);

                if (newSlice.Length != len)
                {
                    // 长度不够就左侧补 0；太长就截断左侧（保留右侧更像输入低位）
                    if (newSlice.Length < len) newSlice = newSlice.PadLeft(len, '0');
                    else newSlice = newSlice.Substring(newSlice.Length - len, len);
                }

                // 把 slice 写回 value 的 [start, start+len-1] 区间
                var newValue = ReplaceBitRange(value, start, len, newSlice);
                ValueEntry.SmartValue = newValue;
            }
        }

        // 生成固定长度的二进制字符串（MSB->LSB）
        private static string ToBinaryString(ulong v, int bits)
        {
            var sb = new StringBuilder(bits);
            for (var i = bits - 1; i >= 0; i--) sb.Append(((v >> i) & 1UL) == 1UL ? '1' : '0');
            return sb.ToString();
        }

        private static string Sanitize01(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
                if (c == '0' || c == '1')
                    sb.Append(c);
            return sb.ToString();
        }

        // newBits 是 MSB->LSB 的字符串，长度 == len，对应写入到 [start .. start+len-1]（LSB 起算）
        private static ulong ReplaceBitRange(ulong original, int start, int len, string newBits)
        {
            // 把 newBits 解析为一个 len 位的 ulong
            ulong segment = 0;
            for (var i = 0; i < len; i++)
            {
                segment <<= 1;
                if (newBits[i] == '1') segment |= 1UL;
            }

            // mask 覆盖要替换的区域（len==64 特判避免 1UL<<64）
            var mask = len == 64 ? ulong.MaxValue : (1UL << len) - 1UL;
            mask <<= start;

            // 清空原区域 + 写入新区域
            var cleared = original & ~mask;
            var shiftedSegment = (segment << start) & mask;
            return cleared | shiftedSegment;
        }
    }
}