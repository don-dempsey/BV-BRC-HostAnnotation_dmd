
namespace HostAnnotation.Utilities {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UsefulAttribute : Attribute {

        public string columnName;
        public bool isRequired;
        public bool isIdentity;      // true = skip on INSERT (DB-generated key)
        public bool isInsertable;    // false = skip on INSERT
        public bool isUpdateable;    // false = skip on UPDATE (e.g. created_at)
        public string? propertyName { get; set; }


        // C-tor
        public UsefulAttribute(string columnName_, bool isRequired_,
            bool isInsertable_ = true, bool isUpdateable_ = true, bool isIdentity_ = false) {
            columnName = columnName_;
            isIdentity = isIdentity_;
            isInsertable = isInsertable_;
            isRequired = isRequired_;
            isUpdateable = isUpdateable_;
        }
    }

    /*
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UsefulAttribute : Attribute {

        public string columnName;
        public bool isRequired;

        public string? propertyName { get; set; }


        // C-tor
        public UsefulAttribute(string columnName_, bool isRequired_) {
            columnName = columnName_;
            isRequired = isRequired_;
        }

    }*/
}