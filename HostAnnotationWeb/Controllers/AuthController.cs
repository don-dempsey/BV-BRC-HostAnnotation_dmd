
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net;

using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotationWeb.Models;
using HostAnnotation.Services;
using HostAnnotation.Utilities;


namespace HostAnnotationWeb.Controllers {

    [ApiController]
    public class AuthController : ControllerBase {

        private readonly ILogger<AuthController> _logger;

        private readonly IPersonService _personService;

        private readonly ITokenService _tokenService;


        // C-tor
        public AuthController(ILogger<AuthController> logger_, IPersonService personService_, ITokenService tokenService_) {
            _logger = logger_;
            _personService = personService_;
            _tokenService = tokenService_;
        }

        


        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<LoginResponse> Login(LoginParameters loginParameters) {

            var response = new LoginResponse();
            
            try {
                if (loginParameters == null) { throw SmartException.create("Invalid login parameters"); }
                string? email = loginParameters.email;
                string? password = loginParameters.password;

                if (Utils.isEmptyElseTrim(ref email)) { throw SmartException.create("Invalid email parameter"); }
                if (Utils.isEmptyElseTrim(ref password)) { throw SmartException.create("Invalid password parameter"); }

                // Authenticate the person and, if successful, return the person object.
                var result = await _personService.authenticate(email!, password!);
                if (result.isFailure || result.value == null) {
                    throw new Exception(result.getErrorMessage("Unable to login"));
                }

                Person? person = result.value;

                // Create an auth token.
                string? authToken = _tokenService.createToken(person);

                // Add the auth token to the Response headers.
                if (Response.Headers.ContainsKey(Names.Header.Authorization)) {
                    Response.Headers[Names.Header.Authorization] = authToken;
                } else {
                    Response.Headers.Append(Names.Header.Authorization, authToken);
                }

                response.isAuthenticated = true;
                response.message = "Login successful"; // TODO: What does the UI need for this?
                response.role = person.role;
                response.statusCode = HttpStatusCode.OK;
            }
            catch (SmartException smExc_) {
                response.message = smExc_.Message;
                response.statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception exc_) {
                response.message = exc_.Message;
                response.statusCode = HttpStatusCode.Unauthorized;
            }

            return response;
        }


        [HttpPost]
        [Authorize(Policy = Names.PolicyName.Administrators)]
        [Route("debugAuthToken")]
        public async Task<ApiResponse> DebugAuthToken() {

            var response = new ApiResponse();

            StringValues headers;

            try {

                // Is there an Authorization header in the HTTP Request?
                if (!Request.Headers.TryGetValue(Names.Header.Authorization, out headers)) { 
                    throw SmartException.create("Invalid authorization header"); 
                }

                // Make sure the JSON Web Token exists and isn't empty.
                string? token = headers.First();
                if (Utils.isEmptyElseTrim(ref token)) {
                    throw SmartException.create("Invalid authorization token");
                }

                var authToken = _tokenService.decodeToken(token!);

                response.data = JsonConvert.SerializeObject(authToken);
                response.statusCode = HttpStatusCode.OK;

            } catch (SmartException smExc_) {
                response.message = smExc_.Message;
                response.statusCode = HttpStatusCode.OK;
            } catch (Exception exc_) {
                response.message = exc_.Message;
                response.statusCode = HttpStatusCode.OK;
            }

            return response;
        }

    }
}
