using System;
using System.Text;

namespace Ship_Game
{
    public enum LocalizationMethod
    {
        Id,      // Localization token id is used to look up the actual strings
        NameId,  // Localization via name id
        RawText, // No extra localization steps are done, this is RAW text
        Parse,   // Text is parsed for localization tags and replaced on demand
    }

    /// <summary>
    /// Text localization utility struct to lazily bind localized text to UI elements.
    /// This enables UI Text elements that adapt to language change
    /// </summary>
    public struct LocalizedText : IEquatable<LocalizedText>
    {
        public readonly int Id; // localization id
        public readonly string String; // could be RawText or Parseable text
        public readonly LocalizationMethod Method;

        public static readonly LocalizedText None = new LocalizedText();

        public LocalizedText(int id)
        {
            Id = id;
            String = null;
            Method = LocalizationMethod.Id;
        }

        public LocalizedText(GameText gameText)
        {
            Id = (int)gameText;
            String = null;
            Method = LocalizationMethod.Id;
        }
        
        // if LocalizationMethod.NameId then text is fetched via localization name
        // if LocalizationMethod.RawText then text is just pure raw text
        // if LocalizationMethod.Parse then text is evaluated dynamically for tokens
        public LocalizedText(string text, LocalizationMethod method)
        {
            Id = 0;
            String = text;
            Method = method;
        }

        public LocalizedText(int id, string text, LocalizationMethod method)
        {
            Id = id;
            String = text;
            Method = method;
        }

        // @note This will allow button.Text = 1;
        public static implicit operator LocalizedText(int id)
        {
            return new LocalizedText(id);
        }

        // @note This will allow button.Text = GameText.LoadSavedGame;
        public static implicit operator LocalizedText(GameText gameText)
        {
            return new LocalizedText(gameText);
        }

        // @note This will allow button.Text = "my raw string";
        public static implicit operator LocalizedText(string text)
        {
            return new LocalizedText(text, LocalizationMethod.RawText);
        }

        public bool IsEmpty  => Id == 0 && String.IsEmpty();
        public bool NotEmpty => Id > 0 || String.NotEmpty();
        public bool IsValid  => Id > 0 || String.NotEmpty();

        public static bool operator==(in LocalizedText a, in LocalizedText b)
        {
            return a.Id == b.Id && a.String == b.String && a.Method == b.Method;
        }

        public static bool operator!=(in LocalizedText a, in LocalizedText b)
        {
            return a.Id != b.Id || a.String != b.String || a.Method != b.Method;
        }

        public bool Equals(LocalizedText other) // auto-generated by ReSharper
        {
            return Id == other.Id && String == other.String && Method == other.Method;
        }

        public override bool Equals(object obj) // auto-generated by ReSharper
        {
            return obj is LocalizedText other && Equals(other);
        }

        public override int GetHashCode() // auto-generated by ReSharper
        {
            unchecked
            {
                int hashCode = Id;
                hashCode = (hashCode * 397) ^ (String != null ? String.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Method;
                return hashCode;
            }
        }

        
        public override string ToString()
        {
            switch (Method)
            {
                case LocalizationMethod.Id:      return "ID/"+(GameText)Id+"/: \""+Text+"\"";
                case LocalizationMethod.NameId:  return "NAMEID/"+Text+"/: \""+Text+"\"";
                case LocalizationMethod.RawText: return "RAW: \""+Text+"\"";
                case LocalizationMethod.Parse:   return "PARSED: \""+Text+"\"";
            }
            return "None";
        }

        // @warning This can be expensive if LocalizationMethod is set to Parse
        public string Text
        {
            get
            {
                switch (Method)
                {
                    default:                         return "";
                    case LocalizationMethod.Id:      return Localizer.Token(Id); // super fast
                    case LocalizationMethod.NameId:  return Localizer.Token(String); // moderate lookup cost
                    case LocalizationMethod.RawText: return String; // super fast
                    case LocalizationMethod.Parse:   return ParseText(String); // very slow
                }
            }
        }

        // Creates a new lazy-initialized parseable text
        // Example usage: new UILabel(LocalizedText.Parse("{RaceNameSingular}: "));
        public static LocalizedText Parse(string parseableText)
        {
            return new LocalizedText(parseableText, LocalizationMethod.Parse);
        }

        // @note This cache speeds up all text parsing ~8x by sacrificing ~1MB of memory
        static readonly Map<string, string> ParseCache = new Map<string, string>();

        public static void ClearCache()
        {
            ParseCache.Clear();
        }

        /**
         * Parse incoming text as localized text.
         * Example:
         * "  {80}: {81}"  -- parsed to '  Astronomers: Races that have extensively studied......'
         * "\{ {80} \}"    -- parsed to '{ Astronomers }'
         * "\"{80}\""      -- parsed to '"Astronomers"'
         * "{RaceNameSingular}: " -- parse to '"Race Name Singular"'
         * If there are no parentheses, then the text is not parsed!!
         */
        public static string ParseText(string text)
        {
            if (text.IsEmpty())
            {
                return "";
            }

            if (ParseCache.TryGetValue(text, out string parsed))
                return parsed;

            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; ++i)
            {
                char c = text[i];
                if (c == '{')
                {
                    int j = i+1;
                    for (; j < text.Length-1; ++j)
                        if (text[j] == '}')
                            break;
                    if (j >= text.Length)
                    {
                        Log.Warning($"Missing localization format END character '}}'! -- LocalizedText not parsed correctly!: {text}");
                        break;
                    }

                    string idString = text.Substring(i+1, (j - i)-1);
                    if (char.IsDigit(idString[0]))
                    {
                        if (!int.TryParse(idString, out int id))
                        {
                            Log.Error($"Failed to parse localization id: {idString}! -- LocalizedText not parsed correctly: {text}");
                            continue;
                        }
                        sb.Append(Localizer.Token(id));
                    }
                    else if (char.IsLetter(idString[0]))
                    {
                        if (!Localizer.Token(idString, out string parsedText))
                        {
                            Log.Error($"Failed to parse localization id: {idString}! -- LocalizedText not parsed correctly: {text}");
                            continue;
                        }
                        sb.Append(parsedText);
                    }
                    else
                    {
                        Log.Error($"Failed to parse localization id: {idString}! -- LocalizedText not parsed correctly: {text}");
                        continue;
                    }

                    i = j;
                }
                else if (c == '\\') // escape character
                {
                    c = text[i++];
                    if      (c == '{') sb.Append('{');
                    else if (c == '}') sb.Append('}');
                    else sb.Append('\\').Append(c); // unrecognized
                }
                else
                {
                    sb.Append(c); // normal char
                }
            }

            string parsedResult = sb.ToString();
            ParseCache[text] = parsedResult;
            return parsedResult;
        }
    }
}