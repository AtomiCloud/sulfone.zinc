# root-chart

![Version: 0.1.0](https://img.shields.io/badge/Version-0.1.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 1.16.0](https://img.shields.io/badge/AppVersion-1.16.0-informational?style=flat-square)

Root Chart to a single Service

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| file://../api_chart | api(dotnet-chart) | 0.1.0 |
| file://../migration_chart | migration(dotnet-migration) | 0.1.0 |
| oci://ghcr.io/dragonflydb/dragonfly/helm | maincache(dragonfly) | v1.9.0 |
| oci://ghcr.io/dragonflydb/dragonfly/helm | altcache(dragonfly) | v1.9.0 |
| oci://registry-1.docker.io/bitnamicharts | mainstorage(minio) | 12.7.0 |
| oci://registry-1.docker.io/bitnamicharts | maindb(postgresql) | 12.5.5 |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| altcache.enable | bool | `false` |  |
| altcache.extraArgs[0] | string | `"--requirepass=supersecret"` |  |
| altcache.nameOverride | string | `"alt-cache"` |  |
| altcache.resources.limits.cpu | int | `1` |  |
| altcache.storage.enabled | bool | `false` |  |
| api.affinity | object | `{}` |  |
| api.appSettings.App.Mode | string | `"Server"` |  |
| api.autoscaling | object | `{}` |  |
| api.configMountPath | string | `"/app/Config/settings.yaml"` |  |
| api.enabled | bool | `true` |  |
| api.fullnameOverride | string | `""` |  |
| api.image.pullPolicy | string | `"IfNotPresent"` |  |
| api.image.repository | string | `"carboxylicacid-carbon-api"` |  |
| api.image.tag | string | `""` |  |
| api.imagePullSecrets | list | `[]` |  |
| api.ingress.enabled | bool | `true` |  |
| api.ingress.hosts[0].host | string | `"api.dotnet.carboxylicacid.lapras.lvh.me"` |  |
| api.ingress.hosts[0].paths[0].path | string | `"/"` |  |
| api.ingress.hosts[0].paths[0].pathType | string | `"ImplementationSpecific"` |  |
| api.ingress.ingressClass | string | `"traefik"` |  |
| api.ingress.tls[0].hosts[0] | string | `"api.dotnet.carboxylicacid.lapras.lvh.me"` |  |
| api.ingress.tls[0].issuerRef | string | `"sample"` |  |
| api.ingress.tls[0].secretName | string | `"sample"` |  |
| api.livenessProbe.httpGet.initialDelaySeconds | int | `5` |  |
| api.livenessProbe.httpGet.path | string | `"/"` |  |
| api.livenessProbe.httpGet.port | string | `"http"` |  |
| api.nameOverride | string | `"api"` |  |
| api.nodeSelector | object | `{}` |  |
| api.podAnnotations | object | `{}` |  |
| api.podSecurityContext | object | `{}` |  |
| api.readinessProbe.httpGet.initialDelaySeconds | int | `5` |  |
| api.readinessProbe.httpGet.path | string | `"/"` |  |
| api.readinessProbe.httpGet.port | string | `"http"` |  |
| api.replicaCount | int | `1` |  |
| api.resources.limits.cpu | int | `1` |  |
| api.resources.limits.memory | string | `"1Gi"` |  |
| api.resources.requests.cpu | string | `"100m"` |  |
| api.resources.requests.memory | string | `"128Mi"` |  |
| api.securityContext | object | `{}` |  |
| api.service.containerPort | int | `9000` |  |
| api.service.port | int | `80` |  |
| api.service.type | string | `"ClusterIP"` |  |
| api.serviceAccount.annotations | object | `{}` |  |
| api.serviceAccount.create | bool | `false` |  |
| api.serviceAccount.name | string | `""` |  |
| api.serviceTree.<<.landscape | string | `"lapras"` |  |
| api.serviceTree.<<.layer | string | `"2"` |  |
| api.serviceTree.<<.platform | string | `"carboxylicacid"` |  |
| api.serviceTree.<<.service | string | `"carbon"` |  |
| api.serviceTree.module | string | `"api"` |  |
| api.tolerations | list | `[]` |  |
| api.topologySpreadConstraints | object | `{}` |  |
| maincache.enable | bool | `false` |  |
| maincache.extraArgs[0] | string | `"--requirepass=supersecret"` |  |
| maincache.nameOverride | string | `"main-cache"` |  |
| maincache.resources.limits.cpu | int | `1` |  |
| maincache.storage.enabled | bool | `false` |  |
| maindb.auth.database | string | `"carboxylicacid-carbon"` |  |
| maindb.auth.password | string | `"supersecret"` |  |
| maindb.auth.username | string | `"admin"` |  |
| maindb.nameOverride | string | `"main-database"` |  |
| maindb.primary.persistence.enabled | bool | `false` |  |
| mainstorage.auth.rootPassword | string | `"supersecret"` |  |
| mainstorage.auth.rootUser | string | `"admin"` |  |
| mainstorage.enable | bool | `false` |  |
| mainstorage.nameOverride | string | `"main-storage"` |  |
| mainstorage.persistence.enabled | bool | `false` |  |
| mainstorage.persistence.size | string | `"10Gi"` |  |
| mainstorage.replicas | int | `1` |  |
| migration.affinity | object | `{}` |  |
| migration.appSettings.App.Mode | string | `"Migration"` |  |
| migration.aspNetEnv | string | `"Development"` |  |
| migration.backoffLimit | int | `4` |  |
| migration.configMountPath | string | `"/app/Config/settings.yaml"` |  |
| migration.enabled | bool | `false` |  |
| migration.fullnameOverride | string | `""` |  |
| migration.image.pullPolicy | string | `"IfNotPresent"` |  |
| migration.image.repository | string | `"carboxylicacid-dotnet-migration"` |  |
| migration.image.tag | string | `""` |  |
| migration.imagePullSecrets | list | `[]` |  |
| migration.nameOverride | string | `"migration"` |  |
| migration.nodeSelector | object | `{}` |  |
| migration.podAnnotations | object | `{}` |  |
| migration.podSecurityContext | object | `{}` |  |
| migration.resources.limits.cpu | string | `"100m"` |  |
| migration.resources.limits.memory | string | `"128Mi"` |  |
| migration.resources.requests.cpu | string | `"500m"` |  |
| migration.resources.requests.memory | string | `"1Gi"` |  |
| migration.securityContext | object | `{}` |  |
| migration.serviceAccount.annotations | object | `{}` |  |
| migration.serviceAccount.create | bool | `false` |  |
| migration.serviceAccount.name | string | `""` |  |
| migration.serviceTree.<<.landscape | string | `"lapras"` |  |
| migration.serviceTree.<<.layer | string | `"2"` |  |
| migration.serviceTree.<<.platform | string | `"carboxylicacid"` |  |
| migration.serviceTree.<<.service | string | `"carbon"` |  |
| migration.serviceTree.module | string | `"migration"` |  |
| migration.tolerations | list | `[]` |  |
| migration.topologySpreadConstraints | object | `{}` |  |
| serviceTree.landscape | string | `"lapras"` |  |
| serviceTree.layer | string | `"2"` |  |
| serviceTree.platform | string | `"carboxylicacid"` |  |
| serviceTree.service | string | `"carbon"` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.11.1](https://github.com/norwoodj/helm-docs/releases/v1.11.1)