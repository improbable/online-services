import re


def generate_bigquery_assets(client_bq, bigquery_asset_list):

    """ This function provisions all required BigQuery datasets & tables.
    We combine datasets & tables within a single function as creating tables
    requires the same references when creating datasets.
    """

    from common.bigquery_schema import bigquery_table_schema_dict
    from google.cloud.exceptions import NotFound
    from google.cloud import bigquery

    def asset_exists(client, asset_type, reference):

        if asset_type not in ['dataset', 'table']:
            raise TypeError('Error - asset_type must be one of: {dataset, table}!')

        try:
            if asset_type == 'dataset':
                client.get_dataset(reference)
            elif asset_type == 'table':
                client.get_table(reference)
            return True
        except NotFound:
            return False

    # Create dataset if it does not exist..
    for dataset_name in set([bq_asset[0] for bq_asset in bigquery_asset_list]):
        dataset_ref = client_bq.dataset(dataset_name)
        if not asset_exists(client_bq, 'dataset', dataset_ref):
            dataset = bigquery.Dataset(dataset_ref)
            client_bq.create_dataset(dataset)

    # Create table if it does not exist..
    table_list = []
    for bq_asset in bigquery_asset_list:
        dataset_name, table_name, table_schema, table_partition = bq_asset
        table_ref = client_bq.dataset(dataset_name).table(table_name)
        if not asset_exists(client_bq, 'table', table_ref):
            table = bigquery.Table(table_ref, schema=bigquery_table_schema_dict[table_schema])
            if table_partition:
                table.time_partitioning = bigquery.TimePartitioning(
                    type_=bigquery.TimePartitioningType.DAY,
                    field=table_partition)
            table_list.append(client_bq.create_table(table))
        else:
            table_list.append(client_bq.get_table(table_ref))

    return table_list


def source_bigquery_assets(client_bq, bigquery_asset_list):

    """ This function sources BigQuery assets. If this operation fails,
    generate_bigquery_assets() should be invoked.
    """

    table_list = []
    for bq_asset in bigquery_asset_list:
        dataset_name, table_name, partition = bq_asset
        dataset_ref = client_bq.dataset(dataset_name)
        table_ref = dataset_ref.table(table_name)
        table_list.append(client_bq.get_table(table_ref))

    return table_list


def generate_backfill_query(gcp, environment, event_schema, event_environment, category_tuple, ds_start, ds_stop, time_part_tuple, scale_test_name=''):

    """ This function generates a SQL query used to verify which files are already ingested into native BigQuery storage.
    Generally, the pipeline calling this function will omit these files from the total set of files it is tasked to ingest
    into native BigQuery storage.

    When you pass an argument as `--event-category=` or `--event-category=None` it will be None. However, it will be parsed as
    an empty string. This means we assume the gspath in this case should match `.*/event_category=/.*`.
    """

    def extract_filter_tuple(sql_condition_list, name):
        if name is None:
            return "('')"
        else:
            return f"{sql_condition_list}"

    if None not in [ds_start, ds_stop]:
        ds_filter = f"BETWEEN '{ds_start}' AND '{ds_stop}'"
    else:
        ds_filter = "IN ('')"

    if scale_test_name:
        scale_test_logs_filter = f"AND gspath LIKE '%{scale_test_name}%'"
        scale_test_events_filter = f"AND event_type = '{scale_test_name}'"
    else:
        scale_test_logs_filter, scale_test_events_filter = '', ''

    query = f"""
    SELECT DISTINCT
      a.gspath
    FROM
        (
        SELECT DISTINCT
          gspath,
          batch_id
        FROM `{gcp}.logs.native_events_{environment}`
        WHERE event_schema = '{event_schema}'
        AND event_category IN {extract_filter_tuple(*category_tuple)}
        AND event_environment = '{event_environment}'
        AND event_ds {ds_filter}
        AND event_time IN {extract_filter_tuple(*time_part_tuple)}
        {scale_test_logs_filter}
        AND event = 'parse_initiated'
        ) a
    INNER JOIN
        (
        SELECT batch_id
        FROM `{gcp}.native.events_{event_schema}_{environment}`
        WHERE event_environment = '{event_environment}'
        {scale_test_events_filter}
        UNION DISTINCT
        SELECT batch_id
        FROM `{gcp}.logs.native_events_debug_{environment}`
        WHERE event_schema = '{event_schema}'
        AND event_category IN {extract_filter_tuple(*category_tuple)}
        AND event_environment = '{event_environment}'
        AND event_ds {ds_filter}
        AND event_time IN {extract_filter_tuple(*time_part_tuple)}
        {scale_test_logs_filter}
        ) b
    ON a.batch_id = b.batch_id
    ;
    """

    return re.sub(r'\n\s*\n', '\n', query, re.MULTILINE)
