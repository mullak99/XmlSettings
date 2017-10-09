using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

public class XmlSettings
{
    private string mainSettingsPath;
    private bool revertOnFail;
    private const string xmlFormatVer = "1.0";

    XmlDocument doc = new XmlDocument();
    XmlDocument loadedDoc = new XmlDocument();
    XmlElement xmlBody;
    string xmlContents;

    /// <summary>
    /// Allows the use of the XmlSettings library.
    /// </summary>
    /// <param name="settingsPath">The path for the settings file, can either be relative or absolute.</param>
    /// <param name="revertToDefaultOnFail">States if the settings should be reverted to the default if it cannot be parsed.</param>
    public XmlSettings(string settingsPath, bool revertToDefaultOnFail = true)
    {
        mainSettingsPath = settingsPath;
        revertOnFail = revertToDefaultOnFail;

        if (!File.Exists(mainSettingsPath))
        {
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration(xmlFormatVer, "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            xmlBody = doc.CreateElement(string.Empty, "body", string.Empty);
            doc.AppendChild(xmlBody);

            saveXML();
        }
    }

    /// <summary>
    /// Used to get the current version of XmlSettings.
    /// </summary>
    /// <returns>The version number of XmlSettings.</returns>
    public string getVersion()
    {
        return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
    }

    /// <summary>
    /// Used to get the max supported formatting version of the Xml Document that can be read by this version of XmlSettings.
    /// </summary>
    /// <returns>The latest supported formatting version that XmlSettings can work with.</returns>
    public string getLatestSupportedFormattingVersion()
    {
        return xmlFormatVer;
    }

    //Creates and sets the default value for various datatypes
    public bool addBoolean(string varName, bool defaultValue)
    {
        if (!doesDuplicateExist(varName))
        {
            XmlNode booleanNode = doc.CreateElement("boolean");
            xmlBody.AppendChild(booleanNode);

            XmlNode varNode = doc.CreateElement(varName);
            XmlAttribute defaultVal = doc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            booleanNode.AppendChild(varNode);

            saveXML();

            return true;
        }
        else
            return false;
    }

    public bool addString(string varName, string defaultValue)
    {
        if (!doesDuplicateExist(varName))
        {
            XmlNode stringNode = doc.CreateElement("string");
            xmlBody.AppendChild(stringNode);

            XmlNode varNode = doc.CreateElement(varName);
            XmlAttribute defaultVal = doc.CreateAttribute("default");
            defaultVal.Value = defaultValue;
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue;
            stringNode.AppendChild(varNode);

            saveXML();

            return true;
        }
        else
            return false;
    }

    public bool addLong(string varName, long defaultValue)
    {
        if (!doesDuplicateExist(varName))
        {
            XmlNode longNode = doc.CreateElement("long");
            xmlBody.AppendChild(longNode);

            XmlNode varNode = doc.CreateElement(varName);
            XmlAttribute defaultVal = doc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            longNode.AppendChild(varNode);

            saveXML();

            return true;
        }
        else
            return false;
    }

    public bool addInt(string varName, int defaultValue)
    {
        if (!doesDuplicateExist(varName))
        {
            XmlNode intNode = doc.CreateElement("int");
            xmlBody.AppendChild(intNode);

            XmlNode varNode = doc.CreateElement(varName);
            XmlAttribute defaultVal = doc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            intNode.AppendChild(varNode);

            saveXML();

            return true;
        }
        else
            return false;
    }

    public bool addDouble(string varName, double defaultValue)
    {
        if (!doesDuplicateExist(varName))
        {
            XmlNode doubleNode = doc.CreateElement("double");
            xmlBody.AppendChild(doubleNode);

            XmlNode varNode = doc.CreateElement(varName);
            XmlAttribute defaultVal = doc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            doubleNode.AppendChild(varNode);

            saveXML();
            return true;
        }
        else
            return false;
    }

    //Sets the value for various datatypes
    public bool setBoolean(string varName, bool state)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            forceSaveOnLoadedXML();
            return true;
        }
        else
            return false;
    }

    public bool setString(string varName, string state)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state;
            forceSaveOnLoadedXML();
            return true;
        }
        else
            return false;
    }

    public bool setInt(string varName, int state)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            forceSaveOnLoadedXML();
            return true;
        }
        else
            return false;
    }

    public bool setLong(string varName, long state)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            forceSaveOnLoadedXML();
            return true;
        }
        else
            return false;
    }

    public bool setDouble(string varName, double state)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            forceSaveOnLoadedXML();
            return true;
        }
        else
            return false;
    }

    //Reads the current value for various datatypes

    public bool readBoolean(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
            if (elemList.Count > 0)
                return bool.Parse(elemList[0].InnerXml);
            else
                return false;
        }
        catch
        {
            if (revertOnFail) revertToDefault(varName);
            return false;
        }
    }

    public string readString(string varName)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return elemList[0].InnerXml.ToString();
        else
            return "";
    }

    public int readInt(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
            if (elemList.Count > 0)
                return int.Parse(elemList[0].InnerXml);
            else
                return 0;
        }
        catch
        {
            if (revertOnFail) revertToDefault(varName);
            return 0;
        }

    }

    public long readLong(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
            if (elemList.Count > 0)
                return long.Parse(elemList[0].InnerXml);
            else
                return 0;
        }
        catch
        {
            if (revertOnFail) revertToDefault(varName);
            return 0;
        }
    }

    public double readDouble(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
            if (elemList.Count > 0)
                return double.Parse(elemList[0].InnerXml);
            else
                return 0;
        }
        catch
        {
            if (revertOnFail) revertToDefault(varName);
            return 0;
        }
    }

    public bool revertToDefault(string varName)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = elemList[0].Attributes["default"].Value;
            forceSaveOnLoadedXML();
            return true;
        }
        else
            return false;
    }

    public bool doesVariableExist(string varName)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return true;
        else
            return false;
    }

    public string getVarType(string varName)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return uppercaseFirst(elemList[0].ParentNode.Name);
        else
            return null;
    }

    private void saveXML()
    {
        doc.Save(mainSettingsPath);
    }

    private void forceSaveOnLoadedXML()
    {
        loadedDoc.Save(mainSettingsPath);
    }

    private void loadXML()
    {
        loadedDoc.Load(mainSettingsPath);
        xmlContents = loadedDoc.InnerXml;
    }

    private bool doesDuplicateExist(string varName)
    {
        loadXML();
        XmlNodeList elemList = loadedDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return true;
        else
            return false;
    }

    private string uppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        return char.ToUpper(s[0]) + s.Substring(1);
    }
}