
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using HostAnnotation.Common;
using HostAnnotationWeb.Controllers.Parameters;
using HostAnnotation.Models;
using HostAnnotationWeb.Models;
using HostAnnotation.Services;
using HostAnnotation.Utilities;

namespace HostAnnotationWeb.Controllers {

    [ApiController]
    [Authorize(Policy = Names.PolicyName.Curators)]
    public class TaxonomyController : AuthenticatedController {

        private readonly ILogger<TaxonomyController> _logger;

        private readonly ITaxonomyService _taxonomyService;


        // C-tor
        public TaxonomyController(ITaxonomyService taxonomyService_, ILogger<TaxonomyController> logger_) {
            _logger = logger_;
            _taxonomyService = taxonomyService_;
        }


        [HttpPost]
        [Route("getTaxonName")]
        public async Task<ApiResponse> GetTaxonName([FromForm] TaxonomyParams.GetTaxonNameForm form) {

            var response = new ApiResponse();

            try {
                // Validate the user identity.
                if (_userIdentity == null || !_userIdentity.isValid) { throw new Exception("Invalid user identity"); }

                // Sanitize the form parameters.
                form.sanitize();

                int taxonNameID = -1;
                string? strID = form.id;
                if (Utils.isEmptyElseTrim(ref strID) || !int.TryParse(strID, out taxonNameID)) { throw new Exception("Invalid taxon name ID"); }


                TaxonName? taxonName = await _taxonomyService.getTaxonName(taxonNameID);

                response.data = taxonName;
                response.statusCode = HttpStatusCode.OK;
            }
            catch (Exception ex_) {
                response.message = ex_.Message;
                response.statusCode = HttpStatusCode.InternalServerError;
            }

            return response;
        }


        [HttpPost]
        [Route("searchTaxonomyDBs")]
        public async Task<ApiResponse> SearchTaxonomyDatabases([FromForm] TaxonomyParams.SearchTaxonomyDatabasesForm form) {

            var response = new ApiResponse();

            try {
                // Validate the user identity.
                if (_userIdentity == null || !_userIdentity.isValid) { throw new Exception("Invalid user identity"); }

                // Sanitize the form parameters.
                form.sanitize();

                string? searchText = form.searchText;
                if (Utils.isEmptyElseTrim(ref searchText) || searchText!.Length < 3) { throw new Exception("Please search using at least 3 characters"); }

                string? strTaxDB = form.taxonomyDB;
                Terms.taxonomy_db? taxonomyDB = null;
                if (!Utils.isEmptyElseTrim(ref strTaxDB) && Enum.TryParse<Terms.taxonomy_db>(strTaxDB, out Terms.taxonomy_db testTaxDB)) { taxonomyDB = testTaxDB; }

                List<TaxonName>? taxonNames = _taxonomyService.searchTaxonNames(searchText, taxonomyDB);

                response.data = taxonNames;
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
