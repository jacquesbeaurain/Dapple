//
// ServiceType.cs
//
// This file was generated by XMLSpy 2007r3 Enterprise Edition.
//
// YOU SHOULD NOT MODIFY THIS FILE, BECAUSE IT WILL BE
// OVERWRITTEN WHEN YOU RE-RUN CODE GENERATION.
//
// Refer to the XMLSpy Documentation for further details.
// http://www.altova.com/xmlspy
//


using System;
using System.Collections;
using System.Xml;
using Altova.Types;

namespace WMS_MS_Capabilities
{
	public class ServiceType : Altova.Xml.Node
	{
		#region Documentation
		public static string GetAnnoDocumentation() { return ""; }
		#endregion

		#region Forward constructors

		public ServiceType() : base() { SetCollectionParents(); }

		public ServiceType(XmlDocument doc) : base(doc) { SetCollectionParents(); }
		public ServiceType(XmlNode node) : base(node) { SetCollectionParents(); }
		public ServiceType(Altova.Xml.Node node) : base(node) { SetCollectionParents(); }
		public ServiceType(Altova.Xml.Document doc, string namespaceURI, string prefix, string name) : base(doc, namespaceURI, prefix, name) { SetCollectionParents(); }
		#endregion // Forward constructors

		public override void AdjustPrefix()
		{

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "Name" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "Name", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new NameType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "Title" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "Title", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new TitleType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "Abstract" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "Abstract", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new AbstractType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "KeywordList" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "KeywordList", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new KeywordListType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "OnlineResource" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "OnlineResource", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new OnlineResourceType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "ContactInformation" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "ContactInformation", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new ContactInformationType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "Fees" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "Fees", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new FeesType(DOMNode).AdjustPrefix();
			}

		    for (	XmlNode DOMNode = GetDomFirstChild( NodeType.Element, "", "AccessConstraints" );
					DOMNode != null; 
					DOMNode = GetDomNextChild( NodeType.Element, "", "AccessConstraints", DOMNode )
				)
			{
				InternalAdjustPrefix(DOMNode, false);
				new AccessConstraintsType(DOMNode).AdjustPrefix();
			}
		}

		public void SetXsiType()
		{
 			XmlElement el = (XmlElement) domNode;
			el.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "Service");
		}


		#region Name Documentation
		public static string GetNameAnnoDocumentation()
		{
			return "";		
		}
		public static string GetNameDefault()
		{
			return "";		
		}
		#endregion

		#region Name accessor methods
		public static int GetNameMinCount()
		{
			return 1;
		}

		public static int NameMinCount
		{
			get
			{
				return 1;
			}
		}

		public static int GetNameMaxCount()
		{
			return 1;
		}

		public static int NameMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetNameCount()
		{
			return DomChildCount(NodeType.Element, "", "Name");
		}

		public int NameCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "Name");
			}
		}

		public bool HasName()
		{
			return HasDomChild(NodeType.Element, "", "Name");
		}

		public NameType NewName()
		{
			return new NameType(domNode.OwnerDocument.CreateElement("Name", ""));
		}

		public NameType GetNameAt(int index)
		{
			return new NameType(GetDomChildAt(NodeType.Element, "", "Name", index));
		}

		public XmlNode GetStartingNameCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "Name" );
		}

		public XmlNode GetAdvancedNameCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "Name", curNode );
		}

		public NameType GetNameValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new NameType( curNode );
		}


		public NameType GetName()
		{
			return GetNameAt(0);
		}

		public NameType Name
		{
			get
			{
				return GetNameAt(0);
			}
		}

		public void RemoveNameAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "Name", index);
		}

		public void RemoveName()
		{
			RemoveNameAt(0);
		}

		public XmlNode AddName(NameType newValue)
		{
			return AppendDomElement("", "Name", newValue);
		}

		public void InsertNameAt(NameType newValue, int index)
		{
			InsertDomElementAt("", "Name", index, newValue);
		}

		public void ReplaceNameAt(NameType newValue, int index)
		{
			ReplaceDomElementAt("", "Name", index, newValue);
		}
		#endregion // Name accessor methods

		#region Name collection
        public NameCollection	MyNames = new NameCollection( );

        public class NameCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public NameEnumerator GetEnumerator() 
			{
				return new NameEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class NameEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public NameEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.NameCount );
			}
			public NameType  Current 
			{
				get 
				{
					return(parent.GetNameAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Name collection

		#region Title Documentation
		public static string GetTitleAnnoDocumentation()
		{
			return "";		
		}
		public static string GetTitleDefault()
		{
			return "";		
		}
		#endregion

		#region Title accessor methods
		public static int GetTitleMinCount()
		{
			return 1;
		}

		public static int TitleMinCount
		{
			get
			{
				return 1;
			}
		}

		public static int GetTitleMaxCount()
		{
			return 1;
		}

		public static int TitleMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetTitleCount()
		{
			return DomChildCount(NodeType.Element, "", "Title");
		}

		public int TitleCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "Title");
			}
		}

		public bool HasTitle()
		{
			return HasDomChild(NodeType.Element, "", "Title");
		}

		public TitleType NewTitle()
		{
			return new TitleType(domNode.OwnerDocument.CreateElement("Title", ""));
		}

		public TitleType GetTitleAt(int index)
		{
			return new TitleType(GetDomChildAt(NodeType.Element, "", "Title", index));
		}

		public XmlNode GetStartingTitleCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "Title" );
		}

		public XmlNode GetAdvancedTitleCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "Title", curNode );
		}

		public TitleType GetTitleValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new TitleType( curNode );
		}


		public TitleType GetTitle()
		{
			return GetTitleAt(0);
		}

		public TitleType Title
		{
			get
			{
				return GetTitleAt(0);
			}
		}

		public void RemoveTitleAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "Title", index);
		}

		public void RemoveTitle()
		{
			RemoveTitleAt(0);
		}

		public XmlNode AddTitle(TitleType newValue)
		{
			return AppendDomElement("", "Title", newValue);
		}

		public void InsertTitleAt(TitleType newValue, int index)
		{
			InsertDomElementAt("", "Title", index, newValue);
		}

		public void ReplaceTitleAt(TitleType newValue, int index)
		{
			ReplaceDomElementAt("", "Title", index, newValue);
		}
		#endregion // Title accessor methods

		#region Title collection
        public TitleCollection	MyTitles = new TitleCollection( );

        public class TitleCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public TitleEnumerator GetEnumerator() 
			{
				return new TitleEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class TitleEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public TitleEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.TitleCount );
			}
			public TitleType  Current 
			{
				get 
				{
					return(parent.GetTitleAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Title collection

		#region Abstract2 Documentation
		public static string GetAbstract2AnnoDocumentation()
		{
			return "";		
		}
		public static string GetAbstract2Default()
		{
			return "";		
		}
		#endregion

		#region Abstract2 accessor methods
		public static int GetAbstract2MinCount()
		{
			return 0;
		}

		public static int Abstract2MinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetAbstract2MaxCount()
		{
			return 1;
		}

		public static int Abstract2MaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetAbstract2Count()
		{
			return DomChildCount(NodeType.Element, "", "Abstract");
		}

		public int Abstract2Count
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "Abstract");
			}
		}

		public bool HasAbstract2()
		{
			return HasDomChild(NodeType.Element, "", "Abstract");
		}

		public AbstractType NewAbstract2()
		{
			return new AbstractType(domNode.OwnerDocument.CreateElement("Abstract", ""));
		}

		public AbstractType GetAbstract2At(int index)
		{
			return new AbstractType(GetDomChildAt(NodeType.Element, "", "Abstract", index));
		}

		public XmlNode GetStartingAbstract2Cursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "Abstract" );
		}

		public XmlNode GetAdvancedAbstract2Cursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "Abstract", curNode );
		}

		public AbstractType GetAbstract2ValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new AbstractType( curNode );
		}


		public AbstractType GetAbstract2()
		{
			return GetAbstract2At(0);
		}

		public AbstractType Abstract2
		{
			get
			{
				return GetAbstract2At(0);
			}
		}

		public void RemoveAbstract2At(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "Abstract", index);
		}

		public void RemoveAbstract2()
		{
			RemoveAbstract2At(0);
		}

		public XmlNode AddAbstract2(AbstractType newValue)
		{
			return AppendDomElement("", "Abstract", newValue);
		}

		public void InsertAbstract2At(AbstractType newValue, int index)
		{
			InsertDomElementAt("", "Abstract", index, newValue);
		}

		public void ReplaceAbstract2At(AbstractType newValue, int index)
		{
			ReplaceDomElementAt("", "Abstract", index, newValue);
		}
		#endregion // Abstract2 accessor methods

		#region Abstract2 collection
        public Abstract2Collection	MyAbstract2s = new Abstract2Collection( );

        public class Abstract2Collection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public Abstract2Enumerator GetEnumerator() 
			{
				return new Abstract2Enumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class Abstract2Enumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public Abstract2Enumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.Abstract2Count );
			}
			public AbstractType  Current 
			{
				get 
				{
					return(parent.GetAbstract2At(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Abstract2 collection

		#region KeywordList Documentation
		public static string GetKeywordListAnnoDocumentation()
		{
			return "";		
		}
		public static string GetKeywordListDefault()
		{
			return "";		
		}
		#endregion

		#region KeywordList accessor methods
		public static int GetKeywordListMinCount()
		{
			return 0;
		}

		public static int KeywordListMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetKeywordListMaxCount()
		{
			return 1;
		}

		public static int KeywordListMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetKeywordListCount()
		{
			return DomChildCount(NodeType.Element, "", "KeywordList");
		}

		public int KeywordListCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "KeywordList");
			}
		}

		public bool HasKeywordList()
		{
			return HasDomChild(NodeType.Element, "", "KeywordList");
		}

		public KeywordListType NewKeywordList()
		{
			return new KeywordListType(domNode.OwnerDocument.CreateElement("KeywordList", ""));
		}

		public KeywordListType GetKeywordListAt(int index)
		{
			return new KeywordListType(GetDomChildAt(NodeType.Element, "", "KeywordList", index));
		}

		public XmlNode GetStartingKeywordListCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "KeywordList" );
		}

		public XmlNode GetAdvancedKeywordListCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "KeywordList", curNode );
		}

		public KeywordListType GetKeywordListValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new KeywordListType( curNode );
		}


		public KeywordListType GetKeywordList()
		{
			return GetKeywordListAt(0);
		}

		public KeywordListType KeywordList
		{
			get
			{
				return GetKeywordListAt(0);
			}
		}

		public void RemoveKeywordListAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "KeywordList", index);
		}

		public void RemoveKeywordList()
		{
			RemoveKeywordListAt(0);
		}

		public XmlNode AddKeywordList(KeywordListType newValue)
		{
			return AppendDomElement("", "KeywordList", newValue);
		}

		public void InsertKeywordListAt(KeywordListType newValue, int index)
		{
			InsertDomElementAt("", "KeywordList", index, newValue);
		}

		public void ReplaceKeywordListAt(KeywordListType newValue, int index)
		{
			ReplaceDomElementAt("", "KeywordList", index, newValue);
		}
		#endregion // KeywordList accessor methods

		#region KeywordList collection
        public KeywordListCollection	MyKeywordLists = new KeywordListCollection( );

        public class KeywordListCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public KeywordListEnumerator GetEnumerator() 
			{
				return new KeywordListEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class KeywordListEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public KeywordListEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.KeywordListCount );
			}
			public KeywordListType  Current 
			{
				get 
				{
					return(parent.GetKeywordListAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // KeywordList collection

		#region OnlineResource Documentation
		public static string GetOnlineResourceAnnoDocumentation()
		{
			return "";		
		}
		public static string GetOnlineResourceDefault()
		{
			return "";		
		}
		#endregion

		#region OnlineResource accessor methods
		public static int GetOnlineResourceMinCount()
		{
			return 1;
		}

		public static int OnlineResourceMinCount
		{
			get
			{
				return 1;
			}
		}

		public static int GetOnlineResourceMaxCount()
		{
			return 1;
		}

		public static int OnlineResourceMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetOnlineResourceCount()
		{
			return DomChildCount(NodeType.Element, "", "OnlineResource");
		}

		public int OnlineResourceCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "OnlineResource");
			}
		}

		public bool HasOnlineResource()
		{
			return HasDomChild(NodeType.Element, "", "OnlineResource");
		}

		public OnlineResourceType NewOnlineResource()
		{
			return new OnlineResourceType(domNode.OwnerDocument.CreateElement("OnlineResource", ""));
		}

		public OnlineResourceType GetOnlineResourceAt(int index)
		{
			return new OnlineResourceType(GetDomChildAt(NodeType.Element, "", "OnlineResource", index));
		}

		public XmlNode GetStartingOnlineResourceCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "OnlineResource" );
		}

		public XmlNode GetAdvancedOnlineResourceCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "OnlineResource", curNode );
		}

		public OnlineResourceType GetOnlineResourceValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new OnlineResourceType( curNode );
		}


		public OnlineResourceType GetOnlineResource()
		{
			return GetOnlineResourceAt(0);
		}

		public OnlineResourceType OnlineResource
		{
			get
			{
				return GetOnlineResourceAt(0);
			}
		}

		public void RemoveOnlineResourceAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "OnlineResource", index);
		}

		public void RemoveOnlineResource()
		{
			RemoveOnlineResourceAt(0);
		}

		public XmlNode AddOnlineResource(OnlineResourceType newValue)
		{
			return AppendDomElement("", "OnlineResource", newValue);
		}

		public void InsertOnlineResourceAt(OnlineResourceType newValue, int index)
		{
			InsertDomElementAt("", "OnlineResource", index, newValue);
		}

		public void ReplaceOnlineResourceAt(OnlineResourceType newValue, int index)
		{
			ReplaceDomElementAt("", "OnlineResource", index, newValue);
		}
		#endregion // OnlineResource accessor methods

		#region OnlineResource collection
        public OnlineResourceCollection	MyOnlineResources = new OnlineResourceCollection( );

        public class OnlineResourceCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public OnlineResourceEnumerator GetEnumerator() 
			{
				return new OnlineResourceEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class OnlineResourceEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public OnlineResourceEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.OnlineResourceCount );
			}
			public OnlineResourceType  Current 
			{
				get 
				{
					return(parent.GetOnlineResourceAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // OnlineResource collection

		#region ContactInformation Documentation
		public static string GetContactInformationAnnoDocumentation()
		{
			return "";		
		}
		public static string GetContactInformationDefault()
		{
			return "";		
		}
		#endregion

		#region ContactInformation accessor methods
		public static int GetContactInformationMinCount()
		{
			return 0;
		}

		public static int ContactInformationMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetContactInformationMaxCount()
		{
			return 1;
		}

		public static int ContactInformationMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetContactInformationCount()
		{
			return DomChildCount(NodeType.Element, "", "ContactInformation");
		}

		public int ContactInformationCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "ContactInformation");
			}
		}

		public bool HasContactInformation()
		{
			return HasDomChild(NodeType.Element, "", "ContactInformation");
		}

		public ContactInformationType NewContactInformation()
		{
			return new ContactInformationType(domNode.OwnerDocument.CreateElement("ContactInformation", ""));
		}

		public ContactInformationType GetContactInformationAt(int index)
		{
			return new ContactInformationType(GetDomChildAt(NodeType.Element, "", "ContactInformation", index));
		}

		public XmlNode GetStartingContactInformationCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "ContactInformation" );
		}

		public XmlNode GetAdvancedContactInformationCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "ContactInformation", curNode );
		}

		public ContactInformationType GetContactInformationValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new ContactInformationType( curNode );
		}


		public ContactInformationType GetContactInformation()
		{
			return GetContactInformationAt(0);
		}

		public ContactInformationType ContactInformation
		{
			get
			{
				return GetContactInformationAt(0);
			}
		}

		public void RemoveContactInformationAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "ContactInformation", index);
		}

		public void RemoveContactInformation()
		{
			RemoveContactInformationAt(0);
		}

		public XmlNode AddContactInformation(ContactInformationType newValue)
		{
			return AppendDomElement("", "ContactInformation", newValue);
		}

		public void InsertContactInformationAt(ContactInformationType newValue, int index)
		{
			InsertDomElementAt("", "ContactInformation", index, newValue);
		}

		public void ReplaceContactInformationAt(ContactInformationType newValue, int index)
		{
			ReplaceDomElementAt("", "ContactInformation", index, newValue);
		}
		#endregion // ContactInformation accessor methods

		#region ContactInformation collection
        public ContactInformationCollection	MyContactInformations = new ContactInformationCollection( );

        public class ContactInformationCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public ContactInformationEnumerator GetEnumerator() 
			{
				return new ContactInformationEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class ContactInformationEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public ContactInformationEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.ContactInformationCount );
			}
			public ContactInformationType  Current 
			{
				get 
				{
					return(parent.GetContactInformationAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // ContactInformation collection

		#region Fees Documentation
		public static string GetFeesAnnoDocumentation()
		{
			return "";		
		}
		public static string GetFeesDefault()
		{
			return "";		
		}
		#endregion

		#region Fees accessor methods
		public static int GetFeesMinCount()
		{
			return 0;
		}

		public static int FeesMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetFeesMaxCount()
		{
			return 1;
		}

		public static int FeesMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetFeesCount()
		{
			return DomChildCount(NodeType.Element, "", "Fees");
		}

		public int FeesCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "Fees");
			}
		}

		public bool HasFees()
		{
			return HasDomChild(NodeType.Element, "", "Fees");
		}

		public FeesType NewFees()
		{
			return new FeesType(domNode.OwnerDocument.CreateElement("Fees", ""));
		}

		public FeesType GetFeesAt(int index)
		{
			return new FeesType(GetDomChildAt(NodeType.Element, "", "Fees", index));
		}

		public XmlNode GetStartingFeesCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "Fees" );
		}

		public XmlNode GetAdvancedFeesCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "Fees", curNode );
		}

		public FeesType GetFeesValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new FeesType( curNode );
		}


		public FeesType GetFees()
		{
			return GetFeesAt(0);
		}

		public FeesType Fees
		{
			get
			{
				return GetFeesAt(0);
			}
		}

		public void RemoveFeesAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "Fees", index);
		}

		public void RemoveFees()
		{
			RemoveFeesAt(0);
		}

		public XmlNode AddFees(FeesType newValue)
		{
			return AppendDomElement("", "Fees", newValue);
		}

		public void InsertFeesAt(FeesType newValue, int index)
		{
			InsertDomElementAt("", "Fees", index, newValue);
		}

		public void ReplaceFeesAt(FeesType newValue, int index)
		{
			ReplaceDomElementAt("", "Fees", index, newValue);
		}
		#endregion // Fees accessor methods

		#region Fees collection
        public FeesCollection	MyFeess = new FeesCollection( );

        public class FeesCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public FeesEnumerator GetEnumerator() 
			{
				return new FeesEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class FeesEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public FeesEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.FeesCount );
			}
			public FeesType  Current 
			{
				get 
				{
					return(parent.GetFeesAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // Fees collection

		#region AccessConstraints Documentation
		public static string GetAccessConstraintsAnnoDocumentation()
		{
			return "";		
		}
		public static string GetAccessConstraintsDefault()
		{
			return "";		
		}
		#endregion

		#region AccessConstraints accessor methods
		public static int GetAccessConstraintsMinCount()
		{
			return 0;
		}

		public static int AccessConstraintsMinCount
		{
			get
			{
				return 0;
			}
		}

		public static int GetAccessConstraintsMaxCount()
		{
			return 1;
		}

		public static int AccessConstraintsMaxCount
		{
			get
			{
				return 1;
			}
		}

		public int GetAccessConstraintsCount()
		{
			return DomChildCount(NodeType.Element, "", "AccessConstraints");
		}

		public int AccessConstraintsCount
		{
			get
			{
				return DomChildCount(NodeType.Element, "", "AccessConstraints");
			}
		}

		public bool HasAccessConstraints()
		{
			return HasDomChild(NodeType.Element, "", "AccessConstraints");
		}

		public AccessConstraintsType NewAccessConstraints()
		{
			return new AccessConstraintsType(domNode.OwnerDocument.CreateElement("AccessConstraints", ""));
		}

		public AccessConstraintsType GetAccessConstraintsAt(int index)
		{
			return new AccessConstraintsType(GetDomChildAt(NodeType.Element, "", "AccessConstraints", index));
		}

		public XmlNode GetStartingAccessConstraintsCursor()
		{
			return GetDomFirstChild( NodeType.Element, "", "AccessConstraints" );
		}

		public XmlNode GetAdvancedAccessConstraintsCursor( XmlNode curNode )
		{
			return GetDomNextChild( NodeType.Element, "", "AccessConstraints", curNode );
		}

		public AccessConstraintsType GetAccessConstraintsValueAtCursor( XmlNode curNode )
		{
			if( curNode == null )
				  throw new Altova.Xml.XmlException("Out of range");
			else
				return new AccessConstraintsType( curNode );
		}


		public AccessConstraintsType GetAccessConstraints()
		{
			return GetAccessConstraintsAt(0);
		}

		public AccessConstraintsType AccessConstraints
		{
			get
			{
				return GetAccessConstraintsAt(0);
			}
		}

		public void RemoveAccessConstraintsAt(int index)
		{
			RemoveDomChildAt(NodeType.Element, "", "AccessConstraints", index);
		}

		public void RemoveAccessConstraints()
		{
			RemoveAccessConstraintsAt(0);
		}

		public XmlNode AddAccessConstraints(AccessConstraintsType newValue)
		{
			return AppendDomElement("", "AccessConstraints", newValue);
		}

		public void InsertAccessConstraintsAt(AccessConstraintsType newValue, int index)
		{
			InsertDomElementAt("", "AccessConstraints", index, newValue);
		}

		public void ReplaceAccessConstraintsAt(AccessConstraintsType newValue, int index)
		{
			ReplaceDomElementAt("", "AccessConstraints", index, newValue);
		}
		#endregion // AccessConstraints accessor methods

		#region AccessConstraints collection
        public AccessConstraintsCollection	MyAccessConstraintss = new AccessConstraintsCollection( );

        public class AccessConstraintsCollection: IEnumerable
        {
            ServiceType parent;
            public ServiceType Parent
			{
				set
				{
					parent = value;
				}
			}
			public AccessConstraintsEnumerator GetEnumerator() 
			{
				return new AccessConstraintsEnumerator(parent);
			}
		
			IEnumerator IEnumerable.GetEnumerator() 
			{
				return GetEnumerator();
			}
        }

        public class AccessConstraintsEnumerator: IEnumerator 
        {
			int nIndex;
			ServiceType parent;
			public AccessConstraintsEnumerator(ServiceType par) 
			{
				parent = par;
				nIndex = -1;
			}
			public void Reset() 
			{
				nIndex = -1;
			}
			public bool MoveNext() 
			{
				nIndex++;
				return(nIndex < parent.AccessConstraintsCount );
			}
			public AccessConstraintsType  Current 
			{
				get 
				{
					return(parent.GetAccessConstraintsAt(nIndex));
				}
			}
			object IEnumerator.Current 
			{
				get 
				{
					return(Current);
				}
			}
    	}

        #endregion // AccessConstraints collection

        private void SetCollectionParents()
        {
            MyNames.Parent = this; 
            MyTitles.Parent = this; 
            MyAbstract2s.Parent = this; 
            MyKeywordLists.Parent = this; 
            MyOnlineResources.Parent = this; 
            MyContactInformations.Parent = this; 
            MyFeess.Parent = this; 
            MyAccessConstraintss.Parent = this; 
	}
}
}