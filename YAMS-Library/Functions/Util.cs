using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace YAMS.Functions
{
    public static class Util
    {

        private static string strJRERegKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
        private static string strJDKRegKey = "SOFTWARE\\JavaSoft\\Java Development Kit";
        private static string strJRERegKey32on64 = "SOFTWARE\\Wow6432Node\\JavaSoft\\Java Runtime Environment";
        private static string strJDKRegKey32on64 = "SOFTWARE\\Wow6432Node\\JavaSoft\\Java Development Kit";

        /// <summary>
        /// Detects if the JRE is installed using the regkey
        /// </summary>
        /// <returns>boolean indicating if the JRE is installed</returns>
        public static bool HasJRE()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey subKey = rk.OpenSubKey(strJRERegKey);
                if (subKey != null) return true;
                else
                {
                    //We need to check if they're running 32-bit Java on a 64-bit OS
                    subKey = rk.OpenSubKey(strJRERegKey32on64);
                    if (subKey != null) return true;
                    else return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detects if the JDK is installed using the regkey
        /// </summary>
        /// <returns>boolean indicating if the JDK is installed</returns>
        public static bool HasJDK()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey subKey = rk.OpenSubKey(strJDKRegKey);
                if (subKey != null) return true;
                else
                {
                    //We need to check if they're running 32-bit Java on a 64-bit OS
                    subKey = rk.OpenSubKey(strJDKRegKey32on64);
                    if (subKey != null) return true;
                    else return false;
                }
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Looks for the Minecraft client in the system user's profile, the service runs as LOCAL SYSTEM, so for
        /// some of the third party apps we need to see if it is in this profile too
        /// </summary>
        /// <returns>boolean indicating if the minecraft client is in the SYSTEM account's AppData</returns>
        public static bool HasMCClientSystem()
        {
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"config\systemprofile\AppData\Roaming\.minecraft\bin\minecraft.jar"))) return true;
            else return false;
        }
        /// <summary>
        /// Checks if the Minecraft client is installed locally
        /// </summary>
        /// <returns>boolean indicating if the Minecraft jar is in the local user's AppData</returns>
        public static bool HasMCClientLocal()
        {
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\bin\minecraft.jar"))) return true;
            else return false;
        }
        public static void CopyMCClient()
        {
            Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @".minecraft\"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"config\systemprofile\AppData\Roaming\.minecraft\"));
        }

        //Get the Java version from the registry
        public static string JavaVersion(string strType = "jre")
        {
            string strKey = "";
            string strKey32on64 = "";
            switch (strType)
            {
                case "jre":
                    strKey = strJRERegKey;
                    strKey32on64 = strJRERegKey32on64;
                    break;
                case "jdk":
                    strKey = strJDKRegKey;
                    strKey32on64 = strJDKRegKey32on64;
                    break;
            }
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey subKey = rk.OpenSubKey(strKey);
            if (subKey != null) return subKey.GetValue("CurrentVersion").ToString();
            else
            {
                subKey = rk.OpenSubKey(strKey32on64);
                if (subKey != null) return subKey.GetValue("CurrentVersion").ToString();
                else return "";
            };
        }

        //Calculate the path to the Java executable
        public static string JavaPath(string strType = "jre")
        {
            string strKey = "";
            string strKey32on64 = "";
            switch (strType)
            {
                case "jre":
                    strKey = strJRERegKey;
                    strKey32on64 = strJRERegKey32on64;
                    break;
                case "jdk":
                    strKey = strJDKRegKey;
                    strKey32on64 = strJDKRegKey32on64;
                    break;
            }
            strKey += "\\" + JavaVersion(strType);
            strKey32on64 += "\\" + JavaVersion(strType);
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey subKey = rk.OpenSubKey(strKey);
            if (subKey != null) return subKey.GetValue("JavaHome").ToString() + "\\bin\\";
            else
            {
                subKey = rk.OpenSubKey(strKey32on64);
                if (subKey != null) return subKey.GetValue("JavaHome").ToString() + "\\bin\\";
                else return "";
            };
        }

        //Replaces file 1 with file 2
        public static bool ReplaceFile(string strFileOriginal, string strFileReplacement)
        {
            try
            {
                if (File.Exists(strFileReplacement))
                {
                    if (File.Exists(strFileOriginal)) File.Delete(strFileOriginal);
                    File.Move(strFileReplacement, strFileOriginal);
                }
                return true;
            }
            catch
            {
                Log.Write("Unable to update " + strFileOriginal, Log.LogSource.Updater, Log.LogLevel.Error);
                return false;
            }
        }

        //What is the bitness of the system
        public static string GetBitness()
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return "x86";
                case 8:
                    return "x64";
                default:
                    return "x86";
            }
        }

        //Convert Boolean to string
        public static string BooleanToString(bool bolInput)
        {
            if (bolInput) return "true";
            else return "false";
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Log.Write(@"Copying " + target.FullName + @"\" + fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        //Check if a port is available
        public static bool PortIsBusy(int port)
        {
            IPGlobalProperties ipGP = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] endpoints = ipGP.GetActiveTcpListeners();
            if (endpoints == null || endpoints.Length == 0) return false;
            for (int i = 0; i < endpoints.Length; i++)
                if (endpoints[i].Port == port)
                    return true;
            return false;
        }

        //Search a text file for a string
        public static bool SearchFile(string strFileName, string strSearchText)
        {
            StreamReader reader = new StreamReader(strFileName);
            String text = reader.ReadToEnd();
            reader.Close();

            if (Regex.IsMatch(text, strSearchText)) return true;
            else return false;
        }

        //Emulates VBScript's Left http://www.mgbrown.com/PermaLink68.aspx
        public static string Left(string text, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "length must be > 0");
            else if (length == 0 || text.Length == 0)
                return "";
            else if (text.Length <= length)
                return text;
            else
                return text.Substring(0, length);
        }

        //Fetch a text file from the web
        public static string GetTextHTTP(string strURL)
        {
            // create a new instance of WebClient
            WebClient client = new WebClient();

            try
            {
                // actually execute the GET request
                string ret = client.DownloadString(strURL);
                return ret;
            }
            catch (WebException we)
            {
                // WebException.Status holds useful information
                Log.Write(we.Message + ": " + we.Status.ToString());
                return null;
            }
            catch (NotSupportedException ne)
            {
                // other errors
                Log.Write(ne.Message);
                return null;
            }
        }
    }
}
