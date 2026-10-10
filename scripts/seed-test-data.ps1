<#
  Seeds base test objects for every entity through the REST API (same calls Scalar makes).
  Run with the API started (Development):   .\scripts\seed-test-data.ps1
  Optional:  -BaseUrl http://localhost:5136   -DbContainer trustpay_postgres
  Safe to re-run: existing users / category / subcategory / tags / lots are reused;
  the order -> review and order -> dispute scenarios are created only together with a NEW lot.
  Output: IDs printed to console + scripts\seed-output.json (IDs and test emails only, no tokens).
#>
param(
    [string]$BaseUrl = "http://localhost:5136",
    [string]$HttpsUrl = "https://localhost:7196",   # from launchSettings.json; used when http redirects to https
    [string]$DbContainer = "trustpay_postgres"
)

$ErrorActionPreference = "Stop"
$Password = "Test123!pass"          # same password for all test users

# --- Detect http -> https redirect (UseHttpsRedirection). PowerShell drops the Authorization header on redirect -> 401. ---
# Dev only: trust the local dev certificate for localhost.
if ($BaseUrl -match "localhost|127\.0\.0\.1") {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
}
try {
    $probe = Invoke-WebRequest -Uri "$BaseUrl/openapi/v1.json" -Method GET -MaximumRedirection 0 -UseBasicParsing
} catch {
    $resp = $_.Exception.Response
    if ($resp -and [int]$resp.StatusCode -ge 300 -and [int]$resp.StatusCode -lt 400) {
        Write-Host "API redirects $BaseUrl to https (status $([int]$resp.StatusCode)), using $HttpsUrl" -ForegroundColor Yellow
        $BaseUrl = $HttpsUrl
    } elseif (-not $resp) {
        throw "API is not reachable at $BaseUrl : $($_.Exception.Message)"
    }
}

function Call($Method, $Path, $Body = $null, $Token = $null) {
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    $args2 = @{ Method = $Method; Uri = "$BaseUrl$Path"; Headers = $headers; ContentType = "application/json" }
    if ($null -ne $Body) { $args2["Body"] = ($Body | ConvertTo-Json -Depth 5) }
    try {
        return Invoke-RestMethod @args2
    } catch {
        $resp = $_.Exception.Response
        $code = if ($resp) { [int]$resp.StatusCode } else { "no response" }
        $detail = $_.ErrorDetails.Message
        if (-not $detail -and $resp) {
            try { $detail = (New-Object System.IO.StreamReader($resp.GetResponseStream())).ReadToEnd() } catch {}
        }
        $auth = $null
        if ($resp -and $resp.Headers) { $auth = $resp.Headers["WWW-Authenticate"] }
        Write-Host "  FAIL $Method $Path -> $code $detail" -ForegroundColor Red
        if ($auth) { Write-Host "       WWW-Authenticate: $auth" -ForegroundColor Red }
        if ($Token) { Write-Host "       (request had a Bearer token: yes)" -ForegroundColor DarkGray }
        else { Write-Host "       (request had a Bearer token: NO)" -ForegroundColor DarkGray }
        return $null
    }
}

function Get-Session($Email, $Nick) {
    $s = Call POST "/api/auth/login" @{ Email = $Email; Password = $Password }
    if (-not $s) { $s = Call POST "/api/auth/register" @{ NickName = $Nick; Email = $Email; Password = $Password } }
    if (-not $s) { throw "Cannot log in or register $Email" }
    return $s
}

Write-Host "== Users ==" -ForegroundColor Cyan
$admin = Call POST "/api/auth/login" @{ Email = "admin@trustpay.com"; Password = "Admin123!" }
if (-not $admin) { throw "Admin login failed. Is the API running in Development and the DB seeded?" }

# --- Auth self-check: does a protected endpoint accept the admin token? (no automatic redirects) ---
function Test-Auth($Base) {
    try {
        $r = Invoke-WebRequest -Uri "$Base/api/users/$($admin.Id)" -Headers @{ Authorization = "Bearer $($admin.Token)" } -MaximumRedirection 0 -UseBasicParsing
        return @{ Status = [int]$r.StatusCode; Location = $null; Auth = $null }
    } catch {
        $resp = $_.Exception.Response
        if (-not $resp) { return @{ Status = 0; Location = $null; Auth = $_.Exception.Message } }
        return @{ Status = [int]$resp.StatusCode; Location = $resp.Headers["Location"]; Auth = $resp.Headers["WWW-Authenticate"] }
    }
}
$chk = Test-Auth $BaseUrl
Write-Host "Auth self-check at ${BaseUrl}: status=$($chk.Status) location=$($chk.Location) www-authenticate=$($chk.Auth)" -ForegroundColor Magenta
if ($chk.Status -ge 300 -and $chk.Status -lt 400) {
    $BaseUrl = if ($chk.Location) { $l = [Uri]$chk.Location; "$($l.Scheme)://$($l.Authority)" } else { $HttpsUrl }
    Write-Host "Redirect detected -> using $BaseUrl" -ForegroundColor Yellow
    $admin = Call POST "/api/auth/login" @{ Email = "admin@trustpay.com"; Password = "Admin123!" }
    $chk = Test-Auth $BaseUrl
    Write-Host "Auth self-check at ${BaseUrl}: status=$($chk.Status) www-authenticate=$($chk.Auth)" -ForegroundColor Magenta
}
if ($chk.Status -eq 401) { throw "Server rejects the admin token (401, www-authenticate='$($chk.Auth)'). Check the API console log for the JwtBearer reason." }

$seller = Get-Session "seller@trustpay.test" "seller_test"
$buyer  = Get-Session "buyer@trustpay.test"  "buyer_test"
$arb    = Get-Session "arbitrator@trustpay.test" "arbitrator_test"
# UserRole: User=0, Arbitrator=1, Admin=2. Role goes into the JWT, so log in again after changing it.
Call PATCH "/api/users/$($arb.Id)/role" @{ NewRole = 1 } $admin.Token | Out-Null
$arb = Get-Session "arbitrator@trustpay.test" "arbitrator_test"
Write-Host "  admin=$($admin.Id)`n  seller=$($seller.Id)`n  buyer=$($buyer.Id)`n  arbitrator=$($arb.Id)"

Write-Host "== Category / SubCategory / Tags ==" -ForegroundColor Cyan
$cat = (Call GET "/api/categories/all") | Where-Object { $_.title -eq "Games (test)" } | Select-Object -First 1
if ($cat) { $categoryId = $cat.id } else {
    # CategoryType: Game=0, Service=1, Software=2, Currency=3, Other=4
    $categoryId = Call POST "/api/categories" @{ Title = "Games (test)"; Description = "Base test category"; Type = 0 } $admin.Token
}
$sub = (Call GET "/api/sub-categories/category/$categoryId") | Where-Object { $_.title -eq "Accounts (test)" } | Select-Object -First 1
if ($sub) { $subCategoryId = $sub.id } else {
    $subCategoryId = Call POST "/api/sub-categories" @{ CategoryId = $categoryId; Title = "Accounts (test)" } $admin.Token
}
$tagIds = @()
foreach ($name in @("test-tag-one", "test-tag-two")) {
    $t = (Call GET "/api/tags?searchTerm=$name") | Where-Object { $_.name -eq $name } | Select-Object -First 1
    if ($t) { $tagIds += $t.id } else { $tagIds += (Call POST "/api/tags" @{ Name = $name } $admin.Token) }
}
Write-Host "  category=$categoryId`n  subCategory=$subCategoryId`n  tags=$($tagIds -join ', ')"

Write-Host "== Lot ==" -ForegroundColor Cyan
$lotTitle = "Test lot: game account"
$lot = (Call GET "/api/lots/user/$($seller.Id)") | Where-Object { $_.title -eq $lotTitle } | Select-Object -First 1
$newLot = $false
if ($lot) { $lotId = $lot.id } else {
    $lotId = Call POST "/api/lots" @{ SubCategoryId = $subCategoryId; Title = $lotTitle; Amount = 500; Currency = "RUB"; ItemsCount = 10 } $seller.Token
    $newLot = $true
}
Write-Host "  lot=$lotId (new: $newLot)"

$orderOk = $null; $orderDisp = $null; $reviewId = $null; $disputeId = $null
if ($newLot -and $lotId) {
    Write-Host "== Order -> Review (completed flow) ==" -ForegroundColor Cyan
    $orderOk = Call POST "/api/orders" @{ LotId = $lotId; Quantity = 1 } $buyer.Token
    Call POST "/api/orders/$orderOk/start"    $null $seller.Token | Out-Null   # only the executor (seller)
    Call POST "/api/orders/$orderOk/complete" $null $buyer.Token  | Out-Null   # only the customer (buyer)
    $reviewId = Call POST "/api/reviews" @{ OrderId = $orderOk; Title = "Great seller"; Message = "Everything works, fast delivery."; Rating = 5 } $buyer.Token
    Write-Host "  order=$orderOk review=$reviewId"

    Write-Host "== Order -> Dispute ==" -ForegroundColor Cyan
    $orderDisp = Call POST "/api/orders" @{ LotId = $lotId; Quantity = 2 } $buyer.Token
    Call POST "/api/orders/$orderDisp/start" $null $seller.Token | Out-Null
    $disputeId = Call POST "/api/disputes" @{ OrderId = $orderDisp; Reason = "Item was not delivered as described in the lot." } $buyer.Token
    Write-Host "  order=$orderDisp dispute=$disputeId"
} else {
    Write-Host "== Orders/Reviews/Disputes skipped (lot already existed) ==" -ForegroundColor Yellow
}

Write-Host "== Wallets ==" -ForegroundColor Cyan
# A wallet is created automatically on user creation; the API has no 'wallet by user' lookup, so read ids from Postgres.
$wallets = @{}
foreach ($u in @(@("seller", $seller), @("buyer", $buyer), @("arbitrator", $arb))) {
    $id = $null
    try {
        $id = (docker exec $DbContainer psql -U postgres -d trustpay_db -t -A -c "SELECT ""Id"" FROM wallets WHERE ""UserId"" = '$($u[1].Id)'" 2>$null) | Select-Object -First 1
    } catch {}
    if ($id) { $wallets[$u[0]] = $id.Trim() }
}
if ($wallets.Count -gt 0) {
    Call POST "/api/wallets/$($wallets['buyer'])/deposits" @{ Amount = 1000; Currency = "RUB" } $buyer.Token | Out-Null
    Write-Host "  wallets: $(($wallets.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join '; ')  (buyer topped up by 1000 RUB)"
} else {
    Write-Host "  could not read wallet ids via docker/psql (column names may differ). Look them up in the DB: SELECT * FROM wallets;" -ForegroundColor Yellow
}

$out = [ordered]@{
    password = $Password
    users = [ordered]@{
        admin      = @{ id = $admin.Id;  email = "admin@trustpay.com";        password = "Admin123!" }
        seller     = @{ id = $seller.Id; email = "seller@trustpay.test" }
        buyer      = @{ id = $buyer.Id;  email = "buyer@trustpay.test" }
        arbitrator = @{ id = $arb.Id;    email = "arbitrator@trustpay.test" }
    }
    categoryId = $categoryId; subCategoryId = $subCategoryId; tagIds = $tagIds; lotId = $lotId
    completedOrderId = $orderOk; reviewId = $reviewId; disputedOrderId = $orderDisp; disputeId = $disputeId
    wallets = $wallets
}
$path = Join-Path $PSScriptRoot "seed-output.json"
$out | ConvertTo-Json -Depth 6 | Set-Content -Path $path -Encoding UTF8
Write-Host "`nSaved IDs to $path" -ForegroundColor Green
Write-Host "Scalar: Auth -> Bearer, paste a token. Fresh tokens (do not commit):" -ForegroundColor Green
Write-Host "  buyer  : $($buyer.Token)"
Write-Host "  seller : $($seller.Token)"
Write-Host "  admin  : $($admin.Token)"
