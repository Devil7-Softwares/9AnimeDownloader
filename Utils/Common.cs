using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Devil7.Utils.Automation.NineAnimeDownloader.Utils
{
    public static class Common
    {
        public static string ArrayToRangeString(string[] stringArray)
        {
            List<string> rangeArray = new List<string>();

            string fromStr = null;
            string toStr = null;
            int fromInt = -1;
            int toInt = -1;

            foreach (string value in stringArray)
            {
                int valueInt;
                if (int.TryParse(value, out valueInt))
                {
                    if (fromStr == null)
                    {
                        fromStr = value;
                        fromInt = valueInt;
                    }
                    else
                    {
                        if (toStr == null)
                        {
                            if ((fromInt + 1) == valueInt)
                            {
                                toStr = value;
                                toInt = valueInt;
                            }
                            else
                            {
                                rangeArray.Add(string.Format("Episode {0}", fromStr));
                                rangeArray.Add(string.Format("Episode {0}", value));
                                fromStr = null;
                                fromInt = -1;
                            }
                        }
                        else
                        {
                            if ((toInt + 1) == valueInt)
                            {
                                toStr = value;
                                toInt = valueInt;
                            }
                            else
                            {
                                if ((toInt - fromInt) == 1)
                                {
                                    rangeArray.Add(string.Format("Episode {0}", fromStr));
                                    rangeArray.Add(string.Format("Episode {0}", toStr));
                                    rangeArray.Add(string.Format("Episode {0}", value));
                                    fromStr = null;
                                    toStr = null;
                                    fromInt = -1;
                                    toInt = -1;
                                }
                                else
                                {
                                    rangeArray.Add(string.Format("Episode {0} to Episode {1}\t({2})", fromStr, toStr, ((toInt - fromInt) + 1)));
                                    fromStr = value;
                                    fromInt = valueInt;
                                    toStr = null;
                                    toInt = -1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (fromStr != null)
                    {
                        if (toStr == null)
                        {
                            rangeArray.Add(string.Format("Episode {0}", fromStr));
                            fromStr = null;
                            fromInt = -1;
                        }
                        else
                        {
                            if ((toInt - fromInt) == 1)
                            {
                                rangeArray.Add(string.Format("Episode {0}", fromStr));
                                rangeArray.Add(string.Format("Episode {0}", toStr));
                                fromStr = null;
                                toStr = null;
                                fromInt = -1;
                                toInt = -1;
                            }
                            else
                            {
                                rangeArray.Add(string.Format("Episode {0} to Episode {1}\t({2})", fromStr, toStr, ((toInt - fromInt) + 1)));
                                fromStr = null;
                                toStr = null;
                                fromInt = -1;
                                toInt = -1;
                            }
                        }
                    }
                    rangeArray.Add(value);
                }
            }
            if (fromStr != null)
            {
                if (toStr == null)
                {
                    rangeArray.Add(string.Format("Episode {0}", fromStr));
                }
                else
                {
                    if ((toInt - fromInt) == 1)
                    {
                        rangeArray.Add(string.Format("Episode {0}", fromStr));
                        rangeArray.Add(string.Format("Episode {0}", toStr));
                    }
                    else
                    {
                        rangeArray.Add(string.Format("Episode {0} to Episode {1}\t({2})", fromStr, toStr, ((toInt - fromInt) + 1)));
                    }
                }
            }

            return string.Format("\t{0}", string.Join("\n\t", rangeArray));
        }

        public static string[] SingleLineStringToStrings(string stringValue, string[] availableValues)
        {
            List<string> values = new List<string>();

            foreach (string str in stringValue.Split(','))
            {
                string value = str.Trim();
                
                if (string.IsNullOrEmpty(value))
                    continue;

                if (value.Contains('-'))
                {
                    try
                    {
                        string[] rangeValues = value.Split('-');
                        if (rangeValues.Length != 2)
                        {
                            Console.Error.WriteLine("Invalid Range '{0}'. Expected 2 Values!", value);
                            continue;
                        }

                        int rangeFrom = -1;
                        int rangeTo = -1;
                        if (!(int.TryParse(rangeValues[0], out rangeFrom) && int.TryParse(rangeValues[1], out rangeTo)))
                        {
                            Console.Error.WriteLine("Invalid Range '{0}'. Range Boundaries Must be a Number!", value);
                            continue;
                        }
                        if (rangeFrom > rangeTo)
                        {
                            Console.Error.WriteLine("Invalid Range '{0}'. From is Greater than To Value!", value);
                            continue;
                        }

                        string numberFormat = Regex.Replace(rangeValues[0].Trim(), "[^0]", "0");
                        foreach (int i in Enumerable.Range(rangeFrom, rangeTo))
                        {
                            string episode = i.ToString(numberFormat);
                            if (availableValues.Contains(episode))
                            {
                                values.Add(episode);
                            }
                            else
                            {
                                Console.Error.WriteLine("Episode {0} in Range '{1}' is Unavailable!", episode, value);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Invalid Range '{0}'. Failed to Parse!", value);
                    }
                }
                else
                {
                    if (availableValues.Contains(value))
                    {
                        values.Add(value);
                    }
                    else
                    {
                        Console.Error.WriteLine("Selected Episode {0} is Unavailable!", value);
                    }
                }
            }

            return values.ToArray();
        }
    }
}
