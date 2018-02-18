﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Aguacongas.Identity.Firebase
{
    public class FirebaseLookupNormalizer : ILookupNormalizer
    {
        private static readonly Regex regex = new Regex(@"\.|\/|\$|#|\[|\]|\@");
        public string Normalize(string key) => NormalizeKey(key);

        public static string NormalizeKey(string key) 
            => key != null ? regex.Replace(key, match => Uri.HexEscape(match.Value[0]))
                .Replace("%", "@")
                .ToUpperInvariant() 
            : null;
    }
}
