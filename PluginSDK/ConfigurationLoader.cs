using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.IO;
using WorldWind;
using WorldWind.Camera;
using WorldWind.Terrain;
using WorldWind.Renderable;
using System.Globalization;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ConfigurationLoader.
	/// </summary>
	public class ConfigurationLoader
	{
		internal static double ParseDouble(string s)
		{
			return double.Parse(s, CultureInfo.InvariantCulture);
		}

		internal static void XMLValidationCallback(object sender, ValidationEventArgs args)
		{
			string file = "(unknown)";
			XmlReader reader = sender as XmlReader;
			if (reader != null)
				file = reader.BaseURI;

			if (args.Severity == XmlSeverityType.Warning)
			{
				Log.Write(Log.Levels.Warning, "CONF", "Warning: " + args.Message);
				Log.Write(Log.Levels.Warning, "CONF", "  in " + file);
			}
			else
			{
				Log.Write(Log.Levels.Error, "CONF", "Error: " + args.Message);
				Log.Write(Log.Levels.Error, "CONF", "  in " + file);
				//throw args.Exception;
			}
		}

		private static bool ParseBool(string booleanString)
		{
			if (booleanString == null || booleanString.Trim().Length == 0)
			{
				return false;
			}

			booleanString = booleanString.Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture);

			if (booleanString == "1")
				return true;
			else if (booleanString == "0")
				return false;
			else if (booleanString == "t")
				return true;
			else if (booleanString == "f")
				return false;
			else
				return bool.Parse(booleanString);

		}

		internal static RenderableObjectList getRenderableFromLayerFile(string layerFile, World parentWorld, Cache cache)
		{
			return getRenderableFromLayerFile(layerFile, parentWorld, cache, true);
		}

		internal static RenderableObjectList getRenderableFromLayerFile(string layerFile, World parentWorld, Cache cache, bool enableRefresh)
		{
			return getRenderableFromLayerFile(layerFile, parentWorld, cache, enableRefresh, null);
		}

		public static RenderableObjectList getRenderableFromLayerFile(string layerFile, World parentWorld, Cache cache, bool enableRefresh, string layerSetSchema)
		{
			Log.Write(Log.Levels.Debug + 1, "CONF", "Loading renderable from " + layerFile);
			try
			{
				XPathDocument docNav = null;
				XPathNavigator nav = null;

				XmlReaderSettings readerSettings = new XmlReaderSettings();

				if (layerSetSchema != null && File.Exists(layerSetSchema))
				{
					Log.Write(Log.Levels.Debug, "CONF", "validating " + layerFile + " against LayerSet.xsd");
					readerSettings.ValidationType = ValidationType.Schema;
					XmlSchemaSet schemas = new XmlSchemaSet();
					schemas.Add(null, layerSetSchema);

					readerSettings.Schemas = schemas;
					readerSettings.ValidationEventHandler += new ValidationEventHandler(XMLValidationCallback);
					readerSettings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
				}
				else
				{
					Log.Write(Log.Levels.Debug, "CONF", "loading " + layerFile + " without validation");
					readerSettings.ValidationType = ValidationType.None;
				}

				try
				{
					if (layerFile.IndexOf(@"http://") < 0)
					{
						XmlReader docReader = XmlReader.Create(layerFile, readerSettings);
						docNav = new XPathDocument(docReader);
                        docReader.Close();
					}
					else
					{
						Angle[] bbox = CameraBase.getViewBoundingBox();
						string viewBBox = string.Format(CultureInfo.InvariantCulture,
							 "{0},{1},{2},{3}",
							 bbox[0].ToString().TrimEnd('°'), bbox[1].ToString().TrimEnd('°'), bbox[2].ToString().TrimEnd('°'), bbox[3].ToString().TrimEnd('°'));

						//See if there is a ? already in the URL
						int flag = layerFile.IndexOf("?");
						if (flag == -1)
							layerFile = layerFile + "?BBOX=" + viewBBox;
						else
							layerFile = layerFile + "&BBOX=" + viewBBox;

						WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(layerFile);
						download.DownloadMemory();

						XmlReader docReader = XmlReader.Create(download.ContentStream, readerSettings);
						docNav = new XPathDocument(docReader);
                        docReader.Close();
					}

					nav = docNav.CreateNavigator();
				}
				catch (Exception ex)
				{
					Log.Write(ex);
					return null;
				}

				XPathNodeIterator iter = nav.Select("/LayerSet");

				if (iter.Count > 0)
				{
					iter.MoveNext();
					string redirect = iter.Current.GetAttribute("redirect", "");
					redirect = redirect.Replace("${WORLDWINDVERSION}", System.Windows.Forms.Application.ProductVersion);
					string redirectWithoutBBOX = redirect;
					if (redirect != null && redirect.Length > 0)
					{
						FileInfo layerFileInfo = new FileInfo(layerFile);

						try
						{
							Angle[] bbox = CameraBase.getViewBoundingBox();
							string viewBBox = string.Format(CultureInfo.InvariantCulture,
								 "{0},{1},{2},{3}",
								 bbox[0].ToString().TrimEnd('°'), bbox[1].ToString().TrimEnd('°'), bbox[2].ToString().TrimEnd('°'), bbox[3].ToString().TrimEnd('°'));

							//See if there is a ? already in the URL
							int flag = redirect.IndexOf("?");
							if (flag == -1)
								redirect = redirect + "?BBOX=" + viewBBox;
							else
								redirect = redirect + "&BBOX=" + viewBBox;

							WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(redirect);

							string username = iter.Current.GetAttribute("username", "");

							if (username != null)
							{
								////	download.UserName = username;
								////	download.Password = password;
							}

							FileInfo tempDownloadFile = new FileInfo(layerFile.Replace(layerFileInfo.Extension, "_.tmp"));

							download.DownloadFile(tempDownloadFile.FullName, WorldWind.Net.DownloadType.Unspecified);

							tempDownloadFile.Refresh();
							if (tempDownloadFile.Exists && tempDownloadFile.Length > 0)
							{
								FileInfo tempStoreFile = new FileInfo(tempDownloadFile.FullName.Replace("_.tmp", ".tmp"));
								if (tempStoreFile.Exists)
									tempStoreFile.Delete();

								tempDownloadFile.MoveTo(tempStoreFile.FullName);
							}

							download.Dispose();

							using (StreamWriter writer = new StreamWriter(layerFile.Replace(layerFileInfo.Extension, ".uri"), false))
							{
								writer.WriteLine(redirectWithoutBBOX);
							}
						}
						catch (Exception ex)
						{
							Log.Write(ex);
						}

						return getRenderableFromLayerFile(layerFile.Replace(layerFileInfo.Extension, ".tmp"), parentWorld, cache);
					}
					else
					{
						RenderableObjectList parentRenderable = null;

						string sourceUri = null;
						if (layerFile.EndsWith(".tmp"))
						{
							//get source url
							using (StreamReader reader = new StreamReader(layerFile.Replace(".tmp", ".uri")))
							{
								sourceUri = reader.ReadLine();
							}
						}
						string refreshString = iter.Current.GetAttribute("Refresh", "");
						if (refreshString != null && refreshString.Length > 0)
						{

							if (iter.Current.Select("Icon").Count > 0)
							{
								parentRenderable = new Icons(iter.Current.GetAttribute("Name", ""),
									 (sourceUri != null ? sourceUri : layerFile),
									 TimeSpan.FromSeconds(ParseDouble(refreshString)),
									 parentWorld,
									 cache);
							}
							else
							{
								parentRenderable = new RenderableObjectList(
									 iter.Current.GetAttribute("Name", ""),
									 (sourceUri != null ? sourceUri : layerFile),
									 TimeSpan.FromSeconds(ParseDouble(refreshString)),
									 parentWorld,
									 cache);
							}

						}
						else
						{
							if (iter.Current.Select("Icon").Count > 0)
							{
								parentRenderable = new Icons(iter.Current.GetAttribute("Name", ""));
							}
							else
							{
								parentRenderable = new RenderableObjectList(iter.Current.GetAttribute("Name", ""));
							}
						}

						parentRenderable.ParentList = parentWorld.RenderableObjects;

						if (World.Settings.useDefaultLayerStates)
						{
							parentRenderable.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
						}
						else
						{
							parentRenderable.IsOn = IsLayerOn(parentRenderable);
						}

						string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
						if (description != null && description.Length > 0)
							parentRenderable.Description = description;

						parentRenderable.ShowOnlyOneLayer = ParseBool(iter.Current.GetAttribute("ShowOnlyOneLayer", ""));

						parentRenderable.MetaData.Add("XmlSource", (sourceUri != null ? sourceUri : layerFile));

						parentRenderable.MetaData.Add("World", parentWorld);
						parentRenderable.MetaData.Add("Cache", cache);
						parentRenderable.ParentList = parentWorld.RenderableObjects;

						string renderPriorityString = iter.Current.GetAttribute("RenderPriority", "");
						if (renderPriorityString != null)
						{
							if (String.Compare(renderPriorityString, "Icons", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.Icons;
							}
							else if (String.Compare(renderPriorityString, "LinePaths", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.LinePaths;
							}
							else if (String.Compare(renderPriorityString, "Placenames", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.Placenames;
							}
							else if (String.Compare(renderPriorityString, "AtmosphericImages", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
							{
								parentRenderable.RenderPriority = RenderPriority.AtmosphericImages;
							}
						}

						string infoUri = iter.Current.GetAttribute("InfoUri", "");

						if (infoUri != null && infoUri.Length > 0)
						{
							if (parentRenderable.MetaData.Contains("InfoUri"))
							{
								parentRenderable.MetaData["InfoUri"] = infoUri;
							}
							else
							{
								parentRenderable.MetaData.Add("InfoUri", infoUri);
							}
						}

						addTiledWFSPlacenameSet(iter.Current.Select("TiledWFSPlacenameSet"), parentWorld, parentRenderable, cache);
						addExtendedInformation(iter.Current.Select("ExtendedInformation"), parentRenderable);

						if (parentRenderable.RefreshTimer != null && enableRefresh)
						{
							parentRenderable.RefreshTimer.Start();
						}
						return parentRenderable;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
				//Log.Write(Log.Levels.Debug, layerFile);
			}
			Log.Write(Log.Levels.Warning, "CONF", "WARNING: no renderable created for " + layerFile);

			return null;
		}



		internal static bool IsLayerOn(RenderableObject ro)
		{
			string path = getRenderablePathString(ro);
			foreach (string s in World.Settings.loadedLayers)
			{
				if (s.Equals(path))
				{
					return true;
				}
			}

			return false;
		}

		private static void addExtendedInformation(XPathNodeIterator iter, RenderableObject renderable)
		{
			if (iter.Count > 0)
			{
				while (iter.MoveNext())
				{
					string toolBarImage = getInnerTextFromFirstChild(iter.Current.Select("ToolBarImage"));

					if (toolBarImage != null)
					{
						if (toolBarImage.Length > 0 && !Path.IsPathRooted(toolBarImage))
							Path.Combine(
								Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath),
								toolBarImage);

						renderable.MetaData.Add("ToolBarImagePath", toolBarImage);
					}
				}
			}
		}

		internal static string GetRenderablePathString(RenderableObject renderable)
		{
			return getRenderablePathString(renderable);
		}

		private static string getRenderablePathString(RenderableObject renderable)
		{
			if (renderable.ParentList == null)
			{
				return renderable.Name;
			}
			else
			{
				return getRenderablePathString(renderable.ParentList) + Path.DirectorySeparatorChar + renderable.Name;
			}
		}

		/* parseColorNode (overload) */
		private static System.Drawing.Color parseColorNode(XPathNavigator parentNode)
		{
			return parseColorNode(parentNode, null);
		}

		/* parseColorNode (overload) */
		private static System.Drawing.Color parseColorNode(XPathNavigator parentNode, string RGBNodeName)
		{
			return parseColorNode(parentNode, RGBNodeName, System.Drawing.Color.White);
		}

		/* parseColorNode
		 * Grabs the appropriate System.Drawing.Color from a parent node that contains a node from the following list:
		 * WinColorName (Windows-type color name e.g. Cyan)
		 * HexColor		(Hexadecimal format, RGB(A) e.g. #0000FF or FFFFFF)
		 * RGBColor		(Contains Red, Green, Blue, and possibly Alpha color subnodes with a value from 0-255)
		 * An optional node name, specified by RGBNodeName, that functions identically to RGBColor
		*/
		private static System.Drawing.Color parseColorNode(XPathNavigator parentNode, string RGBNodeName, System.Drawing.Color defaultColor)
		{
			string hexColorCode = getInnerTextFromFirstChild(parentNode.Select("HexColor"));
			string winColorName = getInnerTextFromFirstChild(parentNode.Select("WinColorName"));

			System.Drawing.Color c = defaultColor;

			if (winColorName != null)
			{
				c = System.Drawing.Color.FromName(winColorName);
			}
			else if (hexColorCode != null)
			{
				c = getHexColor(hexColorCode);
			}
			else if (RGBNodeName != null && parentNode.Select(RGBNodeName).Count > 0)
			{
				c = getRGBColor(parentNode.Select(RGBNodeName));
			}
			else if (parentNode.Select("RGBColor").Count > 0)
			{
				c = getRGBColor(parentNode.Select("RGBColor"));
			}
			return c;
		}

		private static System.Drawing.Color getHexColor(string colorCode)
		{
			byte r = 0;
			byte g = 0;
			byte b = 0;
			byte a = 255;

			if (colorCode.Substring(0, 1) == "#")
			{
				colorCode = colorCode.Substring(1);
			}
			
			string redString = colorCode.Substring(0, 2);
			string greenString = colorCode.Substring(2, 2);
			string blueString = colorCode.Substring(4, 2);

			r = byte.Parse(redString, NumberStyles.HexNumber);
			g = byte.Parse(greenString, NumberStyles.HexNumber);
			b = byte.Parse(blueString, NumberStyles.HexNumber);

			if (colorCode.Length > 6)
			{
				string alphaString = colorCode.Substring(6, 2);
				a = byte.Parse(alphaString, NumberStyles.HexNumber);
			}

			return System.Drawing.Color.FromArgb(a, r, g, b);
		}
		private static System.Drawing.Color getRGBColor(XPathNodeIterator iter)
		{
			iter.MoveNext();
			byte r = 0;
			byte g = 0;
			byte b = 0;
			byte a = 255;

			string redString = getInnerTextFromFirstChild(iter.Current.Select("Red"));
			string greenString = getInnerTextFromFirstChild(iter.Current.Select("Green"));
			string blueString = getInnerTextFromFirstChild(iter.Current.Select("Blue"));
			string alphaString = getInnerTextFromFirstChild(iter.Current.Select("Alpha"));

			r = byte.Parse(redString);
			g = byte.Parse(greenString);
			b = byte.Parse(blueString);
			if (alphaString != null)
			{
				a = byte.Parse(alphaString);
			}

			return System.Drawing.Color.FromArgb(a, r, g, b);
		}

		private static Microsoft.DirectX.Direct3D.FontDescription getDisplayFont(XPathNodeIterator iter)
		{
			Microsoft.DirectX.Direct3D.FontDescription fd = new Microsoft.DirectX.Direct3D.FontDescription();

			if (iter.MoveNext())
			{

				fd.FaceName = getInnerTextFromFirstChild(iter.Current.Select("Family"));
				fd.Height = (int)(float.Parse(getInnerTextFromFirstChild(iter.Current.Select("Size")), NumberStyles.Any, CultureInfo.InvariantCulture) * 1.5f);

				XPathNodeIterator styleIter = iter.Current.Select("Style");
				if (styleIter.Count > 0)
				{
					styleIter.MoveNext();

					string isBoldString = getInnerTextFromFirstChild(styleIter.Current.Select("IsBold"));
					string isItalicString = getInnerTextFromFirstChild(styleIter.Current.Select("IsItalic"));

					if (isBoldString != null)
					{
						bool isBold = ParseBool(isBoldString);
						if (isBold)
							fd.Weight = Microsoft.DirectX.Direct3D.FontWeight.Bold;
					}

					if (isItalicString != null)
					{
						bool isItalic = ParseBool(isItalicString);
						if (isItalic)
							fd.IsItalic = isItalic;
					}
				}
				else
				{
					fd.Weight = Microsoft.DirectX.Direct3D.FontWeight.Regular;
				}
			}

			return fd;
		}

		private static void addTiledWFSPlacenameSet(XPathNodeIterator iter, World parentWorld, RenderableObjectList parentRenderable, Cache cache)
		{
			if (iter.Count > 0)
			{
				while (iter.MoveNext())
				{
					string name = getInnerTextFromFirstChild(iter.Current.Select("Name"));
					double distanceAboveSurface = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("DistanceAboveSurface")));
					double minimumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MinimumDisplayAltitude")));
					double maximumDisplayAltitude = ParseDouble(getInnerTextFromFirstChild(iter.Current.Select("MaximumDisplayAltitude")));

					string wfsBaseUrl = getInnerTextFromFirstChild(iter.Current.Select("WFSBaseURL"));
					string typename = getInnerTextFromFirstChild(iter.Current.Select("TypeName"));
					string labelfield = getInnerTextFromFirstChild(iter.Current.Select("LabelField"));
					/*
					if (!Path.IsPathRooted(wfsBaseUrl))
					{
						 Path.Combine(
							  Path.GetDirectoryName(
							  System.Windows.Forms.Application.ExecutablePath),
							  wfsBaseUrl);
					}
					*/
                    string iconFilePath = getInnerTextFromFirstChild(iter.Current.Select("IconFilePath"));

                    Microsoft.DirectX.Direct3D.FontDescription fd = getDisplayFont(iter.Current.Select("DisplayFont"));

					System.Drawing.Color c = parseColorNode(iter.Current);

					//TODO:Validate URL
					//Construct WFS Base URL
					wfsBaseUrl += "TypeName=" + typename + "&Request=GetFeature&Service=WFS";

					TiledWFSPlacenameSet twps = new TiledWFSPlacenameSet(
						 name,
						 parentWorld,
						 distanceAboveSurface,
						 maximumDisplayAltitude,
						 minimumDisplayAltitude,
						 wfsBaseUrl,
						 typename,
						 labelfield,
						 fd,
						 c,
						 iconFilePath,
				 cache);

					string description = getInnerTextFromFirstChild(iter.Current.Select("Description"));
					if (description != null && description.Length > 0)
						twps.Description = description;

					addExtendedInformation(iter.Current.Select("ExtendedInformation"), twps);

					string infoUri = iter.Current.GetAttribute("InfoUri", "");

					if (infoUri != null && infoUri.Length > 0)
					{
						if (twps.MetaData.Contains("InfoUri"))
						{
							twps.MetaData["InfoUri"] = infoUri;
						}
						else
						{
							twps.MetaData.Add("InfoUri", infoUri);
						}
					}

					twps.MetaData.Add("WFSBaseURL", wfsBaseUrl);
					twps.MetaData.Add("XmlSource", (string)parentRenderable.MetaData["XmlSource"]);
					twps.ParentList = parentRenderable;

					if (World.Settings.useDefaultLayerStates)
					{
						twps.IsOn = ParseBool(iter.Current.GetAttribute("ShowAtStartup", ""));
					}
					else
					{
						twps.IsOn = IsLayerOn(twps);
					}
					parentRenderable.ChildObjects.Add(
						 twps
						 );

					parentRenderable.RenderPriority = RenderPriority.Placenames;

				}
			}
		}

		static string getInnerTextFromFirstChild(XPathNodeIterator iter)
		{
			if (iter.Count == 0)
			{
				return null;
			}
			else
			{
				iter.MoveNext();
				return iter.Current.Value;
			}
		}
	}
}
