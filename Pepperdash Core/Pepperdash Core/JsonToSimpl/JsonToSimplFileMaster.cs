﻿using System;
//using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PepperDash.Core.JsonToSimpl
{
    /// <summary>
    /// Represents a JSON file that can be read and written to
    /// </summary>
    public class JsonToSimplFileMaster : JsonToSimplMaster
    {
        /// <summary>
        ///  Sets the filepath as well as registers this with the Global.Masters list
        /// </summary>
        public string Filepath { get; private set; }

        /// <summary>
        /// Filepath to the actual file that will be read (Portal or local)
        /// </summary>
        public string ActualFilePath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string FilePathName { get; private set; }

        /*****************************************************************************************/
        /** Privates **/


        // The JSON file in JObject form
        // For gathering the incoming data
        object StringBuilderLock = new object();
        // To prevent multiple same-file access
        static object FileLock = new object();

        /*****************************************************************************************/

        /// <summary>
        /// SIMPL+ default constructor.
        /// </summary>
        public JsonToSimplFileMaster()
        {
        }

        /// <summary>
        /// Read, evaluate and udpate status
        /// </summary>
        public void EvaluateFile(string filepath)
        {
            try
            {
                OnBoolChange(false, 0, JsonToSimplConstants.JsonIsValidBoolChange);


                var dirSeparator = Path.DirectorySeparatorChar;         // win-'\', linux-'/'
                var dirSeparatorAlt = Path.AltDirectorySeparatorChar;   // win-'\', linux-'/'

                var is4Series = CrestronEnvironment.ProgramCompatibility == eCrestronSeries.Series4;
                var isServer = CrestronEnvironment.DevicePlatform == eDevicePlatform.Server;

                OnUshrtChange((ushort)CrestronEnvironment.ProgramCompatibility, 0, 
                    JsonToSimplConstants.CrestronSeriesValueChange);
                
                OnUshrtChange((ushort) CrestronEnvironment.DevicePlatform, 0,
                    JsonToSimplConstants.DevicePlatformValueChange);
                                

                var rootDirectory = Directory.GetApplicationRootDirectory();
                OnStringChange(rootDirectory, 0, JsonToSimplConstants.RootDirectory);


                var splusPath = string.Empty;
                if (Regex.IsMatch(filepath, @"user", RegexOptions.IgnoreCase))
                {
                    if (is4Series) 
                        splusPath = Regex.Replace(filepath, "user", "user", RegexOptions.IgnoreCase);
                    else if (isServer) 
                        splusPath = Regex.Replace(filepath, "user", "User", RegexOptions.IgnoreCase);
                }

                filepath = splusPath.Replace(dirSeparatorAlt, dirSeparator);
                
                Filepath = string.Format("{1}{0}{2}", dirSeparator, rootDirectory,
                    filepath.TrimStart(dirSeparator, dirSeparatorAlt));
                
                OnStringChange(string.Format("Attempting to evaluate {0}", Filepath), 0, JsonToSimplConstants.MessageToSimpl);

                if (string.IsNullOrEmpty(Filepath))
                {
                    OnStringChange(string.Format("Cannot evaluate file. JSON file path not set"), 0, JsonToSimplConstants.MessageToSimpl);
                    CrestronConsole.PrintLine("Cannot evaluate file. JSON file path not set");
                    return;
                }

                // get file directory and name to search
                var fileDirectory = Path.GetDirectoryName(Filepath);
                var fileName = Path.GetFileName(Filepath);

                OnStringChange(string.Format("Checking '{0}' for '{1}'", fileDirectory, fileName), 0, JsonToSimplConstants.MessageToSimpl);
                Debug.Console(1, "Checking '{0}' for '{1}'", fileDirectory, fileName);

                if (Directory.Exists(fileDirectory))
                {
                    // get the directory info
                    var directoryInfo = new DirectoryInfo(fileDirectory);
                    var actualFile = directoryInfo.GetFiles(fileName).FirstOrDefault();
                    //var actualFile = new DirectoryInfo(Filepath).GetFiles(fileName).FirstOrDefault();
                    if (actualFile == null)
                    {
                        var msg = string.Format("JSON file not found: {0}", Filepath);
                        OnStringChange(msg, 0, JsonToSimplConstants.MessageToSimpl);
                        CrestronConsole.PrintLine(msg);
                        ErrorLog.Error(msg);
                        return;
                    }

                    // \xSE\xR\PDT000-Template_Main_Config-Combined_DSP_v00.02.json
                    // \USER\PDT000-Template_Main_Config-Combined_DSP_v00.02.json
                    ActualFilePath = actualFile.FullName;                    
                    OnStringChange(ActualFilePath, 0, JsonToSimplConstants.ActualFilePathChange);
                    OnStringChange(string.Format("Actual JSON file is {0}", ActualFilePath), 0, JsonToSimplConstants.MessageToSimpl);
                    Debug.Console(1, "Actual JSON file is {0}", ActualFilePath);

                    Filename = actualFile.Name;
                    OnStringChange(Filename, 0, JsonToSimplConstants.FilenameResolvedChange);
                    OnStringChange(string.Format("JSON Filename is {0}", Filename), 0, JsonToSimplConstants.MessageToSimpl);
                    Debug.Console(1, "JSON Filename is {0}", Filename);


                    FilePathName = string.Format(@"{0}{1}", actualFile.DirectoryName, dirSeparator);
                    OnStringChange(FilePathName, 0, JsonToSimplConstants.FilePathResolvedChange);
                    OnStringChange(string.Format("JSON File Path is {0}", FilePathName), 0, JsonToSimplConstants.MessageToSimpl);
                    Debug.Console(1, "JSON File Path is {0}", FilePathName);

                    var json = File.ReadToEnd(ActualFilePath, System.Text.Encoding.ASCII);

                    JsonObject = JObject.Parse(json);
                    foreach (var child in Children)
                        child.ProcessAll();
                    OnBoolChange(true, 0, JsonToSimplConstants.JsonIsValidBoolChange);
                }
                else
                {
                    OnStringChange(string.Format("'{0}' not found", fileDirectory), 0, JsonToSimplConstants.MessageToSimpl);
                    Debug.Console(1, "'{0}' not found", fileDirectory);
                }
            }
            catch (Exception e)
            {
                var msg = string.Format("EvaluateFile Exception: Message\r{0}", e.Message);
                OnStringChange(msg, 0, JsonToSimplConstants.MessageToSimpl);
                CrestronConsole.PrintLine(msg);
                ErrorLog.Error(msg);

                var stackTrace = string.Format("EvaluateFile: Stack Trace\r{0}", e.StackTrace);
                OnStringChange(stackTrace, 0, JsonToSimplConstants.MessageToSimpl);
                CrestronConsole.PrintLine(stackTrace);
                ErrorLog.Error(stackTrace);
                return;
            }
        }


        /// <summary>
        /// Sets the debug level
        /// </summary>
        /// <param name="level"></param>
        public void setDebugLevel(int level)
        {
            Debug.SetDebugLevel(level);
        }

        /// <summary>
        /// Saves the values to the file
        /// </summary>
        public override void Save()
        {
            // this code is duplicated in the other masters!!!!!!!!!!!!!
            UnsavedValues = new Dictionary<string, JValue>();
            // Make each child update their values into master object
            foreach (var child in Children)
            {
                Debug.Console(1, "Master [{0}] checking child [{1}] for updates to save", UniqueID, child.Key);
                child.UpdateInputsForMaster();
            }

            if (UnsavedValues == null || UnsavedValues.Count == 0)
            {
                Debug.Console(1, "Master [{0}] No updated values to save. Skipping", UniqueID);
                return;
            }
            lock (FileLock)
            {
                Debug.Console(1, "Saving");
                foreach (var path in UnsavedValues.Keys)
                {
                    var tokenToReplace = JsonObject.SelectToken(path);
                    if (tokenToReplace != null)
                    {// It's found
                        tokenToReplace.Replace(UnsavedValues[path]);
                        Debug.Console(1, "JSON Master[{0}] Updating '{1}'", UniqueID, path);
                    }
                    else // No token.  Let's make one 
                    {
                        //http://stackoverflow.com/questions/17455052/how-to-set-the-value-of-a-json-path-using-json-net
                        Debug.Console(1, "JSON Master[{0}] Cannot write value onto missing property: '{1}'", UniqueID, path);

                        //                        JContainer jpart = JsonObject;
                        //                        // walk down the path and find where it goes
                        //#warning Does not handle arrays.
                        //                        foreach (var part in path.Split('.'))
                        //                        {

                        //                            var openPos = part.IndexOf('[');
                        //                            if (openPos > -1)
                        //                            {
                        //                                openPos++; // move to number
                        //                                var closePos = part.IndexOf(']');
                        //                                var arrayName = part.Substring(0, openPos - 1); // get the name
                        //                                var index = Convert.ToInt32(part.Substring(openPos, closePos - openPos));

                        //                                // Check if the array itself exists and add the item if so
                        //                                if (jpart[arrayName] != null)
                        //                                {
                        //                                    var arrayObj = jpart[arrayName] as JArray;
                        //                                    var item = arrayObj[index];
                        //                                    if (item == null)
                        //                                        arrayObj.Add(new JObject());
                        //                                }

                        //                                Debug.Console(0, "IGNORING MISSING ARRAY VALUE FOR NOW");
                        //                                continue;
                        //                            }
                        //                            // Build the 
                        //                            if (jpart[part] == null)
                        //                                jpart.Add(new JProperty(part, new JObject()));
                        //                            jpart = jpart[part] as JContainer;
                        //                        }
                        //                        jpart.Replace(UnsavedValues[path]);
                    }
                }
                using (StreamWriter sw = new StreamWriter(ActualFilePath))
                {
                    try
                    {
                        sw.Write(JsonObject.ToString());
                        sw.Flush();
                    }
                    catch (Exception e)
                    {
                        string err = string.Format("Error writing JSON file:\r{0}", e);
                        Debug.Console(0, err);
                        ErrorLog.Warn(err);
                        return;
                    }
                }
            }
        }
    }
}
