// xmloperations.cs
// This file contains generated code and will be overwritten when you rerun code generation.

using System.Xml;
using Altova.TypeInfo;

namespace Altova.Xml
{
	/// <summary>
	/// Fault exception propagates fault as a node back to main WS method
	/// </summary>
	public class FaultException : System.Exception
	{
		private XmlNode node;
		private string thrower;
			
		public FaultException(XmlNode n, string t)
		{
			node = n;
			thrower = t;
		}
		
		public XmlNode Node { get { return node; } }
		public string Thrower { get { return thrower; } }
	}
	

    public class XmlTreeOperations
    {

		public static void CopyAll(XmlNode src, XmlNode tgt)
        {
            //nodes
            foreach (XmlNode node in src.ChildNodes)
                tgt.AppendChild(tgt.OwnerDocument.ImportNode(node, true));
            // attributes
            foreach (XmlNode attr in src.Attributes)
                tgt.Attributes.Append((XmlAttribute)tgt.OwnerDocument.ImportNode(attr, true));
        }
		
        private static XmlFormatter GetFormatter(MemberInfo member)
        {
            if (member.DataType.Formatter != null)
                return (XmlFormatter) member.DataType.Formatter;
            else
                return (XmlFormatter) Xs.AnySimpleTypeFormatter;
        }

        public class AllIterator : System.Collections.IEnumerable, System.IDisposable
        {
            private XmlNodeList list;

            public AllIterator(XmlNodeList list)
            {
                this.list = list;
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return list.GetEnumerator();
            }
            
            public void Dispose()
            {
            }
        }

        public class MemberIterator : System.Collections.IEnumerable, System.IDisposable
        {
            private AllIterator iterator;
            private MemberInfo memberInfo;

            public MemberIterator(XmlNodeList list, MemberInfo info)
            {
                iterator = new AllIterator(list);
                memberInfo = info;
            }
            
            public System.Collections.IEnumerator GetEnumerator() 
            {
                return new Enumerator(iterator, memberInfo);
            }
            
            public void Dispose()
            {
                iterator.Dispose();
            }
            
            class Enumerator : System.Collections.IEnumerator
            {
                System.Collections.IEnumerator iterator;
                MemberInfo info;
                
                public Enumerator(AllIterator list, MemberInfo info)
                {
                    this.iterator = list.GetEnumerator();
                    this.info = info;
                }
                
                public void Reset()
                {
                    iterator.Reset();
                }

                public object Current
                {
                    get
                    {
                        return iterator.Current;
                    }
                }

                public bool MoveNext()
                {
                    while (iterator.MoveNext())
                    {
                        if (IsMember((XmlNode)iterator.Current, info))
                            return true;
                    }
                    return false;
                }
            }
        }

        public static bool IsEqualString(string a, string b)
        {
            if (a == b) return true;
            if (a == null) a="";
            if (b == null) a="";
            return a == b;
        }

        public static bool IsMember( XmlNode node,  MemberInfo member)
        {
            if (member.LocalName == "")
                return node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA;

            if (node.NodeType != XmlNodeType.Element)
                return false;


            string nodeURI = node.NamespaceURI == null ? "" : node.NamespaceURI;
			string nodeLocalName = node.LocalName == null ? "" : node.LocalName;
			string memberURI = member.NamespaceURI == null ? "" : member.NamespaceURI;
			string memberLocalName = member.LocalName == null ? "" : member.LocalName;

			// soap-array specialty: no-namespace elements are array members.
			if ((member.Flags & MemberFlags.SpecialName) != 0)
				return nodeURI == "";

			if (nodeURI == memberURI && nodeLocalName == memberLocalName)
				return true;
			return false;
        }

		public static XmlNode Dereference(XmlNode node)
		{
			XmlAttribute hrefAtt = node.Attributes["href", ""];
			if (hrefAtt == null)	// common case
				return node;

			string target = hrefAtt.Value;
			if (!target.StartsWith("#"))
				throw new System.InvalidOperationException("Cannot dereference external references.");

			string nodeId = target.Substring(1);
			XmlNode targetNode = node.OwnerDocument.SelectSingleNode("//*[@id='" + nodeId + "']");
			if (targetNode != null)
				return targetNode;
			return node;
		}

	    public static bool Exists( XmlNode node)
        {
            return node != null;
        }

	    public static AllIterator GetElements( XmlNode node)
        {
            return new AllIterator(node.ChildNodes);
        }

	    public static MemberIterator GetElements( XmlNode node,  MemberInfo member)
        {
            return new MemberIterator(node.ChildNodes, member);
        }

	    public static void SetTextValue( XmlNode node,  string value)
        {
            node.InnerText = value;
        }

	    public static string GetTextValue( XmlNode node)
        {
            return node.InnerText;
        }

		static string FindUnusedPrefix(XmlNode node)
		{
			int n = 0;
			while (true)
			{
				string s = string.Format("n{0}", n++);
				if (node.Attributes[s, "http://www.w3.org/2000/xmlns/"] == null)
					return s;
			}
		}

		public static void SetAttribute(XmlNode node, string localName, string namespaceURI, XmlQualifiedName value)
		{
			XmlAttribute att = node.OwnerDocument.CreateAttribute(localName, namespaceURI);
			if (value.Namespace == null || value.Namespace == "")
			{
				att.Value = value.Name;
			}
			else
			{
				string prefix = node.GetPrefixOfNamespace(value.Namespace);
				if (prefix == null || prefix == "")
				{
					prefix = FindUnusedPrefix(node);
					XmlAttribute nsatt = node.OwnerDocument.CreateAttribute("xmlns", prefix, "http://www.w3.org/2000/xmlns/");
					nsatt.Value = value.Namespace;
					node.Attributes.Append(nsatt);
				}
				att.Value = prefix + ":" + value.Name;
			}
			node.Attributes.Append(att);
		}

	    public static void SetValue( XmlNode node,  MemberInfo member,  string value)
        {
            if (member.LocalName != "")
            {
				string prefix = "";
				if( member.NamespaceURI != "" )
				{
					prefix = node.GetPrefixOfNamespace( member.NamespaceURI );
					if( prefix.Length > 0 )
						prefix += ":";
					else
						prefix = FindUnusedPrefix(node) + ":";
				}				

				XmlElement el = (XmlElement) node;
                XmlAttribute attr = node.OwnerDocument.CreateAttribute(prefix + member.LocalName, member.NamespaceURI);
                attr.Value = value;
                el.SetAttributeNode(attr);
            }
            else
                node.InnerText = value;
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, bool b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, int b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, uint b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, long b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, ulong b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

	    public static void SetValue( XmlNode node,  MemberInfo member, double b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

        public static void SetValue(XmlNode node, MemberInfo member, Altova.Types.DateTime b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

        public static void SetValue(XmlNode node, MemberInfo member, Altova.Types.Duration b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }
        
        public static void SetValue(XmlNode node, MemberInfo member, byte[] b)
        {
            SetValue(node, member, GetFormatter(member).Format(b));
        }

		public static void SetValue(XmlNode node,  MemberInfo member, decimal d)
		{
			SetValue(node, member, GetFormatter(member).Format(d));
		}

	    public static XmlNode AddElement( XmlNode node, MemberInfo member)
        {
			string prefix = "";
			if( member.NamespaceURI != null )
			{
				prefix = node.GetPrefixOfNamespace( member.NamespaceURI );
				if( prefix.Length > 0 )
					prefix += ":";
			}

			XmlDocument doc = node.OwnerDocument;
            if (doc == null)
                doc = (XmlDocument)node;            
            XmlNode newNode = doc.CreateElement(prefix + member.LocalName, member.NamespaceURI);
            node.AppendChild(newNode);
            return newNode;
        }

	    public static double CastToDouble( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToDouble(node.InnerText);
        }

	    public static string CastToString( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToString(node.InnerText);
        }

	    public static long CastToInt64( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToInt64(node.InnerText);
        }

	    public static ulong CastToUInt64( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToUInt64(node.InnerText);
        }

	    public static uint CastToUInt( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToUInt(node.InnerText);
        }

	    public static int CastToInt( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToInt(node.InnerText);
        }

	    public static bool CastToBool( XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToBool(node.InnerText);
        }

        public static Altova.Types.DateTime CastToDateTime(XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToDateTime(node.InnerText);
        }

        public static Altova.Types.Duration CastToDuration(XmlNode node, MemberInfo member)
        {
            return Altova.CoreTypes.CastToDuration(node.InnerText);
        }

        public static byte[] CastToBinary(XmlNode node, MemberInfo member)
        {
            return GetFormatter(member).ParseBinary(node.InnerText);
        }

		public static decimal CastToDecimal(XmlNode node, MemberInfo member)
		{
			return Altova.CoreTypes.CastToDecimal(node.InnerText);
		}

	    public static XmlNode FindAttribute( XmlNode node,  MemberInfo member)
        {
            XmlElement el = (XmlElement) node;
            XmlAttributeCollection attrs = el.Attributes;
            return attrs.GetNamedItem(member.LocalName, member.NamespaceURI);
        }

		public static XmlDocument LoadDocument( string filename )
		{
			XmlDocument doc = new XmlDocument();
			doc.Load( filename );
			return doc;
		}

		public static XmlDocument LoadXmlBinary( byte[] xmlTree )
		{
			System.IO.MemoryStream strm = new System.IO.MemoryStream( xmlTree );

			XmlDocument doc = new XmlDocument();
			doc.Load( strm );
			return doc;
		}

		public static XmlDocument LoadXml( string xmlString )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( xmlString );
			return doc;
		}

		public static void SaveDocument( XmlDocument doc, string filename, bool prettyPrint )
		{
			SaveDocument( doc, filename, "utf-8", prettyPrint );
		}
		
		public static void SaveDocument( XmlDocument doc, string filename, string encoding, bool prettyPrint )
		{
			XmlTextWriter writer = new XmlTextWriter( filename, System.Text.Encoding.GetEncoding(encoding) );
			if( prettyPrint )
			{
				writer.Formatting = Formatting.Indented;
				writer.IndentChar = '\t';
				writer.Indentation = 1;
			}
			else
				writer.Formatting = Formatting.None;

			doc.Save( writer );
			writer.Close();
		}

		public static byte[] SaveXmlBinary( XmlDocument doc, string encoding, bool prettyPrint )
		{
			System.IO.MemoryStream strm = new System.IO.MemoryStream();
			XmlTextWriter writer = new XmlTextWriter( strm, System.Text.Encoding.GetEncoding(encoding) );
			if( prettyPrint )
			{
				writer.Formatting = Formatting.Indented;
				writer.IndentChar = '\t';
				writer.Indentation = 1;
			}
			else
				writer.Formatting = Formatting.None;

			doc.Save( writer );
			writer.Close();

			return strm.GetBuffer();
		}

		public static string SaveXml( XmlDocument doc, bool prettyPrint )
		{
			System.IO.StringWriter strwriter = new System.IO.StringWriter();
			XmlTextWriter writer = new XmlTextWriter( strwriter );
			if( prettyPrint )
			{
				writer.Formatting = Formatting.Indented;
				writer.IndentChar = '\t';
				writer.Indentation = 1;
			}
			else
				writer.Formatting = Formatting.None;

			doc.Save( writer );
			writer.Close();
			strwriter.Close();

			return strwriter.ToString();
		}

		public static XmlDocument CreateDocument()
		{
			return new XmlDocument();
		}
	}; // class XmlTreeOperations
} // namespace Altova.Xml