using System;
using System.Collections.Generic;
using HostAnnotation.Common;
using HostAnnotation.Models;

namespace HostAnnotation.Services {

    public interface ITaxonomyService {

        // Get a specific taxon name
        Task<TaxonName?> getTaxonName(int id_);

        // Search all taxon names.
        List<TaxonName>? searchTaxonNames(string? searchText_, Terms.taxonomy_db? taxonomyDB_);

    }
}
