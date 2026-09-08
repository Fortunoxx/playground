$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$kieBaseUrl = 'http://localhost:62600/kie-server/services/rest/server'
$kieCredential = New-Object PSCredential('kieserver', (ConvertTo-SecureString 'kieserver1!' -AsPlainText -Force))
$artifactPath = Join-Path $repositoryRoot 'rules/target/order-rules-1.0.0.jar'
$artifactDirectory = '/opt/jboss/.m2/repository/com/fortunoxx/playground/order-rules/1.0.0'

Push-Location $repositoryRoot
try {
    docker compose build rules
    docker compose up -d rules

    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try {
            Invoke-WebRequest -Uri $kieBaseUrl -Credential $kieCredential -AllowUnencryptedAuthentication | Out-Null
            $ready = $true
            break
        }
        catch {
            & docker compose ps rules | Out-Host
        }
    }
    if (-not $ready) { throw 'KIE Server did not become ready.' }

    docker run --rm -v "$repositoryRoot/rules:/workspace" -w /workspace maven:3.9-eclipse-temurin-11 mvn -q clean package -DskipTests
    docker compose exec -T rules sh -lc "mkdir -p $artifactDirectory"
    docker compose cp $artifactPath "rules:${artifactDirectory}/order-rules-1.0.0.jar"

    $containerXml = '<kie-container container-id="order-rules"><release-id><group-id>com.fortunoxx.playground</group-id><artifact-id>order-rules</artifact-id><version>1.0.0</version></release-id></kie-container>'
    try {
        Invoke-WebRequest -Uri "$kieBaseUrl/containers/order-rules" -Method Delete -Credential $kieCredential -AllowUnencryptedAuthentication | Out-Null
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 404) { throw }
    }
    Invoke-WebRequest -Uri "$kieBaseUrl/containers/order-rules" -Method Put -Credential $kieCredential -AllowUnencryptedAuthentication -ContentType 'application/xml' -Body $containerXml | Out-Null

    $containers = Invoke-WebRequest -Uri "$kieBaseUrl/containers" -Credential $kieCredential -AllowUnencryptedAuthentication -Headers @{ Accept = 'application/xml' }
    if ($containers.Content -notmatch 'container-id="order-rules"[^>]*status="STARTED"') {
        throw "The order-rules container was not started: $($containers.Content)"
    }
    Write-Output 'order-rules deployed and started.'
}
finally {
    Pop-Location
}
