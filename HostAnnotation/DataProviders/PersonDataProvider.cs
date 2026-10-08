
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotation.Utilities;

namespace HostAnnotation.DataProviders {

    public class PersonDataProvider : DataProvider {

        // C-tor
        public PersonDataProvider(string dbConnectionString) : base(dbConnectionString) { }


        public async Task<Result> createPerson(int createdBy_, string email_, string firstName_, string lastName_,
            Guid orgUID_, string passwordHash_, Terms.api_role role_, Terms.user_status status_) {

            var parameters = new List<SqlParameter>() {
                Utils.createSqlParam("@createdBy", SqlDbType.Int, createdBy_),
                Utils.createSqlParam("@email", SqlDbType.VarChar, email_),
                Utils.createSqlParam("@firstName", SqlDbType.NVarChar, firstName_),
                Utils.createSqlOutputParam("@id", SqlDbType.Int),
                Utils.createSqlParam("@lastName", SqlDbType.NVarChar, lastName_),
                Utils.createSqlParam("@orgUID", SqlDbType.UniqueIdentifier, orgUID_.ToString()),
                Utils.createSqlParam("@passwordHash", SqlDbType.NVarChar, passwordHash_),
                Utils.createSqlParam("@role", SqlDbType.VarChar, Terms.enumString(role_)),
                Utils.createSqlParam("@status", SqlDbType.VarChar, Terms.enumString(status_!))
            };

            try {
                // TODO: is the id output parameter in the parameters below?
                await DbQueryManager.runStoredProcedureAsync(_dbConnectionString, "dbo.createPerson", parameters);
            }
            catch (Exception e_) {
                return Result.failure(e_.Message);
            }

            return Result.success();
        }


        public async Task<List<Person>?> getAll() {

            var sql = new StringBuilder();
            sql.AppendLine("SELECT ");
            sql.AppendLine(Person.generatePartialQuery());
            sql.AppendFormat("ORDER BY last_name, first_name ");

            return await UsefulObject.getAsyncList<Person>(_dbConnectionString, sql.ToString());
        }

        // Get a person by email address.
        public async Task<Person?> get(string email_) {

            var sql = new StringBuilder();
            sql.AppendLine("SELECT TOP 1 ");
            sql.AppendLine(Person.generatePartialQuery());
            sql.AppendFormat($"WHERE email = '{email_}' ");

            return await UsefulObject.getAsync<Person>(_dbConnectionString, sql.ToString());
        }

        // Get a person by ID.
        public async Task<Person?> get(int id_) {

            var sql = new StringBuilder();
            sql.AppendLine("SELECT TOP 1 ");
            sql.AppendLine(Person.generatePartialQuery());
            sql.AppendFormat($"WHERE id = {id_} ");

            return await UsefulObject.getAsync<Person>(_dbConnectionString, sql.ToString());
        }


    }
}
