# Order rules

This Maven project builds the Drools/KIE rules artifact loaded by the separate KIE Server container.

## Build

Requires Java 11+ and Maven:

```powershell
mvn -f rules/pom.xml clean verify
```

The build produces `rules/target/order-rules-1.0.0.jar`.

## Runtime contract

The KIE module exposes the stateful session `order-rules-session`.
The current API calls it through the KIE Server container `order-rules`.

The inserted order fact must contain `birthDate` and `evaluationAtUtc` values in ISO-8601 form. The rule writes an `allowed` boolean and `reason` into the `orderDecision` global.

The repository uses regular JAR packaging with KIE metadata. Run
`scripts/deploy-rules.ps1` from the repository root to build the JAR in Docker,
install it into the running KIE Server's local Maven repository, deploy or
replace container `order-rules`, and verify that the container is started.
