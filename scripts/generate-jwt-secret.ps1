$bytes = New-Object byte[] 48
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$secret = [Convert]::ToBase64String($bytes)
Write-Output "JWT secret generado (64 chars, base64):"
Write-Output $secret
Write-Output ""
Write-Output "Setear como variable de entorno:"
Write-Output "  Windows:  `$env:Jwt__Key = '$secret'"
Write-Output "  Linux:    export Jwt__Key='$secret'"
