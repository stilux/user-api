{{/* vim: set filetype=mustache: */}}
{{/*
Expand the name of the chart.
*/}}
{{- define "user-api.name" -}}
{{- default .Chart.Name | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "user-api.fullname" -}}
{{- $name := default .Chart.Name }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "user-api.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "user-api.labels" -}}
helm.sh/chart: {{ include "user-api.chart" . }}
{{ include "user-api.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "user-api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "user-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Full image name
*/}}
{{- define "user-api.fullImageName" -}}
{{- printf "%s:v%s" .Values.api.image.repository .Chart.AppVersion }}
{{- end }}

{{/*
Annotation for config change detection
*/}}
{{- define "user-api.configChangeDetection" -}}
{{- $secret := include (print $.Template.BasePath "/secret.yaml") . | sha256sum -}}
{{- $configmap := include (print $.Template.BasePath "/configmap.yaml") . | sha256sum -}}
checksum/config: {{ print $secret $configmap | sha256sum }}
{{- end }}

{{/*
Create a fully qualified app name.
*/}}
{{- define "user-api.postgresFullname" -}}
{{ printf "%s-%s" (include "user-api.fullname" . ) .Values.dataBase.server }}
{{- end }}

{{/*
Postgres selector labels
*/}}
{{- define "user-api.postgresSelectorLabels" -}}
app.kubernetes.io/name: {{ printf "%s-%s" (include "user-api.name" . ) .Values.dataBase.server }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}