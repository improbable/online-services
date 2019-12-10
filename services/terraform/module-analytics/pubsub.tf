# This file creates a Pub/Sub topic, alongside several GCS notification that trigger
# Pub/Sub messages to be sent to our topic whenever files are created in GCS on specific
# prefixes (file paths).

# Create Pub/Sub Topic.
resource "google_pubsub_topic" "cloud_function_gcs_to_bq_topic" {
  name = "cloud-function-gcs-to-bq-topic"
}

# Enable notifications by giving the correct IAM permission to the unique service account.
data "google_storage_project_service_account" "gcs_account" {}

resource "google_pubsub_topic_iam_member" "member_cloud_function" {
    topic  = google_pubsub_topic.cloud_function_gcs_to_bq_topic.name
    role   = "roles/pubsub.publisher"
    member = "serviceAccount:${data.google_storage_project_service_account.gcs_account.email_address}"
}

# Create GCS to Pub/Sub Topic Notifications.

resource "google_storage_notification" "notification_function_development" {

  depends_on = [
    google_pubsub_topic_iam_member.member_cloud_function,
    google_storage_bucket.analytics_bucket
  ]

  bucket             = "${var.gcloud_project}-analytics"
  payload_format     = "JSON_API_V1"
  topic              = google_pubsub_topic.cloud_function_gcs_to_bq_topic.id
  # See other event_types here: https://cloud.google.com/storage/docs/pubsub-notifications#events
  event_types        = ["OBJECT_FINALIZE"]
  # Only trigger a message to Pub/Sub for files hitting this prefix:
  object_name_prefix = "data_type=json/analytics_environment=development/event_category=function/"
}

resource "google_storage_notification" "notification_function_testing" {

  depends_on = [
    google_pubsub_topic_iam_member.member_cloud_function,
    google_storage_bucket.analytics_bucket
  ]

  bucket             = "${var.gcloud_project}-analytics"
  payload_format     = "JSON_API_V1"
  topic              = google_pubsub_topic.cloud_function_gcs_to_bq_topic.id
  # See other event_types here: https://cloud.google.com/storage/docs/pubsub-notifications#events
  event_types        = ["OBJECT_FINALIZE"]
  # Only trigger a message to Pub/Sub for files hitting this prefix:
  object_name_prefix = "data_type=json/analytics_environment=testing/event_category=function/"
}

resource "google_storage_notification" "notification_function_staging" {

  depends_on = [
    google_pubsub_topic_iam_member.member_cloud_function,
    google_storage_bucket.analytics_bucket
  ]

  bucket             = "${var.gcloud_project}-analytics"
  payload_format     = "JSON_API_V1"
  topic              = google_pubsub_topic.cloud_function_gcs_to_bq_topic.id
  # See other event_types here: https://cloud.google.com/storage/docs/pubsub-notifications#events
  event_types        = ["OBJECT_FINALIZE"]
  # Only trigger a message to Pub/Sub for files hitting this prefix:
  object_name_prefix = "data_type=json/analytics_environment=staging/event_category=function/"
}

resource "google_storage_notification" "notification_function_production" {

  depends_on = [
    google_pubsub_topic_iam_member.member_cloud_function,
    google_storage_bucket.analytics_bucket
  ]

  bucket             = "${var.gcloud_project}-analytics"
  payload_format     = "JSON_API_V1"
  topic              = google_pubsub_topic.cloud_function_gcs_to_bq_topic.id
  # See other event_types here: https://cloud.google.com/storage/docs/pubsub-notifications#events
  event_types        = ["OBJECT_FINALIZE"]
  # Only trigger a message to Pub/Sub for files hitting this prefix:
  object_name_prefix = "data_type=json/analytics_environment=production/event_category=function/"
}

resource "google_storage_notification" "notification_function_live" {

  depends_on = [
    google_pubsub_topic_iam_member.member_cloud_function,
    google_storage_bucket.analytics_bucket
  ]

  bucket             = "${var.gcloud_project}-analytics"
  payload_format     = "JSON_API_V1"
  topic              = google_pubsub_topic.cloud_function_gcs_to_bq_topic.id
  # See other event_types here: https://cloud.google.com/storage/docs/pubsub-notifications#events
  event_types        = ["OBJECT_FINALIZE"]
  # Only trigger a message to Pub/Sub for files hitting this prefix:
  object_name_prefix = "data_type=json/analytics_environment=live/event_category=function/"
}
