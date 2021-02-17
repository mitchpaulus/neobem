using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace src
{
    public class IdfObjectCompare
    {
        /// <summary>
        /// Compare two idf files. Strategy is to:
        ///  1. Remove all comments
        ///  2. Split on semicolons
        ///  3. Split on commas
        ///  4. Check individual fields, case insensitively, trimming whitespace.
        ///
        /// Objects are required to be in the same order, and each corresponding object should have the same amount of fields
        /// </summary>
        /// <param name="idf1">File 1</param>
        /// <param name="idf2">File 2</param>
        /// <returns>Boolean indicating whether the files are equivalent.</returns>
        public static bool Equals(string idf1, string idf2)
        {
            var objectParts1 = ToObjectParts(idf1);
            var objectParts2 = ToObjectParts(idf2);

            if (objectParts1.Count != objectParts2.Count) return false;

            for (int i = 0; i < objectParts1.Count; i++)
            {
                var object1 = objectParts1[i];
                var object2 = objectParts2[i];

                if (object1.Count != object2.Count) return false;

                for (int j = 0; j < object1.Count; j++)
                {
                    if (!string.Equals(object1[j].Trim(), object2[j].Trim(), StringComparison.OrdinalIgnoreCase)) return false;
                }
            }

            return true;
        }

        public static List<List<string>> ToObjectParts(string text)
        {
            string commentsRemoved = RemoveComments(text);
            var splitOnSemicolon = commentsRemoved.Split(';').ToList();
            return splitOnSemicolon.Select(s => s.Split(',').ToList()).ToList();
        }

        public static string RemoveComments(string text) => Regex.Replace(text, "!.*$", "");
    }
}