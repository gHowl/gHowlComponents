using System;
using System.Collections.Generic;
using System.Text;

namespace Bespoke.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Verify that a parameter is not null.
        /// </summary>
        /// <param name="paramName">The name of the paramater to verify.</param>
        /// <param name="param">The object to test for null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="param"/> is null.</exception>
        public static void ParamIsNotNull(string paramName, object param)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

		/// <summary>
		/// Verify that a parameter is not null.
		/// </summary>
		/// <param name="param">The object to test for null.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="param"/> is null.</exception>
		public static void ParamIsNotNull(object param)
		{
            if ((param == null) || ((param is string) && (string.IsNullOrEmpty((string)param))))
            {
                throw new ArgumentNullException();
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        public static void IsTrue(bool condition)
        {
            IsTrue(String.Empty, condition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="condition"></param>
        public static void IsTrue(string paramName, bool condition)
        {
            if (condition == false)
            {
                throw new ArgumentException("Condition false", paramName);
            }
        }
    }
}
