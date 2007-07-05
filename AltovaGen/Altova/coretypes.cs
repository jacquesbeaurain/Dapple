// coretypes.cs
// This file contains generated code and will be overwritten when you rerun code generation.


namespace Altova
{
    public class CoreTypes
    {
        public static int CastToInt(int i)
        {
            return i;
        }

        public static int CastToInt(uint i)
		{
            if (i > (uint)int.MaxValue)
                throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", i));
            return (int)i;
		}

        public static int CastToInt(double d)
        {
            if (d < int.MinValue || d > int.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", d));
			return (int) d;
        }

        public static int CastToInt(string s)
        {
			if (s == null)
				return 0;
			if (s == "")
				return 0;
            return int.Parse(s, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static int CastToInt(long i)
        {
            if (i < int.MinValue || i > int.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", i));
			return (int) i;
        }

        public static int CastToInt(ulong i)
        {
            if (i > (ulong)int.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for int.", i));
			return (int)i;
        }

		public static int CastToInt(decimal d)
		{
			return (int)d;
		}

        public static uint CastToUInt(int i)
        {
            if (i < 0)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", i));
			return (uint) i;

        }

        public static uint CastToUInt(uint i)
        {
            return i;
        }

        public static uint CastToUInt(double d)
        {
            if (d < 0 || d > uint.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", d));
			return (uint) d;
        }

        public static uint CastToUInt(string s)
        {
			if (s == null)
				return 0;
			if (s == "")
				return 0;
            return uint.Parse(s, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static uint CastToUInt(long i)
        {
            if (i < 0 || i > uint.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", i));
			return (uint) i;
        }

        public static uint CastToUInt(ulong i)
        {
            if (i > uint.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for uint.", i));
			return (uint) i;
        }

		public static uint CastToUInt(decimal d)
		{
			return (uint)d;
		}


        public static long CastToInt64(int i)
        {
            return (long) i;
        }

        public static long CastToInt64(uint i)
        {
            return (long)i;
        }

        public static long CastToInt64(double d)
        {
            if (d < long.MinValue || d > long.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for long.", d));
			return (long) d;
        }

        public static long CastToInt64(string s)
        {
			if (s == null)
				return 0;
			if (s == "")
				return 0;
            return long.Parse(s, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static long CastToInt64(long i)
        {
            return i;
        }

        public static long CastToInt64(ulong i)
        {
            if (i > long.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for long.", i));
			return (long) i;
        }

		public static long CastToInt64(decimal d)
		{
			return (long)d;
		}


        public static ulong CastToUInt64(int i)
        {
            if (i < 0)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for ulong.", i));
			return (ulong) i;
        }

        public static ulong CastToUInt64(uint i)
        {
            return (ulong)i;
        }

        public static ulong CastToUInt64(double d)
        {
            if (d < 0 || d > ulong.MaxValue)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for ulong.", d));
			return (ulong) d;
        }

        public static ulong CastToUInt64(string s)
        {
			if (s == null)
				return 0;
			if (s == "")
				return 0;
            return ulong.Parse(s, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static ulong CastToUInt64(long i)
        {
            if (i < 0)
				throw new System.OverflowException(string.Format("Numeric value overflow: {0} out of range for ulong.", i));
			return (ulong) i;
        }

        public static ulong CastToUInt64(ulong i)
        {
            return i;
        }

		public static ulong CastToUInt64(decimal d)
		{
			return (ulong)d;
		}

        public static double CastToDouble(bool b)
        {
            return b?1:0;
        }

        public static double CastToDouble(int i)
        {
            return (double) i;
        }

        public static double CastToDouble(uint i)
        {
            return (double) i;
        }

        public static double CastToDouble(long i)
        {
            return (double) i;
        }

        public static double CastToDouble(ulong i)
        {
            return (double) i;
        }

        public static double CastToDouble(double d)
        {
            return d;
        }

        public static double CastToDouble(string s)
        {
			if (s == null)
				return 0.0;
			if (s == "")
				return 0.0;
            return double.Parse(s, System.Globalization.NumberFormatInfo.InvariantInfo);
        }

		public static double CastToDouble(decimal d)
		{
			return (double)d;
		}
        
		public static decimal CastToDecimal(bool b)
		{
			return b ? 1 : 0;
		}

		public static decimal CastToDecimal(int i)
		{
			return i;
		}

		public static decimal CastToDecimal(uint i)
		{
			return i;
		}

		public static decimal CastToDecimal(long i)
		{
			return i;
		}

		public static decimal CastToDecimal(ulong i)
		{
			return i;
		}

		public static decimal CastToDecimal(double i)
		{
			return (decimal)i;
		}

		public static decimal CastToDecimal(decimal i)
		{
			return i;
		}

		public static decimal CastToDecimal(string s)
		{
			if (s == null)
				return 0m;
			if (s == "")
				return 0m;
			return decimal.Parse(s, System.Globalization.NumberFormatInfo.InvariantInfo);
		}

        public static string CastToString(int i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(uint i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(long i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(ulong i)
        {
            return i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(double d)
        {
            return d.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string CastToString(string s)
        {
            return s;
        }

        public static string CastToString(bool b)
        {
            if (b) 
                return "true";
            return "false";
        }

        public static string CastToString(Altova.Types.DateTime dt)
        {
            return dt.ToString();
        }

		public static string CastToString(Altova.Types.DateTime dt, Altova.Types.DateTimeFormat format)
		{
			return dt.ToString(format);
		}

        public static string CastToString(Altova.Types.Duration dur)
        {
            return dur.ToString();
        }

		public static string CastToString(byte[] val)
		{
			return System.Convert.ToBase64String(val);
		}

		public static string CastToString(decimal d)
		{
			string s = d.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
			int iComma = s.LastIndexOf('.');
			if( iComma >= 0 )
			{
				if (s.Length > iComma)
					s = s.TrimEnd(new char[] { '0' });
				if (s.Length == iComma + 1)
					s = s.Substring(0, iComma);
			}
			if( s.Length == 0 )
				s = "0";
			return s;
		}



        /*
        public static string CastToString(Altova.Types.DayTimeDuration dt)
        {
            return dt.ToString();
        }

        public static string CastToString(Altova.Types.YearMonthDuration dur)
        {
            return dur.ToString();
        }
        */

        public static Altova.Types.DateTime CastToDateTime(string s)
        {
			if (s == null)
                throw new System.NullReferenceException();
			if (s == "")
                throw new Altova.Types.StringParseException("Cast to DateTime failed.");
            return Altova.Types.DateTime.Parse(s);
        }

        public static Altova.Types.Duration CastToDuration(string s)
        {
			if (s == null)
                throw new System.NullReferenceException();
			if (s == "")
                throw new Altova.Types.StringParseException("Cast to Duration failed.");
            return Altova.Types.Duration.Parse(s);
        }

        /*
        public static Altova.Types.YearMonthDuration CastToYearMonthDuration(string s)
        {
            return Altova.Types.YearMonthDuration.Parse(s);
        }

        public static Altova.Types.DayTimeDuration CastToDayTimeDuration(string s)
        {
            return Altova.Types.DayTimeDuration.Parse(s);
        }
        */

        public static Altova.Types.DateTime CastToDateTime(Altova.Types.DateTime s)
        {
            return s;
        }

		public static Altova.Types.DateTime CastToDateTime(Altova.Types.DateTime s, Altova.Types.DateTimeFormat format)
		{
			return s;
		}

		public static Altova.Types.Duration CastToDuration(Altova.Types.Duration s)
        {
            return s;
        }

        /*
        public static Altova.Types.YearMonthDuration CastToYearMonthDuration(Altova.Types.YearMonthDuration s)
        {
            return s;
        }

        public static Altova.Types.DayTimeDuration CastToDayTimeDuration(Altova.Types.DayTimeDuration s)
        {
            return s;
        }
        */

        public static bool CastToBool(bool b)
        {
            return b;
        }

        public static bool CastToBool(int i)
        {
            return i != 0;
        }

        public static bool CastToBool(uint i)
        {
            return i != 0;
        }

        public static bool CastToBool(long i)
        {
            return i != 0;
        }

        public static bool CastToBool(ulong i)
        {
            return i != 0;
        }

        public static bool CastToBool(double d)
        {
            return d != 0;
        }

        public static bool CastToBool(string s)
        {
			if (s == null)
				return false;
            if (s == "false" || s == "0" || s == "")
                return false;
            return true;
        }

		public static bool CastToBool(decimal d)
		{
			return d != 0;
		}
        
        public static bool Exists(object o)
        {
            return o != null;
        }
   		
        public static int CastToInt(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return System.Convert.ToInt32(v, System.Globalization.CultureInfo.InvariantCulture);
		}
        
        

		public static uint CastToUInt(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return System.Convert.ToUInt32(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string CastToString(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is Altova.Types.DateTime)
				return CastToString(CastToDateTime(v));
			return System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static double CastToDouble(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return System.Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static decimal CastToDecimal(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return System.Convert.ToDecimal(v, System.Globalization.CultureInfo.InvariantCulture);
		}


		public static long CastToInt64(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return System.Convert.ToInt64(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static ulong CastToUInt64(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return System.Convert.ToUInt64(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static bool CastToBool(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();

			if (v is string)
				return CastToBool((string)v);
			return System.Convert.ToBoolean(v, System.Globalization.CultureInfo.InvariantCulture);
		}

		public static Altova.Types.DateTime CastToDateTime(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDateTime((string)v);            
			return (Altova.Types.DateTime)v;
		}

		public static Altova.Types.DateTime CastToDateTime(object v, Altova.Types.DateTimeFormat format)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			if (v is string)
				return CastToDateTime((string)v, format);            
			return (Altova.Types.DateTime)v;
		}

		public static Altova.Types.Duration CastToDuration(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return (Altova.Types.Duration)v;
		}

		public static byte[] CastToBinary(object v)
		{
			if (!Exists(v))
				throw new System.NullReferenceException();
			return (byte[]) v;
		}
     
        public static byte[] CastToBinary(byte[] b) { return b; }
        
        public static string FormatNumber(uint value, uint minDigits)
        {
            return value.ToString("D" + minDigits.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }
        
        public static string FormatNumber(int value, uint minDigits)
        {
            return value.ToString("D" + minDigits.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static string FormatTimezone(int value)
        {
            string result = "";
            
            if (value == 0)
                result += 'Z';
            else
            {
                if (value < 0)
                {
                    result += '-';
                    value = -value;
                }
                else
                {
                    result += '+';
                }
                result += FormatNumber((uint) value / 60, 2);
                result += ':';
                result += FormatNumber((uint) value % 60, 2);
            }
            return result;
        }

        public static string FormatFraction(uint value, uint precision)
        {
            string result = "";
	        if (value != 0)
	        {
		        result += '.';
		        result += FormatNumber(value, precision);
                int i = result.Length;
		        while (result[i - 1] == '0')
			        i -= 1;
                result = result.Remove(i, result.Length-i);
	        }
            return result;
        }
    }
}
