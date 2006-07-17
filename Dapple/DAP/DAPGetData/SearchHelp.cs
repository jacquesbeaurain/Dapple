using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for SearchHelp.
	/// </summary>
	public class SearchHelp : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
      private System.Windows.Forms.RichTextBox rtbHelp;
      private System.Windows.Forms.Panel pOutline;

      private string m_strText = @"{\rtf1\ansi\ansicpg1252\uc1\deff0\stshfdbch0\stshfloch0\stshfhich0\stshfbi0\deflang1033\deflangfe1033{\fonttbl{\f0\froman\fcharset0\fprq2{\*\panose 02020603050405020304}Times New Roman;}{\f1\fswiss\fcharset0\fprq2{\*\panose 020b0604020202020204}Arial;}
{\f37\fswiss\fcharset0\fprq2{\*\panose 020b0604030504040204}Verdana;}{\f38\froman\fcharset238\fprq2 Times New Roman CE;}{\f39\froman\fcharset204\fprq2 Times New Roman Cyr;}{\f41\froman\fcharset161\fprq2 Times New Roman Greek;}
{\f42\froman\fcharset162\fprq2 Times New Roman Tur;}{\f43\froman\fcharset177\fprq2 Times New Roman (Hebrew);}{\f44\froman\fcharset178\fprq2 Times New Roman (Arabic);}{\f45\froman\fcharset186\fprq2 Times New Roman Baltic;}
{\f46\froman\fcharset163\fprq2 Times New Roman (Vietnamese);}{\f48\fswiss\fcharset238\fprq2 Arial CE;}{\f49\fswiss\fcharset204\fprq2 Arial Cyr;}{\f51\fswiss\fcharset161\fprq2 Arial Greek;}{\f52\fswiss\fcharset162\fprq2 Arial Tur;}
{\f53\fswiss\fcharset177\fprq2 Arial (Hebrew);}{\f54\fswiss\fcharset178\fprq2 Arial (Arabic);}{\f55\fswiss\fcharset186\fprq2 Arial Baltic;}{\f56\fswiss\fcharset163\fprq2 Arial (Vietnamese);}{\f408\fswiss\fcharset238\fprq2 Verdana CE;}
{\f409\fswiss\fcharset204\fprq2 Verdana Cyr;}{\f411\fswiss\fcharset161\fprq2 Verdana Greek;}{\f412\fswiss\fcharset162\fprq2 Verdana Tur;}{\f415\fswiss\fcharset186\fprq2 Verdana Baltic;}{\f416\fswiss\fcharset163\fprq2 Verdana (Vietnamese);}}
{\colortbl;\red0\green0\blue0;\red0\green0\blue255;\red0\green255\blue255;\red0\green255\blue0;\red255\green0\blue255;\red255\green0\blue0;\red255\green255\blue0;\red255\green255\blue255;\red0\green0\blue128;\red0\green128\blue128;\red0\green128\blue0;
\red128\green0\blue128;\red128\green0\blue0;\red128\green128\blue0;\red128\green128\blue128;\red192\green192\blue192;}{\stylesheet{\ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 
\fs24\lang1033\langfe1033\cgrid\langnp1033\langfenp1033 \snext0 Normal;}{\s1\ql \li0\ri0\sb240\sa60\keepn\widctlpar\aspalpha\aspnum\faauto\outlinelevel0\adjustright\rin0\lin0\itap0 \b\f1\fs32\lang1033\langfe1033\kerning32\cgrid\langnp1033\langfenp1033 
\sbasedon0 \snext0 \styrsid1779773 heading 1;}{\*\cs10 \additive \ssemihidden Default Paragraph Font;}{\*
\ts11\tsrowd\trftsWidthB3\trpaddl108\trpaddr108\trpaddfl3\trpaddft3\trpaddfb3\trpaddfr3\tscellwidthfts0\tsvertalt\tsbrdrt\tsbrdrl\tsbrdrb\tsbrdrr\tsbrdrdgl\tsbrdrdgr\tsbrdrh\tsbrdrv 
\ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 \fs20\lang1024\langfe1024\cgrid\langnp1024\langfenp1024 \snext11 \ssemihidden Normal Table;}{\s15\ql \li0\ri0\sa17\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0 
\f37\fs20\lang1033\langfe1033\cgrid\langnp1033\langfenp1033 \sbasedon0 \snext15 \styrsid13723006 table;}{\*\cs16 \additive \ul\cf2 \sbasedon10 \styrsid13723006 Hyperlink;}}{\*\latentstyles\lsdstimax156\lsdlockeddef0}{\*\pgptbl {\pgp\ipgp0\itap0\li0\ri0
\sb0\sa17}}{\*\rsidtbl \rsid1779773\rsid12781665\rsid13723006}{\*\generator Microsoft Word 11.0.6568;}{\info{\title DAP Text Searches}{\author ian}{\operator ian}{\creatim\yr2006\mo3\dy30\hr13\min12}{\revtim\yr2006\mo3\dy30\hr13\min34}{\version1}
{\edmins22}{\nofpages1}{\nofwords143}{\nofchars816}{\*\company Geosoft Inc}{\nofcharsws958}{\vern24579}}\widowctrl\ftnbj\aenddoc\noxlattoyen\expshrtn\noultrlspc\dntblnsbdb\nospaceforul\formshade\horzdoc\dgmargin\dghspace180\dgvspace180\dghorigin1800
\dgvorigin1440\dghshow1\dgvshow1\jexpand\viewkind1\viewscale90\pgbrdrhead\pgbrdrfoot\splytwnine\ftnlytwnine\htmautsp\nolnhtadjtbl\useltbaln\alntblind\lytcalctblwd\lyttblrtgr\lnbrkrule\nobrkwrptbl\snaptogridincell\allowfieldendsel\wrppunct
\asianbrkrule\nojkernpunct\rsidroot13723006 \fet0\sectd \linex0\endnhere\sectlinegrid360\sectdefaultcl\sftnbj {\*\pnseclvl1\pnucrm\pnstart1\pnindent720\pnhang {\pntxta .}}{\*\pnseclvl2\pnucltr\pnstart1\pnindent720\pnhang {\pntxta .}}{\*\pnseclvl3
\pndec\pnstart1\pnindent720\pnhang {\pntxta .}}{\*\pnseclvl4\pnlcltr\pnstart1\pnindent720\pnhang {\pntxta )}}{\*\pnseclvl5\pndec\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl6\pnlcltr\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}
{\*\pnseclvl7\pnlcrm\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl8\pnlcltr\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}{\*\pnseclvl9\pnlcrm\pnstart1\pnindent720\pnhang {\pntxtb (}{\pntxta )}}\pard\plain 
\s1\ql \li0\ri0\sb240\sa60\keepn\widctlpar\aspalpha\aspnum\faauto\outlinelevel0\adjustright\rin0\lin0\itap0\pararsid1779773 \b\f1\fs32\lang1033\langfe1033\kerning32\cgrid\langnp1033\langfenp1033 {\insrsid1779773\charrsid1779773 DAP Text Searches
\par }\pard\plain \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid13723006 \fs24\lang1033\langfe1033\cgrid\langnp1033\langfenp1033 {\insrsid1779773 
\par }{\insrsid13723006 A text search is made up one or more words with special characters and simple Boolean operators.
\par 
\par The following special characters can be placed in a word:}{\insrsid13723006 
\par  
\par }\pard \ql \fi-720\li1440\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin1440\itap0\pararsid1779773 {\insrsid13723006 ?}{\insrsid13723006 \tab }{\insrsid13723006 matches any }{\insrsid13723006 single }{\insrsid13723006 character
\par  
\par *}{\insrsid13723006 \tab }{\insrsid13723006 matches any number of characters 
\par  
\par ~~}{\insrsid13723006 \tab }{\insrsid13723006 numeric range}{\insrsid1779773 .  For example\line \line \tab }{\i\insrsid13723006\charrsid1779773 12~~25}{\i\insrsid1779773 \line \line }{\insrsid13723006  matches all numbers between }{
\i\insrsid13723006\charrsid1779773 12}{\insrsid13723006  and }{\i\insrsid13723006\charrsid1779773 25}{\insrsid13723006 
\par }\pard \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid13723006 {\insrsid13723006 
\par The following }{\insrsid1779773 Boolean}{\insrsid13723006  operators can be placed between words, or bracketed words:
\par 
\par }\pard \ql \fi-720\li1440\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin1440\itap0\pararsid1779773 {\insrsid13723006 and\tab both the left and right words must exist
\par or\tab the left or the right word must exist
\par }\pard \ql \fi-720\li1440\ri0\widctlpar\tx2160\aspalpha\aspnum\faauto\adjustright\rin0\lin1440\itap0\pararsid1779773 {\insrsid13723006 not\tab }{\insrsid1779773 makes the word that follows have opposite logic.  For example:\line \line \tab }{
\i\insrsid1779773\charrsid1779773 apple and not pear}{\insrsid1779773  \line \line will match text that includes \'93}{\i\insrsid1779773\charrsid1779773 apple}{\insrsid1779773 \'94 and does not include \'93}{\i\insrsid1779773\charrsid1779773 pear}{
\insrsid1779773 \'94}{\insrsid13723006 
\par }\pard \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid13723006 {\insrsid1779773 
\par You can use brackets to clarify the sequence of the search.  For example:\line 
\par }\pard \ql \fi720\li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid1779773 {\i\insrsid1779773 m}{\i\insrsid1779773\charrsid1779773 ag}{\i\insrsid1779773 *}{\i\insrsid1779773\charrsid1779773  and (gold or silver)}{
\i\insrsid1779773 
\par }\pard \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid1779773 {\insrsid1779773 
\par will find all data with words that begin with \'93}{\i\insrsid1779773\charrsid1779773 mag}{\insrsid1779773 \'94 followed by any characters and have }{\i\insrsid1779773\charrsid1779773 gold}{\insrsid1779773  or }{\i\insrsid1779773\charrsid1779773 silver}{
\insrsid1779773 .
\par 
\par Words or sequences with spaces must be enclosed in double quotes.}{\insrsid12781665   For example:}{\insrsid1779773 
\par }{\insrsid12781665 
\par }\pard \ql \fi720\li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid12781665 {\insrsid12781665 \'93}{\i\insrsid12781665\charrsid12781665 red lake}{\insrsid12781665 \'94
\par }\pard \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid12781665 {\insrsid12781665 
\par 
\par }\pard \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid1779773 {\insrsid1779773 \line }{\insrsid1779773\charrsid1779773 
\par }\pard \ql \li0\ri0\widctlpar\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid13723006 {\insrsid13723006  }{\insrsid13723006 
\par }}";


		public SearchHelp()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

         System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
			System.IO.MemoryStream oStream = new System.IO.MemoryStream(encoder.GetBytes(m_strText));
         rtbHelp.LoadFile(oStream, RichTextBoxStreamType.RichText);
         oStream.Close();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
         this.rtbHelp = new System.Windows.Forms.RichTextBox();
         this.pOutline = new System.Windows.Forms.Panel();
         this.pOutline.SuspendLayout();
         this.SuspendLayout();
         // 
         // rtbHelp
         // 
         this.rtbHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.rtbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
         this.rtbHelp.Location = new System.Drawing.Point(10, 0);
         this.rtbHelp.Name = "rtbHelp";
         this.rtbHelp.ReadOnly = true;
         this.rtbHelp.Size = new System.Drawing.Size(622, 398);
         this.rtbHelp.TabIndex = 0;
         this.rtbHelp.Text = "";
         // 
         // pOutline
         // 
         this.pOutline.Controls.Add(this.rtbHelp);
         this.pOutline.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pOutline.DockPadding.Left = 10;
         this.pOutline.Location = new System.Drawing.Point(0, 0);
         this.pOutline.Name = "pOutline";
         this.pOutline.Size = new System.Drawing.Size(632, 398);
         this.pOutline.TabIndex = 1;
         // 
         // SearchHelp
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.BackColor = System.Drawing.SystemColors.Control;
         this.ClientSize = new System.Drawing.Size(632, 398);
         this.Controls.Add(this.pOutline);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.Name = "SearchHelp";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Search Help";
         this.pOutline.ResumeLayout(false);
         this.ResumeLayout(false);

      }
		#endregion
	}
}
