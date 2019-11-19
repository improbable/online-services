# This file defines the MemoryStore instance the Gateway uses as its queue.

resource "google_redis_instance" "queue" {
    name               = "${var.k8s_cluster_name}-queue"
    location_id        = "${var.gcloud_zone}"
    memory_size_gb     = 1
    authorized_network = "${var.container_network}"
}

output "redis_host" {
    value = "${google_redis_instance.queue.host}"
}
