using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

public class XmlSettings
{
    private const string xmlFormatVer = "1.0";

    private XmlDocument doc = new XmlDocument();
    private XmlElement xmlBody;
    private string xmlContents;

    private string mainSettingsPath;
    private bool revertOnFail;
    private bool purgeOnRootFailure;
    private string errorLogPathLoc;
    private bool verboseLogging;

    /// <summary>
    /// Allows the use of the XmlSettings library.
    /// </summary>
    /// <param name="settingsPath">The path for the settings file, can either be relative or absolute.</param>
    /// <param name="revertToDefaultOnFail">States if the settings should be reverted to the default if it cannot be parsed.</param>
    /// <param name="deleteFileIfRootMissing">States if the XML file should be automatically deleted (and recreated) if the root is missing.</param>
    /// <param name="verboseMode">States if XMLSettings outputs verbose logging to the log file, rather than just errors.</param>
    /// <param name="logPath">The path for the log file.</param>
    public XmlSettings(string settingsPath, bool revertToDefaultOnFail = true, bool deleteFileIfRootMissing = true, bool verboseMode = false, string logPath = "XmlSettings.log")
    {
        mainSettingsPath = settingsPath;
        revertOnFail = revertToDefaultOnFail;
        purgeOnRootFailure = deleteFileIfRootMissing;
        errorLogPathLoc = logPath;
        verboseLogging = verboseMode;

        writeToLog("New Instance Started with Verbose Logging... Hello World!");

        if (!File.Exists(mainSettingsPath))
            createXMLFile();
        else
            deleteEmptyParents();
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

    /// <summary>
    /// Creates a Boolean variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns></returns>
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
            writeToLog("Added Boolean: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Creates a String variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns></returns>
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
            writeToLog("Added String: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Creates a Long variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns></returns>
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
            writeToLog("Added Long: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Creates a Int variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns></returns>
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
            writeToLog("Added Int: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Creates a Double variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns></returns>
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
            writeToLog("Added Double: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Sets the Boolean variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns></returns>
    public bool setBoolean(string varName, bool state)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            writeToLog("Changed Boolean: '" + varName + "' to a value of: '" + state + "'");
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Sets the String variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns></returns>
    public bool setString(string varName, string state)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state;
            writeToLog("Changed String: '" + varName + "' to a value of: '" + state + "'");
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Sets the Int variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns></returns>
    public bool setInt(string varName, int state)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            writeToLog("Changed Int: '" + varName + "' to a value of: '" + state + "'");
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Sets the Long variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns></returns>
    public bool setLong(string varName, long state)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            writeToLog("Changed Long: '" + varName + "' to a value of: '" + state + "'");
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Sets the Double variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns></returns>
    public bool setDouble(string varName, double state)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = state.ToString();
            writeToLog("Changed Double: '" + varName + "' to a value of: '" + state + "'");
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Reads the Boolean variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If value was read successfully.</returns>
    public bool readBoolean(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = doc.GetElementsByTagName(varName);
            writeToLog("Reading Boolean: '" + varName + "'");
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

    /// <summary>
    /// Reads the String variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If value was read successfully.</returns>
    public string readString(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = doc.GetElementsByTagName(varName);
            writeToLog("Reading String: '" + varName + "'");
            if (elemList.Count > 0)
                return elemList[0].InnerXml.ToString();
            else
                return "";
        }
        catch
        {
            if (revertOnFail) revertToDefault(varName);
            return "";
        }
    }

    /// <summary>
    /// Reads the Int variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If value was read successfully.</returns>
    public int readInt(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = doc.GetElementsByTagName(varName);
            writeToLog("Reading Int: '" + varName + "'");
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

    /// <summary>
    /// Reads the Long variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If value was read successfully.</returns>
    public long readLong(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = doc.GetElementsByTagName(varName);
            writeToLog("Reading Long: '" + varName + "'");
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

    /// <summary>
    /// Reads the Double variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If value was read successfully.</returns>
    public double readDouble(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = doc.GetElementsByTagName(varName);
            writeToLog("Reading Double: '" + varName + "'");
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

    /// <summary>
    /// Removes the specified Boolean variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeBoolean(string varName)
    {
        writeToLog("Attempting to remove Boolean: '" + varName + "'");
        if (getVarType(varName) == "Boolean")
            return removeXmlElement(varName);
        else
            return false;
    }

    /// <summary>
    /// Removes the specified String variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeString(string varName)
    {
        writeToLog("Attempting to remove String: '" + varName + "'");
        if (getVarType(varName) == "String")
            return removeXmlElement(varName);
        else
            return false;
    }

    /// <summary>
    /// Removes the specified Int variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeInt(string varName)
    {
        writeToLog("Attempting to remove Int: '" + varName + "'");
        if (getVarType(varName) == "Int")
            return removeXmlElement(varName);
        else
            return false;
    }

    /// <summary>
    /// Removes the specified Long variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeLong(string varName)
    {
        writeToLog("Attempting to remove Long: '" + varName + "'");
        if (getVarType(varName) == "Long")
            return removeXmlElement(varName);
        else
            return false;
    }

    /// <summary>
    /// Removes the specified Double variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeDouble(string varName)
    {
        writeToLog("Attempting to remove Double: '" + varName + "'");
        if (getVarType(varName) == "Double")
            return removeXmlElement(varName);
        else
            return false;
    }

    //Removes the XML Element of the specified varName, not accessible publicly to avoid removing incorrect variables.
    private bool removeXmlElement(string varName)
    {
        try
        {
            loadXML();
            XmlNodeList elemList = doc.GetElementsByTagName(varName);
            if (elemList.Count > 0)
            {
                elemList[0].ParentNode.RemoveAll();
                saveXML();
                deleteEmptyParents();
                return true;
            }
            else
                return false;
        }
        catch
        {
            if (revertOnFail) revertToDefault(varName);
            return false;
        }
    }

    /// <summary>
    /// Reverts the specified variable to its default setting.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was reverted successfully.</returns>
    public bool revertToDefault(string varName)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        writeToLog("Reverting: '" + varName + "' to its default value");
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = elemList[0].Attributes["default"].Value;
            saveXML();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Returns if a specified variable exists in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable exists.</returns>
    public bool doesVariableExist(string varName)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Returns the Variable type of the specified variable.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>The name of the variable type.</returns>
    public string getVarType(string varName)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return uppercaseFirst(elemList[0].ParentNode.Name);
        else
            return null;
    }

    //Deletes any leftover, empty, parent nodes from removing variables.
    private void deleteEmptyParents()
    {
        loadXML();

        for (int i = 0; i < doc.ChildNodes.Count; i++)
        {
            for (int n = 0; n < doc.ChildNodes[i].ChildNodes.Count; n++)
            {
                XmlNode node = doc.ChildNodes[i].ChildNodes[n];
                if (node.ChildNodes.Count == 0)
                {
                    writeToLog("Purging empty parent node: '" + node.Name + "'");
                    node.ParentNode.RemoveChild(node);
                } 
            }
        }

        saveXML();
    }

    //Saves the XML file loaded in memory to the disk.
    private void saveXML()
    {
        doc.Save(mainSettingsPath);
    }

    //Loads the XML file from disk into memory.
    private void loadXML(bool skipRootCatch = false)
    {
        try
        {
            doc.Load(mainSettingsPath);
            xmlContents = doc.InnerXml;
            xmlBody = doc.DocumentElement;
        }
        catch (Exception e)
        {
            if (purgeOnRootFailure && !skipRootCatch)
            {
                createXMLFile();
                loadXML();
            }
            else writeToLog(e.ToString(), true);
        }
    }

    //Creates a new XML file with the valid Declarations and Elements.
    private void createXMLFile()
    {
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration(xmlFormatVer, "UTF-8", null);
        XmlElement root = doc.DocumentElement;
        doc.InsertBefore(xmlDeclaration, root);

        xmlBody = doc.CreateElement(string.Empty, "body", string.Empty);
        doc.AppendChild(xmlBody);

        writeToLog("New XML file has been created!");
        saveXML();
    }

    //Checks if another variable exists with the same name.
    private bool doesDuplicateExist(string varName)
    {
        loadXML();
        XmlNodeList elemList = doc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return true;
        else
            return false;
    }

    //Capitalises the first letter of a string.
    private string uppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        return char.ToUpper(s[0]) + s.Substring(1);
    }

    //Writes a string to the log file.
    private void writeToLog(string log, bool isError = false)
    {
        if (isError)
            File.AppendAllText(errorLogPathLoc, "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] [ERROR] " + log + Environment.NewLine);
        else if (verboseLogging)
            File.AppendAllText(errorLogPathLoc, "[" + DateTime.Now.ToString("hh:mm:ss tt") + "] [LOG] " + log + Environment.NewLine);
    }
}