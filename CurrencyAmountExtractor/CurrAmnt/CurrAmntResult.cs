using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CurrencyAmountExtractor.CurrAmnt
{
    /// <summary>
    /// Class that represent currency/amount value detected in a document.
    /// </summary>
    public class CurrAmntResult
    {
        /// <summary>
        /// that can be OK in case detected value has no errors or Error in case value contains erronated digits.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }

        /// <summary>
        /// original value detected in document
        /// </summary>
        public string OriginalValue { get; set; }

        /// <summary>
        /// calculated value from original. In case original value is correct supposed value will be the same.
        /// </summary>
        public string SupposedValue { get; set; }

        /// <summary>
        /// calculated accuracy of predicted value. In case suposed value is the same as original value accuracy is 1
        /// </summary>
        public float Accuracy { get; set; }

        /// <summary>
        /// start index of detected value.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// end index of detected value.
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// number of erronated digits from detected value. Is 0 in case original value has no errors.s
        /// </summary>
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
            var hashCode = 33288;
            hashCode = hashCode * OriginalValue.GetHashCode();
            hashCode = hashCode * StartIndex;
            hashCode = hashCode * EndIndex;
            return hashCode;
        }
    }
}
