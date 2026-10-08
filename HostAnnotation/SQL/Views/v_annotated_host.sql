
-- Delete any existing versions of the view.
IF OBJECT_ID('dbo.v_annotated_host') IS NOT NULL
	DROP VIEW dbo.v_annotated_host
GO

CREATE VIEW dbo.v_annotated_host AS


SELECT 
	ah.[host_id],
	h.text AS host_text,
	ah.id AS ah_id,

	ROUND(ah.sci_name_score, 2) AS score,
	ah.status,

	ah.rank_name,
	ah.scientific_name,

	common_name = CASE
		WHEN ah.common_name IS NULL THEN ''
		WHEN ah.common_name = 'cattle' THEN 'cow'
		ELSE ah.common_name
	END,

	ISNULL(ah.com_name_synonyms,'') AS com_name_synonyms,
	
	tl.lineage,
	
	ISNULL(tl.class_name, '') AS class_sci_name,
	ISNULL(ah.taxon_class_cn, '') AS class_common_name,

	ISNULL(tl.host_group, '') as bvbrc_host_group,

	snTaxDB.term_key AS taxonomy_db,
	ah.sci_name_taxonomy_id AS taxonomy_id,

	h.group_id AS host_group_id

FROM annotated_host ah
JOIN hosts h ON h.id = ah.[host_id]
JOIN term snTaxDB ON snTaxDB.term_id = ah.sci_name_taxonomy_db_tid
LEFT JOIN v_tmp_lineage tl ON (
	tl.taxonomy_db = snTaxDB.term_key
	AND tl.taxonomy_id = ah.sci_name_taxonomy_id
)

