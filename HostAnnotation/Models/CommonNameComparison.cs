
using HostAnnotation.Utilities;

namespace HostAnnotation.Models {

    public class CommonNameComparison : UsefulObject {

        // The name and id of the taxon name record from one taxonomy database.
        [Useful("id_1", false)]
        public int? id1 { get; set; }

        [Useful("name_1", false)]
		public string? name1 { get; set; }

        [Useful("rank_1", false)]
        public string? rank1 { get; set; }

        // The name and id of the taxon name record from another taxonomy database.
        [Useful("id_2", false)]
        public int? id2 { get; set; }
        
        [Useful("name_2", false)]
        public string? name2 { get; set; }

        [Useful("rank_2", false)]
        public string? rank2 { get; set; }

    }
}
