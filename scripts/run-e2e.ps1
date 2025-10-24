param(
  [switch]$Down
)

$ErrorActionPreference = 'Stop'

function Invoke-E2E {
  Write-Host "Subindo stack com Docker Compose..." -ForegroundColor Cyan
  docker compose up -d --build

  Write-Host "Aguardando serviços ficarem prontos (healthz com retry)..." -ForegroundColor Cyan
  $maxTries = 20
  for($i=1; $i -le $maxTries; $i++){
    try {
      $h1 = Invoke-RestMethod -Method Get -Uri 'http://localhost:5154/healthz'
      $h2 = Invoke-RestMethod -Method Get -Uri 'http://localhost:5290/healthz'
      $h3 = Invoke-RestMethod -Method Get -Uri 'http://localhost:5156/healthz'
      if($h1.status -eq 'ok' -and $h2.status -eq 'ok' -and $h3.status -eq 'ok') { break }
    } catch {}
    if($i -eq $maxTries){ throw "Health check não ficou OK a tempo" }
    Start-Sleep -Seconds 2
  }

  Write-Host "Obtendo token via Gateway..." -ForegroundColor Cyan
  $token = (Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/autenticacao/token' -ContentType 'application/json' -Body '{"Usuario":"admin","Senha":"admin"}').token

  Write-Host "Criando produto..." -ForegroundColor Cyan
  $prodBody = @{ nome='Camiseta'; descricao='100% algodao'; preco=49.9; quantidadeEmEstoque=5 } | ConvertTo-Json -Compress
  $produto = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/estoque/produtos' -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' -Body $prodBody

  # Pequeno aguardo para estabilidade de primeiro acesso e caches
  Start-Sleep -Milliseconds 800

  Write-Host "Criando pedido (2 unidades)..." -ForegroundColor Cyan
  $pedidoBody = @{ itens = @(@{ produtoId = $produto.id; quantidade = 2 }) } | ConvertTo-Json -Compress
  $pedido = $null
  for($i=1;$i -le 6;$i++){
    try {
      $pedido = Invoke-RestMethod -Method Post -Uri 'http://localhost:5154/vendas/pedidos' -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' -Body $pedidoBody -TimeoutSec 20
      break
    } catch {
      if($i -eq 6){ throw }
      $wait = [Math]::Min(10, [Math]::Pow(2, $i) / 2)
      Write-Host "Tentativa $i falhou ao criar pedido, aguardando $wait s e tentando novamente..." -ForegroundColor Yellow
      Start-Sleep -Seconds $wait
    }
  }

  # Poll para confirmar baixa de estoque via evento
  $produtoAtual = $null
  for($i=1; $i -le 10; $i++){
    try {
      $produtoAtual = Invoke-RestMethod -Method Get -Uri ("http://localhost:5154/estoque/produtos/" + $produto.id) -Headers @{ Authorization = "Bearer $token" }
      if($produtoAtual.quantidadeEmEstoque -eq 3){ break }
    } catch {}
    Start-Sleep -Seconds 1
  }
  if(-not $produtoAtual -or $produtoAtual.quantidadeEmEstoque -ne 3){
    throw "Baixa de estoque não ocorreu como esperado após o tempo limite. Atual=$($produtoAtual.quantidadeEmEstoque)"
  }

  Write-Host ("E2E OK | Pedido=" + $pedido.id + " | Total=" + $pedido.valorTotal + " | EstoqueAtual=" + $produtoAtual.quantidadeEmEstoque) -ForegroundColor Green
}

try {
  Invoke-E2E
}
finally {
  if($Down){
    Write-Host "Derrubando stack..." -ForegroundColor Yellow
    docker compose down -v
  }
}
