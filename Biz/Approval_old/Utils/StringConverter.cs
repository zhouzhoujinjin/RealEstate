using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Approval.Utils
{
  public static class StringConverterExtensions
  {
    public static string ConvertFirstLetter(this string word, LetterType type)
    {
      if (string.IsNullOrEmpty(word)) return "";
      if (word.Length == 1) return word;
      var firstLetter = type == LetterType.Lower ? word.Substring(0, 1).ToLower() : word.Substring(0, 1).ToUpper();
      var otherLetters = word.Substring(1);
      return firstLetter + otherLetters;
    }
  }
}
