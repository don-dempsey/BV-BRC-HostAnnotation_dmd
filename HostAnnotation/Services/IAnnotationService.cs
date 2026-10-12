
using HostAnnotation.Common;
using HostAnnotation.Models;

namespace HostAnnotation.Services {

    public interface IAnnotationService {

        // Annotate all hosts with this group ID. If a non-null max hosts value is provided, that will be the maximum number of
        // hosts that will be annotated. If null, there is no maximum and all hosts in the group will be annotated. The return
        // value is the number of hosts that were annotated.
        Task<int> annotateHostGroup(int groupID_, int? maxHosts_);

        Task<AnnotatedHost?> annotateHostText(string initialText_);

        Task<AnnotatedHost?> getAnnotatedHost(int hostID_);

        Task<List<HostTaxonMatch>?> getHostTaxaMatches(int hostID_);

        Task<Result> normalizeAllCommonNames();

        Task<Result<List<AnnotatedHost>?>> searchAnnotatedHosts(string searchText_);

    }
}
