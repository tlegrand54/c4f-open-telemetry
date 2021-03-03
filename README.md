# Démo Open Telemetry

## Introduction

Le but de cette démo va être de simuler la tournée du facteur et de s'assurer de la distribution du courrier en ayant une traçabilité complète.
Notre "facteur" (Postman) sera un service ASP.NET et les "destinataires" (Alice et Bob) des services Java. Il envoit le courrier par requête HTTP à des adresses définies (localhost:2000 et 3000) avec un petit message.
Bob habite à une adresse inexistante (il n'y a pas de services qui tourne sur 3000), ça nous permettra de voir comment Open Telemetry se comporte en cas d'erreur. 

## Prérequis

Il vous faut Jaeger qui tourne en local, le plus simple est via docker : 

```bash
  docker run -d --name jaeger \
    -e COLLECTOR_ZIPKIN_HTTP_PORT=9411 \
    -p 5775:5775/udp \
    -p 6831:6831/udp \
    -p 6832:6832/udp \
    -p 5778:5778 \
    -p 16686:16686 \
    -p 14268:14268 \
    -p 14250:14250 \
    -p 9411:9411 \
    jaegertracing/all-in-one:1.22
```

Si vous n'avez pas Docker, vous pouvez toujours utiliser le binaire. Plus d'infos ici : https://www.jaegertracing.io/docs/1.22/getting-started/

Une fois la commande exécutée, le UI de Jaeger doit être accessible sur http://localhost:16686

## Les destinataires (Partie Java)

Du Vert.X pour la partie serveur, OpenTelemetry supporte aussi le réactif !

```bash
  cd Alice
  ./mvnw clean package #Si vous êtes sur le réseau michelin utilisez plutôt mvn directement pour fetch depuis Artifactory
```

Maintenant il faut configurer la SDK d'Open Telemetry avec les exporteurs qu'on souhaite utiliser (ici Jaeger) et nommer notre service. 
Sur Windows, passez bien par CMD et non Powershell ni le nouveau terminal Windows avec cette syntaxe. 

Sur Windows | Sur Linux
------------ | -------------
set OTEL_TRACES_EXPORTER=jaeger | export OTEL_TRACES_EXPORTER=jaeger
set OTEL_METRICS_EXPORTER=none | export OTEL_METRICS_EXPORTER=none
set OTEL_RESOURCE_ATTRIBUTES=service.name=alice | export OTEL_RESOURCE_ATTRIBUTES=service.name=alice

Plus d'infos sur les variables sont dispos ici : https://github.com/open-telemetry/opentelemetry-java/tree/main/sdk-extensions/autoconfigure

```bash
java -javaagent:opentelemetry-javaagent-all.jar -jar .\target\alice-1.0.0-SNAPSHOT-fat.jar 
```

La console doit afficher *"Alice's ready to receive her mails..."*

## Le facteur (Partie .NET)

Documentation : https://github.com/open-telemetry/opentelemetry-dotnet

```bash
  cd Postman
  dotnet add package OpenTelemetry.Extensions.Hosting --prerelease
  dotnet add package OpenTelemetry.Exporter.Jaeger --prerelease
  dotnet add package OpenTelemetry.Instrumentation.AspNetCore --prerelease
  dotnet add package OpenTelemetry.Instrumentation.Http --prerelease
  dotnet run
```

Faites une requête sur https://localhost:5001/mail et le facteur fera sa tournée ! 🚀

## Enrichir un Span 

On aimerait bien pouvoir ajouter de l'information dans les spans qui sont remontés dans Jaeger. 
On peut ajouter le contenu du message (en théorie on ne doit pas mettre d'infos sensibles !) mais j'ai ajouté dans les headers des requêtes HTTP le contenu du message ("mail-message"). Vous pouvez donc l'intercepter avec Open Telemetry et ajouter cette info au Span.

Vous trouverez les explications sur comment faire ici : https://github.com/open-telemetry/opentelemetry-dotnet/blob/metrics/src/OpenTelemetry.Instrumentation.Http/README.md#enrich

<details>
  <summary>Et la solution en spoiler :</summary>

  ```csharp
    .AddHttpClientInstrumentation((options) => options.Enrich = (activity, eventName, rawObject) =>
    {
        if (eventName.Equals("OnStartActivity"))
        {
            if (rawObject is HttpRequestMessage httpRequest)
            {
                activity.SetTag("mail-message", httpRequest.Headers.First(x => x.Key.Equals("mail-message")).Value);
            }
        }
    })
  ```
</details>

## Bonus Open Telemetry

Si vous avez encore du temps et un langage de coeur qui n'est ni le C#, ni le Java, vous pouvez tenter d'implémenter Bob.
Il existe pleins de langages instrumentés sur https://github.com/open-telemetry et tous ont des exemples très simples pour commencer.

## Bonus Jaeger

- Vous pouvez comparer des traces entre elles : http://localhost:16686/trace/...
- Dans la vue détaillée, vous pouvez changer le mode d'affichage (Mode JSON, stats, timeline sont dispos)
- http://localhost:16686/dependencies vous affiche une cartographie dans l'onglet DAG