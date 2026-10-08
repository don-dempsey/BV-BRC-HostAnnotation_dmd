
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotationWeb.Controllers.Parameters;
using HostAnnotationWeb.Models;
using HostAnnotation.Services;
using HostAnnotation.Utilities;

namespace HostAnnotationWeb.Controllers {

    [ApiController]
    [Authorize(Policy = Names.PolicyName.Administrators)]
    public class AdminController : AuthenticatedController {

        private readonly ILogger<AdminController> _logger;

        private readonly IPersonService _personService;


        // C-tor
        public AdminController(ILogger<AdminController> logger_, IPersonService personService_) {
            _logger = logger_;
            _personService = personService_;
        }


        
        [HttpPost]
        [Route("createPerson")]
        public async Task<ApiResponse> CreatePerson([FromForm, FromBody] AdminParams.CreatePersonForm parameters) {

            var response = new ApiResponse();

            try {

                if (string.IsNullOrEmpty(parameters.email)) { throw new Exception("Invalid email parameter"); }

                Guid organizationUID;
                if (!Guid.TryParse(parameters.orgUID, out organizationUID)) { throw new Exception("Invalid org UID"); }

                // Validate the role parameter and convert it to an enum.
                Terms.api_role? apiRole = null;
                string? strRole = parameters.role;
                if (!Utils.isEmptyElseTrim(ref strRole) && Enum.TryParse(strRole, out Terms.api_role testRole)) { apiRole = testRole; }

                // Validate the status parameter and convert it to an enum.
                Terms.user_status? userStatus = null;
                string? strStatus = parameters.status;
                if (!Utils.isEmptyElseTrim(ref strStatus) && Enum.TryParse(strStatus, out Terms.user_status testStatus)) { userStatus = testStatus; }

                // Create a new person.
                var result = await _personService.createPerson(_userIdentity!.personID, parameters.email, parameters.firstName, parameters.lastName,
                    organizationUID, parameters.password, apiRole, userStatus);

                response.data = true; // TODO: Is this what the UI is expecting?
                response.statusCode = HttpStatusCode.OK;
            }
            catch (Exception ex_) {
                response.message = ex_.Message;
                response.statusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        [HttpPost]
        [Route("getPeople")]
        public async Task<ApiResponse> GetPeople() {

            var response = new ApiResponse();

            try {
                // Validate the user identity.
                if (_userIdentity == null || !_userIdentity.isValid) { throw new Exception("Invalid user identity"); }

                // Get all people
                List<Person>? people = await _personService.getPeople();

                response.data = people;
                response.statusCode = HttpStatusCode.OK;
            }
            catch (Exception ex_) {
                response.message = ex_.Message;
                response.statusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }


    }
}
