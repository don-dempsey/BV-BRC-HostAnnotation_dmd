
using HostAnnotation.Utilities;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace HostAnnotation.Utilities {

    // A base class for objects that can be easily created and populated from SQL.
    public class UsefulObject : IUsefulObject {

        // This allows object properties to be accessed by name. Example: MyObject["label"] = "This is the label"
        public object? this[string propertyName_] {

            get {
                // Validate the property name.
                if (string.IsNullOrEmpty(propertyName_)) { throw new ArgumentException("Invalid property name (empty)"); }

                PropertyInfo? property = GetType().GetProperty(propertyName_);
                if (property == null) { throw new Exception($"Unrecognized property {propertyName_}"); }

                return property.GetValue(this, null);
            }

            set {
                // Validate the property name.
                if (string.IsNullOrEmpty(propertyName_)) { throw new ArgumentException("Invalid property name (empty)"); }

                // Get and validate the property with this name.
                PropertyInfo? property = GetType().GetProperty(propertyName_);
                if (property == null) { throw new Exception($"Unrecognized property {propertyName_}"); }

                // The value after conversion (if conversion is necessary).
                object? convertedValue;

                Type propertyType = property.PropertyType;

                bool isNullable = !propertyType.IsValueType; // reference types are always nullable

                // If the property type is a Nullable object, we need to do this to determine its type.
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    Type[] genericArguments = propertyType.GetGenericArguments();
                    if (genericArguments != null && genericArguments.Length > 0 && genericArguments[0] != null) {
                        propertyType = genericArguments[0];
                        isNullable = true;
                    }
                }

                if (value == null || value.GetType().Equals(typeof(DBNull))) {

                    // Provide a default value for null value types.
                    convertedValue = isNullable ? null : Activator.CreateInstance(propertyType);

                } else if (propertyType.IsEnum) {

                    // Cast the value as a string and then try to parse as the appropriate enum.
                    convertedValue = Enum.Parse(propertyType, (string)value);

                } else if (propertyType.Equals(typeof(bool)) && value.GetType().Equals(typeof(int))) {

                    // If the value for a bool is provided as an integer, convert it.
                    convertedValue = (int)value == 1;

                } else if (propertyType.Equals(typeof(Guid))) {

                    convertedValue = value.GetType().Equals(typeof(Guid)) ? (Guid)value : new Guid(value.ToString()!);

                } else {
                    convertedValue = value;
                }

                // Set the property's value using the converted value.
                property.SetValue(this, convertedValue);
            }
        }

        // Create an object of the specified type using the type's UsefulAttributes and a SQL data reader.
        public static TUsefulObject? create<TUsefulObject>(
            List<UsefulAttribute> attributes_, SqlDataReader reader_)
            where TUsefulObject : IUsefulObject, new() {

            // Keep track of how many properties are updated.
            int propertyUpdates = 0;

            // Create an instance of the generic type.
            TUsefulObject? usefulObject = Activator.CreateInstance<TUsefulObject>();

            foreach (UsefulAttribute attribute in attributes_) {

                object? columnValue = null;

                try {
                    // Try to get this column's value from the row.
                    columnValue = reader_[attribute.columnName];
                } catch (Exception) {

                    // If this is a required field and it's missing from the row, raise an exception.
                    if (attribute.isRequired) { throw new Exception($"Column {attribute.columnName} was not found"); }
                }

                // Replace DBNull with null.
                if (columnValue != null && columnValue.GetType().Equals(typeof(DBNull))) { columnValue = null; }

                // If this is a required field and it's empty, raise an exception.
                if (attribute.isRequired && columnValue == null) {
                    throw new Exception($"Invalid value for column {attribute.columnName}");
                }

                // Set the Useful Object property with this value.
                usefulObject[attribute.propertyName!] = columnValue!;

                // Increment the number of updated properties.
                propertyUpdates += 1;
            }

            if (propertyUpdates < 1) {
                usefulObject = default;
            } else {
                usefulObject.process();
            }

            return usefulObject;
        }

        // Execute the SQL query using SQL parameters (if provided) and populate the result object.
        public static async Task<TUsefulObject?> getAsync<TUsefulObject>(
            string dbConnectionString_, string query_,
            List<SqlParameter>? parameters_ = null)
            where TUsefulObject : IUsefulObject, new() {

            // Get all attributes defined on the UsefulObject.
            List<UsefulAttribute>? attributes = getAttributes<TUsefulObject?>();
            if (attributes == null || attributes.Count < 1) { throw new InvalidDataException("Invalid UsefulAttributes"); }

            // Instantiate the object to be returned.
            TUsefulObject? result = default;

            using (var dbConnection = new SqlConnection(dbConnectionString_))
            using (var command = new SqlCommand(query_, dbConnection)) {

                await dbConnection.OpenAsync();

                // Add any SQL parameters that were provided.
                if (parameters_ != null && parameters_.Count > 0) {
                    command.Parameters.AddRange([.. parameters_]);
                }

                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow)) {
                    if (reader.Read()) {
                        result = create<TUsefulObject>(attributes, reader);
                    }
                }
            }

            return result;
        }

        // Execute the SQL query using SQL parameters (if provided) and populate the result object(s).
        public static async Task<List<TUsefulObject>?> getAsyncList<TUsefulObject>(string dbConnectionString_,
            string query_, List<SqlParameter>? parameters_ = null, int? commandTimeout_ = null)
            where TUsefulObject : IUsefulObject, new() {

            // Get all attributes defined on the UsefulObject.
            List<UsefulAttribute>? attributes = getAttributes<TUsefulObject?>();
            if (attributes == null || attributes.Count < 1) { throw new Exception("Invalid Useful Attributes"); }

            // Instantiate the collection of objects to be returned.
            List<TUsefulObject>? results = null;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_))
            using (SqlCommand command = new SqlCommand(query_, dbConnection)) {

                await dbConnection.OpenAsync();

                // Add any SQL parameters that were provided.
                if (parameters_ != null && parameters_.Count > 0) {
                    command.Parameters.AddRange([.. parameters_]);
                }

                // If a (non-negative and non-null) command timeout parameter was provided, add it to the command.
                if (commandTimeout_ != null && commandTimeout_.Value > 0) {
                    command.CommandTimeout = commandTimeout_!.Value;
                }

                using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.Default)) {

                    // Instantiate the collection of objects to be returned.
                    results = new List<TUsefulObject>();

                    while (await reader.ReadAsync()) {

                        // Create the UsefulObject using the UsefulAttributes and data reader.
                        TUsefulObject? usefulObject = create<TUsefulObject>(attributes, reader);

                        // If the object is valid, add it to the collection of results.
                        if (usefulObject != null) { results.Add(usefulObject); }
                    }
                }
            }

            return results;
        }

        // Use reflection on the UsefulObject to find "UsefulAttribute" custom attributes assigned to properties.
        protected static List<UsefulAttribute>? getAttributes<TUsefulObject>() {

            var attributes = new List<UsefulAttribute>();

            Type type = typeof(TUsefulObject);

            foreach (PropertyInfo propertyInfo in type.GetProperties()) {

                // Does this property have a UsefulAttribute?
                UsefulAttribute? usefulAttribute = propertyInfo.GetCustomAttribute<UsefulAttribute>(true);
                if (usefulAttribute == null) { continue; }

                // Set the property name.
                usefulAttribute.propertyName = propertyInfo.Name;

                // Update the results.
                attributes.Add(usefulAttribute);
            }

            return attributes;
        }

        // Insert the object into the database and return the new identity value (if any).
        // This was created by Claude.ai on 05/27/26.
        public static async Task<int?> insertAsync<TUsefulObject>(
            string dbConnectionString_, TUsefulObject obj_, string tableName_)
            where TUsefulObject : IUsefulObject, new() {

            var attributes = getAttributes<TUsefulObject>()!
                .Where(a => a.isInsertable && !a.isIdentity).ToList();

            var columns = string.Join(", ", attributes.Select(a => a.columnName));
            var paramNames = string.Join(", ", attributes.Select(a => "@" + a.columnName));
            var sql = $"INSERT INTO {tableName_} ({columns}) VALUES ({paramNames}); SELECT SCOPE_IDENTITY();";

            var parameters = attributes.Select(a =>
                new SqlParameter("@" + a.columnName, obj_[a.propertyName!] ?? DBNull.Value)).ToList();

            using var conn = new SqlConnection(dbConnectionString_);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        // Update the object in the database and return the number of rows affected.
        // This was created by Claude.ai on 05/27/26.
        public static async Task<int> updateAsync<TUsefulObject>(
            string dbConnectionString_, string keyColumnName_, TUsefulObject obj_, string tableName_)
            where TUsefulObject : IUsefulObject, new() {

            var attributes = getAttributes<TUsefulObject>()!;
            var keyAttr = attributes.First(a => a.columnName == keyColumnName_);
            var updateAttrs = attributes.Where(a => a.isUpdateable && a.columnName != keyColumnName_).ToList();

            var setClauses = string.Join(", ", updateAttrs.Select(a => $"{a.columnName} = @{a.columnName}"));
            var sql = $"UPDATE {tableName_} SET {setClauses} WHERE {keyColumnName_} = @{keyColumnName_}";

            var parameters = updateAttrs.Select(a =>
                new SqlParameter("@" + a.columnName, obj_[a.propertyName!] ?? DBNull.Value))
                .Append(new SqlParameter("@" + keyColumnName_, obj_[keyAttr.propertyName!]!))
                .ToList();

            using var conn = new SqlConnection(dbConnectionString_);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // This can be overridden to perform processing after a UsefulObject has been populated via properties.
        public virtual void process() { }



        #region Methods from the old version (still used in this project)

        // Execute the SQL query (using query parameters) and use the results to populate a collection of (useful) objects.
        public static void get<TUsefulObject>(string dbConnectionString_, List<SqlParameter> parameters_, string query_,
            ref List<TUsefulObject>? results_, int? commandTimeout_ = null)
            where TUsefulObject : IUsefulObject, new() {

            // Get any Useful Attributes defined by the generic type.
            List<UsefulAttribute>? attributes = getAttributes<TUsefulObject?>();
            if (attributes == null || attributes.Count < 1) { throw SmartException.create("Invalid Useful Attributes"); }

            // Instantiate the collection of objects to be returned.
            results_ = new List<TUsefulObject>();

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_))
            using (SqlCommand command = new SqlCommand(query_, dbConnection)) {

                dbConnection.Open();

                // Add any SQL parameters that were provided.
                if (parameters_ != null && parameters_.Count > 0) {
                    foreach (SqlParameter parameter in parameters_) { command.Parameters.Add(parameter); }
                }

                // If a non-negative (and non-null) command timeout parameter was provided, update the command.
                if (Utils.isNonNegative(commandTimeout_)) { command.CommandTimeout = commandTimeout_!.Value; }

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.Default)) {

                    while (reader.Read()) {

                        // Create the UsefulObject using the UsefulAttributes and data reader.
                        TUsefulObject? usefulObject = create<TUsefulObject>(attributes, reader);

                        // If the object is valid, add it to the collection of results.
                        if (usefulObject != null) { results_.Add(usefulObject); }
                    }
                }
            }
        }


        // Execute the SQL query (using query parameters) and use the results to populate the (useful) object.
        public static void get<TUsefulObject>(string dbConnectionString_, List<SqlParameter> parameters_, string query_, ref TUsefulObject? result_)
            where TUsefulObject : IUsefulObject, new() {

            // Get any Useful Attributes defined by the generic type.
            List<UsefulAttribute>? attributes = getAttributes<TUsefulObject?>();
            if (attributes == null || attributes.Count < 1) { throw SmartException.create("Invalid Useful Attributes"); }

            // Instantiate the collection of objects to be returned.
            result_ = default;

            using (SqlConnection dbConnection = new SqlConnection(dbConnectionString_))
            using (SqlCommand command = new SqlCommand(query_, dbConnection)) {

                dbConnection.Open();

                // Add any SQL parameters that were provided.
                if (parameters_ != null && parameters_.Count > 0) {
                    foreach (SqlParameter parameter in parameters_) { command.Parameters.Add(parameter); }
                }

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow)) {
                    if (reader.Read()) { result_ = create<TUsefulObject>(attributes, reader); }
                }
            }
        }

        /*
        // Execute the SQL query and use the results to populate a collection of (useful) objects.
        public static void get<TUsefulObject>(string dbConnectionString_, string query_, ref List<TUsefulObject>? usefulObjects_)
            where TUsefulObject : IUsefulObject, new() {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            // Get any Useful Attributes defined by the generic type.
            List<UsefulAttribute>? attributes = getAttributes<TUsefulObject?>();
            if (attributes == null || attributes.Count < 1) { throw SmartException.create("Invalid Useful Attributes"); }

            // Instantiate the collection of objects to be returned.
            usefulObjects_ = new List<TUsefulObject>();

            using (var dbConnection = new SqlConnection(dbConnectionString_)) {

                dbConnection.Open();

                using (var dataAdaptor = new SqlDataAdapter(query_, dbConnection)) {

                    var dataTable = new DataTable();

                    dataAdaptor.Fill(dataTable);

                    // Validate the data table and its rows
                    if (dataTable == null || dataTable.Rows == null || dataTable.Rows.Count < 1) { return; }

                    // Iterate over all rows in the DataTable.
                    foreach (DataRow dataRow in dataTable.Rows) {

                        if (dataRow == null) { continue; }

                        // Create the UsefulObject using the UsefulAttributes and data row.
                        TUsefulObject? usefulObject = create<TUsefulObject>(attributes, dataRow);

                        // If the object is valid, add it to the collection of results.
                        if (usefulObject != null) { usefulObjects_.Add(usefulObject); }
                    }

                }
            }
        }*/

        /*
        // Execute the SQL query and use the results to populate the (useful) object.
        public static void get<TUsefulObject>(string dbConnectionString_, string query_, ref TUsefulObject? usefulObject_)
            where TUsefulObject : IUsefulObject, new() {

            if (string.IsNullOrEmpty(dbConnectionString_)) { throw SmartException.create("Invalid db connection string"); }
            if (string.IsNullOrEmpty(query_)) { throw SmartException.create("Invalid query"); }

            usefulObject_ = default;

            // Get any Useful Attributes defined by the generic type.
            List<UsefulAttribute>? attributes = getAttributes<TUsefulObject?>();
            if (attributes == null || attributes.Count < 1) { throw SmartException.create("Invalid Useful Attributes"); }

            using (SqlConnection dbConnection = new(dbConnectionString_))
            using (SqlDataAdapter dataAdaptor = new SqlDataAdapter(query_, dbConnection)) {

                dbConnection.Open();

                DataTable dataTable = new DataTable();
                dataAdaptor.Fill(dataTable);

                // Validate the data table and its rows
                if (dataTable == null || dataTable.Rows.Count < 1) { return; }

                DataRow dataRow = dataTable.Rows[0];
                if (dataRow == null) { return; }

                // Create and populate the object using UsefulAttributes and the data row.
                usefulObject_ = create<TUsefulObject>(attributes, dataRow);
            }
        }*/

        #endregion

        /*
         * Suggested by Claude.ai on 05/27/26
        // Use an attribute to apply a column value to a UsefulObject property.
        private static void applyAttribute<TUsefulObject>(TUsefulObject obj, UsefulAttribute attr, object? columnValue)
            where TUsefulObject : IUsefulObject {
            if (columnValue != null && columnValue.GetType() == typeof(DBNull))
                columnValue = null;
            if (attr.isRequired && columnValue == null)
                throw new Exception($"Invalid value for column {attr.columnName}");
            obj[attr.propertyName!] = columnValue;
        }*/

    }
}