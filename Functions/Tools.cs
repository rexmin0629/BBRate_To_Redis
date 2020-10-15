using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection;

namespace BBRate_To_Redis
{
    public class MemberHelper<T>
    {
        public string GetName<U>(Expression<Func<T, U>> expression)
        {
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
                return memberExpression.Member.Name;

            throw new InvalidOperationException("Member expression expected");
        }
    }
    public class IniFile
    {
        /// <summary>
        /// The maximum size of a section in an ini file.
        /// </summary>
        /// <remarks>
        /// This property defines the maximum size of the buffers 
        /// used to retreive data from an ini file.  This value is 
        /// the maximum allowed by the win32 functions 
        /// GetPrivateProfileSectionNames() or 
        /// GetPrivateProfileString().
        /// </remarks>
        public const int MaxSectionSize = 32767; // 32 KB

        //The path of the file we are operating on.
        private string m_path;

        #region P/Invoke declares

        /// <summary>
        /// A static class that provides the win32 P/Invoke signatures 
        /// used by this class.
        /// </summary>
        /// <remarks>
        /// Note:  In each of the declarations below, we explicitly set CharSet to 
        /// Auto.  By default in C#, CharSet is set to Ansi, which reduces 
        /// performance on windows 2000 and above due to needing to convert strings
        /// from Unicode (the native format for all .Net strings) to Ansi before 
        /// marshalling.  Using Auto lets the marshaller select the Unicode version of 
        /// these functions when available.
        /// </remarks>
        [System.Security.SuppressUnmanagedCodeSecurity]
        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern int GetPrivateProfileSectionNames(byte[] iniValue,
                                                                   uint nSize,
                                                                   string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern uint GetPrivateProfileString(string lpAppName,
                                                              string lpKeyName,
                                                              string lpDefault,
                                                              StringBuilder lpReturnedString,
                                                              int nSize,
                                                              string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern uint GetPrivateProfileString(string lpAppName,
                                                              string lpKeyName,
                                                              string lpDefault,
                                                              [In, Out] char[] lpReturnedString,
                                                              int nSize,
                                                              string lpFileName);

            [DllImport("kernel32.dll")]
            public static extern int GetPrivateProfileString(string lpAppName,
                                                             string lpKeyName,
                                                             string lpDefault,
                                                             byte[] iniValue,
                                                             uint nSize,
                                                             string lpFileName);
            [DllImport("kernel32")]
            public static extern int GetPrivateProfileString(byte[] section, 
                                                             byte[] key, 
                                                             byte[] def, 
                                                             byte[] retVal, 
                                                             int size, 
                                                             string filePath);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern int GetPrivateProfileInt(string lpAppName,
                                                          string lpKeyName,
                                                          int lpDefault,
                                                          string lpFileName);

            [DllImport("kernel32.dll")]
            public static extern int GetPrivateProfileSection(string lpAppName,
                                                              byte[] iniValue,
                                                              uint nSize,
                                                              string lpFileName);

            //We explicitly enable the SetLastError attribute here because
            // WritePrivateProfileString returns errors via SetLastError.
            // Failure to set this can result in errors being lost during 
            // the marshal back to managed code.
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool WritePrivateProfileString(string lpAppName,
                                                                string lpKeyName,
                                                                string lpString,
                                                                string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool WritePrivateProfileString(byte[] section, 
                                                                byte[] key, 
                                                                byte[] val, 
                                                                string filePath);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        /// <param name="path">The ini file to read and write from.</param>
        public IniFile(string path)
        {
            //Convert to the full path.  Because of backward compatibility, 
            // the win32 functions tend to assume the path should be the 
            // root Windows directory if it is not specified.  By calling 
            // GetFullPath, we make sure we are always passing the full path
            // the win32 functions.
            List<string> pathParam = new List<string>(path.Split('\\'));
            pathParam.RemoveAt(pathParam.Count - 1);
            string folderPath = string.Join("\\", pathParam);
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            if (!System.IO.File.Exists(path))
            {
                System.IO.FileStream fs = System.IO.File.Create((path));
                fs.Close();
            }

            m_path = System.IO.Path.GetFullPath(path);
        }

        /// <summary>
        /// Gets the full path of ini file this object instance is operating on.
        /// </summary>
        /// <value>A file path.</value>
        public string Path
        {
            get
            {
                return m_path;
            }
        }

        private static byte[] getBytes(string s, string encodingName)
        {
            return null == s ? null : Encoding.GetEncoding(encodingName).GetBytes(s);
        }

        #region Get Value Methods

        /// <summary>
        /// Gets the value of a setting in an ini file as a <see cref="T:System.String"/>.
        /// </summary>
        /// <param name="sectionName">The name of the section to read from.</param>
        /// <param name="keyName">The name of the key in section to read.</param>
        /// <param name="defaultValue">The default value to return if the key
        /// cannot be found.</param>
        /// <returns>The value of the key, if found.  Otherwise, returns 
        /// <paramref name="defaultValue"/></returns>
        /// <remarks>
        /// The retreived value must be less than 32KB in length.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public string GetString(string sectionName,
                                string keyName,
                                string defaultValue)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            if (keyName == null)
                throw new ArgumentNullException("keyName");

            StringBuilder retval = new StringBuilder(IniFile.MaxSectionSize);

            NativeMethods.GetPrivateProfileString(sectionName,
                                                  keyName,
                                                  defaultValue,
                                                  retval,
                                                  IniFile.MaxSectionSize,
                                                  m_path);

            return retval.ToString();
        }

        public string GetString(string section, string key, string def, string fileName, string encodingName = "utf-8", int size = 1024)
        {
            byte[] buffer = new byte[size];
            int count = NativeMethods.GetPrivateProfileString(getBytes(section, encodingName), getBytes(key, encodingName), getBytes(def, encodingName), buffer, size, fileName);
            return Encoding.GetEncoding(encodingName).GetString(buffer, 0, count).Trim();
        }

        /// <summary>
        /// Gets the value of a setting in an ini file as a <see cref="T:System.Int16"/>.
        /// </summary>
        /// <param name="sectionName">The name of the section to read from.</param>
        /// <param name="keyName">The name of the key in section to read.</param>
        /// <param name="defaultValue">The default value to return if the key
        /// cannot be found.</param>
        /// <returns>The value of the key, if found.  Otherwise, returns 
        /// <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public int GetInt16(string sectionName,
                            string keyName,
                            short defaultValue)
        {
            int retval = GetInt32(sectionName, keyName, defaultValue);

            return Convert.ToInt16(retval);
        }

        /// <summary>
        /// Gets the value of a setting in an ini file as a <see cref="T:System.Int32"/>.
        /// </summary>
        /// <param name="sectionName">The name of the section to read from.</param>
        /// <param name="keyName">The name of the key in section to read.</param>
        /// <param name="defaultValue">The default value to return if the key
        /// cannot be found.</param>
        /// <returns>The value of the key, if found.  Otherwise, returns 
        /// <paramref name="defaultValue"/></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public int GetInt32(string sectionName,
                            string keyName,
                            int defaultValue)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            if (keyName == null)
                throw new ArgumentNullException("keyName");


            return NativeMethods.GetPrivateProfileInt(sectionName, keyName, defaultValue, m_path);
        }

        /// <summary>
        /// Gets the value of a setting in an ini file as a <see cref="T:System.Double"/>.
        /// </summary>
        /// <param name="sectionName">The name of the section to read from.</param>
        /// <param name="keyName">The name of the key in section to read.</param>
        /// <param name="defaultValue">The default value to return if the key
        /// cannot be found.</param>
        /// <returns>The value of the key, if found.  Otherwise, returns 
        /// <paramref name="defaultValue"/></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public double GetDouble(string sectionName,
                                string keyName,
                                double defaultValue)
        {
            string retval = GetString(sectionName, keyName, "");

            if (retval == null || retval.Length == 0)
            {
                return defaultValue;
            }

            return Convert.ToDouble(retval, CultureInfo.InvariantCulture);
        }

        #endregion

        #region GetSectionValues Methods

        /// <summary>
        /// Gets all of the values in a section as a list.
        /// <para>(不建議使用, ini檔案太大時會遺失後段的資料)</para>
        /// </summary>
        /// <param name="sectionName">
        /// Name of the section to retrieve values from.
        /// </param>
        /// <returns>
        /// A <see cref="List{T}"/> containing <see cref="KeyValuePair{T1, T2}"/> objects 
        /// that describe this section.  Use this verison if a section may contain
        /// multiple items with the same key value.  If you know that a section 
        /// cannot contain multiple values with the same key name or you don't 
        /// care about the duplicates, use the more convenient 
        /// <see cref="GetSectionValues"/> function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> is a null reference  (Nothing in VB)
        /// </exception>
        public List<KeyValuePair<string, string>> GetSectionValuesAsList(string sectionName)
        {
            List<KeyValuePair<string, string>> retval;
            string[] keyValuePairs;
            string key, value;
            int equalSignPos;

            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            //Allocate a buffer for the returned section names.
            byte[] byteArray = new byte[IniFile.MaxSectionSize];
            //IntPtr ptr = Marshal.AllocCoTaskMem(IniFile.MaxSectionSize);

            try
            {
                //Get the section key/value pairs into the buffer.
                int len = NativeMethods.GetPrivateProfileSection(sectionName,
                                                                 byteArray,
                                                                 IniFile.MaxSectionSize,
                                                                 m_path);

                keyValuePairs = ConvertNullSeperatedStringToStringArray(byteArray, len);
            }
            finally
            {
                //Free the buffer
                byteArray = null;
            }

            //Parse keyValue pairs and add them to the list.
            retval = new List<KeyValuePair<string, string>>(keyValuePairs.Length);

            for (int i = 0; i < keyValuePairs.Length; ++i)
            {
                //Parse the "key=value" string into its constituent parts
                equalSignPos = keyValuePairs[i].IndexOf('=');

                key = keyValuePairs[i].Substring(0, equalSignPos);

                value = keyValuePairs[i].Substring(equalSignPos + 1,
                                                   keyValuePairs[i].Length - equalSignPos - 1);

                retval.Add(new KeyValuePair<string, string>(key, value));
            }

            return retval;
        }

        /// <summary>
        /// Gets all of the values in a section as a dictionary.
        /// </summary>
        /// <param name="sectionName">
        /// Name of the section to retrieve values from.
        /// </param>
        /// <returns>
        /// A <see cref="Dictionary{T, T}"/> containing the key/value 
        /// pairs found in this section.  
        /// </returns>
        /// <remarks>
        /// If a section contains more than one key with the same name, 
        /// this function only returns the first instance.  If you need to 
        /// get all key/value pairs within a section even when keys have the 
        /// same name, use <see cref="GetSectionValuesAsList"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> is a null reference  (Nothing in VB)
        /// </exception>
        public Dictionary<string, string> GetSectionValues(string sectionName)
        {
            List<KeyValuePair<string, string>> keyValuePairs;
            Dictionary<string, string> retval;

            keyValuePairs = GetSectionValuesAsList(sectionName);

            //Convert list into a dictionary.
            retval = new Dictionary<string, string>(keyValuePairs.Count);

            foreach (KeyValuePair<string, string> keyValuePair in keyValuePairs)
            {
                //Skip any key we have already seen.
                if (!retval.ContainsKey(keyValuePair.Key))
                {
                    retval.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return retval;
        }

        #endregion

        #region Get Key/Section Names

        /// <summary>
        /// Gets the names of all keys under a specific section in the ini file.
        /// <para>(不建議使用, ini檔案太大時會遺失後段的資料)</para>
        /// </summary>
        /// <param name="sectionName">
        /// The name of the section to read key names from.
        /// </param>
        /// <returns>An array of key names.</returns>
        /// <remarks>
        /// The total length of all key names in the section must be 
        /// less than 32KB in length.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> is a null reference  (Nothing in VB)
        /// </exception>
        public string[] GetKeyNames(string sectionName)
        {
            int len;
            string[] retval;

            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            //Allocate a buffer for the returned section names.
            byte[] byteArray = new byte[IniFile.MaxSectionSize];
            //IntPtr ptr = Marshal.AllocCoTaskMem(IniFile.MaxSectionSize);

            try
            {
                //Get the section names into the buffer.
                len = NativeMethods.GetPrivateProfileString(sectionName,
                                                            null,
                                                            null,
                                                            byteArray,
                                                            IniFile.MaxSectionSize,
                                                            m_path);

                retval = ConvertNullSeperatedStringToStringArray(byteArray, len);
            }
            finally
            {
                //Free the buffer

                byteArray = null;
            }

            return retval;
        }

        /// <summary>
        /// Gets the names of all sections in the ini file.
        /// <para>(不建議使用, ini檔案太大時會遺失後段的資料)</para>
        /// </summary>
        /// <returns>An array of section names.</returns>
        /// <remarks>
        /// The total length of all section names in the section must be 
        /// less than 32KB in length.
        /// </remarks>

        public string[] GetSectionNames()
        {
            string[] retval;
            int len;

            //Allocate a buffer for the returned section names.
            byte[] byteArray = new byte[IniFile.MaxSectionSize];
            //IntPtr ptr = Marshal.AllocCoTaskMem(IniFile.MaxSectionSize);

            try
            {
                //Get the section names into the buffer.
                len = NativeMethods.GetPrivateProfileSectionNames(byteArray,
                    IniFile.MaxSectionSize, m_path);
                retval = ConvertNullSeperatedStringToStringArray(byteArray, len);
            }
            finally
            {
                //Free the buffer
                byteArray = null;
            }

            return retval;
        }

        /// <summary>
        /// Converts the null seperated pointer to a string into a string array.
        /// </summary>
        /// <param name="ptr">A pointer to string data.</param>
        /// <param name="valLength">
        /// Length of the data pointed to by <paramref name="ptr"/>.
        /// </param>
        /// <returns>
        /// An array of strings; one for each null found in the array of characters pointed
        /// at by <paramref name="ptr"/>.
        /// </returns>
        private static string[] ConvertNullSeperatedStringToStringArray(byte[] byteArray, int valLength)
        {
            string[] retval;

            if (valLength == 0)
            {
                //Return an empty array.
                retval = new string[0];
            }
            else
            {
                //Convert the buffer into a string.  Decrease the length 
                //by 1 so that we remove the second null off the end.
                string buff = System.Text.Encoding.ASCII.GetString(byteArray, 0, valLength - 1);
                //string buff = Marshal.PtrToStringAuto(ptr, valLength - 1);

                //Parse the buffer into an array of strings by searching for nulls.
                retval = buff.Split('\0');
            }

            return retval;
        }

        #endregion

        #region Write Methods
        
        /// <summary>
        /// Writes a <see cref="T:System.String"/> value to the ini file.
        /// </summary>
        /// <param name="sectionName">The name of the section to write to .</param>
        /// <param name="keyName">The name of the key to write to.</param>
        /// <param name="value">The string value to write</param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// The write failed.
        /// </exception>
        private void WriteValueInternal(string sectionName, string keyName, string value)
        {
            if (!NativeMethods.WritePrivateProfileString(sectionName, keyName, value, m_path))
            {
                throw new System.ComponentModel.Win32Exception();
            }
        }

        /// <summary>
        /// Writes a <see cref="T:System.String"/> value to the ini file.
        /// </summary>
        /// <param name="sectionName">The name of the section to write to .</param>
        /// <param name="keyName">The name of the key to write to.</param>
        /// <param name="value">The string value to write</param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// The write failed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> or 
        /// <paramref name="value"/>  are a null reference  (Nothing in VB)
        /// </exception>
        public void WriteValue(string sectionName, string keyName, string value)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            if (keyName == null)
                throw new ArgumentNullException("keyName");

            if (value == null)
                throw new ArgumentNullException("value");

            WriteValueInternal(sectionName, keyName, value);
        }

        /// <summary>
        /// Writes an <see cref="T:System.Int16"/> value to the ini file.
        /// </summary>
        /// <param name="sectionName">The name of the section to write to .</param>
        /// <param name="keyName">The name of the key to write to.</param>
        /// <param name="value">The value to write</param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// The write failed.
        /// </exception>
        public void WriteValue(string sectionName, string keyName, short value)
        {
            WriteValue(sectionName, keyName, (int)value);
        }

        /// <summary>
        /// Writes an <see cref="T:System.Int32"/> value to the ini file.
        /// </summary>
        /// <param name="sectionName">The name of the section to write to .</param>
        /// <param name="keyName">The name of the key to write to.</param>
        /// <param name="value">The value to write</param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// The write failed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public void WriteValue(string sectionName, string keyName, int value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes an <see cref="T:System.Single"/> value to the ini file.
        /// </summary>
        /// <param name="sectionName">The name of the section to write to .</param>
        /// <param name="keyName">The name of the key to write to.</param>
        /// <param name="value">The value to write</param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// The write failed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public void WriteValue(string sectionName, string keyName, float value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes an <see cref="T:System.Double"/> value to the ini file.
        /// </summary>
        /// <param name="sectionName">The name of the section to write to .</param>
        /// <param name="keyName">The name of the key to write to.</param>
        /// <param name="value">The value to write</param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">
        /// The write failed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public void WriteValue(string sectionName, string keyName, double value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public bool WriteString(string section, string key, string value, string fileName, string encodingName = "utf-8")
        {
            return NativeMethods.WritePrivateProfileString(getBytes(section, encodingName), getBytes(key, encodingName), getBytes(value, encodingName), fileName);
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Deletes the specified key from the specified section.
        /// </summary>
        /// <param name="sectionName">
        /// Name of the section to remove the key from.
        /// </param>
        /// <param name="keyName">
        /// Name of the key to remove.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> or <paramref name="keyName"/> are 
        /// a null reference  (Nothing in VB)
        /// </exception>
        public void DeleteKey(string sectionName, string keyName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            if (keyName == null)
                throw new ArgumentNullException("keyName");

            WriteValueInternal(sectionName, keyName, null);
        }

        /// <summary>
        /// Deletes a section from the ini file.
        /// </summary>
        /// <param name="sectionName">
        /// Name of the section to delete.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sectionName"/> is a null reference (Nothing in VB)
        /// </exception>
        public void DeleteSection(string sectionName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            WriteValueInternal(sectionName, null, null);
        }

        #endregion
    }

    //==================================================================================================================Dictionary Extensions

    public static class DictionaryExtensionsClass
    {
        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                someObjectType
                         .GetProperty(item.Key)
                         .SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValueProvider();
        }

        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }
    }

}
