using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace base_converter
{
    class Program
    {
        private static readonly char[] AcceptableBaseChars;

        private static readonly int MaxBaseAllowed = 28;

        private static bool Periodic = false;

        private static readonly Dictionary<char, long> CharactersToDigits = new Dictionary<char, long>();

        static Program()
        {
            List<char> acceptableBaseChars = new List<char> { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r' };
            acceptableBaseChars.AddRange(new string(acceptableBaseChars.ToArray()).ToUpper());
            acceptableBaseChars.AddRange(new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '(', ')' });
            AcceptableBaseChars = acceptableBaseChars.ToArray();

            for (int i = 0; i < MaxBaseAllowed; ++i)
            {
                if (i < 10)
                {
                    char correspondingChar = (char)('0' + i);
                    CharactersToDigits.Add(correspondingChar, i);
                }
                else
                {
                    char correspondingChar = (char)('a' + (i - 10));
                    CharactersToDigits.Add(correspondingChar, i);
                    CharactersToDigits.Add(Char.ToUpper(correspondingChar), i);
                }
            }
        }

        static void Main(string[] args)
        {
            KeyValuePair<string, KeyValuePair<int, int>> kp = ProcessUserInput();

            int startBase = kp.Value.Key;

            int toBase = kp.Value.Value;

            string numString = kp.Key;

            if (numString.Contains("("))
            {
                string[] splited = numString.Split('(', ')');
                string period = splited[1];
                numString = numString.Remove(numString.IndexOf('('));
                numString = numString + period + period + period + period;
                Periodic = true;
            }
            else
            {
                numString = ChangePrecisionForBase(numString, startBase);
            }

            Console.WriteLine();

            KeyValuePair<long, KeyValuePair<long, long>> baseTenValue = ConvertToDecimal(startBase, numString);

            string result = ConvertFromDecimal(toBase, baseTenValue);

            Console.WriteLine(result);

            Console.ReadKey();
        }



        private static KeyValuePair<string, KeyValuePair<int, int>> ProcessUserInput()
        {
            bool validNumber = false;
            bool validFromBase = false;
            bool validToBase = false;

            string number = "";
            int fromBase = -1;
            int toBase = -1;

            while (!validNumber)
            {

                Console.Write("Introduceti numarul: ");
                string input = Console.ReadLine();
                List<int> allowedNumberOfParantheses = new List<int> { 0, 1 };

                if (input.Where((c) => !AcceptableBaseChars.Contains(c)).Count() == 0 && input.Where((c) => c == '.').Count() <= 1
                        && !input.EndsWith(".") &&
                        ((input.Where((c) => c == '(').Count() == 0 && input.Where((c) => c == ')').Count() == 0) ||
                        ((input.Where((c) => c == '(').Count() == 1 && input.Where((c) => c == ')').Count() == 1) && input.Contains('.')))
                        && ((input.IndexOf('(') == -1 && input.IndexOf(')') == -1) || ((input.IndexOf('(') + 1) < input.IndexOf(')')))
                        && input.IndexOf('.') < ((input.IndexOf('(') == -1) ? input.Length : input.IndexOf('('))
                    )
                {
                    validNumber = true;
                    number = input;
                }
                else
                {
                    Console.WriteLine("Introduceti un numar valid!");
                }

            }


            while (!validFromBase)
            {
                Console.Write("Introduceti baza numarului introdus mai devreme: ");
                string input = Console.ReadLine();
                int baseValue;

                try
                {
                    baseValue = Int32.Parse(input);
                    if (baseValue > MaxBaseAllowed || baseValue < 2)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    validFromBase = true;
                    fromBase = baseValue;
                }
                catch (Exception)
                {
                    Console.WriteLine("Introduceti o baza valida!");
                    validFromBase = false;
                }
            }

            while (!validToBase)
            {
                Console.Write("Introduceti baza in care doriti sa convertiti: ");
                string input = Console.ReadLine();
                int baseValue;

                try
                {
                    baseValue = Int32.Parse(input);
                    if (baseValue > MaxBaseAllowed || baseValue < 2)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    validToBase = true;
                    toBase = baseValue;
                }
                catch (Exception)
                {
                    Console.WriteLine("Introduceti o baza valida!");
                    validToBase = false;
                }
            }

            return new KeyValuePair<string, KeyValuePair<int, int>>(number, new KeyValuePair<int, int>(fromBase, toBase));
        }





        private static KeyValuePair<long, KeyValuePair<long, long>> ConvertToDecimal(int fromBase, string numberString)
        {
            char sign = numberString[0];

            if (sign != '+' && sign != '-')
            {
                sign = '+';
            }
            else
            {
                numberString = numberString.Remove(0, 1);
            }

            numberString = numberString.TrimStart('0');

            if (fromBase == 10)
            {
                string[] splitedString = numberString.Split('.');
                int integerPart = Int32.Parse(splitedString[0] == string.Empty ? "0" : splitedString[0]);
                if (splitedString.Length == 1)
                {
                    return new KeyValuePair<long, KeyValuePair<long, long>>(integerPart, new KeyValuePair<long, long>(0, 0));
                }
                KeyValuePair<long, long> fractionPart = ExpressAsDecimalFraction(splitedString[1], 10);
                return new KeyValuePair<long, KeyValuePair<long, long>>(integerPart, fractionPart);
            }




            long integerValue = 0;

            KeyValuePair<long, long> fractionalPart = new KeyValuePair<long, long>();

            int currentPower = numberString.Contains('.') ? numberString.IndexOf('.') - 1 : numberString.Length - 1;

            for (int i = 0; i < numberString.Length; ++i)
            {
                if (numberString[i] == '.')
                {
                    continue;
                }

                long digitValue = CharactersToDigits[numberString[i]];

                if (currentPower >= 0)
                {
                    integerValue += digitValue * (long)Math.Pow(fromBase, currentPower);
                }
                else
                {
                    fractionalPart = ExpressAsDecimalFraction(numberString.Split('.')[1], fromBase);
                    break;
                }

                --currentPower;
            }

            return new KeyValuePair<long, KeyValuePair<long, long>>(integerValue, fractionalPart);
        }




        private static KeyValuePair<long, long> ExpressAsDecimalFraction(string fractionalPartString, int baseValue)
        {
            long lowerPart = (long)Math.Pow(baseValue, fractionalPartString.Length);

            long upperPart = 0;

            for (int i = 1; i <= fractionalPartString.Length; ++i)
            {
                long digitValue = CharactersToDigits[fractionalPartString[i - 1]];
                upperPart += digitValue * (long)Math.Pow(baseValue, (fractionalPartString.Length - i));
            }

            long commonDivisor = GreatestCommonDivider(upperPart, lowerPart);

            return new KeyValuePair<long, long>(upperPart / commonDivisor, lowerPart / commonDivisor);
        }

        private static long GreatestCommonDivider(long a, long b)
        {
            return b == 0 ? a : GreatestCommonDivider(b, a % b);
        }


        private static string ChangePrecisionForBase(string numberString, int baseValue)
        {
            /*if (numberString.Contains('.'))
            {
                string integerPart = numberString.Split('.')[0];
                string fractionalPart = numberString.Split('.')[1];
                
                if(fractionalPart.Length >= 5)
                {
                    if(baseValue >= 4)
                    {
                        fractionalPart = fractionalPart.Remove(9); //only need 10 decimal digits for all bases greater than 4 to maintain 6 decimal digit precision in base 10
                    }
                    else
                    {
                        fractionalPart = fractionalPart.Remove(19); //for base 2 and 3, 20 decimal digits should do
                    }
                }

                return integerPart + "." + fractionalPart;
            }*/

            return numberString;
        }



        private static string ConvertFromDecimal(int targetBase, KeyValuePair<long, KeyValuePair<long, long>> baseTenNumber)
        {
            long baseTenIntegerpart = baseTenNumber.Key;
            StringBuilder integerPartBuilder = new StringBuilder();
            long integerPart = 0;

            while (baseTenIntegerpart != 0)
            {
                long remainder = baseTenIntegerpart % targetBase;
                integerPartBuilder.Append(remainder);
                baseTenIntegerpart = baseTenIntegerpart / targetBase;
            }

            integerPart = baseTenIntegerpart == 0 ? 0 : Int64.Parse(new string(integerPartBuilder.ToString().Reverse().ToArray()));

            if (baseTenNumber.Value.Key == 0)
            {
                return integerPart.ToString();
            }

            StringBuilder decimalPartBuilder = new StringBuilder();

            double decimalPart = (double)baseTenNumber.Value.Key / baseTenNumber.Value.Value;

            double auxDecimalPart = decimalPart;
            int tries = 0;

            while (auxDecimalPart != 0D && tries < 24)
            {
                auxDecimalPart = auxDecimalPart * targetBase;
                decimalPartBuilder.Append(Math.Truncate(auxDecimalPart));
                string decimalStringPart = auxDecimalPart.ToString("0.0000000000000000", CultureInfo.InvariantCulture);
                decimalStringPart = decimalStringPart.Remove(0, 1);
                decimalStringPart = "0" + decimalStringPart;
                decimalStringPart = decimalStringPart.TrimEnd('0');
                auxDecimalPart = Double.Parse(decimalStringPart, CultureInfo.InvariantCulture);
                ++tries;
            }

            string decimalPartString = decimalPartBuilder.ToString();

            if (auxDecimalPart != 0D || Periodic)
            {
                int i = 2;
                string longestSequenceFound = "";
                int startIndex = 0;
                while (longestSequenceFound == "")
                {
                    while (i <= decimalPartString.Length / 2)
                    {
                        string sequence = decimalPartString.Substring(startIndex, i);
                        if (decimalPartString.Replace(sequence, "").Length < sequence.Length)
                        {
                            longestSequenceFound = sequence;
                        }
                        ++i;
                    }

                    ++startIndex;
                    if (startIndex > decimalPartString.Length)
                    {
                        longestSequenceFound = decimalPartString.Substring(0, 4);
                        startIndex = 0;
                    }
                }
                decimalPartString = decimalPartString.Remove(startIndex);
                char c = Char.Parse("" + (targetBase - 1));
                string endTrimmerSequence = longestSequenceFound.TrimEnd(c);
                decimalPartString = decimalPartString + "(" + endTrimmerSequence + ")";
            }

            return integerPart.ToString() + "." + decimalPartString;

        }









    }
}
