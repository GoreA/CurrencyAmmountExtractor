using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CurrencyAmountExtractor.CurrAmnt
{
    public class CurrAmntResult
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }

        public string OriginalValue { get; set; }

        public string SupposedValue { get; set; }

        public float Accuracy { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public int Distance { get; set; }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                CurrAmntResult dr = (CurrAmntResult)obj;
                return (OriginalValue.Equals(dr.OriginalValue)
                    && StartIndex == dr.StartIndex
                    && EndIndex == dr.EndIndex);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
