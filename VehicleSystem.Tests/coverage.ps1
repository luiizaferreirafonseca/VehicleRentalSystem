# 1. Navigate to the test folder
# PT-BR: Entre na pasta de testes
cd VehiclesSystem.Tests

# 2. Run the tests and generate the Cobertura coverage file
# PT-BR: Executa os testes e gera o arquivo Cobertura
dotnet test --collect:"XPlat Code Coverage"

# 3. Automatically locate the most recently created directory inside TestResults
# PT-BR: Localiza automaticamente o diretório mais recente criado dentro de TestResults
$latestTestDir = Get-ChildItem -Path ".\TestResults" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $latestTestDir) {
    Write-Error "Nenhum resultado de teste encontrado."
    exit
}

$reportPath = Join-Path $latestTestDir.FullName "coverage.cobertura.xml"
$targetDir = "CoverageReport"

# 4. Generate the report using ReportGenerator
# PT-BR: Gera o relatório usando o ReportGenerator
# Note: Used the environment variable for the license as per your example
# PT-BR: Nota: Usei a variável de ambiente para a licença conforme seu exemplo
reportgenerator `
    -reports:"$reportPath" `
    -targetdir:"$targetDir" `
    -reporttypes:Html `
    -license:$env:REPORTGENERATOR_LICENSE

# 5. Open the report in the browser
# PT-BR: Abre o relatório no navegador
ii ".\$targetDir\index.html"