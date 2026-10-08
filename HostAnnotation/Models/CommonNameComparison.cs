using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostAnnotation.Utilities;
using HostAnnotation.Common;

namespace HostAnnotation.Models {

    public class CommonNameComparison : UsefulObject {

        // ITIS fields
        [Useful("itis_tn_id", true)]
        public int? itisTnId { get; set; }

        [Useful("itis_name", true)]
        public string? itisName { get; set; }

        [Useful("itis_name_class", true)]
        public Terms.ncbi_name_class? itisNameClass { get; set; }


        // NCBI Taxonomy fields
        [Useful("ncbi_tn_id", true)]
        public int? ncbiTnId { get; set; }

        [Useful("ncbi_name", true)]
        public string? ncbiName { get; set; }

        [Useful("ncbi_name_class", true)]
        public Terms.ncbi_name_class? ncbiNameClass { get; set; }


        // Get all NCBI and ITIS taxa that have the same common name, but with inconsistent case.
        public static string generatePartialQuery() {
            var sql = @"
                SELECT
	                itis.id AS itis_tn_id,
                    itis.name AS itis_name,
	                itis.name_class AS itis_name_class,
	                ncbi.id AS ncbi_tn_id,
                    ncbi.name AS ncbi_name,
                    ncbi.name_class AS ncbi_name_class
        
                FROM v_taxon_name ncbi
                JOIN v_taxon_name itis on itis.name = ncbi.name
                WHERE ncbi.taxonomy_db = 'ncbi'
                AND itis.taxonomy_db = 'itis'
                AND ncbi.name COLLATE LATIN1_GENERAL_CS_AS <> itis.name COLLATE LATIN1_GENERAL_CS_AS
                AND ncbi.name_class IN ('common_name', 'genbank_common_name')
                AND itis.name_class = 'common_name'
            ";

            return sql;
        }
    }
}
