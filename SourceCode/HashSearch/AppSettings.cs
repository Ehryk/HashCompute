using System.Configuration;

namespace HashSearch
{
    public static class AppSettings
    {
        #region Private Properties (loaded from App.config)

        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["Main"] != null ? ConfigurationManager.ConnectionStrings["Main"].ConnectionString : null;

        private static readonly string defaultHashAlgorithm = ConfigurationManager.AppSettings["DefaultHashAlgorithm"];

        private static readonly string sp_Similarity_Insert = ConfigurationManager.AppSettings["SP_Similarity_Insert"];
        
        #endregion

        #region Public Accessors (with defaults)

        public static string ConnectionString { get { return connectionString ?? "Data Source=localhost;Initial Catalog=HashSearch;User ID=HashSearch;Password=password;"; } }

        public static string DefaultHashAlgorithm { get { return defaultHashAlgorithm ?? "MD5"; } }

        public static string SP_Similarity_Insert { get { return sp_Similarity_Insert ?? "HashSimilarity_Insert"; } }
        
        #endregion
    }
}
