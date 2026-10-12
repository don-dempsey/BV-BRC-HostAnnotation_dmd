
/*
Flag records in the iDPCC species dictionary that have different common names than what 
was assigned by the Host Annotation pipeline in BV-BRC. Also, update the inferred rank name.
*/

declare @hostGroupID int = 31626


/*
TODO: uncomment this and comment the select statement to update the idpcc_species_dictionary table.
update idpcc_species_dictionary
set inferred_rank_name = idpcc_rank_name,
	overrides_common_name = 1
*/

select *
from idpcc_species_dictionary
join (
	select
		distinct scientific_name,
		idpcc_com_names,
		idpcc_rank_name,
		idpcc_host_group,
		idpcc_itis_tsn
	from (
		select *,
			common_names_match = case

				-- The scientific name isn't in iDPCC.
				when in_idpcc = 0 then 0

				-- The BV-BRC common name is an exact match for iDPCC.
				when idpcc_filtered_com_names = bvbrc_filtered_com_name then 1

				-- The BV-BRC common name is one of several approved iDPCC common names.
				when idpcc_filtered_com_names like bvbrc_filtered_com_name+';%' then 1 
				when idpcc_filtered_com_names like '%;'+bvbrc_filtered_com_name+';%' then 1
				when idpcc_filtered_com_names like '%;'+bvbrc_filtered_com_name then 1 

				-- The BV-BRC and iDPCC common names differ.
				else 2
			end
		from (
			select distinct scientific_name as scientific_name,
				bvbrc_sci_names.rank_name as bvbrc_rank_name,
				bvbrc_sci_names.common_name as bvbrc_com_name,
				bvbrc_filtered_com_name = replace(replace(replace(replace(bvbrc_sci_names.common_name, ' sp.', ''), 's', ''), '-', ' '), 'a', 'e'),
		
				in_idpcc = case when isd.Host_Species is not null then 1 else 0 end,

				idpcc_com_names = (
					select STRING_AGG(isd.Host_Common_Name, '; ')
					from idpcc_species_dictionary isd
					where isd.Host_Species = bvbrc_sci_names.scientific_name
				),
				idpcc_filtered_com_names = replace(replace(replace(replace((
					select STRING_AGG(isd.Host_Common_Name, ';')
					from idpcc_species_dictionary isd
					where isd.Host_Species = bvbrc_sci_names.scientific_name
				), ' sp.', '') , 's', ''), '-', ' '), 'a', 'e'),
				idpcc_rank_name = case 
					when isd.Host_Species is null then ''
					when isd.[order] = bvbrc_sci_names.scientific_name then 'order'
					when isd.Family = bvbrc_sci_names.scientific_name then 'family'
					when isd.Genus = bvbrc_sci_names.scientific_name then 'genus'
					when isd.Genus+' sp.' = bvbrc_sci_names.scientific_name then 'genus'
					when isd.Species = bvbrc_sci_names.scientific_name then 'species'
					when isd.Genus+' '+isd.Species = bvbrc_sci_names.scientific_name then 'species'
					else NULL
				end,
				isd.Species_Type as idpcc_host_group, -- Or "species type" in their terminology.
				isd.ITIS_Number as idpcc_itis_tsn
			from (
				select 
					distinct ah.scientific_name,
					ah.common_name,
					ah.rank_name
				from hosts h
				join v_annotated_host ah on ah.host_id = h.id
				where h.group_id = @hostGroupID
				and h.is_valid = 1
			) bvbrc_sci_names
			left join idpcc_species_dictionary isd on isd.Host_Species = bvbrc_sci_names.scientific_name
		
		) bvbrc_and_idpcc
	) filtered_names

	-- Only include scientific names that are in iDPCC.
	where in_idpcc = 1

	-- Limit the results to common names that differ.
	and common_names_match = 2
) names_to_override ON names_to_override.scientific_name = idpcc_species_dictionary.Host_species


