using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HashSearch
{
    public static class DataAccess
    {
        #region Properties

        private static SqlConnection connection;

        public static string ConnectionString { get; set; }
        private static SqlConnection Connection
        {
            get
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    return connection;
                
                if (connection != null)
                    connection.Dispose();

                connection = new SqlConnection(ConnectionString);
                connection.Open();
                return connection;
            }
        }

        #endregion

        #region Constructors

        static DataAccess()
        {
            ConnectionString = AppSettings.ConnectionString;
        }

        #endregion

        #region Similarity Methods

        public static bool SimilarityInsert(string algorithmName, byte[] input, byte[] result, int? bitSimilarity = null, int? byteSimilarity = null, bool? fixPoint = null)
        {
            var cmd = new SqlCommand(AppSettings.SP_Similarity_Insert, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AlgorithmName", algorithmName);
            cmd.Parameters.AddWithValue("@Input", input);
            cmd.Parameters.AddWithValue("@Result", result);
            if (bitSimilarity != null)
                cmd.Parameters.AddWithValue("@BitSimilarity", bitSimilarity);
            if (byteSimilarity != null)
                cmd.Parameters.AddWithValue("@ByteSimilarity", byteSimilarity);
            if (fixPoint != null)
                cmd.Parameters.AddWithValue("@FixPoint", fixPoint);

            return cmd.ExecuteNonQuery() == 1;
        }

        #endregion

        #region ChainLength Methods

        public static bool ChainLengthInsert(string algorithmName, byte[] input, long length)
        {
            var cmd = new SqlCommand(AppSettings.SP_ChainLength_Insert, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AlgorithmName", algorithmName);
            cmd.Parameters.AddWithValue("@Input", input);
            cmd.Parameters.AddWithValue("@Length", length);

            return cmd.ExecuteNonQuery() == 1;
        }

        #endregion

        #region Search Methods

        public static int SearchStart(string pAlgorithmName, string pMachineName, string pSearchMode, byte[] pSeed)
        {
            var cmd = new SqlCommand(AppSettings.SP_Search_Start, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@AlgorithmName", pAlgorithmName);
            if (pMachineName != null)
                cmd.Parameters.AddWithValue("@MachineName", pMachineName);
            if (pSearchMode != null)
                cmd.Parameters.AddWithValue("@SearchMode", pSearchMode);
            if (pSeed != null)
                cmd.Parameters.AddWithValue("@Seed", pSeed);

            return (int)cmd.ExecuteScalar();
        }

        public static bool SearchEnd(int pSearchID, long? pInputCount = null, byte[] pLastInput = null)
        {
            var cmd = new SqlCommand(AppSettings.SP_Search_End, Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SearchID", pSearchID);
            if (pInputCount != null)
            cmd.Parameters.AddWithValue("@InputCount", pInputCount);
            if (pLastInput != null)
                cmd.Parameters.AddWithValue("@LastInput", pLastInput);

            return cmd.ExecuteNonQuery() == 1;
        }

        #endregion
    }
}
