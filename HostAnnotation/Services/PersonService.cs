
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

using HostAnnotation.Common;
using HostAnnotation.DataProviders;
using HostAnnotation.Models;
using HostAnnotation.Utilities;
using System.Security.Policy;

namespace HostAnnotation.Services {

    public class PersonService : IPersonService {

        // The configuration properties.
        private readonly IConfiguration _configuration;

        // The data provider for Person objects.
        protected PersonDataProvider _dataProvider;


        // C-tor
        public PersonService(IConfiguration configuration_) {

            _configuration = configuration_;

            // Get and validate the database connection string.
            string? dbConnectionString = _configuration[Names.ConfigKey.DbConnectionString];
            if (string.IsNullOrEmpty(dbConnectionString)) { throw new Exception("Invalid database connection string"); }

            // Initialize the data provider
            _dataProvider = new PersonDataProvider(dbConnectionString);
        }



        // Try to authenticate the user with an email address and password.
        public async Task<Result<Person?>> authenticate(string email_, string password_) {

            Person? person = null;

            try {
                person = await _dataProvider.get(email_);

                if (person == null) { throw SmartException.create("No matching account"); }
                if (string.IsNullOrEmpty(person.passwordHash)) { 
                    throw new Exception("Invalid password hash"); 
                }

                bool isValid = UserSecurity.verifyPassword(person.passwordHash, password_);
                if (!isValid) { 
                    throw new Exception("Invalid password"); 
                }
            }
            catch (Exception exc_) {
                return Result<Person?>.failure(exc_.Message);
            }

            return Result<Person?>.success(person);
        }


        // Create a new person.
        public async Task<Result> createPerson(int createdBy_, string? email_, string? firstName_, string? lastName_, Guid orgUID_,
            string? password_, Terms.api_role? role_, Terms.user_status? status_) {

            // TODO: validate all parameters
            if (Utils.isEmptyElseTrim(ref email_)) { return Result.failure("Invalid email"); }
            if (Utils.isEmptyElseTrim(ref firstName_)) { return Result.failure("Invalid first name"); }
            if (Utils.isEmptyElseTrim(ref lastName_)) { return Result.failure("Invalid last name"); }
            if (Utils.isEmptyElseTrim(ref password_)) { return Result.failure("Invalid password"); }

            if (status_ == null) { status_ = Terms.user_status.active; }

            string? passwordHash = null;
            if (!Utils.isEmptyElseTrim(ref password_)) { passwordHash = UserSecurity.hashPassword(password_); }
            if (string.IsNullOrEmpty(passwordHash)) { 
                return Result.failure("Invalid password hash"); 
            }
            
            return await _dataProvider.createPerson(createdBy_, email_!, firstName_!, lastName_!, orgUID_, passwordHash, role_!.Value, status_.Value);
        }


        // Get all people.
        public async Task<List<Person>?> getPeople() {
            return await _dataProvider.getAll();
        }


        // Get a person by ID.
        public async Task<Person?> getPerson(int id_) {
            return await _dataProvider.get(id_);
        }

    }
}
