
using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotation.Utilities;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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


        // Get all matching entries in the taxonomy databases provided whose names differ by case.
        public async Task<Result<List<CommonNameComparison>?>> getCommonNameComparisons(Terms.taxonomy_db taxDB1_, Terms.taxonomy_db taxDB2_) {

            var taxDB1 = Terms.enumFullKey(taxDB1_);
            var taxDB2 = Terms.enumFullKey(taxDB2_);

            if (taxDB1 == null || taxDB2 == null) {
                return Result<List<CommonNameComparison>?>.failure("One or both taxonomy db parameters is invalid");
            }

            List<CommonNameComparison>? comparisons = null;

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@taxDB1", SqlDbType.VarChar, taxDB1),
                Utils.createSqlParam("@taxDB2", SqlDbType.VarChar, taxDB2)
            };

            var sql = @"
                /* Lookup term IDs for name classes */
                DECLARE @blastNameTID INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = 'ncbi_name_class.blast_name')
                IF @blastNameTID IS NULL RAISERROR('Invalid BLAST name term ID', 18, 1)

                DECLARE @commonNameTID INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = 'ncbi_name_class.common_name')
                IF @commonNameTID IS NULL RAISERROR('Invalid common name term ID', 18, 1)

                DECLARE @genbankCommonNameTID INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = 'ncbi_name_class.genbank_common_name')
                IF @genbankCommonNameTID IS NULL RAISERROR('Invalid GenBank common name term ID', 18, 1)

                DECLARE @synonymTID INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = 'ncbi_name_class.synonym')
                IF @synonymTID IS NULL RAISERROR('Invalid synonym term ID', 18, 1)

                /* Lookup term IDs for taxonomy databases */
                DECLARE @tdb_1_TID INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = @taxDB1)
                IF @tdb_1_TID IS NULL RAISERROR('Invalid term ID for taxonomy DB 1', 18, 1)

                DECLARE @tdb_2_TID INT = (SELECT TOP 1 term_id FROM term WHERE term_full_key = @taxDB2)
                IF @tdb_2_TID IS NULL RAISERROR('Invalid term ID for taxonomy DB 2', 18, 1)

				SELECT 
					tn1.name AS name_1,
					tn1.id AS id_1,
					tn2.name AS name_2,
					tn2.id AS id_2
				FROM taxon_name tn1
				JOIN taxon_name tn2 ON (
					tn2.name = tn1.name
					AND tn2.name COLLATE LATIN1_GENERAL_CS_AS <> tn1.name COLLATE LATIN1_GENERAL_CS_AS
					AND tn2.name_class_tid IN (@blastNameTID, @commonNameTID, @genbankCommonNameTID, @synonymTID)
				)
				WHERE tn1.taxonomy_db_tid = @tdb_1_TID
				AND tn2.taxonomy_db_tid = @tdb_2_TID
				AND tn1.name_class_tid IN (@blastNameTID, @commonNameTID, @genbankCommonNameTID, @synonymTID)

				ORDER BY tn1.name
            ";

            try {
                // Get the common name comparison objects with names and taxon_name IDs from the specified taxonomy databases.
                comparisons = await UsefulObject.getAsyncList<CommonNameComparison>(_dbConnectionString, sql, parameters);
                if (comparisons == null || comparisons.Count == 0) {
                    return Result<List<CommonNameComparison>?>.success(null);
                }

            } catch (Exception exc_) {
                return Result<List<CommonNameComparison>?>.failure(exc_.Message);
            }
            
            return Result<List<CommonNameComparison>?>.success(comparisons);
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


        // Normalize all common names.
        public async Task<Result> normalizeAllCommonNames() {

            Result result;

            // Normalize names that start with "X " or contain " X " to lower case x's.
            result = await normalizeXNames();
            if (result.isFailure) {
                return result;
            }
            
            // NCBI and ITIS are the primary taxonomy databases and we will update common names in both.
            result = await normalizeCommonNames(Terms.taxonomy_db.ncbi, Terms.taxonomy_db.itis, true);
            if (result.isFailure) {
                return result;
            }

            // Make eBird consistent with ITIS.
            result = await normalizeCommonNames(Terms.taxonomy_db.ncbi, Terms.taxonomy_db.ebird, false);
            if (result.isFailure) {
                return result;
            }

            result = await normalizeCommonNames(Terms.taxonomy_db.itis, Terms.taxonomy_db.ebird, false);
            if (result.isFailure) {
                return result;
            }

            return Result.success();
        }

        // Normalize common names in these two taxonomy databases using name matches with different case.
        public async Task<Result> normalizeCommonNames(Terms.taxonomy_db taxDB1_, Terms.taxonomy_db taxDB2_, bool updateFirst_) {

            var result = await getCommonNameComparisons(taxDB1_, taxDB2_);
            if (result.isFailure) {
                return Result.failure(result.getErrorMessage("Invalid comparison objects"));
            } else if (result.value == null) {
                return Result.success();
            }

            var strTaxDB1 = Terms.enumString(taxDB1_);
            var strTaxDB2 = Terms.enumString(taxDB2_);

            List<CommonNameComparison>? comparisons = result.value;

            var errors = new List<string>();

            foreach (var comparison in comparisons) {

                if (string.IsNullOrEmpty(comparison.name1) || string.IsNullOrEmpty(comparison.name2)) {
                    errors.Add($"{strTaxDB1} name = {comparison.name1 ?? "null"} and {strTaxDB2} name = {comparison.name2 ?? "null"}");
                    continue;
                }
                if (comparison.id1 == null || comparison.id2 == null) {
                    errors.Add($"Invalid id value(s) for name = {comparison.name1}");
                    continue;
                }

                // Split the names into one or more words.
                var name1Words = comparison.name1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name2Words = comparison.name2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (name1Words.Length != name2Words.Length) {
                    errors.Add($"{strTaxDB1} name = {comparison.name1 ?? "null"} and {strTaxDB2} name = {comparison.name2 ?? "null"} have different lengths");
                    continue;
                }

                var newWords = new List<string>();

                for (int w = 0; w < name1Words.Length && w < name2Words.Length; w++) {
                    var name1Word = name1Words[w];
                    var name2Word = name2Words[w];

                    char[] newChars = new char[name1Word.Length];
                    for (int c = 0; c < name1Word.Length && c < name2Word.Length; c++) {
                        var itisChar = name1Word[c];
                        var ncbiChar = name2Word[c];

                        if (char.IsUpper(itisChar) && char.IsUpper(ncbiChar)) {
                            newChars[c] = itisChar;
                        } else {
                            newChars[c] = char.ToLower(itisChar);
                        }
                    }

                    newWords.Add(new string(newChars));
                }

                var newName = string.Join(' ', newWords);

                var parameters = new List<SqlParameter>() {
                    Utils.createSqlParam("@newName", SqlDbType.NVarChar, newName),
                    Utils.createSqlParam("@id1", SqlDbType.Int, comparison.id1 ?? (object?)DBNull.Value),
                    Utils.createSqlParam("@id2", SqlDbType.Int, comparison.id2)
                };

                var updateSQL = new StringBuilder();
                updateSQL.AppendLine("UPDATE taxon_name SET name = @newName WHERE @id2 IS NOT NULL AND id = @id2;");
                if (updateFirst_) {
                    updateSQL.AppendLine("UPDATE taxon_name SET name = @newName WHERE @id1 IS NOT NULL AND id = @id1;");
                }
                
                try {
                    // Run the update queries
                    var updateResult = await DbQueryManager.executeNonQueryAsync(_dbConnectionString, updateSQL.ToString(), parameters);

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


        public async Task<Result> normalizeXNames() {

            List<CommonNameComparison>? comparisons = null;
            string sql;

            // Names that start with "X "
            sql = @"
                SELECT tn.id AS id_1, tn.name AS name_1
                FROM taxon_name tn
                WHERE tn.name COLLATE LATIN1_GENERAL_CS_AS LIKE 'X %'
            ";

            comparisons = await UsefulObject.getAsyncList<CommonNameComparison>(_dbConnectionString, sql);
            if (comparisons != null && comparisons.Count > 0) {
                foreach (var comparison in comparisons) {
                    if (comparison.id1 == null || comparison.name1 == null) { continue; }

                    var newName = string.Concat("x ", comparison.name1.AsSpan(2));

                    var parameters = new List<SqlParameter>() {
                        Utils.createSqlParam("@id", SqlDbType.Int, comparison.id1),
                        Utils.createSqlParam("@newName", SqlDbType.NVarChar, newName)
                    };

                    var updateSQL = "UPDATE taxon_name SET name = @newName WHERE id = @id ";

                    var updateResult = await DbQueryManager.executeNonQueryAsync(_dbConnectionString, updateSQL, parameters);
                }
            }

            // Names that contain " x "
            sql = @"
                SELECT tn.id AS id_1, tn.name AS name_1
                FROM taxon_name tn
                WHERE tn.name COLLATE LATIN1_GENERAL_CS_AS LIKE '% X %'
            ";

            comparisons = await UsefulObject.getAsyncList<CommonNameComparison>(_dbConnectionString, sql);
            if (comparisons != null && comparisons.Count > 0) {
                foreach (var comparison in comparisons) {
                    if (comparison.id1 == null || comparison.name1 == null) { continue; }

                    var newName = comparison.name1.Replace(" X ", " x ");

                    var parameters = new List<SqlParameter>() {
                        Utils.createSqlParam("@id", SqlDbType.Int, comparison.id1),
                        Utils.createSqlParam("@newName", SqlDbType.NVarChar, newName)
                    };

                    var updateSQL = "UPDATE taxon_name SET name = @newName WHERE id = @id ";

                    var updateResult = await DbQueryManager.executeNonQueryAsync(_dbConnectionString, updateSQL, parameters);
                }
            }

            return Result.success();
        }


        /*
        public async Task<Result> normalizeCommonNames(Terms.taxonomy_db taxDB1_, Terms.taxonomy_db taxDB2_) {

            var sql = CommonNameComparison.generatePartialQuery();

            // Get the common name comparison objects with names and taxon_name IDs for NCBI, ITIS, and eBird.
            List<CommonNameComparison>? comparisons = await UsefulObject.getAsyncList<CommonNameComparison>(_dbConnectionString, sql);
            if (comparisons == null || comparisons.Count == 0) { return Result.failure("No common names that differ by case were found"); }

            var errors = new List<string>();

            foreach (var comparison in comparisons) {

                if (string.IsNullOrEmpty(comparison.itisName) || string.IsNullOrEmpty(comparison.ncbiName)) {
                    errors.Add($"ITIS name = {comparison.itisName ?? "null"} and NCBI name = {comparison.ncbiName ?? "null"}");
                    continue;
                }

                // Split the ITIS and NCBI names into one or more words.
                var itisWords = comparison.itisName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var ncbiWords = comparison.ncbiName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                string[]? ebirdWords = null;
                if (!string.IsNullOrEmpty(comparison.ebirdName)) {
                    ebirdWords = comparison.ebirdName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                }

                if (itisWords.Length != ncbiWords.Length) {
                    errors.Add($"ITIS name = {comparison.itisName ?? "null"} and NCBI name = {comparison.ncbiName ?? "null"} have different lengths");
                    continue;
                }

                // TODO: should we compare a non-null ebird name's length?

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

                    
                    //This works but only compares the first character of each word.
                    //if (char.IsUpper(itisWord[0]) && char.IsUpper(ncbiWord[0])) {
                    //   newWords.Add(itisWord);
                    //} else {
                    //   newWords.Add(itisWord[0].ToString().ToUpper() + itisWord.Substring(1));
                    //}
                }

                var newName = string.Join(' ', newWords);

                var parameters = new List<SqlParameter>() {
                    Utils.createSqlParam("@ebirdID", SqlDbType.Int, comparison.ebirdTnId ?? (object?)DBNull.Value),
                    Utils.createSqlParam("@itisID", SqlDbType.Int, comparison.itisTnId),
                    Utils.createSqlParam("@name", SqlDbType.NVarChar, newName),
                    Utils.createSqlParam("@ncbiID", SqlDbType.Int, comparison.ncbiTnId)
                };

                var updateSQL = @"
                    UPDATE taxon_name SET name = @name WHERE id = @itisID;
                    UPDATE taxon_name SET name = @name WHERE id = @ncbiID;

                    IF @ebirdID IS NOT NULL UPDATE taxon_name SET name = @name WHERE id = @ebirdID;
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
        }*/


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
