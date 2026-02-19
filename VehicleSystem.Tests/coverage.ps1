# 1. Entre na pasta de testes
cd VehiclesSystem.Tests

# 2. Executa os testes e gera o arquivo Cobertura
dotnet test --collect:"XPlat Code Coverage"

# 3. Localiza automaticamente o diretório mais recente criado dentro de TestResults
$latestTestDir = Get-ChildItem -Path ".\TestResults" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $latestTestDir) {
    Write-Error "Nenhum resultado de teste encontrado."
    exit
}

$reportPath = Join-Path $latestTestDir.FullName "coverage.cobertura.xml"
$targetDir = "CoverageReport"

# 4. Gera o relatório usando o ReportGenerator
# Nota: Usei a variável de ambiente para a licença conforme seu exemplo
reportgenerator `
    -reports:"$reportPath" `
    -targetdir:"$targetDir" `
    -reporttypes:Html `
    -license:$env:REPORTGENERATOR_LICENSE

# 5. Abre o relatório no navegador
ii ".\$targetDir\index.html"