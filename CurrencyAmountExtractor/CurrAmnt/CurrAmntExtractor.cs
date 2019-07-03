using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CurrencyAmountExtractor.CurrAmnt
{
    /// <summary>
    /// Class with main logic for finding and extracting currency/amount values from a given string.
    /// </summary>
    public class CurrAmntExtractor
    {
        private string _stringToScan;
        private int _distance;
        private Dictionary<string, string> _leetDict;

        /// <summary>
        /// Constructor of CurrAmntExtractor.
        /// </summary>
        /// <param name="stringToScan">A string to be scaned for detecting currency/amount</param>
        /// <param name="distance">Maximal admited numer of wrong letters in detected currency/amount</param>
        public CurrAmntExtractor(string stringToScan, int distance)
        {
            if (string.IsNullOrEmpty(stringToScan))
            {
                throw new System.ArgumentException("StringToScan parameter cannot be null or empty", stringToScan);
            }
            else
                _stringToScan = stringToScan;

            if (distance < 0)
            {
                throw new System.ArgumentException("Distance must be equal or greater than 0");
            }
            else
                _distance = distance;

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToLeet = Path.Combine(currentDirectory, @"Resources/Leet.txt");

            _leetDict = Leet.GetLetterDigitDictionary(pathToLeet);
        }

        /// <summary>
        /// Determines possible values of currency/amount of money indicated in scaned string.
        /// </summary>
        /// <returns> a list of $(CurrAmntResult) objects.</returns>
        public List<CurrAmntResult> DetectCurrencyAmount()
        {
            List<string> wordRegexes = GetWordRegexes();
            List<CurrAmntResult> currAmntResults = GetCurrAmntResultListFromWordRegEx(wordRegexes);
            return currAmntResults;
        }

        /// <summary>
        /// Determines possible values of currency/amount of money indicated in scaned string.
        /// </summary>
        /// <returns>a JSON object with all detected $(CurrAmntResult)s in document</returns>
        public string DetectJSONCurrencyAmount()
        {
            List<CurrAmntResult> currAmntResults = DetectCurrencyAmount();
            return JsonConvert.SerializeObject(currAmntResults);
        }

        private List<string> GetWordRegexes()
        {
            List<string> regexes = new List<string>();
            regexes.Add("[\\+\\-]?[\\s]*[$£€¢₽¥₹][\\s]*[\\d\\w]+[\\s]*([\\,][\\s]*[\\d\\w]{3}[\\s]*)*(\\.[\\s]*[\\d\\w]{1,2})?" +
                "|[\\+\\-]?[\\s]*[\\d\\w]+[\\s]*([\\,][\\s]*[\\d\\w]{3}[\\s]*)*(\\.[\\s]*[\\d\\w]{1,2})" +
                "|[\\+\\-]?[\\s]*[$£€¢₽¥₹]?[\\s]*[\\d\\w]+[\\s]*([\\,][\\s]*[\\d\\w]{3}[\\s]*)+");
            return regexes;
        }

        private List<CurrAmntResult> GetCurrAmntResultListFromWordRegEx(List<string> wordRegexes)
        {
            List<CurrAmntResult> currAmntResults = new List<CurrAmntResult>();
            foreach (string wordRegex in wordRegexes)
            {
                currAmntResults = GetCurrAmntResults(currAmntResults, wordRegex);
            }
            return currAmntResults;
        }

        private List<CurrAmntResult> GetCurrAmntResults(List<CurrAmntResult> currAmntResults, string wordRegex)
        {
            Regex regex = new Regex(@wordRegex);
            MatchCollection matchCollection = regex.Matches(_stringToScan);
            foreach (Match match in matchCollection)
            {
                CurrAmntResult currAmntResult = GetCurrAmntResultFromWord(match);
                if (currAmntResult != null)
                {
                    currAmntResults.Add(currAmntResult);
                }
            }
            return currAmntResults;
        }

        private CurrAmntResult GetCurrAmntResultFromWord(Match match)
        {
            Tuple<int, float, string> distance_accuracy_supposedValue;
            distance_accuracy_supposedValue = GetDistanceAccuracySupposedValue(match.Value);
            if (distance_accuracy_supposedValue.Item1 <= _distance)
            {
                return CreateCurrAmntResult(match, distance_accuracy_supposedValue.Item1, distance_accuracy_supposedValue.Item2,
                    distance_accuracy_supposedValue.Item3);
            }
            else return null;
        }

        private Tuple<int, float, string> GetDistanceAccuracySupposedValue(string value)
        {

            string sign;
            string currency;
            Tuple<int, string, int, int> coins_distance_valueToDetec;
            Tuple<int, string, int, int> ammount_distance_valueToDetec;

            value = Regex.Replace(value, @"\s+", "");

            if ("+-".Contains(value[0].ToString()))
            {
                sign = value[0].ToString();
                value = value.Substring(1);
            }
            else
            {
                sign = "+";
            }

            if ("$£€¢₽¥₹".Contains(value[0].ToString()))
            {
                currency = value[0].ToString();
                value = value.Substring(1);
            }
            else
            {
                currency = "$";
            }

            // It is supposed that indicated ammount will have maximum 2 decimals.
            if (value.Substring(value.Length - 3).Contains(".") || value.Substring(value.Length - 3).Contains(","))
            {
                if (".,".Contains(value.Substring(value.Length - 3)[0].ToString()))
                {
                    coins_distance_valueToDetec = GetAmmCoinsDistanceSupposedValueWrongLetters(value.Substring(value.Length - 2));
                    value = value.Substring(0, value.Length - 3);
                }
                else
                {
                    if (".,".Contains(value.Substring(value.Length - 2)[0].ToString()))
                    {
                        coins_distance_valueToDetec = GetAmmCoinsDistanceSupposedValueWrongLetters(value.Substring(value.Length - 1));
                        value = value.Substring(0, value.Length - 2);
                    }
                    else
                    {
                        coins_distance_valueToDetec = new Tuple<int, string, int, int>(0, "00", 0, 0);
                    }
                }
            }
            else
            {
                coins_distance_valueToDetec = new Tuple<int, string, int, int>(0, "00", 0, 0);
            }

            ammount_distance_valueToDetec = GetAmmCoinsDistanceSupposedValueWrongLetters(Regex.Replace(value, @"[\,\.]", ""));

            int finalDistance = coins_distance_valueToDetec.Item1 + ammount_distance_valueToDetec.Item1;
            string finalValue = sign + currency + ammount_distance_valueToDetec.Item2 + "." + coins_distance_valueToDetec.Item2;
            float accuracy = CalculateAccuracy(ammount_distance_valueToDetec.Item2 + coins_distance_valueToDetec.Item2,
                                                ammount_distance_valueToDetec.Item3 + coins_distance_valueToDetec.Item3,
                                                ammount_distance_valueToDetec.Item4 + coins_distance_valueToDetec.Item4);
            return new Tuple<int, float, string>(finalDistance, accuracy, finalValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>a tuple that contains 4 values
        /// <list type="bullet">
        /// <item>
        /// <term>distance</term>
        /// <description>calculated distance</description>
        /// </item>
        /// <item>
        /// <term>newValue</term>
        /// <description>calculated new value</description>
        /// </item>
        /// <item>
        /// <term>leetLetters</term>
        /// <description>number of wrong letters from given value that are present in leet dictionary</description>
        /// </item>
        /// <item>
        /// <term>simpleLetters</term>
        /// <description>number of wrong letters from given value that are present in leet dictionary</description>
        /// </item>
        /// </list>
        /// </returns>
        private Tuple<int, string, int, int> GetAmmCoinsDistanceSupposedValueWrongLetters(string value)
        {
            int distance = 0;
            int leetLetters = 0;
            int simpleLetters = 0;

            string newValue = string.Empty;
            foreach (char digit in value)
            {
                if (!Char.IsDigit(digit))
                {
                    distance++;
                    var exist = _leetDict.TryGetValue(digit.ToString(), out string dictDigit);
                    if (exist)
                    {
                        newValue += dictDigit;
                        leetLetters++;
                    }
                    else
                    {
                        // If erronated letter is not present in leet dictionary default value will be 0 as the most used digit.
                        newValue += "0";
                        simpleLetters++;
                    }
                }
                else
                {
                    newValue += digit.ToString();
                }
            }
            return new Tuple<int, string, int, int>(distance, newValue, leetLetters, simpleLetters);
        }

        /// <summary>
        /// Calculates accuracy of predicted value in dependence of leet letters and other letters which value is not applied to any rules.
        /// Possible values are between 0 and 1. The maximal value is 1 in case the number was detected without any errors. 
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="leetLettersCount"></param>
        /// <param name="simpleLettersCount"></param>
        /// <returns>calculated accuracy of float type</returns>
        private float CalculateAccuracy(string currentValue, int leetLettersCount, int simpleLettersCount)
        {
            return (float)(currentValue.Count() - leetLettersCount * 0.1 - simpleLettersCount * 0.95) / currentValue.Count();
        }

        private CurrAmntResult CreateCurrAmntResult(Match match, int distance, float accuracy, string supposedValue)
        {
            CurrAmntResult currAmntResult = new CurrAmntResult();
            currAmntResult.Status = distance > 0 ? Status.Error : Status.OK;
            currAmntResult.Distance = distance;
            currAmntResult.OriginalValue = Regex.Replace(match.Value, @"\s+", " ");
            currAmntResult.SupposedValue = supposedValue;
            currAmntResult.Accuracy = accuracy;
            currAmntResult.StartIndex = match.Index + 1;
            currAmntResult.EndIndex = match.Index + match.Length - 2;

            return currAmntResult;
        }
    }
}
