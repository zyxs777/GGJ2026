using System;

namespace STool.EditorExtension.SEditorExtension.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryAttribute : Attribute
    {
        public string StartBit;
        public string Length;

        public bool Editable = false;
        public int TotalBits = 64;

        public BinaryAttribute(string startBit, string length)
        {
            StartBit = startBit;
            Length = length;
        }
    }
}