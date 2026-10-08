
using HostAnnotation.Common;
using HostAnnotation.Models;

namespace HostAnnotation.Services {

    public interface IPersonService {

        // Try to authenticate the user with an email address and password.
        Task<Result<Person?>> authenticate(string email_, string password_);

        // Create a new person.
        Task<Result> createPerson(int createdBy_, string? email_, string? firstName_, string? lastName_, Guid orgUID_,
            string? password_, Terms.api_role? role_, Terms.user_status? status_);

        // Get all people.
        Task<List<Person>?> getPeople();

        // Get a person by email.
        //Person? getPerson(string email_);

        // Get a person by ID.
        Task<Person?> getPerson(int id_);



    }
}
