
using System.Data;
using System.Data.SqlClient;

namespace HostAnnotation.Utilities {

    public class DbQueryManager {

        // Execute a non-query that returns no results.
        public async static Task<int?> executeNonQueryAsync(string dbConnectionString_, string? query_, List<SqlParameter>? parameters_ = null) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            int? result = null;

            using (var dbConnection = new SqlConnection(dbConnectionString_))
            using (var dbCommand = new SqlCommand(query_, dbConnection)) {

                try {
                    await dbConnection.OpenAsync();

                    // Add any SQL parameters that were provided.
                    if (parameters_ != null && parameters_.Count > 0) {
                        dbCommand.Parameters.AddRange([.. parameters_]);
                    }

                    result = await dbCommand.ExecuteNonQueryAsync();
                } finally {

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    dbCommand.Parameters.Clear();

                    await dbConnection.CloseAsync();
                }
            }

            return result;
        }

        // Execute a query and return a single-value result.
        public static T? getSingleValue<T>(string dbConnectionString_, string? query_, List<SqlParameter>? parameters_ = null) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            T? result = default;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) 
            using (SqlCommand dbCommand = new SqlCommand(query_, dbConnection)) {

                try {

                    dbConnection.Open();

                    // Add any SQL parameters that were provided.
                    if (parameters_ != null && parameters_.Count > 0) {
                        foreach (SqlParameter parameter in parameters_) { dbCommand.Parameters.Add(parameter); }
                    }

                    object testObject = dbCommand.ExecuteScalar();
                    if (testObject != null && !testObject.GetType().Equals(typeof(DBNull))) {
                        result = (T)testObject;
                    }
                }
                finally {

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    dbCommand.Parameters.Clear();

                    dbConnection.Close(); 
                }
            }

            return result;
        }

        #region Variations on "getSingleValue"

        // Perform a DELETE query and return the number of results.
        public static int delete(string dbConnectionString_, string? query_, List<SqlParameter>? parameters_ = null) {
            return getSingleValue<int>(dbConnectionString_, query_, parameters_);
        }

        // Execute a SQL query that returns a single integer.
        public static int getIntResult(string dbConnectionString_, string? query_, List<SqlParameter>? parameters_ = null) {
            return getSingleValue<int>(dbConnectionString_, query_, parameters_);
        }

        // Perform an INSERT query that might return the unique integer ID of the new row.
        // NOTE: to return the newly-created ID, add this to the end of the SQL: "SELECT SCOPE_IDENTITY() AS [SCOPE_IDENTITY]; "
        public static void insert(string dbConnectionString_, string? query_, ref int? result_, List<SqlParameter>? parameters_ = null) {
            result_ = getSingleValue<int>(dbConnectionString_, query_, parameters_);
        }

        // Perform an SQL UPDATE
        public static int update(string dbConnectionString_, string? query_, List<SqlParameter>? parameters_ = null) {
            return getSingleValue<int>(dbConnectionString_, query_, parameters_);
        }

        #endregion



        // Execute the SQL query (using query parameters) and don't return any results.
        public async static Task<Dictionary<string, object?>?> runStoredProcedureAsync(
            string dbConnectionString_, string? procedureName_, List<SqlParameter>? parameters_ = null) {

            Dictionary<string, object?>? results = null;

            using (var connection = new SqlConnection(dbConnectionString_))
            using (var cmd = new SqlCommand(procedureName_, connection)) {

                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters_ != null && parameters_.Count > 0) {
                    cmd.Parameters.AddRange([.. parameters_]);
                }

                try {
                    await connection.OpenAsync();

                    int result = await cmd.ExecuteNonQueryAsync();

                    // Are there any output parameters?
                    foreach (SqlParameter parameter in cmd.Parameters) {

                        if (parameter.Direction != ParameterDirection.Output) { continue; }
                        if (results == null) { 
                            results = []; 
                        }

                        object? value = parameter.Value;
                        if (value.GetType().Equals(typeof(DBNull))) { value = null; }

                        results.Add(parameter.ParameterName, value);
                    }
                }
                catch (Exception exc_) {
                    throw SmartException.create(exc_.Message);
                }
                finally {

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    cmd.Parameters.Clear();

                    connection.Close();
                }
            }

            return results;
        }


        #region Code graveyard


        /*
        // Execute the SQL query (using query parameters) and don't return any results.
        public static void runStoredProcedure(string dbConnectionString_, string? procedureName_, List<SqlParameter>? parameters_ = null) {

            using (SqlConnection connection = new SqlConnection(dbConnectionString_))
            using (SqlCommand cmd = new SqlCommand(procedureName_, connection)) {

                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters_ != null && parameters_.Count > 0) {

                    foreach (SqlParameter parameter in parameters_) {
                        cmd.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
                    }
                }

                try {
                    connection.Open();

                    int result = cmd.ExecuteNonQuery();
                }
                //catch (Exception exc_) {
                //    throw SmartException.create(exc_.Message);
                //}
                finally {

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    cmd.Parameters.Clear();

                    connection.Close();
                }
            }
        }*/


        /*
        
        // This object maintains the results of a SQL query as a Dictionary of (string) key/value pairs
        public class ResultRow {

            // The key/value pairs
            protected Dictionary<string, string>? _values = null;

            // C-tor
            public ResultRow() {
                _values = new Dictionary<string, string>();
            }

            // Add a value to the row object
            public void addValue(string columnName_, string value_) {

                // Validate the column name
                if (string.IsNullOrEmpty(columnName_)) { throw new Exception("Invalid column name in addValue()"); }

                // Remove any existing instances of this key/value
                if (_values!.ContainsKey(columnName_)) { _values.Remove(columnName_); }

                // Make sure we aren't trying to add a NULL value
                if (value_ == null) {
                    _values.Add(columnName_, "");
                } else {
                    _values.Add(columnName_, value_);
                }
            }

            // The primary method that gets a string value from the collection of key/value pairs. This is called by other methods
            // depending on the data type that needs to be returned.
            public bool getValue(string columnName_, bool isRequired_, ref string? value_) {

                // Default the result value
                value_ = null;

                // If no column name was provided, this is an exception.
                if (string.IsNullOrEmpty(columnName_)) { throw new Exception("Empty column name in getValue()"); }

                // If no value was found to correspond to this column name, fail gracefully.
                if (!_values!.ContainsKey(columnName_)) { return false; }

                // Get the string value and validate, if required. 
                value_ = _values[columnName_];
                if (string.IsNullOrEmpty(value_) && isRequired_) {
                    return false;
                } else {
                    return true;
                }
            }


            #region Variations on getValue() to return the value as the "original" value (not just a string).

            // Decimal
            public bool getValue(string columnName_, bool isRequired_, ref Decimal value_) {

                value_ = 0;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    return Decimal.TryParse(strValue, out value_);
                }

                return false;
            }

            // Double
            public bool getValue(string columnName_, bool isRequired_, ref double value_) {

                value_ = 0.0d;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    return double.TryParse(strValue, out value_);
                } else {
                    return false;
                }
            }

            // Float
            public bool getValue(string columnName_, bool isRequired_, ref float value_) {

                value_ = 0.0f;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    return float.TryParse(strValue, out value_);
                } else {
                    return false;
                }
            }

            // Integer
            public bool getValue(string columnName_, bool isRequired_, ref int value_) {

                value_ = -1;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    return int.TryParse(strValue, out value_);
                } else {
                    return false;
                }
            }

            // Nullable<double>
            public bool getValue(string columnName_, bool isRequired_, ref Nullable<double> value_) {

                value_ = null;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    double test = 0.0d;
                    if (double.TryParse(strValue, out test)) {
                        value_ = test;
                        return true;
                    }
                }

                return false;
            }

            // Nullable<float>
            public bool getValue(string columnName_, bool isRequired_, ref Nullable<float> value_) {

                value_ = null;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    float test = 0.0f;
                    if (float.TryParse(strValue, out test)) {
                        value_ = test;
                        return true;
                    }
                }

                return false;
            }

            // Nullable<int>
            public bool getValue(string columnName_, bool isRequired_, ref Nullable<int> value_) {

                value_ = null;
                string strValue = null;

                if (getValue(columnName_, isRequired_, ref strValue)) {
                    int test = -1;
                    if (int.TryParse(strValue, out test)) {
                        value_ = test;
                        return true;
                    }
                }

                return false;
            }

            #endregion

        }


        // Create an "alias" for a list of ResultRow objects
        public class ResultRows : List<ResultRow?> { }

        // TEST
        public delegate object ProcessQueryResults(SqlDataReader dataReader_, object helper_);

        // Execute a query and return an integer result.
        public static void executeScalar(string dbConnectionString_, string query_, ref int result_) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            result_ = -1;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                // Create a SQL command using the query text and db connection.
                using (SqlCommand dbCommand = new SqlCommand(query_, dbConnection)) {
                    object testObject = dbCommand.ExecuteScalar();
                    if (testObject == null || testObject.GetType().Equals(typeof(DBNull))) {
                        result_ = -1;
                    } else { 
                        result_ = Convert.ToInt32(testObject);
                    }
                }
            }
        }

    
        // dmd 062019: I'm grooming this to be the "Swiss army knife" that processes SQL query results, processes the results, and returns something.
        public static object executeQuery(string dbConnectionString_, object helper_, IEnumerable<SqlParameter> parameters_, ProcessQueryResults processResults_, string query_, CommandBehavior? commandBehavior_ = null) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            object results = null;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                // Create a SQL command using the query text and db connection.
                using (SqlCommand dbCommand = new SqlCommand(query_, dbConnection)) {

                    if (parameters_ != null) {
                        foreach (SqlParameter parameter in parameters_) { dbCommand.Parameters.Add(parameter); }
                    }

                    if (commandBehavior_ == null) { commandBehavior_ = CommandBehavior.Default; }

                    using (SqlDataReader reader = dbCommand.ExecuteReader(commandBehavior_.Value)) {
                        if (!reader.IsClosed && reader.HasRows) { results = processResults_(reader, helper_); }
                    }

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    dbCommand.Parameters.Clear();
                }
            }

            return results;
        } 

      
        // Return the results of an SQL SELECT query.
        public static void getSelectResults(string dbConnectionString_, string query_, ref ResultRows rows_) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            SqlDataAdapter dataAdaptor;
            SqlConnection dbConnection = null;
            DataTable dataTable = new DataTable();

            // Initialize the List of row objects to return (each one is a Dictionary of string key/value pairs).
            rows_ = new ResultRows();

            try {
                // Initialize and open the database connection
                dbConnection = new SqlConnection(dbConnectionString_);
                dbConnection.Open();

                // Create a data adaptor using the query text and db connection.
                dataAdaptor = new SqlDataAdapter(query_, dbConnection);
                dataAdaptor.Fill(dataTable);

                // Validate the .NET data table and its rows
                if (dataTable != null && dataTable.Rows.Count > 0) {
                    foreach (DataRow dataRow in dataTable.Rows) {

                        if (dataRow == null) { throw SmartException.create("Invalid row (null)"); }

                        // A Dictionary of string key/value pairs
                        ResultRow row = new ResultRow();

                        // Iterate thru each .NET column object in the data table
                        foreach (DataColumn column in dataTable.Columns) {

                            if (column == null) { throw SmartException.create("Invalid data column"); }

                            string textValue = "";

                            if (!dataRow.IsNull(column.ColumnName)) {
                                object value = dataRow[column.ColumnName];
                                if (value != null) { textValue = value.ToString(); }
                            }

                            // Update the result row
                            row.addValue(column.ColumnName, textValue);
                        }

                        // Update the collection of row data that will be returned
                        rows_.Add(row);
                    }
                }
            }
            catch (Exception exc) {
                throw exc;
            }
            finally {
                if (dbConnection != null && dbConnection.State != ConnectionState.Closed) { dbConnection.Close(); }
            }
        }

         // Execute a query and return the number of rows affected.
        public static void executeNonQuery(string dbConnectionString_, string query_, ref int resultCount_) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            resultCount_ = 0;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                // Create a SQL command using the query text and db connection.
                using (SqlCommand dbCommand = new SqlCommand(query_, dbConnection)) {
                    resultCount_ = dbCommand.ExecuteNonQuery();
                }
            }
        }

        // Return the results of an SQL SELECT query that uses SQL parameters.
        public static void getSelectResults(string dbConnectionString_, List<SqlParameter> parameters_, string query_, 
            ref ResultRows rows_) {
            
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }
            
            // Initialize the List of row objects to return (each one is a Dictionary of string key/value pairs).
            rows_ = new ResultRows();

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                using (SqlCommand dbCommand = new SqlCommand(query_, dbConnection)) {

                    // Add any SQL parameters that were provided.
                    if (parameters_ != null && parameters_.Count > 0) {
                        foreach (SqlParameter parameter in parameters_) { dbCommand.Parameters.Add(parameter); }
                    }

                    using (SqlDataReader reader = dbCommand.ExecuteReader(CommandBehavior.Default)) {

                        while (reader.Read()) {

                            // A Dictionary of string key/value pairs
                            ResultRow row = new ResultRow();

                            for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex += 1) {

                                string textValue = "";

                                if (!reader.IsDBNull(columnIndex)) {
                                    object columnValue = reader.GetValue(columnIndex);
                                    if (columnValue != null) { textValue = columnValue.ToString(); }
                                }

                                // Get the column name
                                string columnName = reader.GetName(columnIndex);

                                // Update the result row
                                row.addValue(columnName, textValue);
                            }

                            // Update the collection of row data that will be returned
                            rows_.Add(row);
                        }
                    }

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    dbCommand.Parameters.Clear();
                }
            }
        } 

        public static T getSingleResult<T>(string dbConnectionString_, List<SqlParameter> parameters_, string query_) {

            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            // Default the output parameter
            T result = default(T);

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                // Create a SQL command using the query text and db connection.
                using (SqlCommand dbCommand = new SqlCommand(query_, dbConnection)) {

                    if (parameters_ != null) {
                        foreach (SqlParameter parameter in parameters_) { dbCommand.Parameters.Add(parameter); }
                    }

                    object testObject = dbCommand.ExecuteScalar();
                    if (testObject == null || testObject.GetType().Equals(typeof(DBNull))) {
                        result = default(T);
                    } else {
                        result = (T)testObject;
                    }

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    dbCommand.Parameters.Clear();
                }
            }

            return result;
        }


        public static T getSingleResult<T>(string dbConnectionString_, string query_) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            // Default the output parameter
            T result = default(T);
            
            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                using (SqlDataAdapter dataAdaptor = new SqlDataAdapter(query_, dbConnection)) {

                    DataTable dataTable = new DataTable();
                    dataAdaptor.Fill(dataTable);

                    // Validate the .NET data table and its rows
                    if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0) {

                        DataRow dataRow = dataTable.Rows[0];
                        if (dataRow != null) {
                            object testResult = dataRow.ItemArray.GetValue(0);
                            if (testResult == null || testResult.GetType().Equals(typeof(DBNull))) {
                                result = default(T);
                            } else {
                                result = dataRow.Field<T>(0);
                            }
                        }
                    }
                }
            }

            return result;
        }


        // The primary method that gets a single result from a SQL query. This is called by other methods depending on the data type that needs to be returned.
        public static bool getSingleValueResult(string dbConnectionString_, string query_, ref object result_) {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            // Default the output parameter
            result_ = null;

            bool foundResult = false;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                using (SqlDataAdapter dataAdaptor = new SqlDataAdapter(query_, dbConnection)) {

                    DataTable dataTable = new DataTable();
                    dataAdaptor.Fill(dataTable);

                    // Validate the .NET data table and its rows
                    if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0) {

                        DataRow dataRow = dataTable.Rows[0];
                        if (dataRow != null) {
                            result_ = dataRow.ItemArray.GetValue(0);
                            if (result_ == null || result_.GetType().Equals(typeof(DBNull))) {
                                result_ = null;
                            } else {
                                foundResult = true;
                            }
                        }
                    }
                }
            }

            return foundResult;
        }
        
         public static List<Dictionary<string, object>> selectRows(string dbConnectionString_, List<SqlParameter> parameters_, string? query_) {

            string? query = query_;
            if (Utils.isEmptyElseTrim(ref query)) { throw SmartException.create("Invalid query (empty)"); }

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();
                
                using (SqlCommand dbCommand = new SqlCommand(query, dbConnection)) {

                    // Add any SQL parameters that were provided.
                    if (parameters_ != null && parameters_.Count > 0) {
                        foreach (SqlParameter parameter in parameters_) { dbCommand.Parameters.Add(parameter); }
                    }

                    using (SqlDataReader reader = dbCommand.ExecuteReader(CommandBehavior.Default)) {

                        while (reader.Read()) {

                            Dictionary<string, object> row = new Dictionary<string, object>();

                            for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex += 1) {

                                // Get the column name
                                string columnName = reader.GetName(columnIndex);

                                // Get the value and replace DBNull with null.
                                object columnValue = reader.GetValue(columnIndex);
                                if (columnValue != null && columnValue.GetType().Equals(typeof(DBNull))) { columnValue = null; }

                                row.Add(columnName, columnValue);
                            }

                            if (row.Count > 0) { rows.Add(row); }
                        }
                    }

                    // Explicitly clear the parameters to avoid an error that can occur the next time a SqlCommand with
                    // parameters is executed: "The SqlParameter is already contained by another SqlParameterCollection."
                    dbCommand.Parameters.Clear();
                }
            }

            return rows;
        }

        // Execute a SELECT query and return the results as a list of ResultRow objects.
        public static ResultRows selectRows(string dbConnectionString_, string query_) {

            string query = query_;
            if (Utils.isEmptyElseTrim(ref query)) { throw SmartException.create("Invalid query (empty)"); }

            ResultRows rows = new ResultRows();

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                using (SqlDataAdapter dataAdaptor = new SqlDataAdapter(query, dbConnection)) {

                    DataTable dataTable = new DataTable();
                    dataAdaptor.Fill(dataTable);

                    // Validate the .NET data table and its rows
                    if (dataTable != null && dataTable.Rows.Count > 0) {

                        foreach (DataRow dataRow in dataTable.Rows) {

                            if (dataRow == null) { throw SmartException.create("Invalid row (null)"); }

                            // A Dictionary of string key/value pairs
                            ResultRow row = new ResultRow();

                            // Iterate thru each .NET column object in the data table
                            foreach (DataColumn column in dataTable.Columns) {

                                if (column == null) { throw SmartException.create("Invalid data column"); }

                                string textValue = "";

                                if (!dataRow.IsNull(column.ColumnName)) {
                                    object value = dataRow[column.ColumnName];
                                    if (value != null) { textValue = value.ToString(); }
                                }

                                // Update the result row
                                row.addValue(column.ColumnName, textValue);
                            }

                            // Update the collection of row data that will be returned
                            rows.Add(row);
                        }
                    }
                }
            }

            return rows;
        }


        // Execute a SELECT query and return a single ResultRow object.
        public static ResultRow? selectRow(string dbConnectionString_, string? query_) {

            string? query = query_;
            if (Utils.isEmptyElseTrim(ref query)) { throw SmartException.create("Invalid query (empty)"); }

            ResultRow? row = null;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                using (SqlDataAdapter dataAdaptor = new SqlDataAdapter(query, dbConnection)) {

                    DataTable dataTable = new DataTable();
                    dataAdaptor.Fill(dataTable);

                    // Validate the .NET data table and its rows
                    if (dataTable == null || dataTable.Rows.Count < 1) { return null; }

                    DataRow dataRow = dataTable.Rows[0];
                    if (dataRow == null) { throw SmartException.create("Invalid row (null)"); }

                    // A Dictionary of string key/value pairs
                    row = new ResultRow();

                    // Iterate thru each .NET column object in the data table
                    foreach (DataColumn column in dataTable.Columns) {

                        if (column == null) { throw SmartException.create("Invalid data column"); }

                        string textValue = "";

                        if (!dataRow.IsNull(column.ColumnName)) {
                            object value = dataRow[column.ColumnName];
                            if (value != null) { textValue = value.ToString(); }
                        }

                        // Update the result row
                        row.addValue(column.ColumnName, textValue!);
                    }
                }
            }

            return row;
        }

        
        public static Dictionary<string, object>? selectRow(string dbConnectionString_, List<SqlParameter> parameters_, string? query_) {

            string? query = query_;
            if (Utils.isEmptyElseTrim(ref query)) { throw SmartException.create("Invalid query (empty)"); }

            Dictionary<string, object>? row = null;

            List<Dictionary<string, object>> rows = selectRows(dbConnectionString_, parameters_, query);
            if (rows != null && rows.Count > 0) { row = rows[0]; }

            return row;
        }
         
         
         */

        #endregion

    }
}
