using System;
using System.Collections.Generic;
using System.Linq;

namespace WuManberNet
{
	public class WuManber
	{
		struct Alphabet
		{
			public char Letter;
			public char Offset;
		}

		struct PatternMap
		{ // one struct per pattern for this hash
			public int PrefixHash;  // hash of first two characters of the pattern
			public int Index;  // index into patterns for final comparison
		}

		private Alphabet[] m_lu = new Alphabet[256];
		private List<WordMatch> _patterns;
		private int k = 0;  // number of patterns;
		private int m = 0;  // largest common pattern length
		private const int B = 3;  // Wu Manber paper suggests B is 2 or 3 
								  // small number of patterns, use B=2, use an exact table
								  // for large number of patterns, use B=3 use compressed table (their code uses 400 as a cross over )
								  // this class needs to be adjusted for B=2 (in the build shift table portion)
		private static char[] rchExtendedAscii = new[]
		{
			0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f,
			0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97,       0x99, 0x9a,       0x9c, 0x0d,       0x9f,
			0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5,
			0x00
		}.Select(a => Convert.ToChar(a)).ToArray();

		private static char[] rchSpecialCharacters = new[]{ 0x21, 0x22, 0x23, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29,
			0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40, 0x5b, 0x5c, 0x5d,
			0x5e, 0x5f, 0x60, 0x7b, 0x7c, 0x7d, 0x7e,
			0x00
		}.Select(a => Convert.ToChar(a)).ToArray();

		private bool m_bInitialized = false;
		private char m_nSizeOfAlphabet;
		private short m_nBitsInShift;
		private int m_nTableSize;
		private int[] m_ShiftTable;
		private Dictionary<int, List<PatternMap>> m_vPatternMap = new Dictionary<int, List<PatternMap>>();

		public void Initialize(List<WordMatch> patterns, bool bCaseSensitive = false, bool bIncludeSpecialCharacters = false, bool bIncludeExtendedAscii = false)
		{
			_patterns = patterns;
			k = patterns.Count;
			m = 0; // start with 0 and grow from there
			for (var i = 0; i < k; ++i)
			{
				var lenPattern = patterns[i].Word.Length;
				if (B > lenPattern)
					throw new Exception("found pattern less than B in length");
				m = (0 == m) ? lenPattern : Math.Min(m, lenPattern);
			}

			m_nSizeOfAlphabet = (char)1; // at minimum we have a white space character
			for (var i = 0; i <= 255; ++i)
			{
				m_lu[i].Letter = ' '; // table is defaulted to whitespace
				m_lu[i].Offset = (char)0;  // 
				if ((i >= 'a') && (i <= 'z'))
				{
					m_lu[i].Letter = (char)i; // no problems with lower case letters
					m_lu[i].Offset = m_nSizeOfAlphabet++;
				}
				if (bCaseSensitive)
				{
					// case of !bCaseSensitive fixed up later on
					if ((i >= 'A') && (i <= 'Z'))
					{
						m_lu[i].Letter = (char)i; // map upper case to lower case
						m_lu[i].Offset = m_nSizeOfAlphabet++;
					}
				}
				if ((i >= '0') && (i <= '9'))
				{
					m_lu[i].Letter = (char)i; // use digits
					m_lu[i].Offset = m_nSizeOfAlphabet++;
				}
			}

			if (!bCaseSensitive)
			{
				// fix up upper case mappings ( uppercase comes before lower case in ascii table )
				for (var i = (short)'A'; i <= 'Z'; ++i)
				{
					char letter = (char)(i - (short)'A' + (short)'a');  // map upper case to lower case
					m_lu[i].Letter = letter; // map upper case to lower case
					m_lu[i].Offset = m_lu[letter].Offset;
					// no unique characters so don't increment size
				}
			}
			if (bIncludeSpecialCharacters)
			{
				for (var i = 0; i < rchSpecialCharacters.Length; i++)
				{
					var c = rchSpecialCharacters[i];
					m_lu[c].Letter = c;
					m_lu[c].Offset = m_nSizeOfAlphabet++;
				}
			}
			if (bIncludeExtendedAscii)
			{
				for (var i = 0; i < rchExtendedAscii.Length; i++)
				{
					var c = rchExtendedAscii[i];
					m_lu[c].Letter = c;
					m_lu[c].Offset = m_nSizeOfAlphabet++;
				}
			}

			m_nBitsInShift = Convert.ToInt16(Math.Ceiling(Math.Log((double)m_nSizeOfAlphabet) / Math.Log((double)2)));
			// can use fewer bits in shift to turn it into a hash

			m_nTableSize = Convert.ToInt32(Math.Pow(Math.Pow((double)2, m_nBitsInShift), (int)B));
			// 2 ** bits ** B, will be some unused space when not hashed
			m_ShiftTable = new int[m_nTableSize];

			for (var i = 0; i < m_nTableSize; ++i)
			{
				m_ShiftTable[i] = m - B + 1; // default to m-B+1 for shift
			}

			m_vPatternMap = new Dictionary<int, List<PatternMap>>(m_nTableSize);

			for (var j = 0; j < k; ++j)
			{
				// loop through patterns
				for (var q = m; q >= B; --q)
				{
					int hash;
					hash = m_lu[patterns[j].Word[q - 2 - 1]].Offset; // bring in offsets of X in pattern j
					hash <<= m_nBitsInShift;
					hash += m_lu[patterns[j].Word[q - 1 - 1]].Offset;
					hash <<= m_nBitsInShift;
					hash += m_lu[patterns[j].Word[q - 1]].Offset;
					var shiftlen = m - q;
					m_ShiftTable[hash] = Math.Min(m_ShiftTable[hash], shiftlen);
					if (0 == shiftlen)
					{
						var m_PatternMapElement = new PatternMap();
						m_PatternMapElement.Index = j;
						m_PatternMapElement.PrefixHash = m_lu[patterns[j].Word[0]].Offset;
						m_PatternMapElement.PrefixHash <<= m_nBitsInShift;
						m_PatternMapElement.PrefixHash += m_lu[patterns[j].Word[1]].Offset;
						if (!m_vPatternMap.TryGetValue(hash, out var map))
							m_vPatternMap.Add(hash, new List<PatternMap>());
						m_vPatternMap[hash].Add(m_PatternMapElement);
					}
				}
			}
			m_bInitialized = true;
		}

		public IEnumerable<WordMatch> Search(string text)
		{
			if (m_bInitialized)
			{
				var ix = m - 1; // start off by matching end of largest common pattern
				var length = text.Length;
				while (ix < length)
				{
					int hash1;
					hash1 = m_lu[text[ix - 2]].Offset;
					hash1 <<= m_nBitsInShift;
					hash1 += m_lu[text[ix - 1]].Offset;
					hash1 <<= m_nBitsInShift;
					hash1 += m_lu[text[ix]].Offset;
					var shift = m_ShiftTable[hash1];
					if (shift > 0)
					{
						ix += shift;
					}
					else
					{
						// we have a potential match when shift is 0
						int hash2;  // check for matching prefixes
						hash2 = m_lu[text[ix - m + 1]].Offset;
						hash2 <<= m_nBitsInShift;
						hash2 += m_lu[text[ix - m + 2]].Offset;
						List<PatternMap> element = m_vPatternMap[hash1];
						for (var iter = 0; iter < element.Count; iter++)
						{
							if (hash2 == element[iter].PrefixHash)
							{
								// since prefix matches, compare target substring with pattern
								var ixTarget = text.Substring(ix - m + 3); // we know first two characters already match
								var ixPattern = _patterns[element[iter].Index].Word.Substring(2);  // ditto
								var target = 0;
								var targetLength = ixTarget.Length;
								var pattern = 0;
								var patternLength = ixPattern.Length;
								while (target < targetLength && pattern < patternLength)
								{
									// match until we reach end of either string
									if (m_lu[ixTarget[target]].Letter == m_lu[ixPattern[pattern]].Letter)
									{
										// match against chosen case sensitivity
										++target;
										++pattern;
									}
									else
									{
										break;
									}
								}
								if (pattern == patternLength)
								{
									// we found the end of the pattern, so match found
									var match = _patterns[element[iter].Index];
									yield return new WordMatch { Index = ix, Word = match.Word, Id = match.Id, Tag = match.Tag };
								}
							}
						}
						++ix;
					}
				}
			}
		}
	}
}
