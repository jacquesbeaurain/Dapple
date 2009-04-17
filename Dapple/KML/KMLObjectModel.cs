using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System.Globalization;

namespace Dapple.KML
{
	#region Abstract ============================================================

	internal abstract class KMLObject
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

		internal KMLObject(XmlElement element, KMLFile source)
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

		internal String ID
		{
			get { return m_strID; }
		}

		internal String TargetID
		{
			get { return m_strTargetID; }
		}

		#endregion
	}

	internal abstract class KMLStyleSelector : KMLObject
	{
		#region Constructors

		internal KMLStyleSelector(XmlElement element, KMLFile source)
			: base(element, source)
		{
		}

		internal KMLStyleSelector()
			: base()
		{
		}

		#endregion


		#region Properties

		internal abstract KMLStyle NormalStyle { get; }
		internal abstract KMLStyle HighlightStyle { get; }

		#endregion


		#region Static Parsers

		internal static List<KMLStyleSelector> GetStyleSelectors(XmlElement oElement, KMLFile oSource)
		{
			List<KMLStyleSelector> result = new List<KMLStyleSelector>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oPointer = oChild as XmlElement;

				if (oPointer.Name.Equals("Style"))
				{
					result.Add(new KMLStyle(oPointer, oSource));
				}
				else if (oPointer.Name.Equals("StyleMap"))
				{
					result.Add(new KMLStyleMap(oPointer, oSource));
				}
			}

			return result;
		}

		#endregion
	}

	internal abstract class KMLFeature : KMLObject
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

		internal KMLFeature(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_oInlineStyles = KMLStyleSelector.GetStyleSelectors(element, source);

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (!(oChild.NodeType == XmlNodeType.Element)) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("name"))
				{
					m_strName = oChildElement.InnerText;
				}
				else if (oChildElement.Name.Equals("visibility"))
				{
					m_blVisibility = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("open"))
				{
					m_blOpen = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("styleUrl"))
				{
					m_strStyleURL = oChildElement.InnerText;
				}
				else if (oChildElement.Name.Equals("Snippet"))
				{
					m_strSnippet = oChildElement.InnerText;
				}
				else if (oChildElement.Name.Equals("description"))
				{
					m_strDescription = oChildElement.InnerText;
				}
				else if (oChildElement.Name.Equals("Region"))
				{
					m_oRegion = new KMLRegion(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("TimeStamp"))
				{
					m_oTime = new KMLTimeStamp(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("TimeSpan"))
				{
					m_oTime = new KMLTimeSpan(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("Camera"))
				{
					m_oView = new KMLCamera(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("LookAt"))
				{
					m_oView = new KMLLookAt(oChildElement, source);
				}
			}
		}

		#endregion


		#region Properties

		internal String Name
		{
			get { return m_strName; }
		}

		internal bool Visibility
		{
			get { return m_blVisibility; }
		}

		internal bool Open
		{
			get { return m_blOpen; }
		}

		internal String Snippet
		{
			get
			{
				return m_strSnippet;
			}
		}

		internal String Description
		{
			get
			{
				return m_strDescription;
			}
		}

		internal KMLStyleSelector Style
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

		internal KMLRegion Region
		{
			get { return m_oRegion; }
		}

		internal KLMTimePrimitive Time
		{
			get { return m_oTime; }
		}

		internal KMLAbstractView View
		{
			get { return m_oView; }
		}

		#endregion


		#region Static Parsers

		internal static List<KMLFeature> GetFeatures(XmlElement oElement, KMLFile oSource)
		{
			List<KMLFeature> result = new List<KMLFeature>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("NetworkLink"))
				{
					result.Add(new KMLNetworkLink(oChildElement, oSource));
				}
				else if (oChildElement.Name.Equals("Placemark"))
				{
					result.Add(new KMLPlacemark(oChildElement, oSource));
				}
				else if (oChildElement.Name.Equals("PhotoOverlay"))
				{
					result.Add(new KMLPhotoOverlay(oChildElement, oSource));
				}
				else if (oChildElement.Name.Equals("ScreenOverlay"))
				{
					result.Add(new KMLScreenOverlay(oChildElement, oSource));
				}
				else if (oChildElement.Name.Equals("GroundOverlay"))
				{
					result.Add(new KMLGroundOverlay(oChildElement, oSource));
				}
				else if (oChildElement.Name.Equals("Folder"))
				{
					result.Add(new KMLFolder(oChildElement, oSource));
				}
			}

			return result;
		}

		#endregion
	}

	internal abstract class KMLContainer : KMLFeature
	{
		#region Constructors

		internal KMLContainer(XmlElement element, KMLFile source)
			: base(element, source)
		{
		}

		#endregion

		#region Properties

		internal abstract KMLFeature this[int i] { get; }

		internal abstract int Count { get; }

		#endregion
	}

	internal abstract class KMLOverlay : KMLFeature
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

		internal KMLOverlay(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("color"))
				{
					m_oColor = KMLColorStyle.ParseColor(oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("drawOrder"))
				{
					m_iDrawOrder = Int32.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("Icon"))
				{
					m_oIcon = new KMLIconOrLink(oChildElement, source);
				} 
			}
		}

		#endregion


		#region Properties

		internal Color Color
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

		internal int DrawOrder
		{
			get { return m_iDrawOrder; }
		}

		internal KMLIconOrLink Icon
		{
			get { return m_oIcon; }
		}

		#endregion
	}

	internal abstract class KMLGeometry : KMLObject
	{
		#region Member Variables

		private KMLFeature m_oOwner;

		#endregion


		#region Constructors

		internal KMLGeometry(XmlElement element, KMLFeature owner, KMLFile source)
			: base(element, source)
		{
			m_oOwner = owner;
		}

		#endregion


		#region Properties

		internal KMLFeature Owner
		{
			get { return m_oOwner; }
		}

		internal KMLStyleSelector Style
		{
			get
			{
				return m_oOwner.Style;
			}
		}

		#endregion


		#region Static Parsers

		internal static List<KMLGeometry> GetGeometries(XmlElement oElement, KMLPlacemark oOwner, KMLFile oSource)
		{
			List<KMLGeometry> result = new List<KMLGeometry>();

			foreach (XmlNode oChild in oElement.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("Point"))
				{
					result.Add(new KMLPoint(oChildElement, oOwner, oSource));
				}
				else if (oChildElement.Name.Equals("LineString"))
				{
					result.Add(new KMLLineString(oChildElement, oOwner, oSource));
				}
				else if (oChildElement.Name.Equals("LinearRing"))
				{
					result.Add(new KMLLinearRing(oChildElement, oOwner, oSource));
				}
				else if (oChildElement.Name.Equals("Polygon"))
				{
					result.Add(new KMLPolygon(oChildElement, oOwner, oSource));
				}
				else if (oChildElement.Name.Equals("MultiGeometry"))
				{
					result.Add(new KMLMultiGeometry(oChildElement, oOwner, oSource));
				}
				else if (oChildElement.Name.Equals("Model"))
				{
					result.Add(new KMLModel(oChildElement, oOwner, oSource));
				}
			}

			return result;
		}

		#endregion
	}

	internal abstract class KMLColorStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLColorMode? m_eColorMode;

		#endregion


		#region Constructors

		internal KMLColorStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("color"))
				{
					m_oColor = ParseColor(oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("colorMode"))
				{
					m_eColorMode = (KMLColorMode)Enum.Parse(typeof(KMLColorMode), oChildElement.InnerText);
				} 
			}
		}

		internal KMLColorStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal Color Color
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

		internal KMLColorMode ColorMode
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

		internal static Color ParseColor(String strColor)
		{
			// --- This is a hack: some color tags encountered starting with '#' ---
			strColor = strColor.Replace("#", String.Empty);

			return Color.FromArgb(
						  Int32.Parse(strColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture),
						  Int32.Parse(strColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture),
						  Int32.Parse(strColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture),
						  Int32.Parse(strColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture)
						  );
		}

		#endregion
	}

	internal abstract class KLMTimePrimitive : KMLObject
	{
		#region Constructors

		internal KLMTimePrimitive(XmlElement element, KMLFile source)
			: base(element, source)
		{
		}

		#endregion
	}

	internal abstract class KMLAbstractView : KMLObject
	{
		#region Constructor

		internal KMLAbstractView(XmlElement element, KMLFile source)
			: base(element, source)
		{
		}

		#endregion
	}

	#endregion


	#region Enums and Structs ===================================================

	internal enum KMLGridOrigin
	{
		lowerLeft,
		upperLeft
	}

	internal enum KMLShape
	{
		rectangle,
		cylinder,
		sphere
	}

	internal enum KLMRefreshMode
	{
		onChange,
		onInterval,
		onExpire
	}

	internal enum KMLViewRefreshMode
	{
		never,
		onStop,
		onRequest,
		onRegion
	}

	internal enum KMLAltitudeMode
	{
		clampToGround,
		relativeToGround,
		absolute
	}

	internal enum KMLColorMode
	{
		normal,
		random
	}

	internal enum KMLDisplayMode
	{
		DEFAULT,
		HIDE
	}

	internal enum KMLUnits
	{
		pixels,
		fraction,
		insetPixels
	}

	internal enum KMLListItemType
	{
		check,
		checkOffOnly,
		checkHideChildren,
		radioFolder
	}

	[FlagsAttribute]
	internal enum KMLItemIconState
	{
		none = 0,
		open = 1 << 0,
		closed = 1 << 1,
		error = 1 << 2,
		fetching0 = 1 << 3,
		fetching1 = 1 << 4,
		fetching2 = 1 << 5
	}

	internal enum KMLDateTimeType
	{
		Year,
		YearMonth,
		YearMonthDay,
		YearMonthDayTime
	}

	[DebuggerDisplay("Lat = {Latitude}  Lon = {Longitude}  Alt = {Altitude}")]
	internal struct KMLCoordinates
	{
		internal double Latitude;
		internal double Longitude;
		internal double Altitude;

		internal KMLCoordinates(double dLat, double dLon, double dAlt)
		{
			Latitude = dLat;
			Longitude = dLon;
			Altitude = dAlt;
		}

		internal KMLCoordinates(String strTuple)
		{
			String[] oValues = strTuple.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (oValues.Length < 2 || oValues.Length > 3)
			{
				throw new ArgumentException("The KML file contains a malformed 'Coordinates' element.");
			}

			Longitude = Double.Parse(oValues[0], CultureInfo.InvariantCulture);
			Latitude = Double.Parse(oValues[1], CultureInfo.InvariantCulture);
			if (oValues.Length == 3)
			{
				Altitude = Double.Parse(oValues[2], CultureInfo.InvariantCulture);
			}
			else
			{
				Altitude = 0.0;
			}
		}
	}

	internal struct KMLDateTime
	{
		private static Regex oYearRegex = new Regex(@"^(\d\d\d\d)$");
		private static Regex oYMRegex = new Regex(@"^(\d\d\d\d)-(\d\d)$");
		private static Regex oYMDRegex = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)$");
		private static Regex oUTCDateTime = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)Z$");
		private static Regex oNonUTCDateTime = new Regex(@"^(\d\d\d\d)-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)[+-](\d\d):(\d\d)$");

		internal DateTime Time;
		internal KMLDateTimeType Type;

		internal KMLDateTime(String strValue)
		{
			strValue = strValue.Trim();

			if (oYearRegex.IsMatch(strValue))
			{
				Type = KMLDateTimeType.Year;
				Time = new DateTime(Int32.Parse(strValue, CultureInfo.InvariantCulture), 1, 1);
			}
			else if (oYMRegex.IsMatch(strValue))
			{
				Type = KMLDateTimeType.YearMonth;
				Time = DateTime.Parse(strValue, CultureInfo.InvariantCulture);
			}
			else if (oYMDRegex.IsMatch(strValue))
			{
				Type = KMLDateTimeType.YearMonthDay;
				Time = DateTime.Parse(strValue, CultureInfo.InvariantCulture);
			}
			else if (oUTCDateTime.IsMatch(strValue) || oNonUTCDateTime.IsMatch(strValue))
			{
				Type = KMLDateTimeType.YearMonthDayTime;
				Time = DateTime.Parse(strValue, CultureInfo.InvariantCulture);
			}
			else
			{
				throw new ArgumentException("The KML file contains a malformed 'DateTime' element.");
			}
		}
	}

	internal struct KMLVec2
	{
		internal double X;
		internal double Y;
		internal KMLUnits XUnits;
		internal KMLUnits YUnits;

		internal KMLVec2(XmlElement element)
		{
			if (element.HasAttribute("x"))
			{
				X = Double.Parse(element.GetAttribute("x"), CultureInfo.InvariantCulture);
			}
			else
			{
				X = 0.0;
			}

			if (element.HasAttribute("y"))
			{
				Y = Double.Parse(element.GetAttribute("y"), CultureInfo.InvariantCulture);
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

		internal KMLVec2(double x, double y, KMLUnits xunits, KMLUnits yunits)
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

	internal class KMLLocation : KMLObject
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

		internal KMLLocation(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_dLongitude = Double.MinValue;
			m_dLatitude = Double.MinValue;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("longitude"))
				{
					m_dLongitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("latitude"))
				{
					m_dLatitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitude"))
				{
					m_dAltitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				} 
			}

			if (m_dLongitude == Double.MinValue) throw new ArgumentException("The KML file contains a 'Location' element without a 'Longitude' element.");
			if (m_dLatitude == Double.MinValue) throw new ArgumentException("The KML file contains a 'Location' element without a 'Latitude' element.");
		}

		#endregion

		#region Properties

		internal double Longitude
		{
			get { return m_dLongitude; }
		}

		internal double Latitude
		{
			get { return m_dLatitude; }
		}

		internal double Altitude
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

	internal class KMLOrientation : KMLObject
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

		internal KMLOrientation(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("heading"))
				{
					m_dHeading = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("tilt"))
				{
					m_dTilt = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("roll"))
				{
					m_dRoll = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				} 
			}
		}

		internal KMLOrientation()
			: base()
		{
		}

		#endregion


		#region Properties

		internal double Heading
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

		internal double Tilt
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

		internal double Roll
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

	internal class KMLScale : KMLObject
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

		internal KMLScale(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("x"))
				{
					m_dX = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("y"))
				{
					m_dY = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("z"))
				{
					m_dZ = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				} 
			}
		}

		internal KMLScale()
			: base()
		{
		}

		#endregion


		#region Properties

		internal double X
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

		internal double Y
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

		internal double Z
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

	internal class KMLModel : KMLGeometry
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

		internal KMLModel(XmlElement element, KMLPlacemark owner, KMLFile source)
			: base(element, owner, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_oLocation = new KMLLocation(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("Orientation"))
				{
					m_oOrientation = new KMLOrientation(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("Scale"))
				{
					m_oScale = new KMLScale(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("Link"))
				{
					m_oLink = new KMLIconOrLink(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("ResourceMap"))
				{
					foreach (XmlNode oResourceChild in oChildElement.ChildNodes)
					{
						if (oResourceChild.NodeType != XmlNodeType.Element) continue;
						XmlElement oResourceChildElement = oResourceChild as XmlElement;

						if (oResourceChildElement.Name.Equals("Alias"))
						{
							String strTargetHref = String.Empty;
							String strSourceHref = String.Empty;

							foreach (XmlNode oAliasChild in oResourceChildElement.ChildNodes)
							{
								if (oAliasChild.NodeType != XmlNodeType.Element) continue;
								XmlElement oAliasChildElement = oAliasChild as XmlElement;

								if (oAliasChildElement.Name.Equals("targetHref"))
									strTargetHref = oAliasChildElement.InnerText;
								if (oAliasChildElement.Name.Equals("sourceHref"))
									strTargetHref = oAliasChildElement.InnerText;
							}

							if (!String.IsNullOrEmpty(strTargetHref) && !String.IsNullOrEmpty(strSourceHref))
							{
								m_oResourceMap.Add(strSourceHref, strTargetHref);
							}
						}
					}
				}
			}

			if (m_oLocation == null) throw new ArgumentException("The KML file contains a 'Model' element without a 'Location' element.");
			if (m_oLink == null) throw new ArgumentException("The KML file contains a 'Model' element without a 'Link' element.");

			if (m_oOrientation == null) m_oOrientation = new KMLOrientation();
			if (m_oScale == null) m_oScale = new KMLScale();
		}

		#endregion


		#region Properties

		internal KMLAltitudeMode AltitudeMode
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

		internal KMLLocation Location
		{
			get { return m_oLocation; }
		}

		internal KMLOrientation Orientation
		{
			get { return m_oOrientation; }
		}

		internal KMLScale Scale
		{
			get { return m_oScale; }
		}

		internal KMLIconOrLink Link
		{
			get { return m_oLink; }
		}

		internal String MapResource(String strSource)
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

	internal class KMLRegion : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLatLonAltBox m_oBox;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLLod m_oLod;

		#endregion


		#region Constructors

		internal KMLRegion(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("LatLonAltBox"))
				{
					m_oBox = new KMLLatLonAltBox(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("LatLonAltBox"))
				{
					m_oLod = new KMLLod(oChildElement, source);
				} 
			}

			if (m_oBox == null) throw new ArgumentException("The KML file contains a 'Region' element without a 'LatLonAltBox' element.");
		}

		#endregion


		#region Properties

		internal KMLLatLonAltBox LatLonAltBox
		{
			get { return m_oBox; }
		}

		internal KMLLod Lod
		{
			get { return m_oLod; }
		}

		#endregion
	}

	internal class KMLLod : KMLObject
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

		internal KMLLod(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("minLodPixels"))
				{
					m_fMinLodPixels = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("maxLodPixels"))
				{
					m_fMaxLodPixels = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("minFadeExtent"))
				{
					m_fMinFadeExtent = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("maxFadeExtent"))
				{
					m_fMaxFadeExtent = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				} 
			}
		}

		#endregion


		#region Properties

		internal float MinLoDPixels
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

		internal float MaxLoDPixels
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

		internal float MinFadeExtent
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

		internal float MaxFadeExtent
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

	internal class KMLLatLonBox : KMLObject
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

		internal KMLLatLonBox(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_dNorth = Double.MinValue;
			m_dEast = Double.MinValue;
			m_dSouth = Double.MinValue;
			m_dWest = Double.MinValue;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("north"))
				{
					m_dNorth = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("east"))
				{
					m_dEast = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("south"))
				{
					m_dSouth = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("west"))
				{
					m_dWest = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("rotation"))
				{
					m_dRotation = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				} 
			}

			if (m_dNorth == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'north' element.");
			if (m_dEast == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'east' element.");
			if (m_dSouth == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'south' element.");
			if (m_dWest == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonBox' element without a 'west' element.");
		}

		#endregion


		#region Properties

		internal double North
		{
			get { return m_dNorth; }
		}

		internal double East
		{
			get { return m_dEast; }
		}

		internal double South
		{
			get { return m_dSouth; }
		}

		internal double West
		{
			get { return m_dWest; }
		}

		internal double Rotation
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

	internal class KMLLatLonAltBox : KMLObject
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

		internal KMLLatLonAltBox(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_dNorth = Double.MinValue;
			m_dEast = Double.MinValue;
			m_dSouth = Double.MinValue;
			m_dWest = Double.MinValue;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("north"))
				{
					m_dNorth = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("east"))
				{
					m_dEast = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("south"))
				{
					m_dSouth = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("west"))
				{
					m_dWest = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("minAltitude"))
				{
					m_dMinAltitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("maxAltitude"))
				{
					m_dMaxAltitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
			}

			if (m_dNorth == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'north' element.");
			if (m_dEast == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'east' element.");
			if (m_dSouth == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'south' element.");
			if (m_dWest == Double.MinValue) throw new ArgumentException("The KML file contains a 'LatLonAltBox' element without a 'west' element.");
		}

		#endregion


		#region Properties

		internal double North
		{
			get { return m_dNorth; }
		}

		internal double East
		{
			get { return m_dEast; }
		}

		internal double South
		{
			get { return m_dSouth; }
		}

		internal double West
		{
			get { return m_dWest; }
		}

		internal double MinAltitude
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

		internal double MaxAltitude
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

		internal KMLAltitudeMode AltitudeMode
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

	internal class KMLTimeStamp : KLMTimePrimitive
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oWhen;

		#endregion


		#region Constructors

		internal KMLTimeStamp(XmlElement element, KMLFile source)
			: base(element, source)
		{
			bool blWhenSet = false;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("when"))
				{
					m_oWhen = new KMLDateTime(oChildElement.InnerText); 
					blWhenSet = true;
				}				
			}

			if (!blWhenSet) throw new ArgumentException("The KML file contains a 'TimeStamp' element without a 'when' element.");
		}

		#endregion


		#region Properties

		internal KMLDateTime When
		{
			get
			{
				return m_oWhen;
			}
		}

		#endregion
	}

	internal class KMLTimeSpan : KLMTimePrimitive
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oBegin;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLDateTime m_oEnd;

		#endregion


		#region Constructors

		internal KMLTimeSpan(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChild.Name.Equals("begin"))
				{
					m_oBegin = new KMLDateTime(oChildElement.InnerText);
				}
				else if (oChild.Name.Equals("end"))
				{
					m_oEnd = new KMLDateTime(oChildElement.InnerText);
				} 
			}
		}

		#endregion


		#region Properties

		internal KMLDateTime Begin
		{
			get { return m_oBegin; }
		}

		internal KMLDateTime End
		{
			get { return m_oEnd; }
		}

		#endregion
	}

	#endregion


	#region Styles ====================================================

	internal class KMLBalloonStyle : KMLObject
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

		internal KMLBalloonStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("bgColor"))
				{
					m_oBGColor = KMLColorStyle.ParseColor(oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("textColor"))
				{
					m_oTextColor = KMLColorStyle.ParseColor(oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("text"))
				{
					m_strText = oChildElement.InnerText;
				}
				else if (oChildElement.Name.Equals("displayMode"))
				{
					m_eDisplayMode = (KMLDisplayMode)Enum.Parse(typeof(KMLDisplayMode), oChildElement.InnerText.ToUpper());
				}
			}
		}

		internal KMLBalloonStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal Color BackgroundColor
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

		internal Color TextColor
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

		internal String Text
		{
			get
			{
				return m_strText;
			}
		}

		internal KMLDisplayMode DisplayMode
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

	internal class KMLLineStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fWidth;

		#endregion


		#region Constructors

		internal KMLLineStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("width"))
				{
					m_fWidth = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
			}
		}

		internal KMLLineStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal float Width
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

	internal class KMLListStyle : KMLObject
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private Color? m_oBGColor;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private KMLListItemType? m_eType;

		private Dictionary<KMLItemIconState, String> m_oItemIcons = new Dictionary<KMLItemIconState, String>();

		#endregion


		#region Constructors

		internal KMLListStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("bgColor"))
				{
					m_oBGColor = KMLColorStyle.ParseColor(oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("listItemType"))
				{
					m_eType = (KMLListItemType)Enum.Parse(typeof(KMLListItemType), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("ItemIcon"))
				{
					KMLItemIconState eState = KMLItemIconState.none;
					String strHref = null;

					foreach (XmlNode oItemIconChild in oChildElement.ChildNodes)
					{
						if (oItemIconChild.NodeType != XmlNodeType.Element) continue;
						XmlElement oItemIconChildElement = oItemIconChild as XmlElement;

						if (oItemIconChildElement.Name.Equals("state"))
						{
							eState = KMLItemIconState.none;

							foreach (String strMode in oItemIconChildElement.InnerText.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries))
							{
								eState |= (KMLItemIconState)Enum.Parse(typeof(KMLItemIconState), strMode);
							}
						}
						else if (oItemIconChildElement.Name.Equals("href"))
						{
							strHref = oItemIconChildElement.InnerText;
						}
					}

					if (eState != KMLItemIconState.none && !String.IsNullOrEmpty(strHref))
					{
						m_oItemIcons.Add(eState, strHref);
					}
				}
			}
		}

		internal KMLListStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal Color BackgroundColor
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

		internal KMLListItemType ListItemType
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

		internal String GetItemIcon(KMLItemIconState eMode)
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

	internal class KMLIconStyle : KMLColorStyle
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

		internal KMLIconStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			bool blHotSpotSet = false;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("scale"))
				{
					m_fScale = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("heading"))
				{
					m_fHeading = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("Icon"))
				{
					m_oIcon = new KMLIconOrLink(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("hotSpot"))
				{
					m_oHotSpot = new KMLVec2(oChildElement);
					blHotSpotSet = true;
				}
			}

			if (!blHotSpotSet) m_oHotSpot = new KMLVec2(0.5, 0.5, KMLUnits.fraction, KMLUnits.fraction);
		}

		internal KMLIconStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal float Scale
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

		internal float Heading
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

		internal KMLIconOrLink Icon
		{
			get { return m_oIcon; }
		}

		internal KMLVec2 HotSpot
		{
			get { return m_oHotSpot; }
		}

		#endregion
	}

	internal class KMLLabelStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private float? m_fScale;

		#endregion


		#region Constructors

		internal KMLLabelStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("scale"))
				{
					m_fScale = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				} 
			}
		}

		internal KMLLabelStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal float Scale
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

	internal class KMLPolyStyle : KMLColorStyle
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blFill;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private bool? m_blOutline;

		#endregion


		#region Constructors

		internal KMLPolyStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("fill"))
				{
					m_blFill = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("outline"))
				{
					m_blOutline = oChildElement.InnerText.Equals("1");
				}
			}
		}

		internal KMLPolyStyle()
			: base()
		{
		}

		#endregion


		#region Properties

		internal bool Fill
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

		internal bool Outline
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
	internal class KMLStyle : KMLStyleSelector
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

		internal KMLStyle(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChildNode in element.ChildNodes)
			{
				if (oChildNode.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChildNode as XmlElement;

				if (oChildElement.Name.Equals("IconStyle"))
				{
					m_oIconStyle = new KMLIconStyle(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("LabelStyle"))
				{
					m_oLabelStyle = new KMLLabelStyle(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("LineStyle"))
				{
					m_oLineStyle = new KMLLineStyle(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("PolyStyle"))
				{
					m_oPolyStyle = new KMLPolyStyle(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("BalloonStyle"))
				{
					m_oBalloonStyle = new KMLBalloonStyle(oChildElement, source);
				}
				else if (oChildElement.Name.Equals("ListStyle"))
				{
					m_oListStyle = new KMLListStyle(oChildElement, source);
				}
			}

			if (m_oIconStyle == null) m_oIconStyle = new KMLIconStyle();
			if (m_oLabelStyle == null) m_oLabelStyle = new KMLLabelStyle();
			if (m_oLineStyle == null) m_oLineStyle = new KMLLineStyle();
			if (m_oPolyStyle == null) m_oPolyStyle = new KMLPolyStyle();
			if (m_oBalloonStyle == null) m_oBalloonStyle = new KMLBalloonStyle();
			if (m_oListStyle == null) m_oListStyle = new KMLListStyle();
		}

		internal KMLStyle()
			: base()
		{
			m_oLineStyle = new KMLLineStyle();
			m_oListStyle = new KMLListStyle();
			m_oIconStyle = new KMLIconStyle();
			m_oLabelStyle = new KMLLabelStyle();
			m_oPolyStyle = new KMLPolyStyle();
			m_oBalloonStyle = new KMLBalloonStyle();
		}

		#endregion


		#region Properties

		internal override KMLStyle NormalStyle
		{
			get { return this; }
		}

		internal override KMLStyle HighlightStyle
		{
			get { return this; }
		}

		internal KMLLineStyle LineStyle
		{
			get { return m_oLineStyle; }
		}

		internal KMLPolyStyle PolyStyle
		{
			get { return m_oPolyStyle; }
		}

		internal KMLIconStyle IconStyle
		{
			get { return m_oIconStyle; }
		}

		internal KMLLabelStyle LabelStyle
		{
			get { return m_oLabelStyle; }
		}

		internal KMLListStyle ListStyle
		{
			get { return m_oListStyle; }
		}

		internal KMLBalloonStyle BalloonStyle
		{
			get { return m_oBalloonStyle; }
		}

		#endregion
	}

	[DebuggerDisplay("StyleMap, id = {m_strID}")]
	internal class KMLStyleMap : KMLStyleSelector
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strNormalStyle;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private String m_strHighlightStyle;

		#endregion


		#region Constructors

		internal KMLStyleMap(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("Pair"))
				{
					if (oChildElement.ChildNodes[0].InnerText.Equals("normal"))
					{
						m_strNormalStyle = oChildElement.ChildNodes[1].InnerText;
					}
					else
					{
						m_strHighlightStyle = oChildElement.ChildNodes[1].InnerText;
					}
				}

			}
		}

		#endregion


		#region Properties

		internal override KMLStyle NormalStyle
		{
			get { return m_oSourceFile.GetStyle(m_strNormalStyle).NormalStyle; }
		}

		internal override KMLStyle HighlightStyle
		{
			get { return m_oSourceFile.GetStyle(m_strHighlightStyle).HighlightStyle; }
		}

		#endregion
	}

	#endregion


	#region Geometry ==================================================

	internal class KMLLinearRing : KMLGeometry
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

		internal KMLLinearRing(XmlElement element, KMLPlacemark owner, KMLFile source)
			: base(element, owner, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("extrude"))
				{
					m_blExtrude = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("tessellate"))
				{
					m_blTessellate = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("coordinates"))
				{
					String[] oTuples = oChildElement.InnerText.Replace(", ", ",").Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
					foreach (String strTuple in oTuples)
					{
						m_oCoords.Add(new KMLCoordinates(strTuple));
					}
				}
			}

			if (m_oCoords.Count == 0) throw new ArgumentException("The KML file contains a 'LinearRing' element without a 'coordinates' element.");
		}

		#endregion


		#region Properties

		internal bool Extrude
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

		internal bool Tessellate
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

		internal KMLAltitudeMode AltitudeMode
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

		internal KMLCoordinates this[int i]
		{
			get { return m_oCoords[i]; }
		}

		internal int Count
		{
			get { return m_oCoords.Count; }
		}

		#endregion
	}

	internal class KMLPolygon : KMLGeometry
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

		internal KMLPolygon(XmlElement element, KMLPlacemark owner, KMLFile source)
			: base(element, owner, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("extrude"))
				{
					m_blExtrude = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("extrude"))
				{
					m_blTessellate = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("outerBoundaryIs"))
				{
					foreach (XmlNode oOuterBoundaryNode in oChildElement)
					{
						if (oOuterBoundaryNode.NodeType != XmlNodeType.Element) continue;
						XmlElement oOuterBoundaryElement = oOuterBoundaryNode as XmlElement;

						if (oOuterBoundaryElement.Name.Equals("LinearRing"))
						{
							m_oOuterBoundary = new KMLLinearRing(oOuterBoundaryElement, owner, source);
						}
					}
				}
				else if (oChildElement.Name.Equals("innerBoundaryIs"))
				{
					foreach (XmlNode oInnerBoundaryNode in oChildElement)
					{
						if (oInnerBoundaryNode.NodeType != XmlNodeType.Element) continue;
						XmlElement oInnerBoundaryElement = oInnerBoundaryNode as XmlElement;

						if (oInnerBoundaryElement.Name.Equals("LinearRing"))
						{
							m_oInnerBoundaries.Add(new KMLLinearRing(oInnerBoundaryElement, owner, source));
						}
					}
				}
			}

			if (m_oOuterBoundary == null) throw new ArgumentException("The KML file contains a 'Polygon' element without an 'outerBoundaryIs' element.");
		}

		#endregion


		#region Properties

		internal bool Extrude
		{
			get { return m_blExtrude; }
		}

		internal bool Tessellate
		{
			get { return m_blTessellate; }
		}

		internal KMLAltitudeMode AltitudeMode
		{
			get { return m_eAltitudeMode; }
		}

		internal KMLLinearRing OuterBoundary
		{
			get { return m_oOuterBoundary; }
		}

		internal List<KMLLinearRing> InnerBoundaries
		{
			get { return m_oInnerBoundaries; }
		}

		#endregion
	}

	internal class KMLPoint : KMLGeometry
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

		internal KMLPoint(XmlElement element, KMLFeature owner, KMLFile source)
			: base(element, owner, source)
		{
			bool blCoordinatesSet = false;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("extrude"))
				{
					m_blExtrude = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("coordinates"))
				{
					m_oCoords = new KMLCoordinates(oChildElement.InnerText.Trim());
					blCoordinatesSet = true;
				}
			}

			if (!blCoordinatesSet) throw new ArgumentException("The KML file contains a 'Point' element without a 'Coordinates' element.");
		}

		#endregion


		#region Properties

		internal bool Extrude
		{
			get { return m_blExtrude; }
		}

		internal KMLAltitudeMode AltitudeMode
		{
			get { return m_eAltitudeMode; }
		}

		internal KMLCoordinates Coordinates
		{
			get { return m_oCoords; }
		}

		#endregion
	}

	internal class KMLLineString : KMLGeometry
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

		internal KMLLineString(XmlElement element, KMLPlacemark owner, KMLFile source)
			: base(element, owner, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("extrude"))
				{
					m_blExtrude = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("tessellate"))
				{
					m_blTessellate = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("coordinates"))
				{
					String[] oTuples = oChildElement.InnerText.Replace(", ", ",").Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
					foreach (String strTuple in oTuples)
					{
						m_oCoords.Add(new KMLCoordinates(strTuple));
					}
				}
			}

			if (m_oCoords.Count == 0) throw new ArgumentException("The KML file contains a 'LinearRing' element without a 'coordinates' element.");
		}

		#endregion


		#region Properties

		internal bool Extrude
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

		internal bool Tessellate
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

		internal KMLAltitudeMode AltitudeMode
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

		internal KMLCoordinates this[int i]
		{
			get { return m_oCoords[i]; }
		}

		internal int Count
		{
			get { return m_oCoords.Count; }
		}

		#endregion
	}

	internal class KMLMultiGeometry : KMLGeometry
	{
		#region Member Variables

		private List<KMLGeometry> m_oChildren;

		#endregion


		#region Constructors

		internal KMLMultiGeometry(XmlElement element, KMLPlacemark owner, KMLFile source)
			: base(element, owner, source)
		{
			m_oChildren = KMLGeometry.GetGeometries(element, owner, source);
		}

		#endregion


		#region Properties

		internal KMLGeometry this[int i]
		{
			get { return m_oChildren[i]; }
		}

		internal int Count
		{
			get { return m_oChildren.Count; }
		}

		#endregion
	}

	#endregion


	#region Overlays ==================================================

	internal class KMLGroundOverlay : KMLOverlay
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

		internal KMLGroundOverlay(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("altitude"))
				{
					m_dAltitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("LatLonBox"))
				{
					m_oBox = new KMLLatLonBox(oChildElement, source);
				}
			}

			if (m_oBox == null) throw new ArgumentException("The KML file contains a 'GroundOverlay' element without a 'LatLonBox' element.");
		}

		#endregion


		#region Properties

		internal double Altitude
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

		internal KMLAltitudeMode AltitudeMode
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

		internal KMLLatLonBox LatLonBox
		{
			get
			{
				return m_oBox;
			}
		}

		#endregion


		#region Public Methods

		internal String GetUri()
		{
			String result = Icon.HRef;
			if (!String.IsNullOrEmpty(Icon.HTTPQuery))
			{
				result += "&" + Icon.HTTPQuery;
			}
			if (!String.IsNullOrEmpty(Icon.ViewFormat))
			{
				result += "&" + Icon.ViewFormat
					.Replace("[bboxNorth]", m_oBox.North.ToString(CultureInfo.InvariantCulture))
					.Replace("[bboxSouth]", m_oBox.South.ToString(CultureInfo.InvariantCulture))
					.Replace("[bboxEast]", m_oBox.East.ToString(CultureInfo.InvariantCulture))
					.Replace("[bboxWest]", m_oBox.West.ToString(CultureInfo.InvariantCulture));
			}

			return result;
		}

		#endregion
	}

	internal class KMLScreenOverlay : KMLOverlay
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

		internal KMLScreenOverlay(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("overlayXY"))
				{
					m_oOverlayXY = new KMLVec2(oChildElement);
				}
				else if (oChildElement.Name.Equals("screenXY"))
				{
					m_oScreenXY = new KMLVec2(oChildElement);
				}
				else if (oChildElement.Name.Equals("rotationXY"))
				{
					m_oRotationXY = new KMLVec2(oChildElement);
				}
				else if (oChildElement.Name.Equals("size"))
				{
					m_oSize = new KMLVec2(oChildElement);
				}
				else if (oChildElement.Name.Equals("rotation"))
				{
					m_dRotation = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
			}
		}

		#endregion


		#region Properties

		internal KMLVec2 OverlayXY
		{
			get { return m_oOverlayXY; }
		}

		internal KMLVec2 ScreenXY
		{
			get { return m_oScreenXY; }
		}

		internal KMLVec2 RotationXY
		{
			get { return m_oRotationXY; }
		}

		internal KMLVec2 Size
		{
			get { return m_oSize; }
		}

		internal float Rotation
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

	internal class KMLPhotoOverlay : KMLOverlay
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

		internal KMLPhotoOverlay(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("shape"))
				{
					m_eShape = (KMLShape)Enum.Parse(typeof(KMLShape), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("ViewVolume"))
				{
					foreach (XmlNode oViewVolumeChild in oChildElement.ChildNodes)
					{
						if (oViewVolumeChild.NodeType != XmlNodeType.Element) continue;
						XmlElement oViewVolumeChildElement = oViewVolumeChild as XmlElement;

						if (oViewVolumeChildElement.Name.Equals("leftFov"))
						{
							m_dViewVolumeLeftFov = Double.Parse(oViewVolumeChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oViewVolumeChildElement.Name.Equals("rightFov"))
						{
							m_dViewVolumeRightFov = Double.Parse(oViewVolumeChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oViewVolumeChildElement.Name.Equals("bottomFov"))
						{
							m_dViewVolumeBottomFov = Double.Parse(oViewVolumeChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oViewVolumeChildElement.Name.Equals("topFov"))
						{
							m_dViewVolumeTopFov = Double.Parse(oViewVolumeChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oViewVolumeChildElement.Name.Equals("near"))
						{
							m_dViewVolumeNear = Double.Parse(oViewVolumeChildElement.InnerText, CultureInfo.InvariantCulture);
						}
					}
				}
				else if (oChildElement.Name.Equals("roll"))
				{
					m_dRoll = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("ImagePyramid"))
				{
					foreach (XmlNode oImagePyramidChild in oChildElement.ChildNodes)
					{
						if (oImagePyramidChild.NodeType != XmlNodeType.Element) continue;
						XmlElement oImagePyramidChildElement = oImagePyramidChild as XmlElement;

						if (oImagePyramidChildElement.Name.Equals("tileSize"))
						{
							m_iImagePyramidTileSize = Int32.Parse(oImagePyramidChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oImagePyramidChildElement.Name.Equals("maxWidth"))
						{
							m_iImagePyramidMaxWidth = Int32.Parse(oImagePyramidChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oImagePyramidChildElement.Name.Equals("maxHeight"))
						{
							m_iImagePyramidMaxHeight = Int32.Parse(oImagePyramidChildElement.InnerText, CultureInfo.InvariantCulture);
						}
						else if (oImagePyramidChildElement.Name.Equals("gridOrigin"))
						{
							m_eGridOrigin = (KMLGridOrigin)Enum.Parse(typeof(KMLGridOrigin), oImagePyramidChildElement.InnerText);
						}
					}
				}
				else if (oChildElement.Name.Equals("Point"))
				{
					m_oPoint = new KMLPoint(oChildElement, this, source);
				}
			}
		}

		#endregion


		#region Properties

		internal KMLShape Shape
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

		internal double ViewVolumeLeftFOV
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

		internal double ViewVolumeRightFOV
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

		internal double ViewVolumeBottomFOV
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

		internal double ViewVolumeTopFOV
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

		internal double ViewVolumeNear
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

		internal double Roll
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

		internal int ImagePyramidTileSize
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

		internal int ImagePyramidMaxWidth
		{
			get { return m_iImagePyramidMaxWidth; }
		}

		internal int ImagePyramidMaxHeight
		{
			get { return m_iImagePyramidMaxHeight; }
		}

		internal KMLGridOrigin ImagePyramidGridOrigin
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

		internal KMLPoint Point
		{
			get { return m_oPoint; }
		}

		#endregion
	}

	#endregion


	#region Features ==================================================

	internal class KMLNetworkLink : KMLFeature
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

		internal KMLNetworkLink(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("refreshVisibility"))
				{
					m_blRefreshVisibility = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("flyToView"))
				{
					m_blFlyToView = oChildElement.InnerText.Equals("1");
				}
				else if (oChildElement.Name.Equals("Link"))
				{
					m_oLink = new KMLIconOrLink(oChildElement, source);
				}
			}

			if (m_oLink == null) throw new ArgumentException("The KML file contains a 'NetworkLink' element without a 'Link' element.");
		}

		#endregion


		#region Properties

		internal bool RefreshVisibility
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

		internal bool FlyToView
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

		internal KMLIconOrLink Link
		{
			get
			{
				return m_oLink;
			}
		}

		#endregion
	}

	[DebuggerDisplay("Placemark, name = {m_strName}")]
	internal class KMLPlacemark : KMLFeature
	{
		#region Member Variables

		private KMLGeometry m_oGeometry;

		#endregion


		#region Constructor

		internal KMLPlacemark(XmlElement element, KMLFile source)
			: base(element, source)
		{
			List<KMLGeometry> oGeometries = KMLGeometry.GetGeometries(element, this, source);
			if (oGeometries.Count == 0) throw new ArgumentException("The KML file contains a 'Placemark' element without a geometry element.");
			m_oGeometry = oGeometries[0];
		}

		#endregion


		#region Properties

		internal KMLGeometry Geometry
		{
			get { return m_oGeometry; }
		}

		#endregion
	}

	#endregion


	#region Containers ================================================

	[DebuggerDisplay("Folder, name = {m_strName}")]
	internal class KMLFolder : KMLContainer
	{
		#region Member Variables

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private List<KMLFeature> m_oFeatures;

		#endregion


		#region Constructors

		internal KMLFolder(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_oFeatures = KMLFeature.GetFeatures(element, source);
		}

		#endregion


		#region Properties

		internal override KMLFeature this[int i]
		{
			get { return m_oFeatures[i]; }
		}

		internal override int Count
		{
			get { return m_oFeatures.Count; }
		}

		#endregion
	}

	[DebuggerDisplay("Document, name = {m_strName}")]
	internal class KMLDocument : KMLContainer
	{
		#region Member Variables

		private List<KMLFeature> m_oFeatures;
		//TODO Parse out schema elements

		#endregion


		#region Constructors

		internal KMLDocument(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_oFeatures = KMLFeature.GetFeatures(element, source);
		}

		#endregion


		#region Properties

		internal override KMLFeature this[int i]
		{
			get { return m_oFeatures[i]; }
		}

		internal override int Count
		{
			get { return m_oFeatures.Count; }
		}

		#endregion


		#region Public Methods

		internal KMLStyleSelector GetStyle(string strStyleURL)
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

	internal class KMLCamera : KMLAbstractView
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

		internal KMLCamera(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("longitude"))
				{
					m_dLongitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("latitude"))
				{
					m_dLatitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitude"))
				{
					m_dAltitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("heading"))
				{
					m_dHeading = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("tilt"))
				{
					m_dTilt = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("roll"))
				{
					m_dRoll = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
			}
		}

		#endregion


		#region Properties

		internal double Longitude
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

		internal double Latitude
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

		internal double Altitude
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

		internal double Heading
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

		internal double Tilt
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

		internal double Roll
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

		internal KMLAltitudeMode AltitudeMode
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

	internal class KMLLookAt : KMLAbstractView
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

		internal KMLLookAt(XmlElement element, KMLFile source)
			: base(element, source)
		{
			m_dRange = Double.MinValue;

			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("longitude"))
				{
					m_dLongitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("latitude"))
				{
					m_dLatitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitude"))
				{
					m_dAltitude = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("heading"))
				{
					m_dHeading = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("tilt"))
				{
					m_dTilt = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("range"))
				{
					m_dRange = Double.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("altitudeMode"))
				{
					m_eAltitudeMode = (KMLAltitudeMode)Enum.Parse(typeof(KMLAltitudeMode), oChildElement.InnerText);
				}
			}

			if (m_dRange == Double.MinValue) throw new ArgumentException("The KML file contains a 'LookAt' element without a 'range' element.");
		}

		#endregion


		#region Properties

		internal double Longitude
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

		internal double Latitude
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

		internal double Altitude
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

		internal double Heading
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

		internal double Tilt
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

		internal double Range
		{
			get { return m_dRange; }
		}

		internal KMLAltitudeMode AltitudeMode
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

	internal class KMLIconOrLink : KMLObject
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

		internal KMLIconOrLink(XmlElement element, KMLFile source)
			: base(element, source)
		{
			foreach (XmlNode oChild in element.ChildNodes)
			{
				if (oChild.NodeType != XmlNodeType.Element) continue;
				XmlElement oChildElement = oChild as XmlElement;

				if (oChildElement.Name.Equals("href"))
				{
					m_strHRef = oChildElement.InnerText.Trim();
				}
				else if (oChildElement.Name.Equals("refreshMode"))
				{
					m_eRefreshMode = (KLMRefreshMode)Enum.Parse(typeof(KLMRefreshMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("refreshInterval"))
				{
					m_fRefreshInterval = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("viewRefreshMode"))
				{
					m_eViewRefreshMode = (KMLViewRefreshMode)Enum.Parse(typeof(KMLViewRefreshMode), oChildElement.InnerText);
				}
				else if (oChildElement.Name.Equals("viewRefreshTime"))
				{
					m_fViewRefreshTime = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("viewBoundScale"))
				{
					m_fViewBoundScale = Single.Parse(oChildElement.InnerText, CultureInfo.InvariantCulture);
				}
				else if (oChildElement.Name.Equals("viewFormat"))
				{
					m_strViewFormat = oChildElement.InnerText;
				}
				else if (oChildElement.Name.Equals("httpQuery"))
				{
					m_strHTTPQuery = oChildElement.InnerText;
				}
			}
		}

		#endregion


		#region Properties

		internal bool IsLocalFile
		{
			get { return !m_strHRef.StartsWith("http://"); }
		}

		internal String HRef
		{
			get
			{
				return m_strHRef;
			}
		}

		internal KLMRefreshMode RefreshMode
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

		internal float RefreshInterval
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

		internal KMLViewRefreshMode ViewRefreshMode
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

		internal float ViewRefreshTime
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

		internal float ViewBoundScale
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

		internal String ViewFormat
		{
			get
			{
				return m_strViewFormat;
			}
		}

		internal String HTTPQuery
		{
			get
			{
				return m_strHTTPQuery;
			}
		}

		#endregion


		#region Public Methods

		internal String GetUri(double west, double south, double east, double north)
		{
			String result = HRef;
			if (!String.IsNullOrEmpty(HTTPQuery))
			{
				result += "&" + HTTPQuery;
			}
			if (!String.IsNullOrEmpty(ViewFormat))
			{
				result += "&" + ViewFormat
					.Replace("[bboxNorth]", north.ToString(CultureInfo.InvariantCulture))
					.Replace("[bboxSouth]", south.ToString(CultureInfo.InvariantCulture))
					.Replace("[bboxEast]", east.ToString(CultureInfo.InvariantCulture))
					.Replace("[bboxWest]", west.ToString(CultureInfo.InvariantCulture));
			}

			return result;
		}

		#endregion
	}

	internal class KMLFile
	{
		#region Statics

		internal static string KMLTempDirectory = Path.Combine(Path.GetTempPath(), "DappleKML");

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

		internal KMLFile(String strFilename)
			: this(strFilename, false)
		{
		}

		internal KMLFile(String strFilename, bool blValidate)
		{
			XmlReader oDocReader = null;

			try
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
				oDocReader = XmlReader.Create(strFilename, oSettings);

				try
				{
					oDoc.Load(oDocReader);
				}
				catch (XmlException ex)
				{
					throw new ArgumentException("KML file is not valid XML: " + ex.Message);
				}

				if (!oDoc.DocumentElement.Name.Equals("kml"))
				{
					throw new ArgumentException("KML file missing root 'kml; element.");
				}

				m_strVersion = oDoc.DocumentElement.NamespaceURI;

				foreach (XmlNode oNode in oDoc.DocumentElement.ChildNodes)
				{
					if (!(oNode.NodeType == XmlNodeType.Element)) continue;
					XmlElement oPointer = oNode as XmlElement;

					if (oPointer.Name.Equals("Document"))
					{
						m_oDocument = new Dapple.KML.KMLDocument(oPointer, this);
					}
				}

				if (m_oDocument == null) throw new ArgumentException("The KML file contains no 'Document' element.");
			}
			finally
			{
				if (oDocReader != null) oDocReader.Close();
			}
		}

		~KMLFile()
		{
			// --- If we've loaded from a KMZ, delete the KMZ directory ---

			if (m_blLoadedFromKMZ && Directory.Exists(Path.GetDirectoryName(m_strFilename)))
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

		internal KMLDocument Document
		{
			get { return m_oDocument; }
		}

		internal String Filename
		{
			get
			{
				return m_strFilename;
			}
		}

		internal Dictionary<String, KMLObject> NamedElements
		{
			get { return m_oNamedElements; }
		}

		internal String Version
		{
			get { return m_strVersion; }
		}

		#endregion


		#region Public Methods

		internal KMLStyleSelector GetStyle(String strStyleUrl)
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