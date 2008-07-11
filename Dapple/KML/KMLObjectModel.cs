using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

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

		#endregion


		#region Constructors

		public KMLObject(XmlElement element, XmlNamespaceManager manager)
		{
			if (element.HasAttribute("id"))
			{
				m_strID = element.GetAttribute("id");
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

	public abstract class StyleSelector : KMLObject
	{
		#region Constructors

		public StyleSelector(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
		}

		public StyleSelector()
			: base()
		{
		}

		#endregion


		#region Properties

		public abstract Style NormalStyle { get; }
		public abstract Style HighlightStyle { get; }

		#endregion


		#region Static Parsers

		public static List<StyleSelector> GetStyleSelectors(XmlElement oElement, XmlNamespaceManager oManager, Document oOwner)
		{
			List<StyleSelector> result = new List<StyleSelector>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.Name.Equals("Style"))
				{
					result.Add(new Style(oChild as XmlElement, oManager));
				}
				else if (oChild.Name.Equals("StyleMap"))
				{
					result.Add(new StyleMap(oChild as XmlElement, oManager, oOwner));
				}
			}

			return result;
		}

		#endregion
	}

	public abstract class Feature : KMLObject
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
		private TimePrimitive m_oTime;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Region m_oRegion;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AbstractView m_oView;
		// atom:author
		// atom:link
		// address
		// addressdetails
		// phonenumber
		// extendeddata

		private Document m_oOwnerDocument;
		private List<StyleSelector> m_oInlineStyles;

		#endregion


		#region Constructors

		public Feature(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager)
		{
			m_oOwnerDocument = owner;
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
				m_oRegion = new Region(oPointer, manager);
			}

			oPointer = element.SelectSingleNode("kml:TimeStamp", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_oTime = new TimeStamp(oPointer, manager);
			}
			oPointer = element.SelectSingleNode("kml:TimeSpan", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_oTime = new TimeSpan(oPointer, manager);
			}

			oPointer = element.SelectSingleNode("kml:Camera", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oView = new Camera(oPointer, manager);
			}
			oPointer = element.SelectSingleNode("kml:LookAt", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oView = new LookAt(oPointer, manager);
			}

			m_oInlineStyles = StyleSelector.GetStyleSelectors(element, manager, owner);
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

		public StyleSelector Style
		{
			get
			{
				if (String.IsNullOrEmpty(m_strStyleURL))
				{
					if (m_oInlineStyles.Count == 0)
					{
						return new Style();
					}
					else
					{
						return m_oInlineStyles[0];
					}
				}
				else
				{
					return m_oOwnerDocument.GetStyle(m_strStyleURL);
				}
			}
		}

		public Region Region
		{
			get { return m_oRegion; }
		}

		public TimePrimitive Time
		{
			get { return m_oTime; }
		}

		public AbstractView View
		{
			get { return m_oView; }
		}

		#endregion


		#region Static Parsers

		public static List<Feature> GetFeatures(XmlElement oElement, XmlNamespaceManager oManager, Document oOwner)
		{
			List<Feature> result = new List<Feature>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.Name.Equals("NetworkLink"))
				{
					throw new ApplicationException("NetworkLink unimplemented");
				}
				else if (oChild.Name.Equals("Placemark"))
				{
					result.Add(new Placemark(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("PhotoOverlay"))
				{
					throw new ApplicationException("PhotoOverlay unimplemented");
				}
				else if (oChild.Name.Equals("ScreenOverlay"))
				{
					result.Add(new ScreenOverlay(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("GroundOverlay"))
				{
					result.Add(new GroundOverlay(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("Folder"))
				{
					result.Add(new Folder(oChild as XmlElement, oManager, oOwner));
				}
			}

			return result;
		}

		#endregion
	}

	public abstract class Container : Feature
	{
		#region Constructors

		public Container(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
		}

		#endregion

		#region Properties

		public abstract Feature this[int i] { get; }

		public abstract int Count { get; }

		#endregion
	}

	public abstract class Overlay : Feature
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color m_oColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int m_iDrawOrder;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IconOrLink m_oIcon = null;

		#endregion


		#region Constructors

		public Overlay(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:color", manager) as XmlElement;
			if (oPointer != null)
			{
				String strColor = oPointer.ChildNodes[0].Value;
				m_oColor = ColorStyle.ParseColor(strColor);
			}

			oPointer = element.SelectSingleNode("kml:drawOrder", manager) as XmlElement;
			if (oPointer != null)
			{
				m_iDrawOrder = Convert.ToInt32(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Icon", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oIcon = new IconOrLink(oPointer, manager);
			}
		}

		#endregion


		#region Properties

		public Color Color
		{
			get { return m_oColor; }
		}

		public int DrawOrder
		{
			get { return m_iDrawOrder; }
		}

		public IconOrLink Icon
		{
			get { return m_oIcon; }
		}

		#endregion
	}

	public abstract class Geometry : KMLObject
	{
		#region Member Variables

		private Feature m_oOwner;

		#endregion


		#region Constructors

		public Geometry(XmlElement element, XmlNamespaceManager manager, Feature owner)
			: base(element, manager)
		{
			m_oOwner = owner;
		}

		#endregion


		#region Properties

		public Feature Owner
		{
			get { return m_oOwner; }
		}

		public StyleSelector Style
		{
			get
			{
				return m_oOwner.Style;
			}
		}

		#endregion


		#region Static Parsers

		public static List<Geometry> GetGeometries(XmlElement oElement, XmlNamespaceManager oManager, Placemark oOwner)
		{
			List<Geometry> result = new List<Geometry>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.Name.Equals("Point"))
				{
					result.Add(new Point(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("LineString"))
				{
					result.Add(new LineString(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("LinearRing"))
				{
					result.Add(new LinearRing(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("Polygon"))
				{
					result.Add(new Polygon(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("MultiGeometry"))
				{
					result.Add(new MultiGeometry(oChild as XmlElement, oManager, oOwner));
				}
				else if (oChild.Name.Equals("Model"))
				{
					throw new ApplicationException("Model unimplemented");
				}
			}

			return result;
		}

		#endregion
	}

	public abstract class ColorStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ColorMode? m_eColorMode;

		#endregion


		#region Constructors

		public ColorStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
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
				m_eColorMode = (ColorMode)Enum.Parse(typeof(ColorMode), oPointer.ChildNodes[0].Value);
			}
		}

		public ColorStyle()
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

		public ColorMode ColorMode
		{
			get
			{
				if (m_eColorMode != null)
				{
					return m_eColorMode.Value; ;
				}
				else
				{
					return ColorMode.normal;
				}
			}
		}

		#endregion


		#region Static Parsers

		public static Color ParseColor(String strColor)
		{
			return Color.FromArgb(
						  Int32.Parse(strColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
						  Int32.Parse(strColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber),
						  Int32.Parse(strColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
						  Int32.Parse(strColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)
						  );
		}

		#endregion
	}

	public abstract class TimePrimitive : KMLObject
	{
		#region Constructors

		public TimePrimitive(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
		}

		#endregion
	}

	public abstract class AbstractView : KMLObject
	{
		#region Constructor

		public AbstractView(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
		}
		#endregion
	}

	#endregion


	#region Enums and Structs ===================================================

	public enum GridOrigin
	{
		lowerLeft,
		upperLeft
	}

	public enum Shape
	{
		rectangle,
		cylinder,
		sphere
	}

	public enum RefreshMode
	{
		onChange,
		onInterval,
		onExpire
	}

	public enum ViewRefreshMode
	{
		never,
		onStop,
		onRequest,
		onRegion
	}

	public enum AltitudeMode
	{
		clampToGround,
		relativeToGround,
		absolute
	}

	public enum ColorMode
	{
		normal,
		random
	}

	public enum DisplayMode
	{
		DEFAULT,
		HIDE
	}

	public enum Units
	{
		pixels,
		fraction,
		insetPixels
	}

	public enum ListItemType
	{
		check,
		checkOffOnly,
		checkHideChildren,
		radioFolder
	}

	[FlagsAttribute]
	public enum ItemIconState
	{
		none = 0,
		open = 1 << 0,
		closed = 1 << 1,
		error = 1 << 2,
		fetching0 = 1 << 3,
		fetching1 = 1 << 4,
		fetching2 = 1 << 5
	}

	[DebuggerDisplay("Lat = {Latitude}  Lon = {Longitude}  Alt = {Altitude}")]
	public struct Coordinates
	{
		public double Latitude;
		public double Longitude;
		public double Altitude;

		public Coordinates(double dLat, double dLon, double dAlt)
		{
			Latitude = dLat;
			Longitude = dLon;
			Altitude = dAlt;
		}

		public Coordinates(String strTuple)
		{
			String[] oValues = strTuple.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (oValues.Length < 2 || oValues.Length > 3)
			{
				throw new ArgumentException("Bad tuple length");
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

	public enum DateTimeType
	{
		Year,
		YearMonth,
		YearMonthDay,
		YearMonthDayTime
	}

	public struct KMLDateTime
	{
		private static Regex oYearRegex = new Regex(@"^(\d\d\d\d)$");
		private static Regex oYMRegex = new Regex(@"^(\d\d\d\d)-(\d\d)$");
		private static Regex oYMDRegex = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)$");
		private static Regex oUTCDateTime = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)Z$");
		private static Regex oNonUTCDateTime = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)[+-](\d\d):(\d\d)$");

		public DateTime Time;
		public DateTimeType Type;

		public KMLDateTime(String strValue)
		{
			strValue = strValue.Trim();

			if (oYearRegex.IsMatch(strValue))
			{
				Type = DateTimeType.Year;
				Time = new DateTime(Convert.ToInt32(strValue), 1, 1);
			}
			else if (oYMRegex.IsMatch(strValue))
			{
				Type = DateTimeType.YearMonth;
				Time = DateTime.Parse(strValue);
			}
			else if (oYMDRegex.IsMatch(strValue))
			{
				Type = DateTimeType.YearMonthDay;
				Time = DateTime.Parse(strValue);
			}
			else if (oUTCDateTime.IsMatch(strValue) || oNonUTCDateTime.IsMatch(strValue))
			{
				Type = DateTimeType.YearMonthDayTime;
				Time = DateTime.Parse(strValue);
			}
			else
			{
				throw new ArgumentException("Couldn't parse KML DateTime from string " + strValue);
			}
		}
	}

	public struct Vec2
	{
		public double X;
		public double Y;
		public Units XUnits;
		public Units YUnits;

		public Vec2(XmlElement element)
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
				XUnits = (Units)Enum.Parse(typeof(Units), element.GetAttribute("xunits"));
			}
			else
			{
				XUnits = Units.fraction;
			}

			if (element.HasAttribute("yunits"))
			{
				YUnits = (Units)Enum.Parse(typeof(Units), element.GetAttribute("yunits"));
			}
			else
			{
				YUnits = Units.fraction;
			}
		}

		public Vec2(double x, double y, Units xunits, Units yunits)
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

	public class Location : KMLObject
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

		public Location(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Location"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:longitude", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dLongitude = Double.Parse(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:latitude", manager) as XmlElement;
			Debug.Assert(oPointer != null);
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

	public class Orientation : KMLObject
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

		public Orientation(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Location"));

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

		public Orientation()
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

	public class Scale : KMLObject
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

		public Scale(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Location"));

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

		public Scale()
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

	public class Model : Geometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Location m_oLocation;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Orientation m_oOrientation;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Scale m_oScale;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IconOrLink m_oLink;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Dictionary<String, String> m_oResourceMap = new Dictionary<String, String>();

		#endregion


		#region Constructors

		public Model(XmlElement element, XmlNamespaceManager manager, Placemark owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("Model"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Location", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_oLocation = new Location(oPointer, manager);

			oPointer = element.SelectSingleNode("kml:Orientation", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oOrientation = new Orientation(oPointer, manager);
			}
			else
			{
				m_oOrientation = new Orientation();
			}

			oPointer = element.SelectSingleNode("kml:Scale", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oScale = new Scale(oPointer, manager);
			}
			else
			{
				m_oScale = new Scale();
			}

			oPointer = element.SelectSingleNode("kml:Link", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_oLink = new IconOrLink(oPointer, manager);

			foreach (XmlElement oAlias in element.SelectNodes("kml:ResourceMap/kml:Alias", manager))
			{
				String strTargetHref = oAlias.SelectSingleNode("kml:targetHref").ChildNodes[0].Value;
				String strSourceHref = oAlias.SelectSingleNode("kml:sourceHref").ChildNodes[0].Value;

				m_oResourceMap.Add(strSourceHref, strTargetHref);
			}
		}

		#endregion


		#region Properties

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		public Location Location
		{
			get { return m_oLocation; }
		}

		public Orientation Orientation
		{
			get { return m_oOrientation; }
		}

		public Scale Scale
		{
			get { return m_oScale; }
		}

		public IconOrLink Link
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

	public class Region : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LatLonAltBox m_oBox;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Lod m_oLod;

		#endregion


		#region Constructors

		public Region(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Region"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:LatLonAltBox", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_oBox = new LatLonAltBox(oPointer, manager);

			oPointer = element.SelectSingleNode("kml:Lod", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oLod = new Lod(oPointer, manager);
			}
		}

		#endregion


		#region Properties

		public LatLonAltBox LatLonAltBox
		{
			get { return m_oBox; }
		}

		public Lod Lod
		{
			get { return m_oLod; }
		}

		#endregion
	}

	public class Lod : KMLObject
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

		public Lod(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Lod"));

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

	public class LatLonBox : KMLObject
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

		public LatLonBox(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("LatLonBox"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:north", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dNorth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:east", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dEast = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:south", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dSouth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:west", manager) as XmlElement;
			Debug.Assert(oPointer != null);
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

	public class LatLonAltBox : KMLObject
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
		private AltitudeMode? m_eAltitudeMode;

		#endregion


		#region Constructors

		public LatLonAltBox(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("LatLonBox"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:north", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dNorth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:east", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dEast = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:south", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_dSouth = Convert.ToDouble(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:west", manager) as XmlElement;
			Debug.Assert(oPointer != null);
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
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
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

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		#endregion
	}

	#endregion


	#region TimePrimitives ============================================

	public class TimeStamp : TimePrimitive
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oWhen;

		#endregion


		#region Constructors

		public TimeStamp(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("TimeStamp"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:when", manager) as XmlElement;
			Debug.Assert(oPointer != null);
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

	public class TimeSpan : TimePrimitive
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oBegin;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oEnd;

		#endregion


		#region Constructors

		public TimeSpan(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("TimeSpan"));

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

	public class BalloonStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oBGColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oTextColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strText = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private DisplayMode? m_eDisplayMode;

		#endregion


		#region Constructors

		public BalloonStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("BalloonStyle"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:bgColor", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBGColor = ColorStyle.ParseColor(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:textColor", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oTextColor = ColorStyle.ParseColor(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:text", manager) as XmlElement;
			if (oPointer != null)
			{
				m_strText = oPointer.ChildNodes[0].Value;
			}

			oPointer = element.SelectSingleNode("kml:displayMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eDisplayMode = (DisplayMode)Enum.Parse(typeof(DisplayMode), oPointer.ChildNodes[0].Value.ToUpper());
			}
		}

		public BalloonStyle()
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

		public DisplayMode DisplayMode
		{
			get
			{
				if (m_eDisplayMode != null)
				{
					return m_eDisplayMode.Value;
				}
				else
				{
					return DisplayMode.DEFAULT;
				}
			}
		}

		#endregion
	}

	public class LineStyle : ColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fWidth;

		#endregion


		#region Constructors

		public LineStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("LineStyle"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:width", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fWidth = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}
		}

		public LineStyle()
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

	public class ListStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oBGColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ListItemType? m_eType;

		private Dictionary<ItemIconState, String> m_oItemIcons = new Dictionary<ItemIconState, String>();

		#endregion


		#region Constructors

		public ListStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("ListStyle"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:bgColor", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBGColor = ColorStyle.ParseColor(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:listItemType", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eType = (ListItemType)Enum.Parse(typeof(ListItemType), oPointer.ChildNodes[0].Value);
			}

			foreach (XmlElement oItemIconElement in element.SelectNodes("kml:ItemIcon", manager))
			{
				oPointer = element.SelectSingleNode("kml:state", manager) as XmlElement;
				if (oPointer == null) continue;
				ItemIconState oMode = ItemIconState.none;
				foreach (String strMode in oPointer.ChildNodes[0].Value.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries))
				{
					oMode |= (ItemIconState)Enum.Parse(typeof(ItemIconState), strMode);
				}

				oPointer = element.SelectSingleNode("kml:href", manager) as XmlElement;
				if (oPointer == null || oPointer.HasChildNodes == false) continue;

				m_oItemIcons.Add(oMode, oPointer.ChildNodes[0].Value);
			}
		}

		public ListStyle()
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

		public ListItemType ListItemType
		{
			get
			{
				if (m_eType != null)
				{
					return m_eType.Value;
				}
				else
				{
					return ListItemType.check;
				}
			}
		}

		public String GetItemIcon(ItemIconState eMode)
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

	public class IconStyle : ColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fScale;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fHeading;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IconOrLink m_oIcon = null;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Vec2 m_oHotSpot;

		#endregion


		#region Constructors

		public IconStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("IconStyle"));

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
				m_oIcon = new IconOrLink(oPointer, manager);
			}

			oPointer = element.SelectSingleNode("kml:hotSpot", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oHotSpot = new Vec2(oPointer);
			}
			else
			{
				m_oHotSpot = new Vec2(0.5, 0.5, Units.fraction, Units.fraction);
			}
		}

		public IconStyle()
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

		public IconOrLink Icon
		{
			get { return m_oIcon; }
		}

		public Vec2 HotSpot
		{
			get { return m_oHotSpot; }
		}

		#endregion
	}

	public class LabelStyle : ColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fScale;

		#endregion


		#region Constructors

		public LabelStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("LabelStyle"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:scale", manager) as XmlElement;
			if (oPointer != null)
			{
				m_fScale = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}
		}

		public LabelStyle()
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

	public class PolyStyle : ColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blFill;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blOutline;

		#endregion


		#region Constructors

		public PolyStyle(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("PolyStyle"));

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

		public PolyStyle()
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
	public class Style : StyleSelector
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LineStyle m_oLineStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ListStyle m_oListStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IconStyle m_oIconStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LabelStyle m_oLabelStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private PolyStyle m_oPolyStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private BalloonStyle m_oBalloonStyle;

		#endregion


		#region Constructors

		public Style(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Style"));

			XmlElement oPointer;
			oPointer = element.SelectSingleNode("kml:IconStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oIconStyle = new IconStyle(oPointer as XmlElement, manager);
			}
			else
			{
				m_oIconStyle = new IconStyle();
			}

			oPointer = element.SelectSingleNode("kml:LabelStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oLabelStyle = new LabelStyle(oPointer as XmlElement, manager);
			}
			else
			{
				m_oLabelStyle = new LabelStyle();
			}

			oPointer = element.SelectSingleNode("kml:LineStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oLineStyle = new LineStyle(oPointer as XmlElement, manager);
			}
			else
			{
				m_oLineStyle = new LineStyle();
			}

			oPointer = element.SelectSingleNode("kml:PolyStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oPolyStyle = new PolyStyle(oPointer as XmlElement, manager);
			}
			else
			{
				m_oPolyStyle = new PolyStyle();
			}

			oPointer = element.SelectSingleNode("kml:BalloonStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oBalloonStyle = new BalloonStyle(oPointer as XmlElement, manager);
			}
			else
			{
				m_oBalloonStyle = new BalloonStyle();
			}

			oPointer = element.SelectSingleNode("kml:ListStyle", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oListStyle = new ListStyle(oPointer as XmlElement, manager);
			}
			else
			{
				m_oListStyle = new ListStyle();
			}
		}

		public Style()
			: base()
		{
			m_oLineStyle = new LineStyle();
			m_oListStyle = new ListStyle();
			m_oIconStyle = new IconStyle();
			m_oLabelStyle = new LabelStyle();
			m_oPolyStyle = new PolyStyle();
		}

		#endregion


		#region Properties

		public override Style NormalStyle
		{
			get { return this; }
		}

		public override Style HighlightStyle
		{
			get { return this; }
		}

		public LineStyle LineStyle
		{
			get { return m_oLineStyle; }
		}

		public PolyStyle PolyStyle
		{
			get { return m_oPolyStyle; }
		}

		public IconStyle IconStyle
		{
			get { return m_oIconStyle; }
		}

		public LabelStyle LabelStyle
		{
			get { return m_oLabelStyle; }
		}

		public ListStyle ListStyle
		{
			get { return m_oListStyle; }
		}

		#endregion
	}

	[DebuggerDisplay("StyleMap, id = {m_strID}")]
	public class StyleMap : StyleSelector
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strNormalStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strHighlightStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Document m_oOwner;

		#endregion


		#region Constructors

		public StyleMap(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("StyleMap"));

			m_oOwner = owner;

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

		public override Style NormalStyle
		{
			get { return m_oOwner.GetStyle(m_strNormalStyle).NormalStyle; }
		}

		public override Style HighlightStyle
		{
			get { return m_oOwner.GetStyle(m_strHighlightStyle).HighlightStyle; }
		}

		#endregion
	}

	#endregion


	#region Geometry ==================================================

	public class LinearRing : Geometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blExtrude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blTessellate;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<Coordinates> m_oCoords = new List<Coordinates>();

		#endregion


		#region Constructors

		public LinearRing(XmlElement element, XmlNamespaceManager manager, Placemark owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("LinearRing"));

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
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:coordinates", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			String[] oTuples = oPointer.ChildNodes[0].Value.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
			foreach (String strTuple in oTuples)
			{
				m_oCoords.Add(new Coordinates(strTuple));
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

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		public Coordinates this[int i]
		{
			get { return m_oCoords[i]; }
		}

		public int Count
		{
			get { return m_oCoords.Count; }
		}

		#endregion
	}

	public class Polygon : Geometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blExtrude = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blTessellate = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AltitudeMode m_eAltitudeMode = AltitudeMode.clampToGround;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LinearRing m_oOuterBoundary;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<LinearRing> m_oInnerBoundaries = new List<LinearRing>();

		#endregion


		#region Constructors

		public Polygon(XmlElement element, XmlNamespaceManager manager, Placemark owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("Polygon"));

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
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:outerBoundaryIs/kml:LinearRing", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_oOuterBoundary = new LinearRing(oPointer, manager, owner);

			foreach(XmlElement oInnerBound in element.SelectNodes("kml:innerBoundaryIs/kml:LinearRing", manager))
			{
				m_oInnerBoundaries.Add(new LinearRing(oPointer, manager, owner));
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

		public AltitudeMode AltitudeMode
		{
			get { return m_eAltitudeMode; }
		}

		public LinearRing OuterBoundary
		{
			get { return m_oOuterBoundary; }
		}

		public List<LinearRing> InnerBoundaries
		{
			get { return m_oInnerBoundaries; }
		}

		#endregion
	}

	public class Point : Geometry
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool m_blExtrude = false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AltitudeMode m_eAltitudeMode = AltitudeMode.clampToGround;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Coordinates m_oCoords;

		#endregion


		#region Constructors

		public Point(XmlElement element, XmlNamespaceManager manager, Feature owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("Point"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:extrude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_blExtrude = oPointer.ChildNodes[0].Value.Equals("1");
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:coordinates", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_oCoords = new Coordinates(oPointer.ChildNodes[0].Value);
		}

		#endregion


		#region Properties

		public bool Extrude
		{
			get { return m_blExtrude; }
		}

		public AltitudeMode AltitudeMode
		{
			get { return m_eAltitudeMode; }
		}

		public Coordinates Coordinates
		{
			get { return m_oCoords; }
		}

		#endregion
	}

	public class LineString : Geometry
	{
		#region Member Varialbes

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blExtrude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blTessellate;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private List<Coordinates> m_oCoords = new List<Coordinates>();

		#endregion


		#region Constructors

		public LineString(XmlElement element, XmlNamespaceManager manager, Placemark owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("LineString"));

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
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:coordinates", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			String[] oTuples = oPointer.ChildNodes[0].Value.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
			foreach (String strTuple in oTuples)
			{
				m_oCoords.Add(new Coordinates(strTuple));
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

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		public Coordinates this[int i]
		{
			get { return m_oCoords[i]; }
		}

		public int Count
		{
			get { return m_oCoords.Count; }
		}

		#endregion
	}

	public class MultiGeometry : Geometry
	{
		#region Member Variables

		private List<Geometry> m_oChildren;

		#endregion


		#region Constructors

		public MultiGeometry(XmlElement element, XmlNamespaceManager manager, Placemark owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("MultiGeometry"));

			m_oChildren = Geometry.GetGeometries(element, manager, owner);
		}

		#endregion


		#region Properties

		public Geometry this[int i]
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

	public class GroundOverlay : Overlay
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private double? m_dAltitude;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AltitudeMode? m_eAltitudeMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private LatLonBox m_oBox;

		#endregion


		#region Constructors

		public GroundOverlay(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("GroundOverlay"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:altitude", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dAltitude = Convert.ToDouble(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:LatLonBox", manager) as XmlElement;
			Debug.Assert(oPointer != null);
			m_oBox = new LatLonBox(oPointer, manager);
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

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		public LatLonBox LatLonBox
		{
			get
			{
				return m_oBox;
			}
		}

		#endregion
	}

	public class ScreenOverlay : Overlay
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Vec2 m_oOverlayXY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Vec2 m_oScreenXY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Vec2 m_oRotationXY;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Vec2 m_oSize;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_dRotation;

		#endregion


		#region Constructors

		public ScreenOverlay(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("ScreenOverlay"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:overlayXY", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oOverlayXY = new Vec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:screenXY", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oScreenXY = new Vec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:rotationXY", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oRotationXY = new Vec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:size", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oSize = new Vec2(oPointer);
			}

			oPointer = element.SelectSingleNode("kml:rotation", manager) as XmlElement;
			if (oPointer != null)
			{
				m_dRotation = Convert.ToSingle(oPointer.ChildNodes[0].Value);
			}
		}

		#endregion


		#region Properties

		public Vec2 OverlayXY
		{
			get { return m_oOverlayXY; }
		}

		public Vec2 ScreenXY
		{
			get { return m_oScreenXY; }
		}

		public Vec2 RotationXY
		{
			get { return m_oRotationXY; }
		}

		public Vec2 Size
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

	public class PhotoOverlay : Overlay
	{
		#region Member Variables

		private Shape? m_eShape;
		private double? m_dViewVolumeLeftFov;
		private double? m_dViewVolumeRightFov;
		private double? m_dViewVolumeBottomFov;
		private double? m_dViewVolumeTopFov;
		private double? m_dViewVolumeNear;
		private double? m_dRoll;
		private int? m_iImagePyramidTileSize;
		private int m_iImagePyramidMaxWidth;
		private int m_iImagePyramidMaxHeight;
		private GridOrigin? m_eGridOrigin;
		private Point m_oPoint;

		#endregion


		#region Constructors

		public PhotoOverlay(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("PhotoOverlay"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:shape", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eShape = (Shape)Enum.Parse(typeof(Shape), oPointer.ChildNodes[0].Value);
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
				m_eGridOrigin = (GridOrigin)Enum.Parse(typeof(GridOrigin), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:Point", manager) as XmlElement;
			if (oPointer != null)
			{
				m_oPoint = new Point(oPointer, manager, this);
			}
		}

		#endregion


		#region Properties

		public Shape Shape
		{
			get
			{
				if (m_eShape != null)
				{
					return m_eShape.Value;
				}
				else
				{
					return Shape.rectangle;
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

		public GridOrigin ImagePyramidGridOrigin
		{
			get
			{
				if (m_eGridOrigin != null)
				{
					return m_eGridOrigin.Value;
				}
				else
				{
					return GridOrigin.lowerLeft;
				}
			}
		}

		public Point Point
		{
			get { return m_oPoint; }
		}

		#endregion
	}

	#endregion


	#region Features ==================================================

	public class NetworkLink : Feature
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blRefreshVisibility;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blFlyToView;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IconOrLink m_oLink;

		#endregion


		#region Constructors

		public NetworkLink(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("NetworkLink"));

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
			Debug.Assert(oPointer != null);

			m_oLink = new IconOrLink(oPointer, manager);
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

		public IconOrLink Link
		{
			get
			{
				return m_oLink;
			}
		}

		#endregion
	}

	[DebuggerDisplay("Placemark, name = {m_strName}")]
	public class Placemark : Feature
	{
		#region Member Variables

		private Geometry m_oGeometry;

		#endregion


		#region Constructor

		public Placemark(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("Placemark"));

			List<Geometry> oGeometries = Geometry.GetGeometries(element, manager, this);
			Debug.Assert(oGeometries.Count == 1);
			m_oGeometry = oGeometries[0];
		}

		#endregion


		#region Properties

		public Geometry Geometry
		{
			get { return m_oGeometry; }
		}

		#endregion
	}

	#endregion


	#region Containers ================================================

	[DebuggerDisplay("Folder, name = {m_strName}")]
	public class Folder : Container
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private List<Feature> m_oFeatures;

		#endregion


		#region Constructors

		public Folder(XmlElement element, XmlNamespaceManager manager, Document owner)
			: base(element, manager, owner)
		{
			Debug.Assert(element.Name.Equals("Folder"));

			m_oFeatures = Feature.GetFeatures(element, manager, owner);
		}

		#endregion


		#region Properties

		public override Feature this[int i]
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
	public class Document : Container
	{
		#region Member Variables

		private List<Feature> m_oFeatures;
		private List<StyleSelector> m_oSharedStyles;
		//TODO Parse out schema elements

		#endregion


		#region Constructors

		public Document(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager, null)
		{
			Debug.Assert(element.Name.Equals("Document"));

			m_oFeatures = Feature.GetFeatures(element, manager, this);
			m_oSharedStyles = StyleSelector.GetStyleSelectors(element, manager, this);
		}

		#endregion


		#region Properties

		public override Feature this[int i]
		{
			get { return m_oFeatures[i]; }
		}

		public override int Count
		{
			get { return m_oFeatures.Count; }
		}

		#endregion


		#region Public Methods

		public StyleSelector GetStyle(string strStyleURL)
		{
			if (strStyleURL.StartsWith("#"))
			{
				String strStyleID = strStyleURL.Substring(1);

				foreach (StyleSelector oStyles in m_oSharedStyles)
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

	public class Camera : AbstractView
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
		private AltitudeMode? m_eAltitudeMode;

		#endregion


		#region Constructors

		public Camera(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Camera"));

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
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
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

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		#endregion
	}

	public class LookAt : AbstractView
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
		private AltitudeMode? m_eAltitudeMode;

		#endregion


		#region Constructors

		public LookAt(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("LookAt"));

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
			Debug.Assert(oPointer != null);
			m_dRange = Double.Parse(oPointer.ChildNodes[0].Value);

			oPointer = element.SelectSingleNode("kml:altitudeMode", manager) as XmlElement;
			if (oPointer != null)
			{
				m_eAltitudeMode = (AltitudeMode)Enum.Parse(typeof(AltitudeMode), oPointer.ChildNodes[0].Value);
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

		public AltitudeMode AltitudeMode
		{
			get
			{
				if (m_eAltitudeMode != null)
				{
					return m_eAltitudeMode.Value;
				}
				else
				{
					return AltitudeMode.clampToGround;
				}
			}
		}

		#endregion
	}

	#endregion

	public class IconOrLink : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string m_strHRef = String.Empty;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private RefreshMode? m_eRefreshMode;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fRefreshInterval;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ViewRefreshMode? m_eViewRefreshMode;
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

		public IconOrLink(XmlElement element, XmlNamespaceManager manager)
			: base(element, manager)
		{
			Debug.Assert(element.Name.Equals("Icon") || element.Name.Equals("Link"));

			XmlElement oPointer;

			oPointer = element.SelectSingleNode("kml:href", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_strHRef = oPointer.ChildNodes[0].Value.Trim();
			}

			oPointer = element.SelectSingleNode("kml:refreshMode", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_eRefreshMode = (RefreshMode)Enum.Parse(typeof(RefreshMode), oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:refreshInterval", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_fRefreshInterval = Single.Parse(oPointer.ChildNodes[0].Value);
			}

			oPointer = element.SelectSingleNode("kml:viewRefreshMode", manager) as XmlElement;
			if (oPointer != null && oPointer.HasChildNodes)
			{
				m_eViewRefreshMode = (ViewRefreshMode)Enum.Parse(typeof(ViewRefreshMode), oPointer.ChildNodes[0].Value);
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

		public String HRef
		{
			get
			{
				return m_strHRef;
			}
		}

		public RefreshMode RefreshMode
		{
			get
			{
				if (m_eRefreshMode != null)
				{
					return m_eRefreshMode.Value;
				}
				else
				{
					return RefreshMode.onChange;
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

		public ViewRefreshMode ViewRefreshMode
		{
			get
			{
				if (m_eViewRefreshMode != null)
				{
					return m_eViewRefreshMode.Value;
				}
				else
				{
					return ViewRefreshMode.never;
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
	}

	public class KMLFile
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Document m_oDocument;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strFilename;

		#endregion


		#region Constructors

		public KMLFile(String strFilename)
			: this(strFilename, false)
		{
		}

		public KMLFile(String strFilename, bool blValidate)
		{
			try
			{
				m_strFilename = strFilename;

				System.Xml.XmlDocument oDoc = new System.Xml.XmlDocument();
				XmlReaderSettings oSettings = new XmlReaderSettings();
				if (blValidate)
				{
					oSettings.Schemas.Add("http://www.opengis.net/kml/2.2", Path.Combine(Path.GetDirectoryName(strFilename), "ogckml22.xsd"));
					oSettings.ValidationType = ValidationType.Schema;
				}
				XmlReader oDocReader = XmlReader.Create(strFilename, oSettings);

				oDoc.Load(oDocReader);

				Debug.Assert(oDoc.DocumentElement.Name.Equals("kml"));

				System.Xml.XmlNamespaceManager oManager = new System.Xml.XmlNamespaceManager(oDoc.NameTable);
				oManager.AddNamespace("kml", oDoc.DocumentElement.NamespaceURI);

				XmlElement oPointer;

				oPointer = oDoc.DocumentElement.SelectSingleNode("kml:Document", oManager) as XmlElement;
				Debug.Assert(oPointer != null);
				m_oDocument = new Dapple.KML.Document(oPointer, oManager);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message.ToString());
			}
		}

		#endregion


		#region Properties

		public Document Document
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

		#endregion
	}

	#endregion
}