
using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotation.Utilities;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using static HostAnnotation.Common.Names;
using static HostAnnotation.Common.Terms;
using static System.Net.Mime.MediaTypeNames;

namespace HostAnnotation.DataProviders {

    public class AnnotationDataProvider : DataProvider {


        // C-tor
        public AnnotationDataProvider(string dbConnectionString_) : base(dbConnectionString_) { }


        public async Task<Result> createAnnotatedHost(int hostID_, double? scoreThreshold_ = null) {

            if (scoreThreshold_ == null) { scoreThreshold_ = Constants.DEFAULT_ANNOTATION_SCORE_THRESHOLD; }

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@annotatedHostID", SqlDbType.Int, hostID_),
                Utils.createSqlParam("@scoreThreshold", SqlDbType.Float, scoreThreshold_)
            };

            try {
                await DbQueryManager.runStoredProcedureAsync(_dbConnectionString, "dbo.createAnnotatedHost", parameters);
            } catch (Exception ex) {
                return Result.failure(ex.Message);
            }

            return Result.success();
        }

        // Create host name/taxon name matches for a host's tokens.
        public async Task<Result> createHostnameTaxonMatches(int hostID_) {

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@hostID", SqlDbType.Int, hostID_)
            };

            try {
                await DbQueryManager.runStoredProcedureAsync(_dbConnectionString, "dbo.createHostnameTaxonMatches", parameters);
            } catch (Exception exc_) {
                return Result.failure(exc_.Message);
            }
            
            return Result.success();
        }

        public async Task<Result> createHostTokenAnnotations(int? algorithmID_, int hostID_) {

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@algorithmID", SqlDbType.Int, algorithmID_),
                Utils.createSqlParam("@hostID", SqlDbType.Int, hostID_)
            };

            try {
                await DbQueryManager.runStoredProcedureAsync(_dbConnectionString, "dbo.createHostTokenAnnotations_SingleRun", parameters);
            } catch (Exception exc_) {
                return Result.failure(exc_.Message);
            }

            return Result.success(); 
        }


        public async Task<Result> createHostTokenVariations(string baseToken_, Terms.host_token_type baseType_, int hostID_, bool isOneOfMany_) {

            int isOneOfMany = isOneOfMany_ ? 1 : 0;

            // Create text variations on the base token.
            var variations = new HostTokenVariations(baseToken_);

            // Add the variations as SQL parameters.
            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@addSToEnd", SqlDbType.NVarChar, variations.addSToEnd),
                Utils.createSqlParam("@appendSpDot", SqlDbType.NVarChar, variations.appendSpDot),
                Utils.createSqlParam("@baseToken", SqlDbType.NVarChar, variations.baseToken),
                Utils.createSqlParam("@baseType", SqlDbType.NVarChar, Enum.GetName(baseType_)),
                Utils.createSqlParam("@hostID", SqlDbType.Int, hostID_),
                Utils.createSqlParam("@isOneOfMany", SqlDbType.Bit, isOneOfMany),
                Utils.createSqlParam("@minusOneWordLeft", SqlDbType.NVarChar, variations.minusOneWordLeft),
                Utils.createSqlParam("@minusOneWordRight", SqlDbType.NVarChar, variations.minusOneWordRight),
                Utils.createSqlParam("@minusTwoWordsLeft", SqlDbType.NVarChar, variations.minusTwoWordsLeft),
                Utils.createSqlParam("@minusTwoWordsRight", SqlDbType.NVarChar, variations.minusTwoWordsRight),
                Utils.createSqlParam("@minusThreeWordsLeft", SqlDbType.NVarChar, variations.minusThreeWordsLeft),
                Utils.createSqlParam("@minusThreeWordsRight", SqlDbType.NVarChar, variations.minusThreeWordsRight),
                Utils.createSqlParam("@prependCommon", SqlDbType.NVarChar, variations.prependCommon),
                Utils.createSqlParam("@removeCommonAppendSpDot", SqlDbType.NVarChar, variations.removeCommonAppendSpDot),
                Utils.createSqlParam("@removeCommonFromStart", SqlDbType.NVarChar, variations.removeCommonFromStart),
                Utils.createSqlParam("@removeSAppendSpDot", SqlDbType.NVarChar, variations.removeSAppendSpDot),
                Utils.createSqlParam("@removeSFromEnd", SqlDbType.NVarChar, variations.removeSFromEnd),
                Utils.createSqlParam("@removeSpDotFromEnd", SqlDbType.NVarChar, variations.removeSpDotFromEnd)
            };

            try {
                await DbQueryManager.runStoredProcedureAsync(_dbConnectionString, "dbo.createHostTokenVariations", parameters);
            } catch (Exception ex) {
                return Result.failure(ex.Message);
            }

            return Result.success();
        }


        public async Task<AnnotatedHost?> getAnnotatedHost(int hostID_) {

            var sql = new StringBuilder();
            sql.AppendLine("SELECT TOP 1 ");
            sql.AppendLine(AnnotatedHost.generatePartialQuery());
            sql.AppendLine($"WHERE [host_id] = {hostID_} ");

            return await UsefulObject.getAsync<AnnotatedHost>(_dbConnectionString, sql.ToString());
        }


        public async Task<List<HostQuery>?> getHostsByGroup(int groupID_, int? maxHosts_) {

            var limitText = maxHosts_ == null 
                ? "" 
                : $"TOP {maxHosts_.Value}";

            var sql = new StringBuilder();
            sql.AppendLine($"SELECT {limitText}");
            sql.AppendLine("   id, ");
            sql.AppendLine("   filtered_text, ");
            sql.AppendLine("   text ");
            sql.AppendLine("FROM hosts ");
            sql.AppendLine($"WHERE group_id = {groupID_} ");
            sql.AppendLine("AND is_processed = 0 ");
            sql.AppendLine("AND (is_valid IS NULL OR is_valid = 0) ");
            sql.AppendLine("ORDER BY text ");

            return await UsefulObject.getAsyncList<HostQuery>(_dbConnectionString, sql.ToString());
        }


        public async Task<List<HostTaxonMatch>?> getHostTaxaMatches(int hostID_) {

            var sql = new StringBuilder();
            sql.AppendLine("SELECT ");
            sql.AppendLine(HostTaxonMatch.generatePartialQuery());
            sql.AppendLine($"WHERE hhtm.[host_id] = {hostID_} ");

            return await UsefulObject.getAsyncList<HostTaxonMatch>(_dbConnectionString, sql.ToString());
        }


        // Is there already a host with this filtered text? If not, create the host. Either way, return the host ID.
        // TODO: include the concept of "linked host ID".
        public int lookupHostID(string? filteredText_, string? initialText_, ref bool isNew_) {

            if (Utils.isEmptyElseTrim(ref filteredText_)) { throw SmartException.create("Invalid filtered text"); }
            if (Utils.isEmptyElseTrim(ref initialText_)) { throw SmartException.create("Invalid initial text"); }

            // Add the initial host text and filtered text as SQL parameters.
            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@filteredText", SqlDbType.NVarChar, filteredText_),
                Utils.createSqlParam("@text", SqlDbType.NVarChar, initialText_)
            };

            var sql = new StringBuilder();
            sql.AppendLine("DECLARE @isNew AS BIT = 1 ");
            sql.AppendLine("DECLARE @hostID AS INT = (");
            sql.AppendLine("    SELECT TOP 1 id ");
            sql.AppendLine("    FROM hosts ");
            sql.AppendLine($"   WHERE filtered_text = @filteredText ");
            sql.AppendLine(") ");

            // Create the host if it doesn't already exist.
            sql.AppendLine("IF @hostID IS NULL ");
            sql.AppendLine("BEGIN ");
            sql.AppendLine("	INSERT INTO hosts (");
            sql.AppendLine("        filtered_text, ");
            sql.AppendLine("		is_processed, ");
            sql.AppendLine("		text ");
            sql.AppendLine("	) VALUES (");
            sql.AppendLine("		@filteredText, ");
            sql.AppendLine("		0, ");
            sql.AppendLine("		@text ");
            sql.AppendLine("	) ");

            sql.AppendLine("	SET @hostID = (SELECT SCOPE_IDENTITY() AS [SCOPE_IDENTITY]) ");
            sql.AppendLine("END ");
            sql.AppendLine("ELSE SET @isNew = 0 ");

            // Return the host ID and "is new".
            sql.AppendLine("SELECT @hostID AS id, @isNew AS is_new ");

            DbRow? row = DbRow.selectOne(_dbConnectionString, sql.ToString(), parameters);
            if (row == null) { throw SmartException.create("An error occurred retrieving host data"); }

            int? hostID = row.get<int>("id");
            if (hostID == null) { throw SmartException.create("Unable to retrieve a host ID"); }

            isNew_ = row.get<bool>("is_new");

            return hostID.Value;
        }


        public async Task<Result> normalizeCommonNames() {

            var sql = CommonNameComparison.generatePartialQuery();

            List<CommonNameComparison>? comparisons = await UsefulObject.getAsyncList<CommonNameComparison>(_dbConnectionString, sql);
            if (comparisons == null || comparisons.Count == 0) { return Result.failure("No common names that differ by case were found"); }

            var errors = new List<string>();

            foreach (var comparison in comparisons) {

                if (string.IsNullOrEmpty(comparison.itisName) || string.IsNullOrEmpty(comparison.ncbiName)) {
                    errors.Add($"ITIS name = {comparison.itisName ?? "null"} and NCBI name = {comparison.ncbiName ?? "null"}");
                    continue;
                }

                var itisWords = comparison.itisName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var ncbiWords = comparison.ncbiName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (itisWords.Length != ncbiWords.Length) {
                    errors.Add($"ITIS name = {comparison.itisName ?? "null"} and NCBI name = {comparison.ncbiName ?? "null"} have different lengths");
                    continue;
                }

                var newWords = new List<string>();

                for (int w = 0; w < itisWords.Length && w < ncbiWords.Length; w++) {
                    var itisWord = itisWords[w];
                    var ncbiWord = ncbiWords[w];

                    char[] newChars = new char[itisWord.Length];
                    for (int c = 0; c < itisWord.Length && c < ncbiWord.Length; c++) {
                        var itisChar = itisWord[c];
                        var ncbiChar = ncbiWord[c];

                        if (char.IsUpper(itisChar) && char.IsUpper(ncbiChar)) {
                            newChars[c] = itisChar;
                        } else {
                            newChars[c] = char.ToLower(itisChar);
                        }
                    }

                    newWords.Add(new string(newChars));

                    /*
                    This works but only compares the first character of each word.
                    if (char.IsUpper(itisWord[0]) && char.IsUpper(ncbiWord[0])) {
                       newWords.Add(itisWord);
                    } else {
                       newWords.Add(itisWord[0].ToString().ToUpper() + itisWord.Substring(1));
                    }*/
                }

                var newName = string.Join(' ', newWords);

                var parameters = new List<SqlParameter>() {
                    Utils.createSqlParam("@itisID", SqlDbType.Int, comparison.itisTnId),
                    Utils.createSqlParam("@name", SqlDbType.NVarChar, newName),
                    Utils.createSqlParam("@ncbiID", SqlDbType.Int, comparison.ncbiTnId)
                };

                var updateSQL = @"
                    UPDATE taxon_name SET name = @name WHERE id = @itisID;
                    UPDATE taxon_name SET name = @name WHERE id = @ncbiID;
                ";

                try {
                    // Run the update queries
                    var result = await DbQueryManager.executeNonQueryAsync(_dbConnectionString, updateSQL, parameters);

                } catch (Exception exc_) {
                    errors.Add(exc_.Message);
                }
            }

            if (errors.Count > 0) {
                var message = $"{errors.Count} errors" + string.Join("\n", errors);
                return Result.failure(message);
            }

            return Result.success();
        }


        public async Task<Result<List<AnnotatedHost>?>> searchAnnotatedHosts(string? searchText_) {

            List<AnnotatedHost>? annotatedHosts = null;

            try {
                if (Utils.isEmptyElseTrim(ref searchText_) || searchText_!.Length < 3) {
                    throw new Exception("Please enter valid search text with at least 3 characters");
                }

                var parameters = new List<SqlParameter>() {
                    Utils.createSqlParam("@searchText", SqlDbType.NVarChar, searchText_)
                };

                var sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine(AnnotatedHost.generatePartialQuery());
                sql.AppendLine("WHERE host_text LIKE '%'+@searchText+'%' ");
                sql.AppendLine("ORDER BY host_text ");

                annotatedHosts = await UsefulObject.getAsyncList<AnnotatedHost>(_dbConnectionString, sql.ToString(), parameters);

            } catch (Exception exc_) {
                return Result<List<AnnotatedHost>?>.failure(exc_.Message);
            }

            return Result<List<AnnotatedHost>?>.success(annotatedHosts);
        }

        // Update a host after it has been processed.
        public void updateHostStatus(string? filteredText_, int hostID_, bool isValid_, string? messages_ = null) {

            // If the host is invalid, the filtered text might be empty, but if it's valid, the filtered text shouldn't be empty.
            if (isValid_ && string.IsNullOrEmpty(filteredText_)) { throw SmartException.create("Invalid filtered text"); }

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@filteredText", SqlDbType.NVarChar, filteredText_),
                Utils.createSqlParam("@messages", SqlDbType.NVarChar, messages_)
            };

            var sql = new StringBuilder();
            sql.AppendLine("UPDATE hosts SET ");
            sql.AppendLine("filtered_text = @filteredText, ");
            sql.AppendLine("is_processed = 1, ");
            sql.AppendLine($"is_valid = {(isValid_ ? 1 : 0)}, ");
            sql.AppendLine("messages = @messages, ");
            sql.AppendLine("processed_on = SYSDATETIMEOFFSET() ");
            sql.AppendLine($"WHERE id = {hostID_} ");

            DbQueryManager.update(_dbConnectionString, sql.ToString(), parameters);
        }


        /*
        // Update the host's "is valid" and (optional) "messages" attributes.
        public void updateIsValid(int hostID_, bool isValid_, string? messages_ = null) {

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@messages", SqlDbType.NVarChar, messages_)
            };

            var value = isValid_ ? 1 : 0;

            DbQueryManager.update(_dbConnectionString, $"UPDATE hosts SET is_valid = {value}, messages = @messages WHERE id = {hostID_} ", parameters);
        }*/

        /*
        // Populate the hosts filtered_text using its text value.
        public void filterHostTextByGroup(int groupID_) {

            var sql = @$"UPDATE hosts SET filtered_text =
                REPLACE(
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        REPLACE(
                                            REPLACE(
                                                REPLACE(
                                                    REPLACE(
                                                        REPLACE(
                                                            REPLACE(
                                                                REPLACE(lower(text), '''', '')
                                                            , '  ', ' ')
                                                        , '_', ' ')
                                                    , '""', '')
                                                , '`', '')
                                            , '!', '')
                                        , '.', '')
                                    , '?', '')
                                , '(', ',')
                            , ')', ',')
                        , ':', ',')
                    , ';', ',')
                , '-', ' ')

            where group_id = {groupID_} ";

            DbQueryManager.update(_dbConnectionString, sql);
        } */


    }
}
