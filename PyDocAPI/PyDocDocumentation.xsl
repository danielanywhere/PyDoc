<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
 <!-- Running this file in Visual Studio will create an HTML output file. -->
 <!-- Document Template -->
 <!-- Format as a valid HTML document -->
 <xsl:template match="/">
  <!-- <!DOCTYPE html> -->
  <html>
   <head>
    <meta content="text/html; charset=utf-8" http-equiv="Content-Type" />
    <title>PyDoc Source API</title>
    <style type="text/css">
     body, th, td {
     font-family: Calibri, Verdana, Helvetica, sans-serif;
     font-size: 12pt;
     background-color: #f7f7f7;
     }
     h2 {
     color: #333399;
     }
     h3 {
     margin-left: 25px;
     }
     h4 {
     margin-left: 50px;
     font-family: lucida console,monospace;
     font-weight: normal;
     color: #1133cc;
     text-decoration: underline;
     }
     h5 {
     font-size: 12pt;
     font-style: italic;
     font-weight: normal;
     text-decoration: underline;
     }
     h5, p {
     margin-left: 75px;
     }
     hr {
     border-bottom: 2px solid #7f7f7f;
     }
     th {
     text-align: left;
     }
    </style>
   </head>
   <body>
    <xsl:apply-templates select="//assembly"/>
   </body>
  </html>
 </xsl:template>

 <!-- Assembly Template -->
 <!-- Display each assembly name and members. -->
 <xsl:template match="assembly">
 <h1><xsl:value-of select="name"/> API</h1>
 <xsl:apply-templates select="//member[contains(@name,'T:')]"/>
 </xsl:template>

 <!-- Type Template -->
 <!-- Loop through member types and display their properties and methods -->
 <xsl:template match="//member[contains(@name,'T:')]">
 <hr />
 <!-- Create variables for both the short and long names. -->
 <!-- This type's display name. -->
 <xsl:variable name="MemberName" select="substring-after(@name, '.')"/>
 <!-- This type's long name, without the T: prefix -->
 <xsl:variable name="FullMemberName" select="substring-after(@name, ':')"/>
 <!-- Render the information in this type. -->
 <h2><xsl:value-of select="$MemberName"/></h2>
 <xsl:apply-templates />

 <!-- Output any public fields. -->
 <xsl:if test="//member[contains(@name,concat('F:',$FullMemberName))]">
 <h3>Fields</h3>
 <xsl:for-each select="//member[contains(@name,concat('F:',$FullMemberName))]">
 <h4><xsl:value-of select="substring-after(@name, concat('F:',$FullMemberName,'.'))"/></h4>
 <xsl:apply-templates/>
 </xsl:for-each>
 </xsl:if>

 <!-- Output the properties. -->
 <xsl:if test="//member[contains(@name,concat('P:',$FullMemberName))]">
 <h3>Properties</h3>
 <xsl:for-each select="//member[contains(@name,concat('P:',$FullMemberName))]">
 <h4><xsl:value-of select="substring-after(@name, concat('P:',$FullMemberName,'.'))"/></h4>
 <xsl:apply-templates/>
 </xsl:for-each>
 </xsl:if>

 <!-- Output any methods. -->
 <xsl:if test="//member[contains(@name,concat('M:',$FullMemberName))]">
 <h3>Methods</h3>
 <xsl:for-each select="//member[contains(@name,concat('M:',$FullMemberName))]">
 <!-- When the item is a constructor, it will be "#ctor" by default. -->
 <!-- Change that to the name of the type -->
 <h4>
 <xsl:choose>
 <xsl:when test="contains(@name, '#ctor')">
 Constructor:
 <xsl:value-of select="$MemberName"/>
 <xsl:value-of select="substring-after(@name, '#ctor')"/>
 </xsl:when>
 <xsl:otherwise>
 <xsl:value-of select="substring-after(@name, concat('M:',$FullMemberName,'.'))"/>
 </xsl:otherwise>
 </xsl:choose>
 </h4>
 <xsl:apply-templates select="summary"/>

 <!-- Method parameters -->
 <xsl:if test="count(param)!=0">
 <h5>Parameters</h5>
 <xsl:apply-templates select="param"/>
 </xsl:if>

 <!-- Method return value -->
 <xsl:if test="count(returns)!=0">
 <h5>Return Value</h5>
 <xsl:apply-templates select="returns"/>
 </xsl:if>

 <!-- Possible Exceptions to be thrown. -->
 <xsl:if test="count(exception)!=0">
 <h5>Exceptions</h5>
 <xsl:apply-templates select="exception"/>
 </xsl:if>

 <!-- Render any examples that were provided in the comment. -->
 <xsl:if test="count(example)!=0">
 <h5>Example</h5>
 <xsl:apply-templates select="example"/>
 </xsl:if>

 </xsl:for-each>

 </xsl:if>
 </xsl:template>

 <!-- Support -->
 <!-- Templates that provide support while rendering. -->
 <xsl:template match="c">
 <code><xsl:apply-templates /></code>
 </xsl:template>

 <xsl:template match="code">
 <pre><xsl:apply-templates /></pre>
 </xsl:template>

 <xsl:template match="example">
 <p><b>Example: </b><xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="exception">
 <p><b><xsl:value-of select="substring-after(@cref,'T:')"/></b>: <xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="include">
 <a href="{@file}">External file</a>
 </xsl:template>

 <xsl:template match="para">
 <p><xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="param">
 <p><b><xsl:value-of select="@name"/></b>: <xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="paramref">
 <em><xsl:value-of select="@name" /></em>
 </xsl:template>

 <xsl:template match="permission">
 <p><b>Permission</b>: <em><xsl:value-of select="@cref" /></em><xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="remarks">
 <p><xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="returns">
 <p><b>Return Value</b>: <xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="see">
 <em>See: <xsl:value-of select="@cref" /></em>
 </xsl:template>

 <xsl:template match="seealso">
 <em>See also: <xsl:value-of select="@cref" /></em>
 </xsl:template>

 <xsl:template match="summary">
 <p><xsl:apply-templates /></p>
 </xsl:template>

 <xsl:template match="list">
 <xsl:choose>
 <xsl:when test="@type='bullet'">
 <ul>
 <xsl:for-each select="listheader">
 <li><b><xsl:value-of select="term"/></b>: <xsl:value-of select="definition"/></li>
 </xsl:for-each>
 <xsl:for-each select="list">
 <li><b><xsl:value-of select="term"/></b>: <xsl:value-of select="definition"/></li>
 </xsl:for-each>
 </ul>
 </xsl:when>
 <xsl:when test="@type='number'">
 <ol>
 <xsl:for-each select="listheader">
 <li><b><xsl:value-of select="term"/></b>: <xsl:value-of select="definition"/></li>
 </xsl:for-each>
 <xsl:for-each select="list">
 <li><b><xsl:value-of select="term"/></b>: <xsl:value-of select="definition"/></li>
 </xsl:for-each>
 </ol>
 </xsl:when>
 <xsl:when test="@type='table'">
 <table>
 <xsl:for-each select="listheader">
 <tr>
 <th><xsl:value-of select="term"/></th>
 <th><xsl:value-of select="definition"/></th>
 </tr>
 </xsl:for-each>
 <xsl:for-each select="list">
 <tr>
 <td><b><xsl:value-of select="term"/>: </b></td>
 <td><xsl:value-of select="definition"/></td>
 </tr>
 </xsl:for-each>
 </table>
 </xsl:when>
 </xsl:choose>
 </xsl:template>

</xsl:stylesheet>
