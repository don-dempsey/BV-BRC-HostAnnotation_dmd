
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotation.Utilities;

namespace HostAnnotation.DataProviders {

    public class CuratedWordDataProvider : DataProvider {

        // C-tor
        public CuratedWordDataProvider(string dbConnectionString) : base(dbConnectionString) { }


        public async Task<List<CuratedWord>?> getValidWords() {

            var sql = new StringBuilder();

            // Get the term ID for the curation type "ignore".
            sql.AppendLine("DECLARE @ignoreTID AS INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = 'curation_type.ignore') ");

            // Get all valid curated words.
            sql.AppendLine("SELECT ");
            sql.AppendLine(CuratedWord.generatePartialQuery());
            sql.AppendLine("WHERE is_valid = 1 ");
            sql.AppendLine("AND type_tid NOT IN (@ignoreTID) ");
            sql.AppendLine("ORDER BY [type], search_text ");

            return await UsefulObject.getAsyncList<CuratedWord>(_dbConnectionString, sql.ToString());
        }


        // Get a specific curated word.
        public async Task<CuratedWord?> getWord(Guid uid_) {

            var sql = new StringBuilder();
            sql.AppendLine("SELECT TOP 1 ");
            sql.AppendLine(CuratedWord.generatePartialQuery());
            sql.AppendLine($"WHERE [uid] = '{uid_.ToString()}'");

            return await UsefulObject.getAsync<CuratedWord>(_dbConnectionString, sql.ToString());
        }


        public List<CuratedWord>? search(string? searchText_, Terms.curation_type? type_ = null) {

            List<CuratedWord>? words = null;

            var parameters = new List<SqlParameter>() { 
                Utils.createSqlParam("@searchText", SqlDbType.NVarChar, searchText_) 
            };

            string type = type_ == null ? "NULL" : $"'{Terms.enumString(type_.Value)}'";

            var sql = new StringBuilder();

            // Get the term ID for the curation type "ignore".
            sql.AppendLine("DECLARE @ignoreTID AS INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = 'curation_type.ignore') ");

            // If a type constraint was provided, lookup its term ID.
            sql.AppendLine($"DECLARE @type AS VARCHAR(60) = {type} ");
            sql.AppendLine("DECLARE @typeTID AS INT = CASE ");
            sql.AppendLine("	WHEN @type IS NULL OR LEN(@type) < 1 THEN NULL ");
            sql.AppendLine("    WHEN @type = 'ignore' THEN NULL ");
            sql.AppendLine("	ELSE (");
            sql.AppendLine("		SELECT TOP 1 term_id ");
            sql.AppendLine("		FROM term ");
            sql.AppendLine("		WHERE term_full_key = 'curation_type.'+@type ");
            sql.AppendLine("	) ");
            sql.AppendLine("END ");

            // Search the curation (curated words) or return all if no search text was provided.
            sql.AppendLine("SELECT ");
            sql.AppendLine(CuratedWord.generatePartialQuery());
            sql.AppendLine("WHERE (");
            sql.AppendLine("	(@searchText IS NULL OR LEN(@searchText) < 1) ");
            sql.AppendLine("	OR (alternate_text LIKE '%'+@searchText+'%') ");
            sql.AppendLine("	OR (search_text LIKE '%'+@searchText+'%') ");
            sql.AppendLine(") ");
            sql.AppendLine("AND type_tid NOT IN (@ignoreTID) ");
            sql.AppendLine("AND (@typeTID IS NULL OR (@typeTID IS NOT NULL AND type_tid = @typeTID)) ");
            sql.AppendLine("ORDER BY [type], search_text ");

            UsefulObject.get<CuratedWord>(_dbConnectionString, parameters, sql.ToString(), ref words);

            return words;
        }



    }
}
