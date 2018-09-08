using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

public class XmlSettings
{
    private const string _xmlFormatVer = "1.1";
    private const string _xmlSettingsMinSupported = "1.0";

    private XmlDocument _xmlDoc = new XmlDocument();
    private XmlElement _xmlDocBody;
    private string _xmlDocContent, _xmlFileFormatVer, _xmlFileFormat, _xmlSettingsFileVer;
    private XmlDeclaration _xmlDocDec;

    private string _mainSettingsPath, _errorLogPathLoc, _backupPath, _defaultLoggingName;
    private bool _revertOnFail, _purgeOnRootFailure, _verboseLogging, _verboseLevelMode, _purgeIfXmlDecMissing, _purgeOnUnsupportedVer, _performBackups;

    private bool _xmlCompatible = true;
    private bool _xmlLock = false;

    /// <summary>
    /// Allows the use of the XmlSettings library.
    /// </summary>
    /// <param name="settingsPath">The path for the settings file, can either be relative or absolute.</param>
    /// <param name="revertToDefaultOnFail">States if the settings should be reverted to the default if it cannot be parsed.</param>
    /// <param name="deleteFileIfRootMissing">States if the XML file should be automatically deleted (and recreated) if the root is missing.</param>
    /// <param name="verbose">States if XMLSettings outputs verbose logging to the log file, rather than just errors.</param>
    /// <param name="enhancedVerbose">The level of verbose logging that is performed (if it is enabled).</param>
    /// <param name="deleteIfXmlDeclarationMissing">States if the XML file should be automatically deleted (and recreated) if the XML Declaration is missing.</param>
    /// <param name="deleteOnUnsupportedVersion">States if the XML file should be automatically deleted (and recreated) if the XmlSettings Encoding version is unsupported and unmigratable.</param>
    /// <param name="skipVersionMigration">States if the XML file should be not be automatically updated to support the latest XmlSettings encoding version.</param>
    /// <param name="logPath">The path for the log file.</param>
    /// <param name="overrideBackupFile">Override the default path for file backups (default path is the settingsPath plus '.bkup').</param>
    /// <param name="loggingName">Sets the name used in the log file when the program is referring to itself.</param>
    public XmlSettings(string settingsPath, bool revertToDefaultOnFail = true, bool deleteFileIfRootMissing = true, bool verbose = false, bool enhancedVerbose = false, bool deleteIfXmlDeclarationMissing = true, bool deleteOnUnsupportedVersion = false, bool alwaysBackupBeforeDeletion = true, string logPath = "XmlSettings.log", string overrideBackupFile = null, string loggingName = "XmlSettings")
    {
        _mainSettingsPath = settingsPath;
        _revertOnFail = revertToDefaultOnFail;
        _purgeOnRootFailure = deleteFileIfRootMissing;
        _errorLogPathLoc = logPath;
        _verboseLogging = verbose;
        _verboseLevelMode = enhancedVerbose;
        _purgeIfXmlDecMissing = deleteIfXmlDeclarationMissing;
        _purgeOnUnsupportedVer = deleteOnUnsupportedVersion;
        _performBackups = alwaysBackupBeforeDeletion;
        _defaultLoggingName = loggingName;


        if (String.IsNullOrEmpty(overrideBackupFile))
            _backupPath = settingsPath.TrimEnd(new char[] { '\\', '/' }) + ".bkup";
        else _backupPath = overrideBackupFile;

        if (_verboseLogging && !enhancedVerbose)
        {
            writeToLog("New Instance Started with Verbose Logging... Hello World! (" + DateTime.Now.ToString("yyyy/MM/dd") + ")");
        }
        else if (_verboseLogging && enhancedVerbose)
        {
            writeToLog("New Instance Started with Enhanced Verbose Logging... Hello World! (" + DateTime.Now.ToString("yyyy/MM/dd") + ")");
            writeToLog("settingsPath: " + settingsPath + ", revertOnFail: " + _revertOnFail + ", deleteFileIfRootMissing: " + deleteFileIfRootMissing + ", verbose: " + verbose + ", enhancedVerbose: " + enhancedVerbose + ", deleteIfXmlDeclarationMissing: "
           + deleteIfXmlDeclarationMissing + ", deleteOnUnsupportedVersion: " + deleteOnUnsupportedVersion + ", alwaysBackupBeforeDeletion: " + alwaysBackupBeforeDeletion + ", logPath: " + logPath + ", overrideBackupFile: " + overrideBackupFile);
        }
        else
            writeToLog("New Instance Started... Hello World! (" + DateTime.Now.ToString("yyyy/MM/dd") + ")");

        initXMLSettings();
    }

    /// <summary>
    /// Checks, loads and cleans up the XML file if it already exists. Or creates a new XML file if one doesn't.
    /// </summary>
    private void initXMLSettings()
    {
        if (!File.Exists(_mainSettingsPath))
        {
            _xmlLock = true;
            createXMLFile();
        }
        else cleanupXML();

        checkXML();
    }

    /// <summary>
    /// Resets XMLSettings variables to their original values and reloads the XML from scratch.
    /// </summary>
    public void reloadXMLSettings()
    {
        writeToLog("Reloading Instance... (" + DateTime.Now.ToString("yyyy/MM/dd") + ")");

        _xmlDoc = new XmlDocument();
        _xmlDocBody = null;
        _xmlDocContent = _xmlFileFormatVer = _xmlFileFormat = _xmlSettingsFileVer = null;
        _xmlDocDec = null;
        _xmlCompatible = true;
        _xmlLock = false;

        initXMLSettings();
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
    /// Used to get the current XmlSettings encoding version.
    /// </summary>
    /// <returns>The current XmlSettings encoding version.</returns>
    public string getEncodingVersion()
    {
        return _xmlFormatVer;
    }

    /// <summary>
    /// Used to get the XmlSettings encoding version of XML file.
    /// </summary>
    /// <returns>The XmlSettings encoding version of XML file.</returns>
    public string getFileXmlSettingsVersion()
    {
        return _xmlSettingsFileVer;
    }

    /// <summary>
    /// Used to get the minimum supported XmlSettings encoding version.
    /// </summary>
    /// <returns>The minimum supported XmlSettings encoding version.</returns>
    public string getMinSupportedEncodingVersion()
    {
        return _xmlSettingsMinSupported;
    }

    /// <summary>
    /// Used to get the XML version of the currently loaded file.
    /// </summary>
    /// <returns>The XML version number of the loaded file.</returns>
    public string getXmlVersion()
    {
        return _xmlFileFormatVer;
    }

    /// <summary>
    /// Used to get the encoding of the currently loaded file.
    /// </summary>
    /// <returns>The encoding type of the loaded file.</returns>
    public string getFileEncoding()
    {
        return _xmlFileFormat;
    }

    /// <summary>
    /// Creates a Boolean variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addBoolean(string varName, bool defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode booleanNode = addToHeadingNode("boolean");
            _xmlDocBody.AppendChild(booleanNode);

            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            booleanNode.AppendChild(varNode);
            writeToLog("Added Boolean: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a String variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addString(string varName, string defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode stringNode = addToHeadingNode("string");
            _xmlDocBody.AppendChild(stringNode);

            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue;
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue;
            stringNode.AppendChild(varNode);
            writeToLog("Added String: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a Long variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addLong(string varName, long defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode longNode = addToHeadingNode("long");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            longNode.AppendChild(varNode);
            writeToLog("Added Long: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a Int variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addInt(string varName, int defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode intNode = addToHeadingNode("int");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            intNode.AppendChild(varNode);
            writeToLog("Added Int: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a Int64 variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addInt64(string varName, Int64 defaultValue)
    {
        return addLong(varName, defaultValue);
    }

    /// <summary>
    /// Creates a Int32 variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addInt32(string varName, Int32 defaultValue)
    {
        return addInt(varName, defaultValue);
    }

    /// <summary>
    /// Creates a Int16 variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addInt16(string varName, Int16 defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode intNode = addToHeadingNode("Int16");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            intNode.AppendChild(varNode);
            writeToLog("Added Int16: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a UInt64 variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addUInt64(string varName, UInt64 defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode intNode = addToHeadingNode("UInt64");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            intNode.AppendChild(varNode);
            writeToLog("Added UInt64: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a UInt32 variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addUInt32(string varName, UInt32 defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode intNode = addToHeadingNode("UInt32");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            intNode.AppendChild(varNode);
            writeToLog("Added UInt32: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a UInt16 variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addUInt16(string varName, UInt16 defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode intNode = addToHeadingNode("Int16");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            intNode.AppendChild(varNode);
            writeToLog("Added UInt16: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a Double variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addDouble(string varName, double defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode doubleNode = addToHeadingNode("double");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            doubleNode.AppendChild(varNode);
            writeToLog("Added Double: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates a Byte variable in the XML file with the specified value.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="defaultValue">The value that will be assigned to the variable. Also used as a 'default' and can be reverted to at a later date.</param>
    /// <returns>If the variables was added successfully.</returns>
    public bool addByte(string varName, byte defaultValue)
    {
        if (!doesDuplicateExist(varName) && checkIfXmlCompatible())
        {
            XmlNode doubleNode = addToHeadingNode("byte");
            XmlNode varNode = _xmlDoc.CreateElement(varName);
            XmlAttribute defaultVal = _xmlDoc.CreateAttribute("default");
            defaultVal.Value = defaultValue.ToString();
            varNode.Attributes.Append(defaultVal);
            varNode.InnerText = defaultValue.ToString();
            doubleNode.AppendChild(varNode);
            writeToLog("Added Byte: '" + varName + "' with a value of: '" + defaultValue + "'");
            saveXML();

            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the Boolean variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setBoolean(string varName, bool state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed Boolean: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the String variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setString(string varName, string state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state;
                writeToLog("Changed String: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the Int variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setInt(string varName, int state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed Int: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the Int64 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setInt64(string varName, Int64 state)
    {
        return setLong(varName, state);
    }

    /// <summary>
    /// Sets the Int32 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setInt32(string varName, Int32 state)
    {
        return setInt(varName, state);
    }

    /// <summary>
    /// Sets the Int16 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setInt16(string varName, Int16 state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed Int16: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the UInt64 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setUInt64(string varName, UInt64 state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed UInt64: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the UInt32 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setUInt32(string varName, UInt32 state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed UInt32: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the UInt16 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setUInt16(string varName, UInt16 state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed UInt16: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the Long variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setLong(string varName, long state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed Long: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the Double variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setDouble(string varName, double state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed Double: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets the Byte variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <param name="state">The value that will be assigned to the variable.</param>
    /// <returns>If the variables was set successfully.</returns>
    public bool setByte(string varName, byte state)
    {
        if (checkIfXmlCompatible())
        {
            loadXML();
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
            if (elemList.Count == 1)
            {
                elemList[0].InnerText = state.ToString();
                writeToLog("Changed Byte: '" + varName + "' to a value of: '" + state + "'");
                saveXML();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Reads the Boolean variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public bool readBoolean(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading Boolean: '" + varName + "'");
                if (elemList.Count > 0)
                    return bool.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return false;
    }

    /// <summary>
    /// Reads the String variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public string readString(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading String: '" + varName + "'");
                if (elemList.Count > 0)
                    return elemList[0].InnerXml.ToString();
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return "";
    }

    /// <summary>
    /// Reads the Int variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public int readInt(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading Int: '" + varName + "'");
                if (elemList.Count > 0)
                    return int.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the Int64 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public Int64 readInt64(string varName)
    {
        return readLong(varName);
    }

    /// <summary>
    /// Reads the Int32 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public Int32 readInt32(string varName)
    {
        return readInt(varName);
    }

    /// <summary>
    /// Reads the Int16 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public int readInt16(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading Int16: '" + varName + "'");
                if (elemList.Count > 0)
                    return Int16.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the UInt64 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public UInt64 readUInt64(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading UInt64: '" + varName + "'");
                if (elemList.Count > 0)
                    return UInt64.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the UInt32 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public UInt32 readUInt32(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading UInt32: '" + varName + "'");
                if (elemList.Count > 0)
                    return UInt32.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the UInt16 variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public UInt64 readUInt16(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading UInt16: '" + varName + "'");
                if (elemList.Count > 0)
                    return UInt16.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the Long variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public long readLong(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading Long: '" + varName + "'");
                if (elemList.Count > 0)
                    return long.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the Double variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public double readDouble(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading Double: '" + varName + "'");
                if (elemList.Count > 0)
                    return double.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
    }

    /// <summary>
    /// Reads the Byte variable of an already existing variable in the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the value was read successfully.</returns>
    public double readByte(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                writeToLog("Reading Byte: '" + varName + "'");
                if (elemList.Count > 0)
                    return byte.Parse(elemList[0].InnerXml);
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return 0;
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

        return false;
    }

    /// <summary>
    /// Removes the specified Int64 variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeInt64(string varName)
    {
        return removeLong(varName);
    }

    /// <summary>
    /// Removes the specified Int32 variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeInt32(string varName)
    {
        return removeInt(varName);
    }

    /// <summary>
    /// Removes the specified Int16 variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeInt16(string varName)
    {
        writeToLog("Attempting to remove Int16: '" + varName + "'");
        if (getVarType(varName) == "Int16")
            return removeXmlElement(varName);

        return false;
    }

    /// <summary>
    /// Removes the specified UInt64 variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeUInt64(string varName)
    {
        writeToLog("Attempting to remove UInt64: '" + varName + "'");
        if (getVarType(varName) == "UInt64")
            return removeXmlElement(varName);

        return false;
    }

    /// <summary>
    /// Removes the specified UInt32 variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeUInt32(string varName)
    {
        writeToLog("Attempting to remove UInt32: '" + varName + "'");
        if (getVarType(varName) == "UInt32")
            return removeXmlElement(varName);

        return false;
    }

    /// <summary>
    /// Removes the specified UInt16 variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeUInt16(string varName)
    {
        writeToLog("Attempting to remove UInt16: '" + varName + "'");
        if (getVarType(varName) == "UInt16")
            return removeXmlElement(varName);

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

        return false;
    }

    /// <summary>
    /// Removes the specified Byte variable from the XML File.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was removed sucessfully.</returns>
    public bool removeByte(string varName)
    {
        writeToLog("Attempting to remove Byte: '" + varName + "'");
        if (getVarType(varName) == "Byte")
            return removeXmlElement(varName);

        return false;
    }

    /// <summary>
    /// Removes the XML Element of the specified varName, not accessible publicly to avoid removing incorrect variables.
    /// </summary>
    /// <param name="varName">The name of the variable to remove.</param>
    /// <returns>If the XML Element was removed sucessfully.</returns>
    private bool removeXmlElement(string varName)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                loadXML();
                XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
                if (elemList.Count > 0)
                {
                    elemList[0].ParentNode.RemoveAll();
                    saveXML();
                    deleteEmptyParents();
                    return true;
                }
            }
            catch
            {
                if (_revertOnFail) revertToDefault(varName);
            }
        }
        return false;
    }

    /// <summary>
    /// Reverts the specified variable to its default setting.
    /// </summary>
    /// <param name="varName">The name of the variable in the XML File.</param>
    /// <returns>If the variable was reverted successfully.</returns>
    public bool revertToDefault(string varName)
    {
        loadXML();
        XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
        writeToLog("Reverting: '" + varName + "' to its default value");
        if (elemList.Count == 1)
        {
            elemList[0].InnerText = elemList[0].Attributes["default"].Value;
            saveXML();
            return true;
        }
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
        XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return true;

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
        XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return uppercaseFirst(elemList[0].ParentNode.Name);

        return null;
    }

    /// <summary>
    /// Cleans up the XML file. Deleting any unneeded information or moving nodes around.
    /// </summary>
    public void cleanupXML()
    {
        condenseXMLNodes();
        setXmlSettingsEncodingVersion(getEncodingVersion());
        deleteEmptyParents();
        loadXML();
    }

    /// <summary>
    /// Deletes any leftover, empty, parent nodes from removing variables.
    /// </summary>
    private void deleteEmptyParents()
    {
        loadXML();

        for (int i = 0; i < _xmlDoc.ChildNodes.Count; i++)
        {
            for (int n = 0; n < _xmlDoc.ChildNodes[i].ChildNodes.Count; n++)
            {
                XmlNode node = _xmlDoc.ChildNodes[i].ChildNodes[n];
                if (node.ChildNodes.Count == 0)
                {
                    writeToLog("Purging empty parent node: '" + node.Name + "'");
                    node.ParentNode.RemoveChild(node);
                }
            }
        }
        saveXML();
    }

    /// <summary>
    /// Condenses XML Nodes of the same type into one parent (Updates 1.0 formatting to 1.1).
    /// </summary>
    private void condenseXMLNodes()
    {
        deleteEmptyParents();

        XmlNodeList intNodes = _xmlDoc.SelectNodes("//int");
        XmlNodeList longNodes = _xmlDoc.SelectNodes("//long");
        XmlNodeList boolNodes = _xmlDoc.SelectNodes("//boolean");
        XmlNodeList stringNodes = _xmlDoc.SelectNodes("//string");
        XmlNodeList doubleNodes = _xmlDoc.SelectNodes("//double");

        if (intNodes.Count > 1 || longNodes.Count > 1 || boolNodes.Count > 1 || stringNodes.Count > 1 || doubleNodes.Count > 1)
        {
            writeToLog("Uncondensed XML nodes detected: Condensing XML nodes", false, true);

            performXMLBackup();

            List<XmlNode> childIntNodes = new List<XmlNode>();
            List<XmlNode> childLongNodes = new List<XmlNode>();
            List<XmlNode> childBoolNodes = new List<XmlNode>();
            List<XmlNode> childStringNodes = new List<XmlNode>();
            List<XmlNode> childDoubleNodes = new List<XmlNode>();

            foreach (XmlNode node in intNodes)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    childIntNodes.Add(childNode);
                }
            }

            foreach (XmlNode node in longNodes)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    childLongNodes.Add(childNode);
                }
            }

            foreach (XmlNode node in boolNodes)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    childBoolNodes.Add(childNode);
                }
            }

            foreach (XmlNode node in stringNodes)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    childStringNodes.Add(childNode);
                }
            }

            foreach (XmlNode node in doubleNodes)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    childDoubleNodes.Add(childNode);
                }
            }

            deleteXMLFile(false);
            createXMLFile();

            XmlNode intNode = addToHeadingNode("int");
            XmlNode longNode = addToHeadingNode("long");
            XmlNode boolNode = addToHeadingNode("boolean");
            XmlNode stringNode = addToHeadingNode("string");
            XmlNode doubleNode = addToHeadingNode("double");

            _xmlDocBody.AppendChild(intNode);
            _xmlDocBody.AppendChild(longNode);
            _xmlDocBody.AppendChild(boolNode);
            _xmlDocBody.AppendChild(stringNode);
            _xmlDocBody.AppendChild(doubleNode);

            try
            {
                foreach (XmlNode node in childIntNodes)
                {
                    intNode.AppendChild(_xmlDoc.ImportNode(node, true));
                }

                foreach (XmlNode node in childLongNodes)
                {
                    longNode.AppendChild(_xmlDoc.ImportNode(node, true));
                }

                foreach (XmlNode node in childBoolNodes)
                {
                    boolNode.AppendChild(_xmlDoc.ImportNode(node, true));
                }

                foreach (XmlNode node in childStringNodes)
                {
                    stringNode.AppendChild(_xmlDoc.ImportNode(node, true));
                }

                foreach (XmlNode node in childDoubleNodes)
                {
                    doubleNode.AppendChild(_xmlDoc.ImportNode(node, true));
                }

                saveXML();
                reloadXMLSettings();
            }
            catch (Exception e)
            {
                writeToLog("An error occurred while trying to condense the XML nodes! Error: " + e.ToString(), true);
            }
        }
    }

    /// <summary>
    /// Saves the XML file loaded in memory to the disk.
    /// </summary>
    private void saveXML()
    {
        _xmlDoc.Save(_mainSettingsPath);
    }

    /// <summary>
    /// Loads the XML file from disk into memory.
    /// </summary>
    /// <param name="skipRootCatch">Toggles whether the file should be recreated if the root node is missing.</param>
    private void loadXML(bool skipRootCatch = false)
    {
        if (checkIfXmlCompatible())
        {
            try
            {
                _xmlDoc.Load(_mainSettingsPath);
                _xmlDocContent = _xmlDoc.InnerXml;
                _xmlDocBody = _xmlDoc.DocumentElement;
            }
            catch (Exception e)
            {
                if (_purgeOnRootFailure && !skipRootCatch)
                {
                    writeToLog("The XML root node is missing. Deleting and recreating the file...", false, true);
                    recreateXMLFile(false);
                }
                else writeToLog("The XML root node is missing. Not deleting the file on request. Error: " + e.ToString(), true);
            }
        }
    }

    /// <summary>
    /// Creates a new XML file with the valid Declarations and Elements.
    /// </summary>
    /// 
    public void createXMLFile()
    {
        if (_xmlLock)
        {
            XmlDeclaration xmlDeclaration = _xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = _xmlDoc.DocumentElement;

            XmlElement pi = _xmlDoc.CreateElement("XmlSettingsEncodingVersion");
            pi.InnerText = getEncodingVersion();

            _xmlDoc.InsertBefore(xmlDeclaration, root);

            _xmlDocBody = _xmlDoc.CreateElement(string.Empty, "body", string.Empty);
            _xmlDoc.AppendChild(_xmlDocBody);
            _xmlDocBody.AppendChild(pi);

            _xmlLock = false;

            writeToLog("A new " + _defaultLoggingName + " file has been created!");
            saveXML();
        }
        else writeToLog("Cannot create a new " + _defaultLoggingName + " file since another is already loaded and in use.", false, true);
    }

    /// <summary>
    /// Recreates an empty XML file with the valid Declarations and Elements.
    /// </summary>
    private void recreateXMLFile(bool deleteBackups = true, bool skipBackup = false)
    {
        if (_performBackups && !deleteBackups && !skipBackup) performXMLBackup();
        deleteXMLFile(deleteBackups);
        createXMLFile();
        loadXML();
    }

    /// <summary>
    /// Creates a backup of the XML file.
    /// </summary>
    public void performXMLBackup()
    {
        if (File.Exists(_mainSettingsPath))
        {
            if (File.Exists(_backupPath)) File.Delete(_backupPath);
            File.Copy(_mainSettingsPath, _backupPath);
        }
    }

    /// <summary>
    /// Deletes the XML file backups.
    /// </summary>
    public void deleteXMLBackups()
    {
        writeToLog(_defaultLoggingName + " backup has been deleted.", false, true);
        File.Delete(_backupPath);
    }

    /// <summary>
    /// Deletes the entire XML File and locks out any modification or read attempts.
    /// </summary>
    /// <param name="deleteBackups">Also deletes the XML file backups.</param>
    public void deleteXMLFile(bool deleteBackups = true)
    {
        lockXMLSettings();

        if (deleteBackups) deleteXMLBackups();
        File.Delete(_mainSettingsPath);
    }

    /// <summary>
    /// Checks if another variable exists with the same name.
    /// </summary>
    /// <param name="varName">Variable name to check.</param>
    /// <returns>A boolean depending on if the variable already exists.</returns>
    private bool doesDuplicateExist(string varName)
    {
        loadXML();
        XmlNodeList elemList = _xmlDoc.GetElementsByTagName(varName);
        if (elemList.Count > 0)
            return true;

        return false;
    }

    /// <summary>
    /// Capitalises the first letter of a string.
    /// </summary>
    /// <param name="s">String use as source.</param>
    /// <returns>The original string with the first letter capitalised.</returns>
    private string uppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        return char.ToUpper(s[0]) + s.Substring(1);
    }

    /// <summary>
    /// Writes a string to the log file.
    /// </summary>
    /// <param name="log">Message to add to log file.</param>
    /// <param name="isError">Whether the log message is an error.</param>
    private void writeToLog(string log, bool isError = false, bool isWarn = false)
    {
        if (!String.IsNullOrEmpty(log))
        {
            if (isError)
                File.AppendAllText(_errorLogPathLoc, "[" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] [ERROR] " + log + Environment.NewLine);
            else if (_verboseLogging && isWarn)
                File.AppendAllText(_errorLogPathLoc, "[" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] [WARN] " + log + Environment.NewLine);
            else if (_verboseLogging)
                File.AppendAllText(_errorLogPathLoc, "[" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] [LOG] " + log + Environment.NewLine);
        }
    }

    /// <summary>
    /// Checks to see if the specified node exists and selects it, if not it will create a new one (and selects that).
    /// </summary>
    /// <param name="headingName">Node to check for.</param>
    /// <returns>An XMLNode of the specified node.</returns>
    private XmlNode addToHeadingNode(string headingName)
    {
        XmlNode node;

        if (_xmlDoc.DocumentElement[headingName] != null)
            node = _xmlDoc.DocumentElement[headingName];
        else
        {
            node = _xmlDoc.CreateElement(headingName);
            _xmlDocBody.AppendChild(node);
        }
        return node;
    }

    /// <summary>
    /// Checks to ensure the specified XML file is compatible with the current version of XMLSettings.
    /// </summary>
    /// <returns>A boolean of if the XML file is supported.</returns>
    public bool checkIfXmlCompatible()
    {
        if (_xmlLock) writeToLog("This XMLSettings file has been locked and cannot be read from or written to.", true);
        return _xmlCompatible;
    }

    /// <summary>
    /// Does various checks to the XML file on initialization.
    /// </summary>
    private void checkXML()
    {
        if (!isXmlFileVerSupported())
        {
            _xmlDoc = null;
            writeToLog("The specified " + _defaultLoggingName + " file has not been encoded in a supported " + _defaultLoggingName + " version!", true);

            if (_purgeOnUnsupportedVer)
            {
                writeToLog("Deleting unsupported " + _defaultLoggingName + " File.", false, true);
                recreateXMLFile(false);
            }
            else _xmlCompatible = false;
        }
    }

    /// <summary>
    /// Parses the version into an int array.
    /// </summary>
    /// <param name="version">Version as a string.</param>
    /// <returns>Version as an int array.</returns>
    private int[] parseVersion(string version)
    {
        if (!String.IsNullOrEmpty(version))
        {
            string[] sVersionAsArray = version.Split('.');
            List<int> iVersionAsList = new List<int>();

            foreach (String s in sVersionAsArray)
            {
                iVersionAsList.Add(Convert.ToInt32(s));
            }

            return iVersionAsList.ToArray();
        }
        return null;
    }

    /// <summary>
    /// Gets if the XML File Version is supported.
    /// </summary>
    /// <returns>If the XML file is supported.</returns>
    private bool isXmlFileVerSupported()
    {
        loadXmlDeclaration();
        loadXMLInformation();

        int[] fileVersion = parseVersion(getFileXmlSettingsVersion());
        int[] libVersion = parseVersion(getEncodingVersion());
        int[] supVersion = parseVersion(getMinSupportedEncodingVersion());

        int longestVersionArray = fileVersion.Length;
        if (libVersion.Length > longestVersionArray) longestVersionArray = libVersion.Length;

        if (fileVersion[0] > supVersion[0])
        {
            if (fileVersion[0] > libVersion[0])
            {
                return false;
            }
        }

        if (fileVersion[1] > supVersion[1])
        {
            if (fileVersion[1] > libVersion[1])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Loads the XML Declatation.
    /// </summary>
    private void loadXmlDeclaration()
    {
        loadXML();
        try
        {
            if (_xmlDoc.ChildNodes[0].NodeType == XmlNodeType.XmlDeclaration)
            {
                _xmlDocDec = _xmlDoc.ChildNodes[0] as XmlDeclaration;
                _xmlFileFormatVer = _xmlDocDec.Version;
                _xmlFileFormat = _xmlDocDec.Encoding;
            }
            else if (!_purgeIfXmlDecMissing)
            {
                writeToLog("The XML Declaration is missing. Assuming the version as '1.0' and encoding as 'UTF-8'.", false, true);
                _xmlFileFormatVer = "1.0";
                _xmlFileFormat = "UTF-8";
            }
            else
            {
                writeToLog("The XML Declaration is missing. Deleting and recreating the file...", false, true);
                recreateXMLFile(false);
            }
        }
        catch (Exception e)
        {
            writeToLog("An error occurred while trying to load the XML declaration! Error: " + e.ToString(), true);
        }
    }

    /// <summary>
    /// Sets the XML Encoding Version variable in the XML file.
    /// </summary>
    /// <param name="version">The new version.</param>
    private void setXmlSettingsEncodingVersion(string version)
    {
        loadXML();
        XmlNodeList elemList = _xmlDoc.GetElementsByTagName("XmlSettingsEncodingVersion");
        if (elemList.Count > 0)
        {
            elemList[0].InnerText = version;
        }
        else
        {
            XmlElement pi = _xmlDoc.CreateElement("XmlSettingsEncodingVersion");
            pi.InnerText = version;

            _xmlDoc.DocumentElement.InsertBefore(pi, _xmlDoc.DocumentElement.FirstChild);
        }
        saveXML();
    }

    /// <summary>
    /// Loads the XmlSettings Metadata from the XML file.
    /// </summary>
    private void loadXMLInformation()
    {
        loadXML();
        try
        {
            XmlNodeList elemList = _xmlDoc.GetElementsByTagName("XmlSettingsEncodingVersion");
            if (elemList.Count > 0)
            {
                _xmlSettingsFileVer = elemList[0].InnerXml;
            }
            else
            {
                writeToLog("The " + _defaultLoggingName + " Metadata information is missing. Assuming the encoded " + _defaultLoggingName + " version was '1.0'.", false, true);
                _xmlSettingsFileVer = "1.0";
            }
        }
        catch (Exception e)
        {
            writeToLog(e.ToString(), true);
        }
    }

    /// <summary>
    /// Locks the XML file to disable read and writes.
    /// </summary>
    private void lockXMLSettings()
    {
        _xmlDoc = new XmlDocument();
        _xmlDocBody = null;
        _xmlDocContent = _xmlFileFormatVer = _xmlFileFormat = _xmlSettingsFileVer = null;
        _xmlDocDec = null;
        _xmlCompatible = false;
        _xmlLock = true;
    }
}