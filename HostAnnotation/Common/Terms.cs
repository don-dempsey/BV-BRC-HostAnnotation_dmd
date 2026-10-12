using System;
using System.Collections.Generic;
using System.Text;

namespace HostAnnotation.Common {

    public class Terms {

        // Return the enum type and value as "<enum type>.<enum value>". Generally, this
        // corresponds to <vocab_key>.<term_key> or term.term_full_key in the database.
        // Created by Claude Haiku 4.5 (duck.ai)
        public static string enumFullKey(Enum? enumValue) {

            if (enumValue == null) {
                return string.Empty;
            }

            var enumType = enumValue!.GetType();
            var enumName = enumType.Name;
            var valueName = enumValue!.ToString();

            return $"{enumName}.{valueName}";
        }

        // Return the enum's name as a string.
        public static string? enumString(Enum? enum_) {
            if (enum_ == null) { return null; }
            return Enum.GetName(enum_.GetType(), enum_);
        }


        public enum vocabularies {
            unknown,

            account_request_status,
            account_request_type,
            api_role,
            curation_type,
            environment,
            host_token_type,
            ncbi_name_class,
            taxonomy_db,
            taxon_match_type,
            user_status
        }


        public enum account_request_status {
            unknown,

            completed,
            error,
            expired,
            not_requested,
            pending,
            unable_to_send
        }

        public enum account_request_type {
            unknown,

            invite_user,
            reset_password,
            verify_email
        }

        public enum api_role {
            unknown,

            administrator,
            api_user,
            curator,
            viewer
        }

        public enum curation_type {
            unknown,

            alternate_spelling,
            filtered_characters,
            ignore,
            stop_word,
            subspecies_qualifier,
            synonym
        }

        public enum environment {
            unknown,

            development,
            production,
            test
        }

        public enum host_token_type {
            unknown,

            add_s_to_end,
            alt_spelling_or_synonym,
            append_spdot,
            curation_ignored,
            minus_one_word,
            minus_one_word_left,
            minus_three_words,
            minus_three_words_left,
            minus_two_words,
            minus_two_words_left,
            //original,
            prepend_common,
            remove_common_append_spdot,
            remove_common_from_start,
            remove_s_append_spdot,
            remove_s_from_end,
            remove_spdot_from_end,
            stop_words_removed,
            unmodified
        }

        public enum ncbi_name_class {
            unknown,

            acronym,
            blast_name,
            common_name,
            equivalent_name,
            genbank_acronym,
            genbank_common_name,
            scientific_name,
            synonym
        }


        public enum ScoreComponentName {
            crossref_score,
            //dbref_count = "dbref_count",
            hostname_type_score,
            match_type_score,
            name_class_score,
            preferred_db_score,
            priority_score,
            rank_name_score,
            taxonomy_id_consensus,
            taxon_name_consensus
        }

        public enum taxonomy_db {
            unknown,

            bv_brc,
            ebird,
            ictv,
            idpcc,
            itis,
            ncbi
        }

        public enum taxon_match_type {
            unknown,

            custom_text,
            direct_match,
            direct_match_cross_reference,
            indirect_match,
            indirect_match_cross_reference,
            manual_selection
        }

        public enum user_status {
            unknown,

            active,
            deleted,
            pending,
            removed
        }

    }
}
