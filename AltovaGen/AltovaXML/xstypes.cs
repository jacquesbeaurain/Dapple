// xstypes.cs 
// This file contains generated code and will be overwritten when you rerun code generation.

using Altova.TypeInfo;
using Altova.Types;
using System.Xml;
using System.Text;

namespace Altova.Xml
{
	
	public class XmlFormatter : ValueFormatter
	{
		public virtual string Format(DateTime v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(Duration v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(long v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(ulong v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(double v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(string v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(byte[] v)
		{
			return System.Convert.ToBase64String(v);
		}
		
		public virtual string Format(bool v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual string Format(decimal v)
		{
			return CoreTypes.CastToString(v);
		}
		
		public virtual byte[] ParseBinary(string v)
		{
			return System.Convert.FromBase64String(v);
		}
	}
	
	public class XmlTimeFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_time);
		}
	}
	
	public class XmlDateFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_date);
		}
	}
	
	class XmlGYearFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_gYear);
		}
	}
	
	class XmlGMonthFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_gMonth);
		}
	}


	class XmlGDayFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_gDay);
		}
	}
	
	class XmlGYearMonthFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_gYearMonth);
		}
	}
	
	class XmlGMonthDayFormatter : XmlFormatter
	{
		public override string Format(DateTime dt)
		{
			return dt.ToString(Altova.Types.DateTimeFormat.W3_gMonthDay);
		}
	}
	
	class XmlHexBinaryFormatter : XmlFormatter
	{
		private static string EncodingTable = "0123456789ABCDEF";
		private static sbyte[] aDecodingTable = new sbyte[256]
		{
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			0,	 1,	 2,	 3,	 4,	 5,	 6,	 7,	 8,	 9,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	10,	11,	12,	13,	14,	15,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	10,	11,	12,	13,	14,	15,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,
			-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1,	-1
		};

		public override string Format(byte[] v)
		{
			string result = "";	
			for(int i=0; i<v.Length; ++i)
            {
                result += EncodingTable[v[i] >> 4];
                result += EncodingTable[v[i] & 15];
            }
            return result;
		}

        public override byte[] ParseBinary(string s)
        {
			if( s == null ) return null;
			StringBuilder sb = new StringBuilder(s);
			sb.Replace(" ", "");
			sb.Replace("\t", "");
			sb.Replace("\n", "");
			sb.Replace("\r", "");
			string newvalue = sb.ToString().Trim();
			if( newvalue.Length == 0 ) return new byte[0];
			char[] cSrc = newvalue.ToCharArray();
			byte[] value = new byte[ cSrc.Length / 2 ];
			int nSrcIndex = 0;
			int nTarIndex = 0;
			while( nSrcIndex < cSrc.Length )
			{
				sbyte c = aDecodingTable[ cSrc[ nSrcIndex++ ] ];
				if( c != -1 )
				{
					value[ nTarIndex >> 1 ] |= (byte)( (nTarIndex & 1) == 1 ? c : (c << 4) );
					nTarIndex++;
				}
			}
			return value;
        }
	}
	
	class XmlIntegerFormatter : XmlFormatter
	{
		public override string Format(double v)
		{
			return CoreTypes.CastToString((long) v);
		}
	}
	public class Xs
	{
		public readonly static ValueFormatter StandardFormatter = new XmlFormatter();
		public readonly static ValueFormatter TimeFormatter = new XmlTimeFormatter();
		public readonly static ValueFormatter DateFormatter = new XmlDateFormatter();
		public readonly static ValueFormatter DateTimeFormatter = StandardFormatter;
		public readonly static ValueFormatter GYearFormatter = new XmlGYearFormatter();
		public readonly static ValueFormatter GMonthFormatter = new XmlGMonthFormatter();
		public readonly static ValueFormatter GDayFormatter = new XmlGDayFormatter();
		public readonly static ValueFormatter GYearMonthFormatter = new XmlGYearMonthFormatter();
		public readonly static ValueFormatter GMonthDayFormatter = new XmlGMonthDayFormatter();
		public readonly static ValueFormatter HexBinaryFormatter = new XmlHexBinaryFormatter();
		public readonly static ValueFormatter IntegerFormatter = new XmlIntegerFormatter();
		public readonly static ValueFormatter DecimalFormatter = StandardFormatter;
		public readonly static ValueFormatter AnySimpleTypeFormatter = StandardFormatter;
		public readonly static ValueFormatter DurationFormatter = StandardFormatter;
		public readonly static ValueFormatter DoubleFormatter = StandardFormatter;
		public readonly static ValueFormatter Base64BinaryFormatter = StandardFormatter;
	}
		
	public class XsValidation
	{
		public static string ReplaceWhitespace(string input)
		{
			return input; 
		}
		
		public static string CollapseWhitespace(string input)
		{
			return input; 
		}
		
		public static bool Validate(string input, TypeInfo.TypeInfo info)
		{
			return true; 
		}
		
		private class LengthFacetCheckHelper
		{
			public static bool IsWhitespace(char c) { return c == '\t' || c =='\n' || c == '\r' || c == ' '; }
			
			public static int ComputeLength(string value, WhitespaceType whitespaceNormalization)
			{
				if (whitespaceNormalization == WhitespaceType.Collapse)
				{
					int length = 0;
					bool pendingSpace = false;
					for (int i=0; i< value.Length; i++)
					{
						if (IsWhitespace(value[i]))
						{
							if (length != 0)
								pendingSpace=true;
						}
						else
						{
							if (pendingSpace)
							{
								length += 1;
								pendingSpace=false;
							}
							length += 1;
						}
					}
					return length;
				}
				return value.Length;
			}
			
			public static bool IsEqual(string value, string normalizedCompare, WhitespaceType whitespaceNormalization)
			{
				if (whitespaceNormalization == WhitespaceType.Collapse)
				{
					bool flag  =false;
					bool pendingSpace = false;
					for (int i=0, j=0; i< value.Length; ++i)
					{
						if (IsWhitespace(value[i]))
						{
							if (flag)
								pendingSpace = true;
						}
						else
						{
							flag = true;
							if (j == normalizedCompare.Length)
								return false;
							if (pendingSpace)
							{
								if (normalizedCompare[j] != ' ')
									return false;
								++j;
								if (j == normalizedCompare.Length)
									return false;
								pendingSpace = false;
							}
							if (value[i] != normalizedCompare[j])
								return false;
							++j;
						}
					}
					return true;
				}
				else if (whitespaceNormalization == WhitespaceType.Replace)
				{
					int i=0, j=0;
					for (; i< value.Length && j<normalizedCompare.Length; ++i, ++j)
					{
						if(IsWhitespace(value[i]))
						{
							if (normalizedCompare[j] != ' ')
								return false;
						}
						else
						{
							if (value[i] != normalizedCompare[j])
								return false;
						}
					}
					if((i == value.Length) != (j == normalizedCompare.Length))
						return false;
					return true;
				}
				return value == normalizedCompare;
			}
		}
		
		public class FacetCheck_Success : FacetCheckInterface
		{
			public static FacetCheckResult Check(string s, FacetInfo facet, WhitespaceType whitespace)
			{
				return FacetCheckResult.Success;
			}
		}
		
		public class FacetCheck_string_length : FacetCheckInterface
		{
			public static FacetCheckResult Check(string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.ComputeLength(value, whitespaceNormalization) == facet.intValue)
					return FacetCheckResult.Success;
				return FacetCheckResult.Fail;
			}
		}

		public class FacetCheck_string_minLength : FacetCheckInterface
		{
			public static FacetCheckResult Check(string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.ComputeLength(value, whitespaceNormalization) >= facet.intValue)
					return FacetCheckResult.Success;
				return FacetCheckResult.Fail;
			}
		}
		
		public class FacetCheck_string_maxLength : FacetCheckInterface
		{
			public static FacetCheckResult Check(string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.ComputeLength(value, whitespaceNormalization) <= facet.intValue)
					return FacetCheckResult.Success;
				return FacetCheckResult.Fail;
			}
		}

		public class FacetCheck_string_enumeration : FacetCheckInterface
		{
			public static FacetCheckResult Check (string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.IsEqual(value, facet.stringValue, whitespaceNormalization))
					return FacetCheckResult.EnumSuccess;
				return FacetCheckResult.EnumFail;
			}
		}
		
		public class FacetCheck_hexBinary_length : FacetCheckInterface
		{
			public static FacetCheckResult Check(string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.ComputeLength(value, whitespaceNormalization) == facet.intValue * 2)
					return FacetCheckResult.Success;
				return FacetCheckResult.Fail;
			}
		}
		
		public class FacetCheck_hexBinary_minLength : FacetCheckInterface
		{
			public static FacetCheckResult Check(string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.ComputeLength(value, whitespaceNormalization) >= facet.intValue * 2)
					return FacetCheckResult.Success;
				return FacetCheckResult.Fail;
			}
		}
		
		public class FacetCheck_hexBinary_maxLength : FacetCheckInterface
		{
			public static FacetCheckResult Check(string value, FacetInfo facet, WhitespaceType whitespaceNormalization)
			{
				if (LengthFacetCheckHelper.ComputeLength(value, whitespaceNormalization) <= facet.intValue * 2)
					return FacetCheckResult.Success;
				return FacetCheckResult.Fail;
			}
		}
		
		public readonly static FacetCheckInterface facetCheck_Success = new FacetCheck_Success();
			
		public readonly static FacetCheckInterface facetCheck_string_length = new FacetCheck_string_length();
		public readonly static FacetCheckInterface facetCheck_string_minLength = new FacetCheck_string_minLength();
		public readonly static FacetCheckInterface facetCheck_string_maxLength = new FacetCheck_string_maxLength();
		public readonly static FacetCheckInterface facetCheck_string_enumeration = new FacetCheck_string_enumeration();
			
		public readonly static FacetCheckInterface facetCheck_hexBinary_length = new FacetCheck_hexBinary_length();
		public readonly static FacetCheckInterface facetCheck_hexBinary_minLength = new FacetCheck_hexBinary_minLength();
		public readonly static FacetCheckInterface facetCheck_hexBinary_maxLength = new FacetCheck_hexBinary_maxLength();
	} // class XsValidation
} // namesake AltovaXml.Xml.Xs