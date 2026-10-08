
using Microsoft.Extensions.Configuration;
using HostAnnotation.Common;
using HostAnnotation.Models;
using HostAnnotation.DataProviders;

namespace HostAnnotation.Services {

    public class TaxonomyService : ITaxonomyService {

        // The configuration properties.
        private readonly IConfiguration _configuration;

        // The data provider for taxonomy databases.
        protected TaxonomyDataProvider _dataProvider;


        // C-tor
        public TaxonomyService(IConfiguration configuration_) {

            _configuration = configuration_;

            // Get and validate the database connection string.
            string? dbConnectionString = _configuration[Names.ConfigKey.DbConnectionString];
            if (string.IsNullOrEmpty(dbConnectionString)) { throw new Exception("Invalid database connection string"); }

            // Initialize the data provider
            _dataProvider = new TaxonomyDataProvider(dbConnectionString);
        }

        // Get a specific taxon name
        public async Task<TaxonName?> getTaxonName(int id_) {
            return await _dataProvider.getTaxonName(id_);
        }

        // Search all taxon names.
        public List<TaxonName>? searchTaxonNames(string? searchText_, Terms.taxonomy_db? taxonomyDB_) {
            return _dataProvider.search(searchText_, taxonomyDB_);
        }

    }
}
