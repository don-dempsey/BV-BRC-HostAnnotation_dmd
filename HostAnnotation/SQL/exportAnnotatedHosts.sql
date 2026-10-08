

-- Use this query to populate a CSV file containing all annotated host names.

-- Return annotated hosts from a specific group whose common name might be overridden by an iDPCC common name.
DECLARE @hostGroupID int = 31626

-- Header row for CSV export: 
-- host_text,rank_name,scientific_name,common_name,com_name_synonyms,lineage,class_sci_name,class_common_name,host_group,taxonomy_db,taxonomy_id
SELECT
	host_text,
	rank_name,
	scientific_name,
	common_name = CASE

		-- Use the BV-BRC common name when the scientific name isn't in iDPCC.
		WHEN in_idpcc = 0 THEN bvbrc_common_name

		-- Use the BV-BRC common name when the scientific name is in iDPCC and the 
		-- BV-BRC common name is an approved iDPCC name.
		WHEN idpcc_filtered_names = filtered_common_name THEN bvbrc_common_name
		WHEN idpcc_filtered_names like filtered_common_name+';%' THEN bvbrc_common_name 
		WHEN idpcc_filtered_names like '%;'+filtered_common_name+';%' THEN bvbrc_common_name
		WHEN idpcc_filtered_names like '%;'+filtered_common_name THEN bvbrc_common_name

		-- Defer to the iDPCC common name when it's different from the BV-BRC common name.
		ELSE ISNULL(idpcc_common_names, '')
	END,
	com_name_synonyms,
	lineage,
	class_sci_name,
	class_common_name,
	ISNULL(bvbrc_host_group, '') AS host_group,
	taxonomy_db,
	taxonomy_id
FROM (
	select
		-- Columns from v_annotated_host
		[host_id],
		h.group_id AS host_group_id,
		host_text,
		score,
		[status],
		rank_name,
		scientific_name,
		common_name AS bvbrc_common_name,
		com_name_synonyms,
		lineage,
		class_sci_name,
		class_common_name,
		bvbrc_host_group,
		taxonomy_db,
		taxonomy_id,

		-- The following columns aren't in v_annotated_host:

		-- Filter the BV-BRC common name for easier comparison with (filtered) iDPCC common names.
		filtered_common_name = REPLACE(REPLACE(REPLACE(REPLACE(common_name, ' sp.', '') , 's', ''), '-', ' '), 'a', 'e'),
		idpcc_common_names,
		idpcc_filtered_names,
		idpcc_host_group,
		in_idpcc = CASE WHEN idpcc.Host_Species IS NULL THEN 0 ELSE 1 END

	FROM v_annotated_host ah
	JOIN hosts h ON h.id = ah.host_id
	LEFT JOIN (

		-- If there are multiple iDPCC common names (multiple records) for a single Host_Species (scientific name), combine
		-- them into a single record as a semicolon-delimited list of common names.
		SELECT isd.Host_Species,
			idpcc_common_names = STRING_AGG(isd.Host_Common_Name, ';'),
			idpcc_filtered_names = STRING_AGG(
				REPLACE(REPLACE(REPLACE(REPLACE(isd.Host_Common_Name, ' sp.', '') , 's', ''), '-', ' '), 'a', 'e'),';'),
			idpcc_host_group = MAX(isd.Species_Type)

		FROM idpcc_species_dictionary isd
		GROUP BY isd.Host_Species

	) idpcc ON idpcc.Host_Species = ah.scientific_name
	where h.group_id = @hostGroupID
	AND h.is_valid = 1

) bvbrc_and_idpcc

ORDER BY host_text
