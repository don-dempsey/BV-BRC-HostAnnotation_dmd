using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAnnotation.Common {

    /*
    TODO: consider including something like this (or severity):
    
    public enum ErrorType {
        Failure = 0,
        Validation = 1,
        Problem = 2,
        NotFound = 3,
        Conflict = 4
    }

    */


    public class Error {

        public string? location { get; set; }

        public string? message { get; set; }


        // C-tor
        public Error(string? message_, string? location_ = null) {
            message = message_;
        }

        public static Error create(string? message_, string? location_ = null) {
            return new Error(message_, location_);
        }
    }
}
