
�
metadata/metadata.protometadatagoogle/api/annotations.proto"�
UpdateDeploymentMetadataRequest#
deployment_id (	RdeploymentIdS
metadata (27.metadata.UpdateDeploymentMetadataRequest.MetadataEntryRmetadata;
MetadataEntry
key (	Rkey
value (	Rvalue:8""
 UpdateDeploymentMetadataResponse"p
!SetDeploymentMetadataEntryRequest#
deployment_id (	RdeploymentId
key (	Rkey
value (	Rvalue"$
"SetDeploymentMetadataEntryResponse"C
GetDeploymentMetadataRequest#
deployment_id (	RdeploymentId"�
GetDeploymentMetadataResponseH
value (22.metadata.GetDeploymentMetadataResponse.ValueEntryRvalue8

ValueEntry
key (	Rkey
value (	Rvalue:8"Z
!GetDeploymentMetadataEntryRequest#
deployment_id (	RdeploymentId
key (	Rkey":
"GetDeploymentMetadataEntryResponse
value (	Rvalue"F
DeleteDeploymentMetadataRequest#
deployment_id (	RdeploymentId"�
 DeleteDeploymentMetadataResponseT
metadata (28.metadata.DeleteDeploymentMetadataResponse.MetadataEntryRmetadata;
MetadataEntry
key (	Rkey
value (	Rvalue:8"]
$DeleteDeploymentMetadataEntryRequest#
deployment_id (	RdeploymentId
key (	Rkey"=
%DeleteDeploymentMetadataEntryResponse
value (	Rvalue2�
DeploymentMetadataService�
UpdateDeploymentMetadata).metadata.UpdateDeploymentMetadataRequest*.metadata.UpdateDeploymentMetadataResponse")���#"/v1/update_deployment_metadata:*�
SetDeploymentMetadataEntry+.metadata.SetDeploymentMetadataEntryRequest,.metadata.SetDeploymentMetadataEntryResponse",���&"!/v1/set_deployment_metadata_entry:*�
GetDeploymentMetadata&.metadata.GetDeploymentMetadataRequest'.metadata.GetDeploymentMetadataResponse"&��� "/v1/get_deployment_metadata:*�
GetDeploymentMetadataEntry+.metadata.GetDeploymentMetadataEntryRequest,.metadata.GetDeploymentMetadataEntryResponse",���&"!/v1/get_deployment_metadata_entry:*�
DeleteDeploymentMetadata).metadata.DeleteDeploymentMetadataRequest*.metadata.DeleteDeploymentMetadataResponse")���#"/v1/delete_deployment_metadata:*�
DeleteDeploymentMetadataEntry..metadata.DeleteDeploymentMetadataEntryRequest/.metadata.DeleteDeploymentMetadataEntryResponse"/���)"$/v1/delete_deployment_metadata_entry:*B+�(Improbable.OnlineServices.Proto.Metadatabproto3