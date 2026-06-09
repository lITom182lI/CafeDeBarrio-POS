$ErrorActionPreference = 'Stop'
try {
  $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5138/api/auth/login" -Method Post -Body '{"email":"admin@cafedebarrio.com","password":"Admin2026!"}' -ContentType "application/json"
  $token = $tokenResponse.token
  $headers = @{ "Authorization" = "Bearer $token" }
  
  $response = Invoke-RestMethod -Uri "http://localhost:5138/api/operadores" -Method Post -Body '{"nombre":"TestOp","pin":"12345"}' -ContentType "application/json" -Headers $headers
  Write-Output "SUCCESS:"
  Write-Output ($response | ConvertTo-Json -Depth 5)
} catch {
  Write-Output "ERROR:"
  if ($null -ne $_.Exception.Response) {
    Write-Output $_.Exception.Response.StatusCode
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $errStr = $reader.ReadToEnd()
    Write-Output $errStr
  } else {
    Write-Output $_.Exception.Message
  }
}
