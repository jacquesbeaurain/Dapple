<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:xs="http://www.w3.org/2001/XMLSchema">
	<xsl:output version="1.0" method="html" indent="no" encoding="UTF-8"/>
   <xsl:template match="/">
      <html>
			<head>
            <script type="text/javascript" src="geometatree.js"/>
            <link href="geosoft.meta.css" type="text/css" rel="stylesheet"/>
         </head>
			<body>
               <xsl:for-each select="/">
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="Title">
								<h2>
									<xsl:apply-templates/>
								</h2>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
               <h3>
                  <xsl:text>Abstract</xsl:text>
               </h3>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="Abstract">
								<p>
									<xsl:apply-templates/>
								</p>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<h3>
						<xsl:text>Contact Information</xsl:text>
					</h3>
					<b>
						<xsl:text>Primary Contact: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
								<xsl:for-each select="ContactPersonPrimary">
									<xsl:for-each select="ContactPerson">
										<xsl:apply-templates/>
									</xsl:for-each>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Organization: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
								<xsl:for-each select="ContactPersonPrimary">
									<xsl:for-each select="ContactOrganization">
										<xsl:apply-templates/>
									</xsl:for-each>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Position: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
								<xsl:for-each select="ContactPosition">
									<xsl:apply-templates/>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
               <table border="0" cellpadding="0" cellspacing="0">
                  <tr>
                     <td>
                        <b>
                           <xsl:text>Address: </xsl:text>
                        </b>
                     </td><td>
            <xsl:for-each select="WMT_MS_Capabilities">
               <xsl:for-each select="Service">
                  <xsl:for-each select="ContactInformation">
                     <xsl:for-each select="ContactAddress">
                        <xsl:for-each select="Address">
                           <xsl:apply-templates/>
                        </xsl:for-each>
                     </xsl:for-each>
                  </xsl:for-each>
               </xsl:for-each>
            </xsl:for-each>
                     </td>
                  </tr>
                  <tr>
                     <td></td><td>
            <xsl:for-each select="WMT_MS_Capabilities">
               <xsl:for-each select="Service">
                  <xsl:for-each select="ContactInformation">
                     <xsl:for-each select="ContactAddress">
                        <xsl:for-each select="City">
                           <xsl:apply-templates/>
                        </xsl:for-each>
                     </xsl:for-each>
                  </xsl:for-each>
               </xsl:for-each>
            </xsl:for-each>
                     </td>
                  </tr>
                  <tr>
                     <td></td><td>
            <xsl:for-each select="WMT_MS_Capabilities">
               <xsl:for-each select="Service">
                  <xsl:for-each select="ContactInformation">
                     <xsl:for-each select="ContactAddress">
                        <xsl:for-each select="StateOrProvince">
                           <xsl:apply-templates/>
                        </xsl:for-each>
                     </xsl:for-each>
                  </xsl:for-each>
               </xsl:for-each>
            </xsl:for-each>
                     </td>
                  </tr>
                  <tr>
                     <td></td><td>
            <xsl:for-each select="WMT_MS_Capabilities">
               <xsl:for-each select="Service">
                  <xsl:for-each select="ContactInformation">
                     <xsl:for-each select="ContactAddress">
                        <xsl:for-each select="PostCode">
                           <xsl:apply-templates/>
                        </xsl:for-each>
                     </xsl:for-each>
                  </xsl:for-each>
               </xsl:for-each>
            </xsl:for-each>
                     </td>
                  </tr>
                  <tr>
                     <td></td><td>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
								<xsl:for-each select="ContactAddress">
									<xsl:for-each select="Country">
										<xsl:apply-templates/>
									</xsl:for-each>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
                     </td>
                  </tr>
               </table>
					<b>
						<xsl:text>Voice Phone Number: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
								<xsl:for-each select="ContactVoiceTelephone">
									<xsl:apply-templates/>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Fax Number: </xsl:text>
					</b>
					<xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
								<xsl:for-each select="ContactFacsimileTelephone">
									<xsl:apply-templates/>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
					<b>
						<xsl:text>Email Address: </xsl:text>
					</b>
               <xsl:for-each select="WMT_MS_Capabilities">
						<xsl:for-each select="Service">
							<xsl:for-each select="ContactInformation">
                        <xsl:for-each select="ContactElectronicMailAddress">
                              <a>
                                 <xsl:attribute name="href">
                                    mailto:<xsl:value-of select="."
                                   />
                                 </xsl:attribute>
                                 <xsl:value-of select="."/>
                              </a>
                        </xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
					<br/>
				</xsl:for-each>
            <h3>
               <xsl:text>Layers</xsl:text>
            </h3>
            <xsl:for-each select="WMT_MS_Capabilities">
               <xsl:for-each select="Capability">
                  <xsl:apply-templates select="Layer">
                     <xsl:with-param name="pstrIndent" select="''"/>
                  </xsl:apply-templates>
               </xsl:for-each>
            </xsl:for-each>
         </body>
      </html>
   </xsl:template>
   <xsl:template match="Layer">
	<xsl:param name="pstrIndent"/>
	<xsl:variable name="vthisIndent" select="concat($pstrIndent, '-')"/>
	<span class="trigger">
		<xsl:attribute name="onClick">showBranch('<xsl:number count="*[Title]" level="any"/>');</xsl:attribute>
		<img src="collapsed.gif">
			<xsl:attribute name="id">I<xsl:number count="*[Title]" level="any"/></xsl:attribute>
		</img>
		<xsl:text> </xsl:text>
		<xsl:value-of select="Title"/>
		<br/>
	</span>
	<span class="branch">
		<xsl:attribute name="id"><xsl:number count="*[Title]" level="any"/></xsl:attribute>
		<xsl:apply-templates select="Layer">
			<xsl:with-param name="pstrIndent" select="$vthisIndent"/>
		</xsl:apply-templates>
	</span>
   </xsl:template>
</xsl:stylesheet>

