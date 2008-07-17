using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace Dapple.KML
{
	#region Abstract ============================================================

	public abstract class KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strID = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strTargetID = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected KMLFile m_oSourceFile;

		#endregion


		#region Constructors

		public KMLObject(XmlElement element, XmlNamespaceManager manager, KMLFile source)
		{
			m_oSourceFile = source;

			if (element.HasAttribute("id"))
			{
				m_strID = element.GetAttribute("id");

				if (!source.NamedElements.ContainsKey(m_strID))
				{
					source.NamedElements.Add(m_strID, this);
				}
			}
			if (element.HasAttribute("targetId"))
			{
				m_strTargetID = element.GetAttribute("targetId");
			}
		}

		protected KMLObject()
		{
		}

		#endregion


		#region Properties

		public String ID
		{
			get { return m_strID; }
		}

		public String TargetID
		{
			get { return m_strTargetID; }
		}

		#endregion
	}

	public abstract class KMLStyleSelector : KMLObject
	{
		#region Constructors

		public KMLStyleSelector(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
		}

		public KMLStyleSelector()
			: base()
		{
		}

		#endregion


		#region Properties

		public abstract KMLStyle NormalStyle { get; }
		public abstract KMLStyle HighlightStyle { get; }

		#endregion


		#region Static Parsers

		public static List<KMLStyleSelector> GetStyleSelectors(XmlElement oElement, XmlNamespaceManager oManager, KMLFile oSource)
		{
			List<KMLStyleSelector> result = new List<KMLStyleSelector>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.Name.Equals("Style"))
				{
					result.Add(new KMLStyle(oChild as XmlElement, oManager, oSource));
				}
				else if (oChild.Name.Equals("StyleMap"))
				{
					result.Add(new KMLStyleMap(oChild as XmlElement, oManager, oSource));
				}
			}

			return result;
		}

		#endregion
	}

	public abstract class KMLFeature : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strName = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blVisibility = true;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blOpen = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strStyleURL;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strSnippet = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strDescription = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KLMTimePrimitive m_oTime;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLRegion m_oRegion;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAbstractView m_oView;
		// atom:author
		// atom:link
		// address
		// addressdetails
		// phonenumber
		// extendeddata

		protected List<KMLStyleSelector> m_oInlineStyles;

		#endregion


		#region Constructors

		public KMLFeature(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:name", manager) as XmlElement;
			if (oPointer != null)
			{
				m_strName = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:visibility", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blVisibility = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:open", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blOpen = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:styleUrl", manager) as XmlElement;
			if (oPointer != null)
			{
				m_strStyleURL = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:Snippet", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_strSnippet = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:description", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_strDescription = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:Region", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_oRegion = new KMLRegion(oPointer, manager, source);
			}

			oPointer = element.SelectSingleNode("kml:TimeStamp", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_oTime = new KMLTimeStamp(oPointer, manager, source);
			}
			oPointer = element.SelectSingleNode("kml:TimeSpan", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_oTime = new KMLTimeSpan(oPointer, manager, source);
			}

			oPointer = element.SelectSingleNode("kml:Camera", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oView = new KMLCamera(oPointer, manager, source);
			}
			oPointer = element.SelectSingleNode("kml:LookAt", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oView = new KMLLookAt(oPointer, manager, source);
			}

			m_oInlineStyles = KMLStyleSelector.GetStyleSelectors(element, manager, source);
		}

		#endregion


		#region Properties

		public String Name
		{
			get { return m_strName; }
		}

		public bool Visibility
		{
			get { return m_blVisibility; }
		}

		public bool Open
		{
			get { return m_blOpen; }
		}

		public String Snippet
		{
			get
			{
				return m_strSnippet;
			}
		}

		public String Description
		{
			get
			{
				return m_strDescription;
			}
		}

		public KMLStyleSelector Style
		{
			get
			{
				if (String.IsNullOrEmpty(m_strStyleURL))
				{
					if (m_oInlineStyles.Count == 0)
					{
						return new KMLStyle();
					}
					else
					{
						return m_oInlineStyles[0];
					}
				}
				else
				{
					return m_oSourceFile.GetStyle(m_strStyleURL);
				}
			}
		}

		public KMLRegion Region
		{
			get { return m_oRegion; }
		}

		public KLMTimePrimitive Time
		{
			get { return m_oTime; }
		}

		public KMLAbstractView View
		{
			get { return m_oView; }
		}

		#endregion


		#region Static Parsers

		public static List<KMLFeature> GetFeatures(XmlElement oElement, XmlNamespaceManager oManager, KMLFile oSource)
		{
			List<KMLFeature> result = new List<KMLFeature>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.Name.Equals("NetworkLink"))
				{
					throw new ApplicationException("NetworkLink unimplemented");
				}
				else if (oChild.Name.Equals("Placemark"))
				{
					result.Add(new KMLPlacemark(oChild as XmlElement, oManager, oSource));
				}
				else if (oChild.Name.Equals("PhotoOverlay"))
				{
					throw new ApplicationException("PhotoOverlay unimplemented");
				}
				else if (oChild.Name.Equals("ScreenOverlay"))
				{
					result.Add(new KMLScreenOverlay(oChild as XmlElement, oManager, oSource));
				}
				else if (oChild.Name.Equals("GroundOverlay"))
				{
					result.Add(new KMLGroundOverlay(oChild as XmlElement, oManager, oSource));
				}
				else if (oChild.Name.Equals("Folder"))
				{
					result.Add(new KMLFolder(oChild as XmlElement, oManager, oSource));
				}
			}

			return result;
		}

		#endregion
	}

	public abstract class KMLContainer : KMLFeature
	{
		#region Constructors

		public KMLContainer(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
		}

		#endregion

		#region Properties

		public abstract KMLFeature this[int i] { get; }

		public abstract int Count { get; }

		#endregion
	}

	public abstract class KMLOverlay : KMLFeature
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int m_iDrawOrder;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLIconOrLink m_oIcon = null;

		#endregion


		#region Constructors

		public KMLOverlay(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:color", manager) as XmlElement;
			if (oPointer != null)
			{
				String strColor = oPointer.ChildNodes[0].Value;
				m_oColor = KMLColorStyle.ParseColor(strColor);
			}

			oPointer = element.SelectSingleNode("kml:drawOrder", manager) as XmlElement;
			if (oPointer != null)
			{
				m_iDrawOrder = Convert.ToInt32(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Icon", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oIcon = new KMLIconOrLink(oPointer, manager, source);
			}
		}

		#endregion


		#region Properties

		public Color Color
		{
			get
			{
				if (m_oColor != null)
				{
					return m_oColor.Value;
				}
				else
				{
					return Color.FromArgb(255, 255, 255, 255);
				}
			}
		}

		public int DrawOrder
		{
			get { return m_iDrawOrder; }
		}

		public KMLIconOrLink Icon
		{
			get { return m_oIcon; }
		}

		#endregion
	}

	public abstract class KMLGeometry : KMLObject
	{
		#region Member Variables

		private KMLFeature m_oOwner;

		#endregion


		#region Constructors

		public KMLGeometry(XmlElement element, XmlNamespaceManager manager, KMLFeature owner, KMLFile source)
			: base(element, manager, source)
		{
			m_oOwner = owner;
		}

		#endregion


		#region Properties

		public KMLFeature Owner
		{
			get { return m_oOwner; }
		}

		public KMLStyleSelector Style
		{
			get
			{
				return m_oOwner.Style;
			}
		}

		#endregion


		#region Static Parsers

		public static List<KMLGeometry> GetGeometries(XmlElement oElement, XmlNamespaceManager oManager, KMLPlacemark oOwner, KMLFile oSource)
		{
			List<KMLGeometry> result = new List<KMLGeometry>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.Name.Equals("Point"))
				{
					result.Add(new KMLPoint(oChild as XmlElement, oManager, oOwner, oSource));
				}
				else if (oChild.Name.Equals("LineString"))
				{
					result.Add(new KMLLineString(oChild as XmlElement, oManager, oOwner, oSource));
				}
				else if (oChild.Name.Equals("LinearRing"))
				{
					result.Add(new KMLLinearRing(oChild as XmlElement, oManager, oOwner, oSource));
				}
				else if (oChild.Name.Equals("Polygon"))
				{
					result.Add(new KMLPolygon(oChild as XmlElement, oManager, oOwner, oSource));
				}
				else if (oChild.Name.Equals("MultiGeometry"))
				{
					result.Add(new KMLMultiGeometry(oChild as XmlElement, oManager, oOwner, oSource));
				}
				else if (oChild.Name.Equals("Model"))
				{
					result.Add(new KMLModel(oChild as XmlElement, oManager, oOwner, oSource));
				}
			}

			return result;
		}

		#endregion
	}

	public abstract class KMLColorStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLColorMode? m_eColorMode;

		#endregion


		#region Constructors

		public KMLColorStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:color", manager) as XmlElement;
			if (oPointer != null)
			{
				String strColor = oPointer.ChildNodes[0].Value;

				m_oColor = ParseColor(strColor);
			}

			oPointer = element.SelectSingleNode("kml:colorMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eColorMode = (KMLColorMode)Enum.Parse(typeof(KMLColorMode), oPointer.ChildNodes[0].Value);
			}
		}

		public KMLColorStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public Color Color
		{
			get
			{
				if (m_oColor != null)
				{
					return m_oColor.Value;
				}
				else
				{
					return Color.FromArgb(255, 255, 255, 255);
				}
			}
		}

		public KMLColorMode ColorMode
		{
			get
			{
				if (m_eColorMode != null)
				{
					return m_eColorMode.Value; ;
				}
				else
				{
					return KMLColorMode.normal;
				}
			}
		}

		#endregion


		#region Static Parsers

		public static Color ParseColor(String strColor)
		{
			// --- This is a hack: some color tags encountered starting with '#' ---
			strColor = strColor.Replace("#", String.Empty);

			return Color.FromArgb(
						  Int32.Parse(strColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
						  Int32.Parse(strColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber),
						  Int32.Parse(strColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
						  Int32.Parse(strColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)
						  );
		}

		#endregion
	}

	public abstract class KLMTimePrimitive : KMLObject
	{
		#region Constructors

		public KLMTimePrimitive(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
		}

		#endregion
	}

	public abstract class KMLAbstractView : KMLObject
	{
		#region Constructor

		public KMLAbstractView(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
		}
		#endregion
	}

	#endregion


	#region Enums and Structs ===================================================

	public enum KMLGridOrigin
	{
		lowerLeft,
		upperLeft
	}

	public enum KMLShape
	{
		rectangle,
		cylinder,
		sphere
	}

	public enum KLMRefreshMode
	{
		onChange,
		onInterval,
		onExpire
	}

	public enum KMLViewRefreshMode
	{
		never,
		onStop,
		onRequest,
		onRegion
	}

	public enum KMLAltitudeMode
	{
		clampToGround,
		relativeToGround,
		absolute
	}

	public enum KMLColorMode
	{
		normal,
		random
	}

	public enum KMLDisplayMode
	{
		DEFAULT,
		HIDE
	}

	public enum KMLUnits
	{
		pixels,
		fraction,
		insetPixels
	}

	public enum KMLListItemType
	{
		check,
		checkOffOnly,
		checkHideChildren,
		radioFolder
	}

	[FlagsAttribute]
	public enum KMLItemIconState
	{
		none = 0,
		open = 1 << 0,
		closed = 1 << 1,
		error = 1 << 2,
		fetching0 = 1 << 3,
		fetching1 = 1 << 4,
		fetching2 = 1 << 5
	}

	public enum KMLDateTimeType
	{
		Year,
		YearMonth,
		YearMonthDay,
		YearMonthDayTime
	}

	[DebuggerDisplay("Lat = {Latitude}  Lon = {Longitude}  Alt = {Altitude}")]
	public struct KMLCoordinates
	{
		public double Latitude;
		public double Longitude;
		public double Altitude;

		public KMLCoordinates(double dLat, double dLon, double dAlt)
		{
			Latitude = dLat;
			Longitude = dLon;
			Altitude = dAlt;
		}

		public KMLCoordinates(String strTuple)
		{
			String[] oValues = strTuple.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (oValues.Length < 2 || oValues.Length > 3)
			{
				throw new ArgumentException("The KML file contains a malformed 'Coordinates' element.");
			}

			Longitude = Convert.ToDouble(oValues[0]);
			Latitude = Convert.ToDouble(oValues[1]);
			if (oValues.Length == 3)
			{
				Altitude = Convert.ToDouble(oValues[2]);
			}
			else
			{
				Altitude = 0.0;
			}
		}
	}

	public struct KMLDateTime
	{
		private static Regex oYearRegex = new Regex(@"^(\d\d\d\d)$");
		private static Regex oYMRegex = new Regex(@"^(\d\d\d\d)-(\d\d)$");
		private static Regex oYMDRegex = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)$");
		private static Regex oUTCDateTime = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)Z$");
		private static Regex oNonUTCDateTime = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)[+-](\d\d):(\d\d)$");

		public DateTime Time;
		public KMLDateTimeType Type;

		public KMLDateTime(String strValue)
		{
			strValue = strValue.Trim();

			if (oYearRegex.IsMatch(strValue))
			{
				Type = KMLDateTimeType.Year;
				Time = new DateTime(Convert.ToInt32(strValue), 1, 1);
			}
			else if (oYMRegex.IsMatch(strValue))
			{
				Type = KMLDateTimeType.YearMonth;
				Time = DateTime.Parse(strValue);
			}
			else if (oYMDRegex.IsMatch(strValue))
			{
				Type = KMLDateTimeType.YearMonthDay;
				Time = DateTime.Parse(strValue);
			}
			else if (oUTCDateTime.IsMatch(strValue) || oNonUTCDateTime.IsMatch(strValue))
			{
				Type = KMLDateTimeType.YearMonthDayTime;
				Time = DateTime.Parse(strValue);
			}
			else
			{
				throw new ArgumentException("The KML file contains a malformed 'DateTime' element.");
			}
		}
	}

	public struct KMLVec2
	{
		public double X;
		public double Y;
		public KMLUnits XUnits;
		public KMLUnits YUnits;

		public KMLVec2(XmlElement element)
		{
			if (element.HasAttribute("x"))
			{
				X = Double.Parse(element.GetAttribute("x"));
			}
			else
			{
				X = 0.0;
			}

			if (element.HasAttribute("y"))
			{
				Y = Double.Parse(element.GetAttribute("y"));
			}
			else
			{
				Y = 0.0;
			}

			if (element.HasAttribute("xunits"))
			{
				XUnits = (KMLUnits)Enum.Parse(typeof(KMLUnits), element.GetAttribute("xunits"));
			}
			else
			{
				XUnits = KMLUnits.fraction;
			}

			if (element.HasAttribute("yunits"))
			{
				YUnits = (KMLUnits)Enum.Parse(typeof(KMLUnits), element.GetAttribute("yunits"));
			}
			else
			{
				YUnits = KMLUnits.fraction;
			}
		}

		public KMLVec2(double x, double y, KMLUnits xunits, KMLUnits yunits)
		{
			X = x;
			Y = y;
			XUnits = xunits;
			YUnits = yunits;
		}
	}

	#endregion


	#region Concrete ============================================================

	#region Models ====================================================

	public class KMLLocation : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dLongitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dLatitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dAltitude;

		#endregion

		#region Constructors

		public KMLLocation(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:longitude", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Location' element without a 'Longitude' element.");
			m_dLongitude = Double.Parse(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:latitude", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Location' element without a 'Latitude' element.");
			m_dLatitude = Double.Parse(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:altitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dAltitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}
		}

		#endregion

		#region Properties

		public double Longitude
		{
			get { return m_dLongitude; }
		}

		public double Latitude
		{
			get { return m_dLatitude; }
		}

		public double Altitude
		{
			get
			{
				if (m_dAltitude != null)
				{
					return m_dAltitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		#endregion
	}

	public class KMLOrientation : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dHeading;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dTilt;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dRoll;

		#endregion


		#region Constructors

		public KMLOrientation(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:heading", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dHeading = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:tilt", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dTilt = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:roll", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dRoll = Double.Parse(oPointer.ChildNodes[0].Value);
			}
		}

		public KMLOrientation()
			: base()
		{
		}

		#endregion


		#region Properties

		public double Heading
		{
			get
			{
				if (m_dHeading != null)
				{
					return m_dHeading.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Tilt
		{
			get
			{
				if (m_dTilt != null)
				{
					return m_dTilt.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Roll
		{
			get
			{
				if (m_dRoll != null)
				{
					return m_dRoll.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		#endregion
	}

	public class KMLScale : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dX;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dZ;

		#endregion


		#region Constructors

		public KMLScale(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:x", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dX = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:y", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dY = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:z", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dZ = Double.Parse(oPointer.ChildNodes[0].Value);
			}
		}

		public KMLScale()
			: base()
		{
		}

		#endregion


		#region Properties

		public double X
		{
			get
			{
				if (m_dX != null)
				{
					return m_dX.Value;
				}
				else
				{
					return 1.0;
				}
			}
		}

		public double Y
		{
			get
			{
				if (m_dY != null)
				{
					return m_dY.Value;
				}
				else
				{
					return 1.0;
				}
			}
		}

		public double Z
		{
			get
			{
				if (m_dZ != null)
				{
					return m_dZ.Value;
				}
				else
				{
					return 1.0;
				}
			}
		}

		#endregion
	}

	public class KMLModel : KMLGeometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLocation m_oLocation;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLOrientation m_oOrientation;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLScale m_oScale;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLIconOrLink m_oLink;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Dictionary<String, String> m_oResourceMap = new Dictionary<String, String>();

		#endregion


		#region Constructors

		public KMLModel(XmlElement element, XmlNamespaceManager manager, KMLPlacemark owner, KMLFile source)
			: base(element, manager, owner, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Location", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Model' element without a 'Location' element.");
			m_oLocation = new KMLLocation(oPointer, manager, source);

			oPointer = element.SelectSingleNode("kml:Orientation", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oOrientation = new KMLOrientation(oPointer, manager, source);
			}
			else
			{
				m_oOrientation = new KMLOrientation();
			}

			oPointer = element.SelectSingleNode("kml:Scale", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oScale = new KMLScale(oPointer, manager, source);
			}
			else
			{
				m_oScale = new KMLScale();
			}

			oPointer = element.SelectSingleNode("kml:Link", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Model' element without a 'Link' element.");
			m_oLink = new KMLIconOrLink(oPointer, manager, source);

			foreach (XmlElement oAlias in element.SelectNodes("kml:ResourceMap/kml:Alias", manager))
			{
				String strTargetHref = oAlias.SelectSingleNode("kml:targetHref").ChildNodes[0].Value;
				String strSourceHref = oAlias.SelectSingleNode("kml:sourceHref").ChildNodes[0].Value;

				m_oResourceMap.Add(strSourceHref, strTargetHref);
			}
		}

		#endregion


		#region Properties

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		public KMLLocation Location
		{
			get { return m_oLocation; }
		}

		public KMLOrientation Orientation
		{
			get { return m_oOrientation; }
		}

		public KMLScale Scale
		{
			get { return m_oScale; }
		}

		public KMLIconOrLink Link
		{
			get { return m_oLink; }
		}

		public String MapResource(String strSource)
		{
			if (m_oResourceMap.ContainsKey(strSource))
			{
				return m_oResourceMap[strSource];
			}
			else
			{
				return null;
			}
		}

		#endregion
	}

	#endregion


	#region Bounding Boxes/Regions ====================================

	public class KMLRegion : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLatLonAltBox m_oBox;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLod m_oLod;

		#endregion


		#region Constructors

		public KMLRegion(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:LatLonAltBox", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Region' element without a 'LatLonAltBox' element.");
			m_oBox = new KMLLatLonAltBox(oPointer, manager, source);

			oPointer = element.SelectSingleNode("kml:Lod", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oLod = new KMLLod(oPointer, manager, source);
			}
		}

		#endregion


		#region Properties

		public KMLLatLonAltBox LatLonAltBox
		{
			get { return m_oBox; }
		}

		public KMLLod Lod
		{
			get { return m_oLod; }
		}

		#endregion
	}

	public class KMLLod : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fMinLodPixels;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fMaxLodPixels;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fMinFadeExtent;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fMaxFadeExtent;

		#endregion


		#region Constructors

		public KMLLod(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:minLodPixels", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fMinLodPixels = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:maxLodPixels", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fMaxLodPixels = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:minFadeExtent", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fMinFadeExtent = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:maxFadeExtent", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fMaxFadeExtent = Single.Parse(oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public float MinLoDPixels
		{
			get
			{
				if (m_fMinLodPixels != null)
				{
					return m_fMinLodPixels.Value;
				}
				else
				{
					return 0.0f;
				}
			}
		}

		public float MaxLoDPixels
		{
			get
			{
				if (m_fMaxLodPixels != null)
				{
					return m_fMaxLodPixels.Value;
				}
				else
				{
					return -1.0f;
				}
			}
		}

		public float MinFadeExtent
		{
			get
			{
				if (m_fMinFadeExtent != null)
				{
					return m_fMinFadeExtent.Value;
				}
				else
				{
					return 0.0f;
				}
			}
		}

		public float MaxFadeExtent
		{
			get
			{
				if (m_fMaxFadeExtent != null)
				{
					return m_fMaxFadeExtent.Value;
				}
				else
				{
					return 0.0f;
				}
			}
		}

		#endregion
	}

	public class KMLLatLonBox : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dNorth;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dEast;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dSouth;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dWest;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dRotation;

		#endregion


		#region Constructors

		public KMLLatLonBox(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:north", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'north' element.");
			m_dNorth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:east", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonBox' element without an 'east' element.");
			m_dEast = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:south", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'south' element.");
			m_dSouth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:west", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'west' element.");
			m_dWest = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:rotation", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dRotation = Convert.ToDouble(oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public double North
		{
			get { return m_dNorth; }
		}

		public double East
		{
			get { return m_dEast; }
		}

		public double South
		{
			get { return m_dSouth; }
		}

		public double West
		{
			get { return m_dWest; }
		}

		public double Rotation
		{
			get
			{
				if (m_dRotation != null)
				{
					return m_dRotation.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		#endregion
	}

	public class KMLLatLonAltBox : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dNorth;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dEast;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dSouth;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dWest;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dMinAltitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dMaxAltitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;

		#endregion


		#region Constructors

		public KMLLatLonAltBox(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:north", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'north' element.");
			m_dNorth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:east", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without an 'east' element.");
			m_dEast = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:south", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'south' element.");
			m_dSouth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:west", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'west' element.");
			m_dWest = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:minAltitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dMinAltitude = Convert.ToDouble(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:maxAltitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dMaxAltitude = Convert.ToDouble(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public double North
		{
			get { return m_dNorth; }
		}

		public double East
		{
			get { return m_dEast; }
		}

		public double South
		{
			get { return m_dSouth; }
		}

		public double West
		{
			get { return m_dWest; }
		}

		public double MinAltitude
		{
			get
			{
				if (m_dMinAltitude != null)
				{
					return m_dMinAltitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double MaxAltitude
		{
			get
			{
				if (m_dMaxAltitude != null)
				{
					return m_dMaxAltitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		#endregion
	}

	#endregion


	#region TimePrimitives ============================================

	public class KMLTimeStamp : KLMTimePrimitive
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oWhen;

		#endregion


		#region Constructors

		public KMLTimeStamp(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:when", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'TimeStamp' element without a 'when' element.");
			m_oWhen = new KMLDateTime(element.ChildNodes[0].Value);
		}

		#endregion


		#region Properties

		public KMLDateTime When
		{
			get
			{
				return m_oWhen;
			}
		}

		#endregion
	}

	public class KMLTimeSpan : KLMTimePrimitive
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oBegin;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oEnd;

		#endregion


		#region Constructors

		public KMLTimeSpan(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:begin", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBegin = new KMLDateTime(element.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:end", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oEnd = new KMLDateTime(element.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public KMLDateTime Begin
		{
			get { return m_oBegin; }
		}

		public KMLDateTime End
		{
			get { return m_oEnd; }
		}

		#endregion
	}

	#endregion


	#region Styles ====================================================

	public class KMLBalloonStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oBGColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oTextColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strText = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDisplayMode? m_eDisplayMode;

		#endregion


		#region Constructors

		public KMLBalloonStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:bgColor", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBGColor = KMLColorStyle.ParseColor(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:textColor", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oTextColor = KMLColorStyle.ParseColor(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:text", manager) as XmlElement;
			if (oPointer != null)
			{
				m_strText = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:displayMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eDisplayMode = (KMLDisplayMode)Enum.Parse(typeof(KMLDisplayMode), oPointer.ChildNodes[0].Value.ToUpper());
			}
		}

		public KMLBalloonStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public Color BackgroundColor
		{
			get
			{
				if (m_oBGColor != null)
				{
					return m_oBGColor.Value;
				}
				else
				{
					return Color.FromArgb(255, 255, 255, 255);
				}
			}
		}

		public Color TextColor
		{
			get
			{
				if (m_oTextColor != null)
				{
					return m_oTextColor.Value;
				}
				else
				{
					return Color.FromArgb(255, 0, 0, 0);
				}
			}
		}

		public String Text
		{
			get
			{
				return m_strText;
			}
		}

		public KMLDisplayMode DisplayMode
		{
			get
			{
				if (m_eDisplayMode != null)
				{
					return m_eDisplayMode.Value;
				}
				else
				{
					return KMLDisplayMode.DEFAULT;
				}
			}
		}

		#endregion
	}

	public class KMLLineStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fWidth;

		#endregion


		#region Constructors

		public KMLLineStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:width", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fWidth = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}
		}

		public KMLLineStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public float Width
		{
			get
			{
				if (m_fWidth != null)
				{
					return m_fWidth.Value;
				}
				else
				{
					return 1.0f;
				}
			}
		}

		#endregion
	}

	public class KMLListStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oBGColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLListItemType? m_eType;

		private Dictionary<KMLItemIconState, String> m_oItemIcons = new Dictionary<KMLItemIconState, String>();

		#endregion


		#region Constructors

		public KMLListStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:bgColor", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBGColor = KMLColorStyle.ParseColor(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:listItemType", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eType = (KMLListItemType)Enum.Parse(typeof(KMLListItemType), oPointer.ChildNodes[0].Value);
			}

			foreach (XmlElement oItemIconElement in element.SelectNodes("kml:ItemIcon", manager))
			{
				oPointer = element.SelectSingleNode("kml:state", manager) as XmlElement;
				if (oPointer == null) continue;
				KMLItemIconState oMode = KMLItemIconState.none;
				foreach (String strMode in oPointer.ChildNodes[0].Value.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries))
				{
					oMode |= (KMLItemIconState)Enum.Parse(typeof(KMLItemIconState), strMode);
				}

				oPointer = element.SelectSingleNode("kml:href", manager) as XmlElement;
				if (oPointer == null || oPointer.HasChildNodes == false) continue;

				m_oItemIcons.Add(oMode, oPointer.ChildNodes[0].Value);
			}
		}

		public KMLListStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public Color BackgroundColor
		{
			get
			{
				if (m_oBGColor == null)
				{
					return Color.FromArgb(255, 255, 255, 255);
				}
				else
				{
					return m_oBGColor.Value;
				}
			}
		}

		public KMLListItemType ListItemType
		{
			get
			{
				if (m_eType != null)
				{
					return m_eType.Value;
				}
				else
				{
					return KMLListItemType.check;
				}
			}
		}

		public String GetItemIcon(KMLItemIconState eMode)
		{
			if (m_oItemIcons.ContainsKey(eMode))
			{
				return m_oItemIcons[eMode];
			}
			else
			{
				return null;
			}
		}

		#endregion
	}

	public class KMLIconStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fScale;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fHeading;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLIconOrLink m_oIcon = null;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLVec2 m_oHotSpot;

		#endregion


		#region Constructors

		public KMLIconStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:scale", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fScale = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:heading", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fHeading = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Icon", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oIcon = new KMLIconOrLink(oPointer, manager, source);
			}

			oPointer = element.SelectSingleNode("kml:hotSpot", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oHotSpot = new KMLVec2(oPointer);
			}
			else
			{
				m_oHotSpot = new KMLVec2(0.5, 0.5, KMLUnits.fraction, KMLUnits.fraction);
			}
		}

		public KMLIconStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public float Scale
		{
			get
			{
				if (m_fScale != null)
				{
					return m_fScale.Value;
				}
				else
				{
					return 1.0f;
				}
			}
		}

		public float Heading
		{
			get
			{
				if (m_fHeading != null)
				{
					return m_fHeading.Value;
				}
				else
				{
					return 0.0f;
				}
			}
		}

		public KMLIconOrLink Icon
		{
			get { return m_oIcon; }
		}

		public KMLVec2 HotSpot
		{
			get { return m_oHotSpot; }
		}

		#endregion
	}

	public class KMLLabelStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fScale;

		#endregion


		#region Constructors

		public KMLLabelStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:scale", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fScale = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}
		}

		public KMLLabelStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public float Scale
		{
			get
			{
				if (m_fScale != null)
				{
					return m_fScale.Value;
				}
				else
				{
					return 1.0f;
				}
			}
		}

		#endregion
	}

	public class KMLPolyStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blFill;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blOutline;

		#endregion


		#region Constructors

		public KMLPolyStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:fill", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blFill = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:outline", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blOutline = oPointer.ChildNodes[0].Value.Equals("1");
			}
		}

		public KMLPolyStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		public bool Fill
		{
			get
			{
				if (m_blFill != null)
				{
					return m_blFill.Value;
				}
				else
				{
					return true;
				}
			}
		}

		public bool Outline
		{
			get
			{
				if (m_blOutline != null)
				{
					return m_blOutline.Value;
				}
				else
				{
					return true;
				}
			}
		}

		#endregion
	}

	[DebuggerDisplay("Style, id = {m_strID}")]
	public class KMLStyle : KMLStyleSelector
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLineStyle m_oLineStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLListStyle m_oListStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLIconStyle m_oIconStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLabelStyle m_oLabelStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLPolyStyle m_oPolyStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLBalloonStyle m_oBalloonStyle;

		#endregion


		#region Constructors

		public KMLStyle(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;
			oPointer = element.SelectSingleNode("kml:IconStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oIconStyle = new KMLIconStyle(oPointer as XmlElement, manager, source);
			}
			else
			{
				m_oIconStyle = new KMLIconStyle();
			}

			oPointer = element.SelectSingleNode("kml:LabelStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oLabelStyle = new KMLLabelStyle(oPointer as XmlElement, manager, source);
			}
			else
			{
				m_oLabelStyle = new KMLLabelStyle();
			}

			oPointer = element.SelectSingleNode("kml:LineStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oLineStyle = new KMLLineStyle(oPointer as XmlElement, manager, source);
			}
			else
			{
				m_oLineStyle = new KMLLineStyle();
			}

			oPointer = element.SelectSingleNode("kml:PolyStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oPolyStyle = new KMLPolyStyle(oPointer as XmlElement, manager, source);
			}
			else
			{
				m_oPolyStyle = new KMLPolyStyle();
			}

			oPointer = element.SelectSingleNode("kml:BalloonStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBalloonStyle = new KMLBalloonStyle(oPointer as XmlElement, manager, source);
			}
			else
			{
				m_oBalloonStyle = new KMLBalloonStyle();
			}

			oPointer = element.SelectSingleNode("kml:ListStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oListStyle = new KMLListStyle(oPointer as XmlElement, manager, source);
			}
			else
			{
				m_oListStyle = new KMLListStyle();
			}
		}

		public KMLStyle()
			: base()
		{
			m_oLineStyle = new KMLLineStyle();
			m_oListStyle = new KMLListStyle();
			m_oIconStyle = new KMLIconStyle();
			m_oLabelStyle = new KMLLabelStyle();
			m_oPolyStyle = new KMLPolyStyle();
		}

		#endregion


		#region Properties

		public override KMLStyle NormalStyle
		{
			get { return this; }
		}

		public override KMLStyle HighlightStyle
		{
			get { return this; }
		}

		public KMLLineStyle LineStyle
		{
			get { return m_oLineStyle; }
		}

		public KMLPolyStyle PolyStyle
		{
			get { return m_oPolyStyle; }
		}

		public KMLIconStyle IconStyle
		{
			get { return m_oIconStyle; }
		}

		public KMLLabelStyle LabelStyle
		{
			get { return m_oLabelStyle; }
		}

		public KMLListStyle ListStyle
		{
			get { return m_oListStyle; }
		}

		#endregion
	}

	[DebuggerDisplay("StyleMap, id = {m_strID}")]
	public class KMLStyleMap : KMLStyleSelector
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strNormalStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strHighlightStyle;

		#endregion


		#region Constructors

		public KMLStyleMap(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			foreach (XmlElement oPair in element.SelectNodes("kml:Pair", manager))
			{
				if (oPair.ChildNodes[0].ChildNodes[0].Value.Equals("normal"))
				{
					m_strNormalStyle = oPair.ChildNodes[1].ChildNodes[0].Value;
				}
				else
				{
					m_strHighlightStyle = oPair.ChildNodes[1].ChildNodes[0].Value;
				}
			}
		}

		#endregion


		#region Properties

		public override KMLStyle NormalStyle
		{
			get { return m_oSourceFile.GetStyle(m_strNormalStyle).NormalStyle; }
		}

		public override KMLStyle HighlightStyle
		{
			get { return m_oSourceFile.GetStyle(m_strHighlightStyle).HighlightStyle; }
		}

		#endregion
	}

	#endregion


	#region Geometry ==================================================

	public class KMLLinearRing : KMLGeometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blExtrude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blTessellate;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<KMLCoordinates> m_oCoords = new List<KMLCoordinates>();

		#endregion


		#region Constructors

		public KMLLinearRing(XmlElement element, XmlNamespaceManager manager, KMLPlacemark owner, KMLFile source)
			: base(element, manager, owner, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:extrude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blExtrude = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:tessellate", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blTessellate = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:coordinates", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LinearRing' element without a 'Coordinates' element.");
			String[] oTuples = oPointer.ChildNodes[0].Value.Replace(", ", ",").Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
			foreach (String strTuple in oTuples)
			{
				m_oCoords.Add(new KMLCoordinates(strTuple));
			}
		}

		#endregion


		#region Properties

		public bool Extrude
		{
			get
			{
				if (m_blExtrude != null)
				{
					return m_blExtrude.Value;
				}
				else
				{
					return false;
				}
			}
		}

		public bool Tessellate
		{
			get
			{
				if (m_blTessellate != null)
				{
					return m_blTessellate.Value;
				}
				else
				{
					return false;
				}
			}
		}

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		public KMLCoordinates this[int i]
		{
			get { return m_oCoords[i]; }
		}

		public int Count
		{
			get { return m_oCoords.Count; }
		}

		#endregion
	}

	public class KMLPolygon : KMLGeometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blExtrude = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blTessellate = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode m_eAltitudeMode = KMLAltitudeMode.clampToGround;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLinearRing m_oOuterBoundary;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<KMLLinearRing> m_oInnerBoundaries = new List<KMLLinearRing>();

		#endregion


		#region Constructors

		public KMLPolygon(XmlElement element, XmlNamespaceManager manager, KMLPlacemark owner, KMLFile source)
			: base(element, manager, owner, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:extrude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blExtrude = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:tessellate", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blTessellate = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:outerBoundaryIs/kml:LinearRing", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Polygon' element without an 'outerBoundaryIs' element.");
			m_oOuterBoundary = new KMLLinearRing(oPointer, manager, owner, source);

			foreach (XmlElement oInnerBound in element.SelectNodes("kml:innerBoundaryIs/kml:LinearRing", manager))
			{
				m_oInnerBoundaries.Add(new KMLLinearRing(oPointer, manager, owner, source));
			}
		}

		#endregion


		#region Properties

		public bool Extrude
		{
			get { return m_blExtrude; }
		}

		public bool Tessellate
		{
			get { return m_blTessellate; }
		}

		public KMLAltitudeMode AltitudeMode
		{
			get { return m_eAltitudeMode; }
		}

		public KMLLinearRing OuterBoundary
		{
			get { return m_oOuterBoundary; }
		}

		public List<KMLLinearRing> InnerBoundaries
		{
			get { return m_oInnerBoundaries; }
		}

		#endregion
	}

	public class KMLPoint : KMLGeometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blExtrude = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode m_eAltitudeMode = KMLAltitudeMode.clampToGround;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLCoordinates m_oCoords;

		#endregion


		#region Constructors

		public KMLPoint(XmlElement element, XmlNamespaceManager manager, KMLFeature owner, KMLFile source)
			: base(element, manager, owner, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:extrude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blExtrude = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:coordinates", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'Point' element without a 'Coordinates' element.");
			m_oCoords = new KMLCoordinates(oPointer.ChildNodes[0].Value);
		}

		#endregion


		#region Properties

		public bool Extrude
		{
			get { return m_blExtrude; }
		}

		public KMLAltitudeMode AltitudeMode
		{
			get { return m_eAltitudeMode; }
		}

		public KMLCoordinates Coordinates
		{
			get { return m_oCoords; }
		}

		#endregion
	}

	public class KMLLineString : KMLGeometry
	{
		#region Member Varialbes

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blExtrude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blTessellate;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<KMLCoordinates> m_oCoords = new List<KMLCoordinates>();

		#endregion


		#region Constructors

		public KMLLineString(XmlElement element, XmlNamespaceManager manager, KMLPlacemark owner, KMLFile source)
			: base(element, manager, owner, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:extrude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blExtrude = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:tessellate", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blTessellate = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:coordinates", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LineString' element without a 'Coordinates' element.");
			String[] oTuples = oPointer.ChildNodes[0].Value.Replace(", ", ",").Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
			foreach (String strTuple in oTuples)
			{
				m_oCoords.Add(new KMLCoordinates(strTuple));
			}
		}

		#endregion


		#region Properties

		public bool Extrude
		{
			get
			{
				if (m_blExtrude != null)
				{
					return m_blExtrude.Value;
				}
				else
				{
					return false;
				}
			}
		}

		public bool Tessellate
		{
			get
			{
				if (m_blTessellate != null)
				{
					return m_blTessellate.Value;
				}
				else
				{
					return false;
				}
			}
		}

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		public KMLCoordinates this[int i]
		{
			get { return m_oCoords[i]; }
		}

		public int Count
		{
			get { return m_oCoords.Count; }
		}

		#endregion
	}

	public class KMLMultiGeometry : KMLGeometry
	{
		#region Member Variables

		private List<KMLGeometry> m_oChildren;

		#endregion


		#region Constructors

		public KMLMultiGeometry(XmlElement element, XmlNamespaceManager manager, KMLPlacemark owner, KMLFile source)
			: base(element, manager, owner, source)
		{
			m_oChildren = KMLGeometry.GetGeometries(element, manager, owner, source);
		}

		#endregion


		#region Properties

		public KMLGeometry this[int i]
		{
			get { return m_oChildren[i]; }
		}

		public int Count
		{
			get { return m_oChildren.Count; }
		}

		#endregion
	}

	#endregion


	#region Overlays ==================================================

	public class KMLGroundOverlay : KMLOverlay
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dAltitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLatLonBox m_oBox;

		#endregion


		#region Constructors

		public KMLGroundOverlay(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:altitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dAltitude = Convert.ToDouble(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:LatLonBox", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'GroundOverlay' element without a 'LatLonBox' element.");
			m_oBox = new KMLLatLonBox(oPointer, manager, source);
		}

		#endregion


		#region Properties

		public double Altitude
		{
			get
			{
				if (m_dAltitude != null)
				{
					return m_dAltitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		public KMLLatLonBox LatLonBox
		{
			get
			{
				return m_oBox;
			}
		}

		#endregion

		#region Public Methods

		public String GetUri()
		{
			String result = Icon.HRef;
			if (!String.IsNullOrEmpty(Icon.HTTPQuery))
			{
				result += "&" + Icon.HTTPQuery;
			}
			if (!String.IsNullOrEmpty(Icon.ViewFormat))
			{
				result += "&" + Icon.ViewFormat.Replace("[bboxNorth]", m_oBox.North.ToString()).Replace("[bboxSouth]", m_oBox.South.ToString()).Replace("[bboxEast]", m_oBox.East.ToString()).Replace("[bboxWest]", m_oBox.West.ToString());
			}

			return result;
		}

		#endregion
	}

	public class KMLScreenOverlay : KMLOverlay
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLVec2 m_oOverlayXY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLVec2 m_oScreenXY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLVec2 m_oRotationXY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLVec2 m_oSize;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_dRotation;

		#endregion


		#region Constructors

		public KMLScreenOverlay(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:overlayXY", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oOverlayXY = new KMLVec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:screenXY", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oScreenXY = new KMLVec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:rotationXY", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oRotationXY = new KMLVec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:size", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oSize = new KMLVec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:rotation", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dRotation = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public KMLVec2 OverlayXY
		{
			get { return m_oOverlayXY; }
		}

		public KMLVec2 ScreenXY
		{
			get { return m_oScreenXY; }
		}

		public KMLVec2 RotationXY
		{
			get { return m_oRotationXY; }
		}

		public KMLVec2 Size
		{
			get { return m_oSize; }
		}

		public float Rotation
		{
			get
			{
				if (m_dRotation != null)
				{
					return m_dRotation.Value;
				}
				else
				{
					return 0.0f;
				}
			}
		}

		#endregion
	}

	public class KMLPhotoOverlay : KMLOverlay
	{
		#region Member Variables

		private KMLShape? m_eShape;
		private double? m_dViewVolumeLeftFov;
		private double? m_dViewVolumeRightFov;
		private double? m_dViewVolumeBottomFov;
		private double? m_dViewVolumeTopFov;
		private double? m_dViewVolumeNear;
		private double? m_dRoll;
		private int? m_iImagePyramidTileSize;
		private int m_iImagePyramidMaxWidth;
		private int m_iImagePyramidMaxHeight;
		private KMLGridOrigin? m_eGridOrigin;
		private KMLPoint m_oPoint;

		#endregion


		#region Constructors

		public KMLPhotoOverlay(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:shape", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eShape = (KMLShape)Enum.Parse(typeof(KMLShape), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:ViewVolume/kml:leftFov", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dViewVolumeLeftFov = Double.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ViewVolume/kml:rightVov", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dViewVolumeRightFov = Double.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ViewVolume/kml:bottomFov", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dViewVolumeBottomFov = Double.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ViewVolume/kml:topFov", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dViewVolumeTopFov = Double.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ViewVolume/kml:near", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dViewVolumeNear = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:roll", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dRoll = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:ImagePyramid/kml:tileSize", manager) as XmlElement;
			if (oPointer != null)
			{
				m_iImagePyramidTileSize = Int32.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ImagePyramid/kml:maxWidth", manager) as XmlElement;
			if (oPointer != null)
			{
				m_iImagePyramidMaxWidth = Int32.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ImagePyramid/kml:maxHeight", manager) as XmlElement;
			if (oPointer != null)
			{
				m_iImagePyramidMaxHeight = Int32.Parse(oPointer.ChildNodes[0].Value);
			}
			oPointer = element.SelectSingleNode("kml:ImagePyramid/kml:gridOrigin", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eGridOrigin = (KMLGridOrigin)Enum.Parse(typeof(KMLGridOrigin), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Point", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oPoint = new KMLPoint(oPointer, manager, this, source);
			}
		}

		#endregion


		#region Properties

		public KMLShape Shape
		{
			get
			{
				if (m_eShape != null)
				{
					return m_eShape.Value;
				}
				else
				{
					return KMLShape.rectangle;
				}
			}
		}

		public double ViewVolumeLeftFOV
		{
			get
			{
				if (m_dViewVolumeLeftFov != null)
				{
					return m_dViewVolumeLeftFov.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double ViewVolumeRightFOV
		{
			get
			{
				if (m_dViewVolumeRightFov != null)
				{
					return m_dViewVolumeRightFov.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double ViewVolumeBottomFOV
		{
			get
			{
				if (m_dViewVolumeBottomFov != null)
				{
					return m_dViewVolumeBottomFov.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double ViewVolumeTopFOV
		{
			get
			{
				if (m_dViewVolumeTopFov != null)
				{
					return m_dViewVolumeTopFov.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double ViewVolumeNear
		{
			get
			{
				if (m_dViewVolumeNear != null)
				{
					return m_dViewVolumeNear.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Roll
		{
			get
			{
				if (m_dRoll != null)
				{
					return m_dRoll.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public int ImagePyramidTileSize
		{
			get
			{
				if (m_iImagePyramidTileSize != null)
				{
					return m_iImagePyramidTileSize.Value;
				}
				else
				{
					return 256;
				}
			}
		}

		public int ImagePyramidMaxWidth
		{
			get { return m_iImagePyramidMaxWidth; }
		}

		public int ImagePyramidMaxHeight
		{
			get { return m_iImagePyramidMaxHeight; }
		}

		public KMLGridOrigin ImagePyramidGridOrigin
		{
			get
			{
				if (m_eGridOrigin != null)
				{
					return m_eGridOrigin.Value;
				}
				else
				{
					return KMLGridOrigin.lowerLeft;
				}
			}
		}

		public KMLPoint Point
		{
			get { return m_oPoint; }
		}

		#endregion
	}

	#endregion


	#region Features ==================================================

	public class KMLNetworkLink : KMLFeature
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blRefreshVisibility;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blFlyToView;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLIconOrLink m_oLink;

		#endregion


		#region Constructors

		public KMLNetworkLink(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:refreshVisibility", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blRefreshVisibility = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:flyToView", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blFlyToView = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:Link", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'NetworkLink' element without a 'Link' element.");

			m_oLink = new KMLIconOrLink(oPointer, manager, source);
		}

		#endregion


		#region Properties

		public bool RefreshVisibility
		{
			get
			{
				if (m_blRefreshVisibility != null)
				{
					return m_blRefreshVisibility.Value;
				}
				else
				{
					return false;
				}
			}
		}

		public bool FlyToView
		{
			get
			{
				if (m_blFlyToView != null)
				{
					return m_blFlyToView.Value;
				}
				else
				{
					return false;
				}
			}
		}

		public KMLIconOrLink Link
		{
			get
			{
				return m_oLink;
			}
		}

		#endregion
	}

	[DebuggerDisplay("Placemark, name = {m_strName}")]
	public class KMLPlacemark : KMLFeature
	{
		#region Member Variables

		private KMLGeometry m_oGeometry;

		#endregion


		#region Constructor

		public KMLPlacemark(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			List<KMLGeometry> oGeometries = KMLGeometry.GetGeometries(element, manager, this, source);
			if (oGeometries.Count == 0) throw new ArgumentException("The KML file contains a 'Placemark' element without a geometry element.");
			m_oGeometry = oGeometries[0];
		}

		#endregion


		#region Properties

		public KMLGeometry Geometry
		{
			get { return m_oGeometry; }
		}

		#endregion
	}

	#endregion


	#region Containers ================================================

	[DebuggerDisplay("Folder, name = {m_strName}")]
	public class KMLFolder : KMLContainer
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private List<KMLFeature> m_oFeatures;

		#endregion


		#region Constructors

		public KMLFolder(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			m_oFeatures = KMLFeature.GetFeatures(element, manager, source);
		}

		#endregion


		#region Properties

		public override KMLFeature this[int i]
		{
			get { return m_oFeatures[i]; }
		}

		public override int Count
		{
			get { return m_oFeatures.Count; }
		}

		#endregion
	}

	[DebuggerDisplay("Document, name = {m_strName}")]
	public class KMLDocument : KMLContainer
	{
		#region Member Variables

		private List<KMLFeature> m_oFeatures;
		//TODO Parse out schema elements

		#endregion


		#region Constructors

		public KMLDocument(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			m_oFeatures = KMLFeature.GetFeatures(element, manager, source);
		}

		#endregion


		#region Properties

		public override KMLFeature this[int i]
		{
			get { return m_oFeatures[i]; }
		}

		public override int Count
		{
			get { return m_oFeatures.Count; }
		}

		#endregion


		#region Public Methods

		public KMLStyleSelector GetStyle(string strStyleURL)
		{
			if (strStyleURL.StartsWith("#"))
			{
				String strStyleID = strStyleURL.Substring(1);

				foreach (KMLStyleSelector oStyles in m_oInlineStyles)
				{
					if (oStyles.ID.Equals(strStyleID))
					{
						return oStyles;
					}
				}

				return null;
			}
			else
			{
				return null;
			}
		}

		#endregion
	}

	#endregion


	#region Abstract Views ============================================

	public class KMLCamera : KMLAbstractView
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dLongitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dLatitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dAltitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dHeading;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dTilt;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dRoll;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;

		#endregion


		#region Constructors

		public KMLCamera(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:longitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dLongitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:latitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dLatitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:altitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dAltitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:heading", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dHeading = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:tilt", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dTilt = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:roll", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dRoll = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public double Longitude
		{
			get
			{
				if (m_dLongitude != null)
				{
					return m_dLongitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Latitude
		{
			get
			{
				if (m_dLatitude != null)
				{
					return m_dLatitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Altitude
		{
			get
			{
				if (m_dAltitude != null)
				{
					return m_dAltitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Heading
		{
			get
			{
				if (m_dHeading != null)
				{
					return m_dHeading.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Tilt
		{
			get
			{
				if (m_dTilt != null)
				{
					return m_dTilt.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Roll
		{
			get
			{
				if (m_dRoll != null)
				{
					return m_dRoll.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		#endregion
	}

	public class KMLLookAt : KMLAbstractView
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dLongitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dLatitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dAltitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dHeading;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dTilt;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double m_dRange;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLAltitudeMode? m_eAltitudeMode;

		#endregion


		#region Constructors

		public KMLLookAt(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:longitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dLongitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:latitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dLatitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:altitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dAltitude = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:heading", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dHeading = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:tilt", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dTilt = Double.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:range", manager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains a 'LookAt' element without a 'range' element.");
			m_dRange = Double.Parse(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public double Longitude
		{
			get
			{
				if (m_dLongitude != null)
				{
					return m_dLongitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Latitude
		{
			get
			{
				if (m_dLatitude != null)
				{
					return m_dLatitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Altitude
		{
			get
			{
				if (m_dAltitude != null)
				{
					return m_dAltitude.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Heading
		{
			get
			{
				if (m_dHeading != null)
				{
					return m_dHeading.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Tilt
		{
			get
			{
				if (m_dTilt != null)
				{
					return m_dTilt.Value;
				}
				else
				{
					return 0.0;
				}
			}
		}

		public double Range
		{
			get { return m_dRange; }
		}

		public KMLAltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return KMLAltitudeMode.clampToGround;
				}
			}
		}

		#endregion
	}

	#endregion

	public class KMLIconOrLink : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string m_strHRef = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KLMRefreshMode? m_eRefreshMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fRefreshInterval;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLViewRefreshMode? m_eViewRefreshMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fViewRefreshTime;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fViewBoundScale;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strViewFormat = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strHTTPQuery = String.Empty;

		#endregion


		#region Constructors

		public KMLIconOrLink(XmlElement element, XmlNamespaceManager manager, KMLFile source)
			: base(element, manager, source)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:href", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_strHRef = oPointer.ChildNodes[0].Value.Trim();
			}

			oPointer = element.SelectSingleNode("kml:refreshMode", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_eRefreshMode = (KLMRefreshMode)Enum.Parse(typeof(KLMRefreshMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:refreshInterval", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_fRefreshInterval = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:viewRefreshMode", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_eViewRefreshMode = (KMLViewRefreshMode)Enum.Parse(typeof(KMLViewRefreshMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:viewRefreshTime", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_fViewRefreshTime = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:viewBoundScale", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_fViewBoundScale = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:viewFormat", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_strViewFormat = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:httpQuery", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_strHTTPQuery = oPointer.ChildNodes[0].Value;
			}
		}

		#endregion


		#region Properties

		public bool IsLocalFile
		{
			get { return !m_strHRef.StartsWith("http://"); }
		}

		public String HRef
		{
			get
			{
				return m_strHRef;
			}
		}

		public KLMRefreshMode RefreshMode
		{
			get
			{
				if (m_eRefreshMode != null)
				{
					return m_eRefreshMode.Value;
				}
				else
				{
					return KLMRefreshMode.onChange;
				}
			}
		}

		public float RefreshInterval
		{
			get
			{
				if (m_fRefreshInterval != null)
				{
					return m_fRefreshInterval.Value;
				}
				else
				{
					return 4.0f;
				}
			}
		}

		public KMLViewRefreshMode ViewRefreshMode
		{
			get
			{
				if (m_eViewRefreshMode != null)
				{
					return m_eViewRefreshMode.Value;
				}
				else
				{
					return KMLViewRefreshMode.never;
				}
			}
		}

		public float ViewRefreshTime
		{
			get
			{
				if (m_fViewRefreshTime != null)
				{
					return m_fViewRefreshTime.Value;
				}
				else
				{
					return 4.0f;
				}
			}
		}

		public float ViewBoundScale
		{
			get
			{
				if (m_fViewBoundScale != null)
				{
					return m_fViewBoundScale.Value;
				}
				else
				{
					return 1.0f;
				}
			}
		}

		public String ViewFormat
		{
			get
			{
				return m_strViewFormat;
			}
		}

		public String HTTPQuery
		{
			get
			{
				return m_strHTTPQuery;
			}
		}

		#endregion

		#region Public Methods

		public String GetUri(double west, double south, double east, double north)
		{
			String result = HRef;
			if (!String.IsNullOrEmpty(HTTPQuery))
			{
				result += "&" + HTTPQuery;
			}
			if (!String.IsNullOrEmpty(ViewFormat))
			{
				result += "&" + ViewFormat.Replace("[bboxNorth]", north.ToString()).Replace("[bboxSouth]", south.ToString()).Replace("[bboxEast]", east.ToString()).Replace("[bboxWest]", west.ToString());
			}

			return result;
		}

		#endregion
	}

	public class KMLFile
	{
		#region Statics

		public static string KMLTempDirectory = Path.Combine(Path.GetTempPath(), "DappleKML");

		#endregion


		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDocument m_oDocument;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strFilename;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blLoadedFromKMZ = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strVersion;

		private Dictionary<String, KMLObject> m_oNamedElements = new Dictionary<String, KMLObject>();

		#endregion


		#region Constructors

		public KMLFile(String strFilename)
			: this(strFilename, false)
		{
		}

		public KMLFile(String strFilename, bool blValidate)
		{
			if (Path.GetExtension(strFilename).Equals(".kmz", StringComparison.InvariantCultureIgnoreCase))
			{
				strFilename = UnzipKMZFile(strFilename);
				if (strFilename == null)
				{
					throw new ArgumentException("Could not find a KML file to load inside the KMZ file.");
				}
				m_blLoadedFromKMZ = true;
			}

			m_strFilename = strFilename;

			System.Xml.XmlDocument oDoc = new System.Xml.XmlDocument();
			XmlReaderSettings oSettings = new XmlReaderSettings();
			if (blValidate)
			{
				oSettings.Schemas.Add("http://www.opengis.net/kml/2.2", Path.Combine(Path.GetDirectoryName(strFilename), "ogckml22.xsd"));
				oSettings.ValidationType = ValidationType.Schema;
			}
			XmlReader oDocReader = XmlReader.Create(strFilename, oSettings);

			try
			{
				oDoc.Load(oDocReader);
			}
			catch (XmlException ex)
			{
				throw new ArgumentException("KML file failed validation: " + ex.Message);
			}

			m_strVersion = oDoc.DocumentElement.NamespaceURI;

			System.Xml.XmlNamespaceManager oManager = new System.Xml.XmlNamespaceManager(oDoc.NameTable);
			oManager.AddNamespace("kml", oDoc.DocumentElement.NamespaceURI);

			XmlElement oPointer;

			oPointer = oDoc.DocumentElement.SelectSingleNode("kml:Document", oManager) as XmlElement;
			if (oPointer == null) throw new ArgumentException("The KML file contains no 'Document' element.");
			m_oDocument = new Dapple.KML.KMLDocument(oPointer, oManager, this);
		}

		~KMLFile()
		{
			// --- If we've loaded from a KMZ, delete the KMZ directory ---

			if (m_blLoadedFromKMZ)
			{
				foreach (String strFile in Directory.GetFiles(Path.GetDirectoryName(m_strFilename)))
				{
					try
					{
						File.Delete(strFile);
					}
					catch (Exception)
					{
						// --- Not a huge deal if we can't delete a temp file ---
					}
				}

				try
				{
					Directory.Delete(Path.GetDirectoryName(m_strFilename), true);
				}
				catch (Exception)
				{
					// --- Not a huge deal if we can't delete a temp directory ---
				}
			}
		}

		private string UnzipKMZFile(string strFilename)
		{
			// --- Create temporary subdirectory for KMZ files in the KML temp directory ---

			String strTempDirectory = Path.Combine(KMLTempDirectory, Path.GetFileNameWithoutExtension(strFilename));
			if (!Directory.Exists(strTempDirectory))
			{
				Directory.CreateDirectory(strTempDirectory);
			}


			// --- Extract KMZ into subdirectory ---

			FastZip oFastZip = new FastZip();
			oFastZip.ExtractZip(strFilename, strTempDirectory, String.Empty);


			// --- Find the KMZ file to load ---

			String[] oKMLs = Directory.GetFiles(strTempDirectory, "*.kml");

			if (oKMLs.Length > 0)
			{
				return oKMLs[0];
			}
			else
			{
				return null;
			}
		}

		#endregion


		#region Properties

		public KMLDocument Document
		{
			get { return m_oDocument; }
		}

		public String Filename
		{
			get
			{
				return m_strFilename;
			}
		}

		public Dictionary<String, KMLObject> NamedElements
		{
			get { return m_oNamedElements; }
		}

		#endregion


		#region Public Methods

		public KMLStyleSelector GetStyle(String strStyleUrl)
		{
			KMLStyleSelector result = Document.GetStyle(strStyleUrl);
			if (result != null)
			{
				return result;
			}
			else if (m_oNamedElements.ContainsKey(strStyleUrl) && m_oNamedElements[strStyleUrl] is KMLStyleSelector)
			{
				return m_oNamedElements[strStyleUrl] as KMLStyleSelector;
			}
			else
			{
				return new KMLStyle();
			}
		}

		#endregion
	}

	#endregion
}