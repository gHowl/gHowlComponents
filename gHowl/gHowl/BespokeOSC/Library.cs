using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace Bespoke.Common
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void VoidHandler();

    /// <summary>
    /// Represents the method that executes on a System.Threading.Thread.
    /// </summary>
    /// <param name="obj">An object that contains data for the thread procedure.</param>
    public delegate void ThreadStartWrapperHandler(object obj);

#if WINDOWS
	/// <summary>
	/// 
	/// </summary>
	/// <param name="progressBar"></param>
	/// <param name="value"></param>
	public delegate void UpdateProgressValueHandler(System.Windows.Forms.ProgressBar progressBar, int value);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="label"></param>
	/// <param name="text"></param>
	public delegate void UpdateLabelHandler(System.Windows.Forms.Label label, string text);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="textBox"></param>
	/// <param name="text"></param>
	public delegate void UpdateTextBoxHandler(System.Windows.Forms.TextBox textBox, string text);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="button"></param>
	/// <param name="text"></param>
	public delegate void UpdateButtonTextHandler(System.Windows.Forms.Button button, string text);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="pictureBox"></param>
	/// <param name="image"></param>
	public delegate void UpdatePictureBoxImageHandler(System.Windows.Forms.PictureBox pictureBox, Image image);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="button"></param>
    public delegate void ButtonPerformClickHandler(System.Windows.Forms.Button button);   
#endif

    /// <summary>
    /// A replacement for parameterized thread start (missing from the .NET CF)
    /// </summary>
    public class ThreadStartDelegateWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="obj"></param>
        public ThreadStartDelegateWrapper(ThreadStartWrapperHandler handler, object obj)
        {
            mHandler = handler;
            mObject = obj;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            mHandler(mObject);
        }

        private ThreadStartWrapperHandler mHandler;
        private object mObject;
    }

    /// <summary>
	/// 
	/// </summary>
	public static class Library
	{
#if WINDOWS
        /// <summary>
        /// SetForegroundWindow import.
        /// </summary>        
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
#endif

#if PocketPC
        /// <summary>
        /// SystemIdelTimerReset import.
        /// </summary>
        [DllImport("coredll.dll")]
        public static extern void SystemIdleTimerReset();
#endif

        /// <summary>
        /// Convert a byte array of ASCII characters to a string.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns>A string with the same characters in the byte array.</returns>
        public static string ASCIIByteArrayToString(byte[] characters)
        {
            if (characters == null)
            {
                return string.Empty;
            }
            else
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
#if WINDOWS
                return encoding.GetString(characters);
#else
                return encoding.GetString(characters, 0, characters.Length);
#endif
            }
        }

        /// <summary>
        /// Convert a byte array of ASCII characters to a string.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns>A string with the same characters in the byte array.</returns>
        public static string UnicodeByteArrayToString(byte[] characters)
        {
            if (characters == null)
            {
                return string.Empty;
            }
            else
            {
                UnicodeEncoding encoding = new UnicodeEncoding();
#if WINDOWS
                return encoding.GetString(characters);
#else
                return encoding.GetString(characters, 0, characters.Length);
#endif
            }
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceDirectory"></param>
		/// <param name="destinationDirectory"></param>
		public static void CopyDirectory(string sourceDirectory, string destinationDirectory)
		{
			DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourceDirectory);
			if (sourceDirectoryInfo.Exists == false)
			{
				throw new DirectoryNotFoundException(sourceDirectoryInfo.FullName);
			}

			DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(destinationDirectory);
			if (destinationDirectoryInfo.Exists == false)
			{
				destinationDirectoryInfo.Create();
			}

			foreach (DirectoryInfo subDirectoryInfo in sourceDirectoryInfo.GetDirectories())
			{
				CopyDirectory(subDirectoryInfo.FullName, destinationDirectoryInfo.FullName + Path.DirectorySeparatorChar + subDirectoryInfo.Name);
			}

			foreach (FileInfo fileInfo in sourceDirectoryInfo.GetFiles())
			{
				fileInfo.CopyTo(destinationDirectoryInfo.FullName + Path.DirectorySeparatorChar + fileInfo.Name, true);
			}
		}

        public static bool FindFile(string fileName, string startDirectory, out FileInfo foundFile)
        {
            bool success = false;
            foundFile = null;

            FileInfo specifiedFileInfo = new FileInfo(fileName);
            if (specifiedFileInfo.Exists)
            {
                foundFile = specifiedFileInfo;
                success = true;
            }
            else
            {
                DirectoryInfo startDirectoryInfo = new DirectoryInfo(startDirectory);
#if WINDOWS
                FileInfo[] foundFiles = startDirectoryInfo.GetFiles(specifiedFileInfo.Name, SearchOption.AllDirectories);
#else
                FileInfo[] foundFiles = startDirectoryInfo.GetFiles(specifiedFileInfo.Name);
#endif
                if (foundFiles.Length > 0)
                {
                    foundFile = foundFiles[0];
                    success = true;
                }
            }

            return success;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <param name="proxmityThreshold"></param>
		/// <returns></returns>
        public static bool ValuesInProximity(int value1, int value2, int proxmityThreshold)
        {
            return Math.Abs(value1 - value2) <= proxmityThreshold;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <param name="proxmityThreshold"></param>
		/// <returns></returns>
        public static bool ValuesInProximity(double value1, double value2, double proxmityThreshold)
        {
            return Math.Abs(value1 - value2) <= proxmityThreshold;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <param name="proxmityThreshold"></param>
		/// <returns></returns>
        public static bool ValuesInProximity(DateTime value1, DateTime value2, TimeSpan proxmityThreshold)
        {
            TimeSpan difference;

            if (value1 > value2)
            {
                difference = value1.Subtract(value2);
            }
            else
            {
                difference = value2.Subtract(value1);
            }

            return difference <= proxmityThreshold;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(string value)
        {
            int result;
            return IsNumeric(value, out result);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
        public static bool IsNumeric(string value, out int result)
        {
#if WINDOWS
            if (int.TryParse(value, out result))
            {
                return true;
            }
            else
            {
                return false;
            }
#else
            try
            {
                result = int.Parse(value);
                return true;
            }
            catch
            {
                result = default(int);
                return false;
            }
#endif
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        public static float ToDegrees(float radians)
        {
            return (radians * 57.29578f);
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians</returns>
        public static float ToRadians(float degrees)
        {
            return (degrees * 0.01745329f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Enum[] GetEnumValues(Type enumType)
        {
            if (enumType.BaseType == typeof(Enum))
            {
                FieldInfo[] info = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
                Enum[] values = new Enum[info.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (Enum)info[i].GetValue(null);
                }

                return values;
            }
            else
            {
                throw new Exception("Given type is not an Enum.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static T[] CopySubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }


        /// <summary>
        /// Swap byte order.
        /// </summary>
        /// <param name="data">The source data.</param>
        /// <returns>The swapped data source.</returns>
        public static byte[] SwapEndian(byte[] data)
        {
            byte[] swapped = new byte[data.Length];
            for (int i = data.Length - 1, j = 0; i >= 0; i--, j++)
            {
                swapped[j] = data[i];
            }

            return swapped;
        }
	}
}
