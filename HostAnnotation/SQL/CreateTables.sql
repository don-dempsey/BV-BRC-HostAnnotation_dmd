USE [HOST_ANNOTATION]


/*---------------------------------------------------------------------------
  Vocabulary
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[vocabulary](
	[vocab_id] [int] IDENTITY(1,1) NOT NULL,
	[vocab_description] [nvarchar](max) NULL,
	[vocab_is_view] [int] NULL,
	[vocab_json] [nvarchar](max) NULL,
	[vocab_key] [nvarchar](128) NOT NULL,
	[vocab_label] [nvarchar](128) NOT NULL,
	[vocab_parent_id] [int] NULL,
 CONSTRAINT [PK_vocabulary] PRIMARY KEY CLUSTERED 
(
	[vocab_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_vocabulary_key] UNIQUE NONCLUSTERED 
(
	[vocab_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_vocabulary_label] UNIQUE NONCLUSTERED 
(
	[vocab_label] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Make sure the key is unique.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'vocabulary', @level2type=N'CONSTRAINT',@level2name=N'UK_vocabulary_key'


/*---------------------------------------------------------------------------
  Term
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[term](
	[term_id] [int] IDENTITY(1,1) NOT NULL,
	[term_description] [nvarchar](2000) NULL,
	[term_display_order] [int] NULL,
	[term_full_key] [varchar](256) NOT NULL,
	[term_key] [varchar](128) NOT NULL,
	[term_label] [nvarchar](128) NOT NULL,
	[term_parent_id] [int] NULL,
	[term_vocab_id] [int] NOT NULL,
 CONSTRAINT [PK_term] PRIMARY KEY CLUSTERED 
(
	[term_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_term_full_key] UNIQUE NONCLUSTERED 
(
	[term_full_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_term_key] UNIQUE NONCLUSTERED 
(
	[term_key] ASC,
	[term_vocab_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_term_label] UNIQUE NONCLUSTERED 
(
	[term_label] ASC,
	[term_vocab_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[term]  WITH CHECK ADD  CONSTRAINT [FK_term_vocabulary] FOREIGN KEY([term_vocab_id])
REFERENCES [dbo].[vocabulary] ([vocab_id])

ALTER TABLE [dbo].[term] CHECK CONSTRAINT [FK_term_vocabulary]


/*---------------------------------------------------------------------------
  Account request
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[account_request](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[expires_on] [datetimeoffset](7) NULL,
	[person_id] [int] NOT NULL,
	[requested_by] [int] NOT NULL,
	[requested_on] [datetimeoffset](7) NULL,
	[request_type_tid] [int] NOT NULL,
	[status_tid] [int] NOT NULL,
	[token] [nvarchar](100) NULL,
 CONSTRAINT [PK_account_request] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[account_request] ADD  CONSTRAINT [DF_account_request_requested_on]  DEFAULT (sysdatetimeoffset()) FOR [requested_on]


/*---------------------------------------------------------------------------
  Algorithm version
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[algorithm_version](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[description] [nvarchar](1024) NULL,
	[factor_crossref_score] [int] NULL,
	[factor_hostname_type_score] [float] NULL,
	[factor_match_type_score] [float] NULL,
	[factor_name_class_score] [float] NULL,
	[factor_priority_score] [float] NULL,
	[factor_rank_name_score] [float] NULL,
	[factor_taxon_name_consensus] [float] NULL,
	[factor_taxonomy_id_consensus] [float] NULL,
	[is_active] [bit] NULL,
	[max_crossref_score] [float] NULL,
	[max_hostname_type_score] [float] NULL,
	[max_match_type_score] [float] NULL,
	[max_name_class_score] [float] NULL,
	[max_priority_score] [float] NULL,
	[max_rank_name_score] [float] NULL,
	[max_taxon_name_consensus] [float] NULL,
	[max_taxonomy_id_consensus] [float] NULL,
	[min_crossref_score] [float] NULL,
	[min_hostname_type_score] [float] NULL,
	[min_match_type_score] [float] NULL,
	[min_name_class_score] [float] NULL,
	[min_priority_score] [float] NULL,
	[min_rank_name_score] [float] NULL,
	[min_taxon_name_consensus] [float] NULL,
	[min_taxonomy_id_consensus] [float] NULL,
	[order_crossref_score] [int] NULL,
	[order_hostname_type_score] [int] NULL,
	[order_match_type_score] [int] NULL,
	[order_name_class_score] [int] NULL,
	[order_priority_score] [int] NULL,
	[order_rank_name_score] [float] NULL,
	[order_taxon_name_consensus] [int] NULL,
	[order_taxonomy_id_consensus] [int] NULL,
	[score_multiplier] [float] NULL,
	[created_by] [int] NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_algorithm_version] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[algorithm_version] ADD  CONSTRAINT [DF_algorithm_version_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]


/*---------------------------------------------------------------------------
  Annotated host
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[annotated_host](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[algorithm_id] [int] NULL,
	[common_name] [nvarchar](300) NULL,
	[com_name_is_avian] [bit] NULL,
	[com_name_score] [float] NULL,
	[com_name_synonyms] [nvarchar](max) NULL,
	[com_name_taxonomy_db_tid] [int] NULL,
	[com_name_taxonomy_id] [int] NULL,
	[com_name_taxon_name_match_id] [int] NULL,
	[host_id] [int] NOT NULL,
	[rank_name] [varchar](100) NULL,
	[scientific_name] [nvarchar](300) NULL,
	[sci_name_is_avian] [bit] NULL,
	[sci_name_score] [float] NULL,
	[sci_name_taxonomy_db_tid] [int] NULL,
	[sci_name_taxonomy_id] [int] NULL,
	[sci_name_taxon_name_match_id] [int] NULL,
	[status] [varchar](100) NULL,
	[status_details] [nvarchar](100) NULL,
	[taxon_class_cn] [nvarchar](300) NULL,
	[taxon_class_sn] [nvarchar](300) NULL,
	[taxon_order_sn] [nvarchar](300) NULL,
	[taxon_family_sn] [nvarchar](300) NULL,
	[taxon_genus_sn] [nvarchar](300) NULL,
	[taxon_species_sn] [nvarchar](300) NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_annotated_host] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[annotated_host] ADD  CONSTRAINT [DF_annotated_host_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]
ALTER TABLE [dbo].[annotated_host]  WITH CHECK ADD  CONSTRAINT [FK_ah__hosts] FOREIGN KEY([host_id]) REFERENCES [dbo].[hosts] ([id])
ALTER TABLE [dbo].[annotated_host] CHECK CONSTRAINT [FK_ah__hosts]

/*---------------------------------------------------------------------------
  Curation
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[curation](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[alternate_text] [nvarchar](500) NULL,
	[alternate_text_filtered] [nvarchar](500) NULL,
	[is_valid] [bit] NOT NULL,
	[search_text] [nvarchar](500) NOT NULL,
	[search_text_filtered] [nvarchar](500) NULL,
	[taxonomy_db_tid] [int] NULL,
	[taxonomy_id] [int] NULL,
	[type_tid] [int] NOT NULL,
	[uid] [uniqueidentifier] NULL,
	[created_by] [int] NULL,
	[created_on] [datetimeoffset](7) NULL,
	[validated_by] [int] NULL,
	[validated_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_curation] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[curation] ADD  CONSTRAINT [DF_curation_is_valid]  DEFAULT ((0)) FOR [is_valid]
ALTER TABLE [dbo].[curation] ADD  CONSTRAINT [DF_curation_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]
ALTER TABLE [dbo].[curation]  WITH CHECK ADD  CONSTRAINT [FK_curation_type] FOREIGN KEY([type_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[curation] CHECK CONSTRAINT [FK_curation_type]

/*---------------------------------------------------------------------------
  Custom annotation
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[custom_annotation](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[common_name] [nvarchar](300) NULL,
	[current_host_id] [int] NULL,
	[hostname] [nvarchar](300) NOT NULL,
	[is_active] [bit] NOT NULL,
	[rank_name] [varchar](100) NULL,
	[search_text] [nvarchar](300) NULL,
	[scientific_name] [nvarchar](300) NULL,
	[created_by] [int] NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_custom_annotation] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[custom_annotation] ADD  CONSTRAINT [DF_custom_annotation_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]


/*---------------------------------------------------------------------------
  Ebird
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[ebird](
	[id] [int] NULL,
	[rank_name] [varchar](100) NULL,
	[common_name] [varchar](300) NULL,
	[scientific_name] [varchar](300) NULL,
	[order_name] [varchar](300) NULL,
	[family_name] [varchar](300) NULL,
	[extinct] [smallint] NULL
) ON [PRIMARY]


/*---------------------------------------------------------------------------
  Host token
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[host_token](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[direct_matches] [int] NULL,
	[direct_xref_matches] [int] NULL,
	[filtered_text] [nvarchar](300) NOT NULL,
	[indirect_matches] [int] NULL,
	[indirect_xref_matches] [int] NULL,
	[search_text] [nvarchar](300) NULL,
	[text] [nvarchar](300) NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_host_token] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[host_token] ADD  CONSTRAINT [DF_host_token_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]


/*---------------------------------------------------------------------------
  Host token annotation
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[host_token_annotation](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[algorithm_id] [int] NULL,
	[crossref_score] [float] NULL,
	[crossref_source_name] [nvarchar](300) NULL,
	[crossref_source_name_class_tid] [int] NULL,
	[crossref_source_taxonomy_db_tid] [int] NULL,
	[dbref_count] [int] NULL,
	[hostname] [nvarchar](300) NOT NULL,
	[host_token_id] [int] NOT NULL,
	[host_id] [int] NOT NULL,
	[hostname_type_score] [float] NULL,
	[hostname_type_tid] [int] NOT NULL,
	[is_avian] [bit] NULL,
	[is_curated] [bit] NULL,
	[is_one_of_many] [bit] NULL,
	[is_scientific_name] [bit] NULL,
	[is_valid] [bit] NULL,
	[match_type_score] [float] NULL,
	[match_type_tid] [int] NOT NULL,
	[name_class_score] [float] NULL,
	[name_class_tid] [int] NOT NULL,
	[priority_score] [float] NULL,
	[rank_name] [varchar](100) NULL,
	[rank_name_score] [float] NULL,
	[score] [float] NULL,
	[taxonomy_db_tid] [int] NOT NULL,
	[taxonomy_id] [int] NOT NULL,
	[taxonomy_id_consensus] [float] NULL,
	[taxon_name] [nvarchar](300) NULL,
	[taxon_name_consensus] [float] NULL,
	[taxon_name_match_id] [int] NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_host_token_annotation] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[host_token_annotation] ADD  CONSTRAINT [DF_host_token_annotation_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]
ALTER TABLE [dbo].[host_token_annotation]  WITH CHECK ADD  CONSTRAINT [FK_hta__host] FOREIGN KEY([host_id]) REFERENCES [dbo].[hosts] ([id])
ALTER TABLE [dbo].[host_token_annotation] CHECK CONSTRAINT [FK_hta__host]
ALTER TABLE [dbo].[host_token_annotation]  WITH CHECK ADD  CONSTRAINT [FK_hta__host_token] FOREIGN KEY([host_token_id]) REFERENCES [dbo].[host_token] ([id])
ALTER TABLE [dbo].[host_token_annotation] CHECK CONSTRAINT [FK_hta__host_token]
ALTER TABLE [dbo].[host_token_annotation]  WITH CHECK ADD  CONSTRAINT [FK_hta__host_token_taxon_name_match] FOREIGN KEY([taxon_name_match_id]) REFERENCES [dbo].[host_token_taxon_name_match] ([id])
ALTER TABLE [dbo].[host_token_annotation] CHECK CONSTRAINT [FK_hta__host_token_taxon_name_match]
ALTER TABLE [dbo].[host_token_annotation]  WITH CHECK ADD  CONSTRAINT [FK_hta__hostname_type] FOREIGN KEY([hostname_type_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[host_token_annotation] CHECK CONSTRAINT [FK_hta__hostname_type]
ALTER TABLE [dbo].[host_token_annotation]  WITH CHECK ADD  CONSTRAINT [FK_hta__match_type] FOREIGN KEY([match_type_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[host_token_annotation] CHECK CONSTRAINT [FK_hta__match_type]
ALTER TABLE [dbo].[host_token_annotation]  WITH CHECK ADD  CONSTRAINT [FK_hta__name_class] FOREIGN KEY([name_class_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[host_token_annotation] CHECK CONSTRAINT [FK_hta__name_class]


/*---------------------------------------------------------------------------
  Host token / taxon name match
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[host_token_taxon_name_match](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[custom_name] [nvarchar](500) NULL,
	[custom_name_class_tid] [int] NULL,
	[custom_rank_name] [varchar](100) NULL,
	[host_token_id] [int] NOT NULL,
	[is_active] [bit] NOT NULL,
	[match_type_tid] [int] NOT NULL,
	[parent_match_id] [int] NULL,
	[taxon_name_id] [int] NULL,
	[created_by] [int] NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_host_token_taxon_name_match] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[host_token_taxon_name_match] ADD  CONSTRAINT [DF_host_token_taxon_name_match_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]
ALTER TABLE [dbo].[host_token_taxon_name_match]  WITH CHECK ADD  CONSTRAINT [FK_ht_tnm__host_token] FOREIGN KEY([host_token_id]) REFERENCES [dbo].[host_token] ([id])
ALTER TABLE [dbo].[host_token_taxon_name_match] CHECK CONSTRAINT [FK_ht_tnm__host_token]
ALTER TABLE [dbo].[host_token_taxon_name_match]  WITH CHECK ADD  CONSTRAINT [FK_ht_tnm__match_type] FOREIGN KEY([match_type_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[host_token_taxon_name_match] CHECK CONSTRAINT [FK_ht_tnm__match_type]
ALTER TABLE [dbo].[host_token_taxon_name_match]  WITH CHECK ADD  CONSTRAINT [FK_ht_tnm__taxon_name] FOREIGN KEY([taxon_name_id]) REFERENCES [dbo].[taxon_name] ([id])
ALTER TABLE [dbo].[host_token_taxon_name_match] CHECK CONSTRAINT [FK_ht_tnm__taxon_name]


/*---------------------------------------------------------------------------
  Hosts
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[hosts](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[filtered_text] [nvarchar](600) NULL,
	[group_id] [int] NULL,
	[has_annotations] [bit] NULL,
	[has_derived_hostnames] [bit] NULL,
	[has_hostnames] [bit] NULL,
	[has_taxon_name_matches] [bit] NULL,
	[is_curated] [bit] NULL,
	[is_duplicate] [bit] NULL,
	[is_processed] [bit] NULL,
	[is_valid] [bit] NULL,
	[linked_host_id] [int] NULL,
	[messages] [nvarchar](500) NULL,
	[pre_filtered_text] [nvarchar](600) NULL,
	[text] [nvarchar](600) NOT NULL,
	[created_by] [int] NULL,
	[created_on] [datetimeoffset](7) NULL,
	[processed_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_hosts] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[hosts] ADD  CONSTRAINT [DF_hosts_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]


/*---------------------------------------------------------------------------
  Hosts / host token map
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[hosts_host_token_map](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[base_token_id] [int] NULL,
	[host_id] [int] NOT NULL,
	[host_token_id] [int] NOT NULL,
	[is_one_of_many] [bit] NULL,
	[relation_type_tid] [int] NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_hosts_host_token_map] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[hosts_host_token_map] ADD  CONSTRAINT [DF_hosts_host_token_map_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]
ALTER TABLE [dbo].[hosts_host_token_map]  WITH CHECK ADD  CONSTRAINT [FK_hhtm__relation_type] FOREIGN KEY([relation_type_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[hosts_host_token_map] CHECK CONSTRAINT [FK_hhtm__relation_type]
ALTER TABLE [dbo].[hosts_host_token_map]  WITH CHECK ADD  CONSTRAINT [FK_hhtm_host] FOREIGN KEY([host_id]) REFERENCES [dbo].[hosts] ([id])
ALTER TABLE [dbo].[hosts_host_token_map] CHECK CONSTRAINT [FK_hhtm_host]
ALTER TABLE [dbo].[hosts_host_token_map]  WITH CHECK ADD  CONSTRAINT [FK_hhtm_host_token] FOREIGN KEY([host_token_id]) REFERENCES [dbo].[host_token] ([id])
ALTER TABLE [dbo].[hosts_host_token_map] CHECK CONSTRAINT [FK_hhtm_host_token]


/*---------------------------------------------------------------------------
  Organization
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[organization](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[description] [nvarchar](1024) NULL,
	[icon_url] [nvarchar](500) NULL,
	[long_name] [nvarchar](200) NOT NULL,
	[short_name] [nvarchar](50) NOT NULL,
	[status_tid] [int] NULL,
	[uid] [uniqueidentifier] NULL,
	[created_by] [int] NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_organization] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[organization] ADD  CONSTRAINT [DF_organization_uid]  DEFAULT (newid()) FOR [uid]
ALTER TABLE [dbo].[organization] ADD  CONSTRAINT [DF_organization_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]


/*---------------------------------------------------------------------------
  Person
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[person](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[email] [nvarchar](300) NOT NULL,
	[first_name] [nvarchar](100) NULL,
	[last_name] [nvarchar](100) NULL,
	[organization_id] [int] NOT NULL,
	[password_hash] [nvarchar](300) NULL,
	[role_tid] [int] NOT NULL,
	[status_tid] [int] NOT NULL,
	[uid] [uniqueidentifier] NULL,
	[created_by] [int] NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[person]  WITH CHECK ADD  CONSTRAINT [FK_user_role] FOREIGN KEY([role_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[person] CHECK CONSTRAINT [FK_user_role]
ALTER TABLE [dbo].[person]  WITH CHECK ADD  CONSTRAINT [FK_user_status] FOREIGN KEY([status_tid]) REFERENCES [dbo].[term] ([term_id])
ALTER TABLE [dbo].[person] CHECK CONSTRAINT [FK_user_status]
ALTER TABLE [dbo].[person] ADD  CONSTRAINT [DF_person_uid]  DEFAULT (newid()) FOR [uid]
ALTER TABLE [dbo].[person] ADD  CONSTRAINT [DF_user_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]


/*---------------------------------------------------------------------------
  Taxon name
---------------------------------------------------------------------------*/
CREATE TABLE [dbo].[taxon_name](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[is_valid] [bit] NULL,
	[filtered_name] [nvarchar](500) NOT NULL,
	[name] [nvarchar](500) NOT NULL,
	[name_class_tid] [int] NOT NULL,
	[rank_name] [nvarchar](200) NULL,
	[taxonomy_db_tid] [int] NOT NULL,
	[taxonomy_id] [int] NOT NULL,
	[created_on] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_taxon_name] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[taxon_name] ADD  CONSTRAINT [DF_taxon_name_created_on]  DEFAULT (sysdatetimeoffset()) FOR [created_on]



































