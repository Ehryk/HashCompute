using System;
using System.Configuration;

namespace HashSearch
{
    public static class AppSettings
    {
        #region Private Properties (loaded from App.config)

        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["Main"] != null ? ConfigurationManager.ConnectionStrings["Main"].ConnectionString : null;

        private static readonly string defaultHashAlgorithm = ConfigurationManager.AppSettings["DefaultHashAlgorithm"];
        private static readonly string updateInterval = ConfigurationManager.AppSettings["UpdateInterval"];

        private static readonly string sp_Similarity_Insert = ConfigurationManager.AppSettings["SP_Similarity_Insert"];
        private static readonly string sp_ChainLength_Insert = ConfigurationManager.AppSettings["SP_ChainLength_Insert"];
        private static readonly string sp_Search_Start = ConfigurationManager.AppSettings["SP_Search_Start"];
        private static readonly string sp_Search_End = ConfigurationManager.AppSettings["SP_Search_End"];
        
        #endregion

        #region Public Accessors (with defaults)

        public static string ConnectionString { get { return connectionString ?? "Data Source=localhost;Initial Catalog=HashSearch;User ID=HashSearch;Password=password;"; } }

        public static string DefaultHashAlgorithm { get { return defaultHashAlgorithm ?? "MD5"; } }
        public static TimeSpan UpdateInterval { get { return TimeSpan.Parse(updateInterval ?? "00:01:00"); } }

        public static string SP_Similarity_Insert { get { return sp_Similarity_Insert ?? "HashSimilarity_Insert"; } }
        public static string SP_ChainLength_Insert { get { return sp_ChainLength_Insert ?? "ChainLength_Insert"; } }
        public static string SP_Search_Start { get { return sp_Search_Start ?? "HashSearch_Start"; } }
        public static string SP_Search_End { get { return sp_Search_End ?? "HashSearch_End"; } }
        
        #endregion
    }
}
